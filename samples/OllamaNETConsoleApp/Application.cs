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

            var response = await client.AskAsync(conversationId, message!);
            Console.WriteLine(response.Message?.Content);
            Console.WriteLine();
        }
        while (!string.IsNullOrWhiteSpace(message));
    }
}