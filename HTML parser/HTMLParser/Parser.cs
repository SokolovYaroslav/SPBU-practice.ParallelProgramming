using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HTMLParser
{
    public static class Parser
    {
        //1.5 hours and 6190 sites on MaxDepth = 2
        //5-7 minutes and 166 sites on MaxDepth = 1
        private const int MaxDepth = 1;
        private const int ParallelDepth = 1;

        private static readonly ConcurrentDictionary<string, int> GlobalUrls = 
            new ConcurrentDictionary<string, int>();
        private static readonly Regex UrlRegEx =  
            new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Compiled);

        public static async void GetLen(string link) {
            GlobalUrls.TryAdd(link, 42);
            await ParseAsync(link, 0);
            Console.WriteLine($"{GlobalUrls.Count} sites was processed");
        }

        private static async Task ParseAsync(string link, int depth) {
            var html = PrintAndReturnHTML(link);
            
            if (depth >= MaxDepth) {
                return;
            }
            
            var urls = GetUrls(html);
            if (depth >= ParallelDepth) {
                foreach (var url in urls) {
                    Parse(url, depth + 1);
                }
                return;
            }
            
            var tasks = urls.Select(url => ParseAsync(url, depth + 1)).ToArray();

            await Task.WhenAll(tasks);
        }

        private static void Parse(string link, int depth) {
            var html = PrintAndReturnHTML(link);

            if (depth >= MaxDepth) {
                return;
            }
            
            var urls = GetUrls(html);
            foreach (var url in urls) {
                Parse(url, depth + 1);
            }
        }

        private static string PrintAndReturnHTML(string url) {
            string html;
            using (var client = new WebClient()) {
                try {
                    html = client.DownloadString(url);
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    return "";
                }
            }
            
            Console.WriteLine($"Length of {url} is {html.Length}");

            return html;
        }
        
        private static IEnumerable<string> GetUrls(string html) {
            var matches = UrlRegEx.Matches(html);

            var urls = new HashSet<string>();
            foreach (Match match in matches) {
                if (GlobalUrls.TryAdd(match.Value, 42)) {
                    urls.Add(match.Value);                    
                }
            }
            
            return urls;
        }
    }
}