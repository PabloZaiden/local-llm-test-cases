using System.Text;
using HtmlAgilityPack;
using Microsoft.SemanticKernel;

namespace LocalLlmTestCases.Plugins
{
    public class WebBrowser
    {
        [KernelFunction]
        public string ReadWebsiteContent(string url)
        {
            using (var client = new HttpClient())
            {
                var html = client.GetStringAsync(url).Result;
                return ExtractTextWithHyperlinks(html);
            }
        }

        static string ExtractTextWithHyperlinks(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var sb = new StringBuilder();
            foreach (var node in doc.DocumentNode.SelectNodes("//body//text()") ?? Enumerable.Empty<HtmlNode>())
            {
                var text = node.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                }
            }
            
            return sb.ToString();
        }
    }
}