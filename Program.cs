using System.Threading.Tasks.Dataflow;
using LocalLlmTestCases;
using LocalLlmTestCases.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var openAICompatibleEndpoint = Config.Get("Endpoint");
var model = Config.GetOptional("Model");
var logLevel = Config.GetOptional("LogLevel");
var promptsDirectory = Config.GetOptional("Prompts");

if (string.IsNullOrWhiteSpace(promptsDirectory))
{
    promptsDirectory = AppContext.BaseDirectory + "../../../prompts";
}

LogLevel logLevelKernel = LogLevel.Information;
if (!string.IsNullOrWhiteSpace(logLevel))
{
    if (!Enum.TryParse<LogLevel>(logLevel, true, out var parsedLogLevel))
    {
        throw new ArgumentException($"Invalid log level: {logLevel}");
    }

    logLevelKernel = parsedLogLevel;
}

if (string.IsNullOrWhiteSpace(model))
{
    model = "default";
}

if (openAICompatibleEndpoint.EndsWith("/"))
{
    openAICompatibleEndpoint = openAICompatibleEndpoint.Substring(0, openAICompatibleEndpoint.Length - 1);
}

if (!openAICompatibleEndpoint.EndsWith("/v1"))
{
    openAICompatibleEndpoint += "/v1";
}

var builder = Kernel.CreateBuilder();

builder.AddOpenAIChatCompletion(model, new Uri(openAICompatibleEndpoint), null);
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(logLevelKernel));
var kernel = builder.Build();

var chatService = kernel.GetRequiredService<IChatCompletionService>();
var promptExecutionSettings = new OpenAIPromptExecutionSettings() {
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};

AddPluginsToKernel(kernel);

var prompts = new DirectoryInfo(promptsDirectory).GetFiles("*.txt");

var systemPrompt = prompts.SingleOrDefault(p => p.Name == "system.txt")?.ReadAllText();

var testCases = prompts
    .Where(p => char.IsNumber(p.Name[0]))
    .Select(p => new {
        number = int.Parse(p.Name.Split('-')[0]),
        file = p.Name,
        text = p.ReadAllText()
        })
    .OrderBy(p => p.number).ToArray();

System.Console.WriteLine("Write the number of the test case you want to run or press enter to run all: ");
var testCaseToRun = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(testCaseToRun))
{
    var number = int.Parse(testCaseToRun);

    if (!testCases.Any(p => p.number == number))
    {
        System.Console.WriteLine("Invalid test case number");
        return;
    }

    testCases = testCases.Where(p => p.number == number).ToArray();
}

foreach (var prompt in testCases)
{
    System.Console.WriteLine();
    System.Console.WriteLine();
    Print("Test", prompt.file);
    System.Console.WriteLine();
    
    var history = new ChatHistory();
    if (systemPrompt != null)
    {
        Print("System", systemPrompt);
        history.AddSystemMessage(systemPrompt);
    }

    var userPrompt = prompt.text;
    while (!string.IsNullOrWhiteSpace(userPrompt))
    {
        Print("User", userPrompt);
        history.AddUserMessage(userPrompt);

        var streamedResponses = chatService.GetStreamingChatMessageContentsAsync(history, promptExecutionSettings, kernel);
        if (streamedResponses == null)
        {
            Print("Bot", "No response");
            userPrompt = null;
            continue;
        }

        StartPrint("Bot", "");
        var currentStr = "";
        await foreach (var response in streamedResponses)
        {
            currentStr += response.Content!;
            if (!IsPrefix(currentStr, "[TOOL_REQUEST]"))
            {
                ContinuePrint(response.Content!);
                currentStr = "";
            }
        }
        EndPrint();

        System.Console.WriteLine("Write your response or press enter to continue: ");
        userPrompt = System.Console.ReadLine();
    }
}

bool IsPrefix(string prefix, string str)
{
    if (prefix.Length > str.Length)
    {
        return false;
    }

    for (int i = 0; i < prefix.Length; i++)
    {
        if (prefix[i] != str[i])
        {
            return false;
        }
    }

    return true;
}

void StartPrint(string role, string text)
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss");
    System.Console.Write($"{timestamp} - {role}: {text}");
}

void ContinuePrint(string text)
{
    System.Console.Write(text);
}

void Print(string role, string text)
{
    StartPrint(role, text);
    EndPrint();
}

void EndPrint()
{
    System.Console.WriteLine();
}

void AddPluginsToKernel(Kernel kernel)
{
    kernel.Plugins.AddFromType<UserContextData>();
    kernel.Plugins.AddFromType<Weather>();
    kernel.Plugins.AddFromType<WebBrowser>();
    kernel.Plugins.AddFromType<NewsSources>();
}

static class FileInfoExtensions
{
    public static string ReadAllText(this FileInfo fileInfo)
    {
        return File.ReadAllText(fileInfo.FullName);
    }
}