using OllamaNET;
using OllamaNET.Extensions;
using OllamaNET.WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddOllamaClient().WithMemoryCache();

var app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
    });
}

app.MapPost("/api/chat/ask", async (ChatRequest request, IOllamaClient client, HttpContext httpContext) =>
{
    var response = await client.AskAsync(request.ConversationId, request.Message, cancellationToken: httpContext.RequestAborted);
    return TypedResults.Ok(new ChatResponse(response.Message?.Content!));
});

app.MapPost("/api/chat/stream", async (ChatRequest request, IOllamaClient client, HttpContext httpContext) =>
{
    return Stream();

    async IAsyncEnumerable<string> Stream()
    {
        var responseStream = client.AskStreamingAsync(request.ConversationId, request.Message, cancellationToken: httpContext.RequestAborted);

        await foreach (var response in responseStream.WithCancellation(httpContext.RequestAborted))
        {
            await Task.Delay(10, httpContext.RequestAborted);
            yield return response.Message.Content;
        }
    }
});

app.MapPost("/api/embeddings", async (EmbeddingRequest request, IOllamaClient client, HttpContext httpContext) =>
{
    var response = await client.CreateEmbeddingAsync(request.Content, cancellationToken: httpContext.RequestAborted);
    return TypedResults.Ok(response);
});

app.Run();