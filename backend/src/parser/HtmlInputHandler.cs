using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace backend.Parser
{
    public class HtmlInputHandler
    {
        private readonly HttpClient _httpClient;

        public HtmlInputHandler()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetHtmlContentAsync(string inputType, string content)
        {
            if (inputType.Equals("1")) 
            {
                try
                {
                    Console.WriteLine("\nMengakses URL dan mengambil HTML nya...");
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    return await _httpClient.GetStringAsync(content);
                }
                catch (HttpRequestException ex)
                {
                    throw new InvalidOperationException($"Gagal mengambil data dari URL: {ex.Message}", ex);
                }
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
    }
}