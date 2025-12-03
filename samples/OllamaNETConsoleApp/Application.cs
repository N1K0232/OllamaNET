using OllamaNET;

namespace OllamaNETConsoleApp;

public class Application(IOllamaClient client)
{
    public async Task ExecuteAsync()
    {
        //var text = "Questo è un testo";
        //var embeddings = await client.CreateEmbeddingAsync(text);

        //Console.WriteLine(embeddings?.Embedding?.Length);

        var conversationId = Guid.CreateVersion7();
        Console.Write("Instruct the AI on his behaviour: (press enter to skip without instructions) ");

        var setupMessage = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(setupMessage))
        {
            await client.SetupAsync(Guid.CreateVersion7(), setupMessage);
        }

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
                await Task.Delay(5);
                Console.Write(response.Message!.Content);
            }

            Console.WriteLine();
            Console.WriteLine();
        }
        while (!string.IsNullOrWhiteSpace(message));
    }
}