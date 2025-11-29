using OllamaNET;

namespace OllamaNETConsoleApp;

public class Application(IOllamaClient client)
{
    public async Task ExecuteAsync()
    {
        var text = "Questo è un testo";
        var embeddings = await client.CreateEmbeddingAsync(text);

        Console.WriteLine(embeddings?.Embedding?.Length);

        var setupMessage = """
            You're an assistant that must help with software development tasks.
            If the user asks anything else than software development related questions, you must politely refuse to answer.
            If you don't know the answer, reply suggesting to refine the question.
            Reply in the same language of the question.
            """;

        var conversationId = await client.SetupAsync(Guid.CreateVersion7(), setupMessage);
        string? message;

        do
        {
            Console.Write("Ask me anything: ");
            message = Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine();

            var responseStream = client.AskStreamingAsync(conversationId, message!);

            await foreach (var response in responseStream)
            {
                await Task.Delay(10);
                Console.Write(response.Message!.Content);
            }

            Console.WriteLine();
            Console.WriteLine();
        }
        while (!string.IsNullOrWhiteSpace(message));
    }
}