using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Parser
{
    public class HtmlInputHandler
    {
        private const int MaxHtmlBytes = 5 * 1024 * 1024;
        private static readonly TimeSpan FetchTimeout = TimeSpan.FromSeconds(20);
        private readonly HttpClient _httpClient;

        public HtmlInputHandler()
        {
            _httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/124.0 Safari/537.36"
            );
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd(
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
            );
        }

        public async Task<string> GetHtmlContentAsync(string inputType, string content)
        {
            if (inputType.Equals("1")) 
            {
                return await FetchUrlAsync(content.Trim());
            }
            else if (inputType.Equals("2")) 
            {
                return content;
            }
            else
            {
                throw new ArgumentException("Tipe masukan tidak dikenali.");
            }
        }

        private async Task<string> FetchUrlAsync(string url)
        {
            try
            {
                Console.WriteLine("\nMengakses URL dan mengambil HTML nya...");
                using var timeoutCts = new CancellationTokenSource(FetchTimeout);
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeoutCts.Token
                );

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"Website mengembalikan status {(int)response.StatusCode} {response.ReasonPhrase}."
                    );
                }

                var contentLength = response.Content.Headers.ContentLength;
                if (contentLength > MaxHtmlBytes)
                {
                    throw new InvalidOperationException(
                        $"Ukuran HTML terlalu besar ({FormatBytes(contentLength.Value)}). Batas maksimal {FormatBytes(MaxHtmlBytes)}."
                    );
                }

                await using var stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
                using var buffer = new MemoryStream();
                var chunk = new byte[8192];
                int read;

                while ((read = await stream.ReadAsync(chunk, timeoutCts.Token)) > 0)
                {
                    if (buffer.Length + read > MaxHtmlBytes)
                    {
                        throw new InvalidOperationException(
                            $"Ukuran HTML terlalu besar. Batas maksimal {FormatBytes(MaxHtmlBytes)}."
                        );
                    }

                    buffer.Write(chunk, 0, read);
                }

                return DecodeHtml(buffer.ToArray(), response);
            }
            catch (OperationCanceledException ex)
            {
                throw new InvalidOperationException(
                    $"Timeout saat mengambil URL. Website tidak merespons dalam {FetchTimeout.TotalSeconds:0} detik.",
                    ex
                );
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Gagal mengambil data dari URL: {ex.Message}", ex);
            }
        }

        private static string DecodeHtml(byte[] bytes, HttpResponseMessage response)
        {
            var charset = response.Content.Headers.ContentType?.CharSet?.Trim('"');
            if (!string.IsNullOrWhiteSpace(charset))
            {
                try
                {
                    return Encoding.GetEncoding(charset).GetString(bytes);
                }
                catch (ArgumentException)
                {
                    // Fallback ke UTF-8 jika server mengirim charset yang tidak dikenali runtime.
                }
            }

            return Encoding.UTF8.GetString(bytes);
        }

        private static string FormatBytes(long bytes)
        {
            return bytes >= 1024 * 1024
                ? $"{bytes / 1024d / 1024d:0.##} MB"
                : $"{bytes / 1024d:0.##} KB";
        }
    }
}
