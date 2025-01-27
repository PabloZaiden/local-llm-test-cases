using Microsoft.SemanticKernel;

namespace LocalLlmTestCases.Plugins
{
    public class NewsSources
    {
        [KernelFunction()]
        public NewsSource[] GetNewsSources()
        {
            return [
                new NewsSource {
                    Name = "CNN",
                    Url = "https://www.cnn.com",
                    Language = "en"
                },
                new NewsSource {
                    Name = "BBC",
                    Url = "https://www.bbc.com",
                    Language = "en"
                },
                new NewsSource {
                    Name = "Infobae",
                    Url = "https://infobae.com/?noredirect",
                    Language = "es"
                },
                new NewsSource {
                    Name = "The Wall Street Journal",
                    Url = "https://www.wsj.com",
                    Language = "en"
                },

            ];
        }
    }

    public class NewsSource {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Language { get; set; }
    }
}