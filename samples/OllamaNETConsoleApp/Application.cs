using OllamaNET;
using OllamaNET.Models;

namespace OllamaNETConsoleApp;

public class Application(IOllamaClient client)
{
    public async Task ExecuteAsync()
    {
        var conversationId = Guid.CreateVersion7();
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
        }
        while (!string.IsNullOrWhiteSpace(message));
    }
}