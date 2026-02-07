using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace LaboratoireProgrammation.Project.Services;

public class MemfyAI {
    private static readonly HttpClient _httpClient = new() {
        BaseAddress = new Uri("http://localhost:8000"),
        Timeout = Timeout.InfiniteTimeSpan
    };

    public async IAsyncEnumerable<string> AskQuestionAsync(
        string question,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var encodedQuestion = Uri.EscapeDataString(question);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/chat/{encodedQuestion}");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var buffer = new char[512];
        int charsRead;

        while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0) {
            if (cancellationToken.IsCancellationRequested) yield break;

            yield return new string(buffer, 0, charsRead);
        }
    }
}