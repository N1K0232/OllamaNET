using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using OllamaNET.Caching;
using OllamaNET.Exceptions;
using OllamaNET.Models;

namespace OllamaNET;

internal class OllamaClient : IOllamaClient
{
    private readonly HttpClient httpClient;
    private readonly IOllamaCache cache;
    private readonly OllamaClientOptions options;

    public OllamaClient(HttpClient httpClient, IOllamaCache cache, OllamaClientOptions options)
    {
        this.httpClient = httpClient;
        this.cache = cache;

        if (options.ServiceUrl != OllamaClientOptions.DefaultServiceUrl)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(options.ApiKey, nameof(options.ApiKey));
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        }

        this.options = options;
    }

    public async Task<OllamaChatResponse> AskAsync(Guid conversationId, string message, IEnumerable<Stream>? imagesStreams, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        conversationId = conversationId == Guid.Empty ? Guid.CreateVersion7() : conversationId;

        var messages = await CreateMessageListAsync(conversationId, message, cancellationToken).ConfigureAwait(false);
        var images = imagesStreams is not null ? await StreamToBase64Async(imagesStreams, cancellationToken).ConfigureAwait(false) : null;

        var request = CreateRequest(messages, false, chatOptions, model, images);
        using var httpResponse = await httpClient.PostAsJsonAsync("api/chat", request, cancellationToken).ConfigureAwait(false);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new OllamaClientException(content, (int)httpResponse.StatusCode);
        }

        var response = await httpResponse.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken).ConfigureAwait(false);
        if (addToConversationHistory)
        {
            await AddAssistantResponseAsync(conversationId, messages, response!.Message, cancellationToken).ConfigureAwait(false);
        }

        return response!;
    }

    public async IAsyncEnumerable<OllamaChatResponse> AskStreamingAsync(Guid conversationId, string message, IEnumerable<Stream>? imagesStream = null, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        conversationId = conversationId == Guid.Empty ? Guid.CreateVersion7() : conversationId;

        var messages = await CreateMessageListAsync(conversationId, message, cancellationToken).ConfigureAwait(false);
        var images = imagesStream is not null ? await StreamToBase64Async(imagesStream, cancellationToken).ConfigureAwait(false) : null;

        var request = CreateRequest(messages, true, chatOptions, model, images);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        using var httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!httpResponse.IsSuccessStatusCode)
        {
            var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new OllamaClientException(content, (int)httpResponse.StatusCode);
        }

        var contentBuilder = new StringBuilder();

        using var responseStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var streamReader = new StreamReader(responseStream);

        string? line = null;

        while ((line = await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var chunk = JsonSerializer.Deserialize<OllamaStreamChunk>(line);
            if (chunk?.Message?.Content is not null)
            {
                var content = chunk.Message.Content;
                contentBuilder.Append(content);

                yield return new OllamaChatResponse
                {
                    Message = chunk.Message,
                    Model = chunk.Model!
                };
            }

            if (chunk?.Done ?? false)
            {
                break;
            }
        }

        if (addToConversationHistory)
        {
            await AddAssistantResponseAsync(conversationId, messages, new() { Content = contentBuilder.ToString(), Role = OllamaRoles.Assistant }, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<Guid> SetupAsync(Guid conversationId, string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        conversationId = conversationId == Guid.Empty ? Guid.CreateVersion7() : conversationId;

        var messages = new List<OllamaChatMessage>
        {
            new()
            {
                Role = OllamaRoles.System,
                Content = message
            }
        };

        await cache.SetAsync(conversationId, messages, options.MessageExpiration, cancellationToken).ConfigureAwait(false);
        return conversationId;
    }

    public async Task<IEnumerable<OllamaChatMessage>> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await cache.GetConversationAsync(conversationId, cancellationToken).ConfigureAwait(false);
        var messages = conversation?.ToList() ?? Enumerable.Empty<OllamaChatMessage>();

        return messages;
    }

    public async Task<bool> ConversationExistsAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversationExists = await cache.ExistsAsync(conversationId, cancellationToken).ConfigureAwait(false);
        return conversationExists;
    }

    public async Task DeleteConversationAsync(Guid conversationId, bool preserveSetup = false, CancellationToken cancellationToken = default)
    {
        if (!preserveSetup)
        {
            await cache.RemoveAsync(conversationId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var messages = await cache.GetConversationAsync(conversationId, cancellationToken).ConfigureAwait(false);
            if (messages is not null)
            {
                messages = messages.Where(m => m.Role != OllamaRoles.System);
                await cache.SetAsync(conversationId, messages, options.MessageExpiration, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public async Task AddInteractionAsync(Guid conversationId, string question, string answer, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question, nameof(question));
        ArgumentException.ThrowIfNullOrWhiteSpace(answer, nameof(answer));

        var messages = await cache.GetConversationAsync(conversationId, cancellationToken).ConfigureAwait(false) ?? [];
        messages = messages.Union([
            new()
            {
                Role = OllamaRoles.User,
                Content = question
            },
            new()
            {
                Role = OllamaRoles.Assistant,
                Content = answer
            }
        ]);

        await UpdateCacheAsync(conversationId, messages, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OllamaEmbeddingResponse> CreateEmbeddingAsync(string content, string? model = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

        var request = new OllamaEmbeddingRequest
        {
            Model = !string.IsNullOrWhiteSpace(model) ? model : options.DefaultEmbeddingModel,
            Prompt = content
        };

        using var httpResponse = await httpClient.PostAsJsonAsync("api/embeddings", request, cancellationToken).ConfigureAwait(false);
        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new EmbeddingException(errorMessage, (int)httpResponse.StatusCode);
        }

        var response = await httpResponse.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken).ConfigureAwait(false);
        return response!;
    }

    public async Task<Guid> LoadConversationAsync(Guid conversationId, IEnumerable<OllamaChatMessage> messages, bool replaceHistory = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages, nameof(messages));
        conversationId = conversationId == Guid.Empty ? Guid.CreateVersion7() : conversationId;

        if (!replaceHistory)
        {
            var conversationHistory = await cache.GetConversationAsync(conversationId, cancellationToken).ConfigureAwait(false) ?? [];
            messages = conversationHistory.Union(messages);
        }

        await UpdateCacheAsync(conversationId, messages, cancellationToken).ConfigureAwait(false);
        return conversationId;
    }

    private static async Task<string> StreamToBase64Async(Stream stream, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static async Task<string[]> StreamToBase64Async(IEnumerable<Stream> streams, CancellationToken cancellationToken = default)
    {
        var images = new List<string>();

        foreach (var stream in streams)
        {
            images.Add(await StreamToBase64Async(stream, cancellationToken).ConfigureAwait(false));
        }

        return [.. images];
    }

    private async Task AddAssistantResponseAsync(Guid conversationId, IList<OllamaChatMessage> messages, OllamaChatMessage? message, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(message?.Content?.Trim()))
        {
            messages.Add(message);
        }

        await UpdateCacheAsync(conversationId, messages, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IList<OllamaChatMessage>> CreateMessageListAsync(Guid conversationId, string message, CancellationToken cancellationToken = default)
    {
        var conversation = await cache.GetConversationAsync(conversationId, cancellationToken).ConfigureAwait(false);
        var messages = conversation?.ToList() ?? [];

        messages.Add(new OllamaChatMessage
        {
            Role = OllamaRoles.User,
            Content = message
        });

        return messages;
    }

    private async Task UpdateCacheAsync(Guid conversationId, IEnumerable<OllamaChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var conversation = messages.Where(m => m.Role != OllamaRoles.System);

        if (conversation.Count() > options.MessageLimit)
        {
            conversation = conversation.TakeLast(options.MessageLimit);

            var firstMessage = messages.First();
            if (firstMessage.Role == OllamaRoles.System)
            {
                conversation = conversation.Prepend(firstMessage);
            }

            messages = conversation;
        }

        await cache.SetAsync(conversationId, messages, options.MessageExpiration, cancellationToken).ConfigureAwait(false);
    }

    private OllamaChatRequest CreateRequest(IEnumerable<OllamaChatMessage> messages, bool stream, OllamaChatOptions? chatOptions, string? model, string[]? images)
    {
        var request = new OllamaChatRequest
        {
            Options = chatOptions,
            Stream = stream,
            Messages = [..messages],
            Images = images,
            Model = !string.IsNullOrWhiteSpace(model) ? model : options.DefaultChatModel
        };

        return request;
    }
}