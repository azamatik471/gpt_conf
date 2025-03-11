using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gpt_conf
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BaseUrl = "https://wiki.domrf.ru/rest/api/";
        private const string BearerToken = "ODkwNTY0NTkyNzAyOrPf673+/UYKeLSvRZYnVO5Tun41";

        static void Main(string[] args)
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BearerToken);

           
                var pages = GetConfluencePages("Domb2c");
                SavePagesToFile(pages, "confluence_data2.txt");
                Console.WriteLine($"Экспортировано {pages.Count} страниц в файл confluence_data.txt");
            
            
        }

        static List<PageContent> GetConfluencePages(string spaceKey)
        {
            var result = new List<PageContent>();
            int start = 0;
            const int limit = 100;
            bool hasMore = true;

            while (hasMore)
            {
                var response = client.GetAsync(
                    $"{BaseUrl}content?spaceKey={spaceKey}&limit={limit}&start={start}&expand=body.storage")
                    .Result;

                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().Result;
                var responseData = JsonConvert.DeserializeObject<ConfluenceResponse>(content);

                result.AddRange(responseData.Results);
                start += limit;
                hasMore = responseData.Size >= limit;
                Console.WriteLine(start);
            }

            return result;
        }

        static void SavePagesToFile(List<PageContent> pages, string filePath)
        {
            var writer = new StreamWriter(filePath);
            foreach (var page in pages)
            {
                var cleanContent = CleanHtml(page.Body.Storage.Value);
                writer.WriteLine($"### {page.Title}");
                writer.WriteLine(cleanContent);
                writer.WriteLine();
            }
        }

        static string CleanHtml(string html)
        {
            // Удаляем HTML-теги
            return Regex.Replace(html, "<.*?>", string.Empty);
        }
    }

    // Модели данных
    public class ConfluenceResponse
    {
        public List<PageContent> Results { get; set; }
        public int Size { get; set; }
    }

    public class PageContent
    {
        public string Title { get; set; }
        public BodyContent Body { get; set; }
    }

    public class BodyContent
    {
        public StorageContent Storage { get; set; }
    }

    public class StorageContent
    {
        public string Value { get; set; }
    }
}
