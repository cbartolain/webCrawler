using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoderByteTest
{
    class Program
    {
        public static List<string> processedLinks;
        static async Task Main(string[] args)
        {

            var baseUrl = "https://therecount.github.io/interview-materials/project-a";
            var link = baseUrl + "/1.html";
            List<string> numbers = new List<string>();
            processedLinks = new List<string>();

            numbers.AddRange(await GetContentAndProcess(link, baseUrl));

            foreach (var n in numbers)
            {
                Console.WriteLine(n.ToString());
            }
            
        }

        private static async Task<List<string>> GetContentAndProcess(string link, string baseUrl)
        {
            List<string> retval = new List<string>();
            var content = await GetContent(link);
            retval.AddRange(FindPhoneNumber(content));
            var links = GetOtherLinks(content, baseUrl);
            var tasks = new List<Task<List<string>>>();
            foreach (var l in links)
            {
                if (!processedLinks.Contains(l))
                {
                    processedLinks.Add(link);
                    retval.AddRange(await GetContentAndProcess(l, baseUrl));
                }
                    //tasks.Add(GetContentAndProcess(l, baseUrl));
            }
            //foreach (var task in await Task.WhenAll(tasks))
            //{
            //    retval.AddRange(task);
            //}
            return retval;
        }

            private static List<string> FindPhoneNumber(string content)
        {
            List<string> retval = new List<string>();

            string strRegex = @"([0-9]{3}-[0-9]{3}-[0-9]{4})";
            Regex matcher = new Regex(strRegex, RegexOptions.None);
            MatchCollection matches = matcher.Matches(@content);

            foreach (Match m in matches)
            {
                retval.Add(m.Value);
            }

            return retval;
        }

        private static List<string> GetOtherLinks(string content, string baseUrl)
        {
            List<string> Links = new List<string>();
            string strRegex = @"<a [^>]*href=(?:'(?<href>.*?)')|(?:""(?<href>.*?)"")";
            Regex matcher = new Regex(strRegex, RegexOptions.None);

            MatchCollection matches = matcher.Matches(@content);

            foreach (Match m in matches)
            {
                //Uri testUri = new Uri(m.Value, UriKind.RelativeOrAbsolute);
                //if (testUri.IsAbsoluteUri)
                //{
                //    Links.Add(testUri.ToString());
                //} else
                //{
                //    Links.Add(new Uri((baseUrl, testUri.ToString()).ToString());
                //}
                if (m.Value.StartsWith("http"))
                {
                    Links.Add(m.Value.Trim('"'));
                }
                else
                {
                    var baseParts = baseUrl.Split("/");
                    string urlBuilder = "";
                    string hrefVal = m.Value.Trim('"').TrimStart('/');

                    urlBuilder += baseParts[0] + "//";
                    for (var i = 1; i < baseParts.Length; i++)
                    {
                        if (!hrefVal.Contains(baseParts[i]))
                        {
                            urlBuilder += baseParts[i] + "/";
                        }
                    }
                    urlBuilder += hrefVal;


                    Links.Add(urlBuilder);
                }
            }
            return Links;
        }

        private static async Task<string> GetContent(string url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
            var retval = await client.GetStringAsync(url);

            return retval;

        }
    }
}
