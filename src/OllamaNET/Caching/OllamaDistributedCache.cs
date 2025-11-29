using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OllamaNET.Models;

namespace OllamaNET.Caching;

internal class OllamaDistributedCache(IDistributedCache cache, ILogger<OllamaDistributedCache> logger) : IOllamaCache
{
    public async Task RemoveAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Deleting conversation from cache");
            await cache.RemoveAsync(conversationId.ToString(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Deleting conversation failed");
            throw;
        }
    }

    public async Task<IEnumerable<OllamaChatMessage>?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var content = await cache.GetStringAsync(conversationId.ToString(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var conversation = JsonSerializer.Deserialize<IEnumerable<OllamaChatMessage>>(content);
        return conversation;
    }

    public async Task<bool> ExistsAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var content = await cache.GetStringAsync(conversationId.ToString(), cancellationToken).ConfigureAwait(false);
        return !string.IsNullOrWhiteSpace(content);
    }

    public async Task SetAsync(Guid conversationId, IEnumerable<OllamaChatMessage> messages, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("updating conversation history");
            var content = JsonSerializer.Serialize(messages);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await cache.SetStringAsync(conversationId.ToString(), content, options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to update the conversation history");
            throw;
        }
    }
}