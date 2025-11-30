using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OllamaNET.Models;

namespace OllamaNET.Extensions;

public static class AsyncEnumerableExtensions
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "This method returns an IAsyncEnumerable")]
    public static async IAsyncEnumerable<string> AsDeltas(this IAsyncEnumerable<OllamaChatResponse> responseStream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in responseStream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return response.Message.Content;
        }
    }
}