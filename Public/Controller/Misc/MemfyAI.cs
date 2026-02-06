using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace LaboratoireProgrammation.Public.Controller.Misc;

public class MemfyAI {
    
    private static readonly HttpClient _httpClient = new HttpClient {
        BaseAddress = new Uri("http://localhost:8000"),
        Timeout = Timeout.InfiniteTimeSpan
    };
    
    public async IAsyncEnumerable<string> AskQuestionAsync(
        string question, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
    
        string encodedQuestion = Uri.EscapeDataString(question);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/chat/{encodedQuestion}");
    
        using var response = await _httpClient.SendAsync(
            request, 
            HttpCompletionOption.ResponseHeadersRead, 
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        char[] buffer = new char[512]; 
        int charsRead;

        while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0) {
            if (cancellationToken.IsCancellationRequested) yield break;

            yield return new string(buffer, 0, charsRead);
        }
    }
}