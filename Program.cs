using Microsoft.Playwright;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Args;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Xml.Linq;
using Nest;
using Elasticsearch.Net;
using Humanizer;
using static Humanizer.In;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    private static readonly TelegramBotClient Bot = new TelegramBotClient("8070962725:AAG2fceMAkrsFmVSfVrZ59iL-DwvQnV2uDY");
    private static ElasticClient client;
    
    static async Task Main(string[] args)
    {
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("websites")
            .BasicAuthentication("elastic", "elastic")
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll) // Ignores certificate validation
            .DisableDirectStreaming()
            .EnableDebugMode();
        client = new ElasticClient(settings);
        var pingResponse = client.Ping();
        if (!pingResponse.IsValid)
        {
            Console.WriteLine("Error connecting to Elasticsearch.");
            Console.WriteLine($"Debug Information: {pingResponse.DebugInformation}");

            if (pingResponse.OriginalException != null)
            {
                Console.WriteLine($"Exception: {pingResponse.OriginalException.Message}");
            }
            return;
        }


        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        Bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("Bot is running...");
        await Task.Delay(-1, cts.Token);

       
    }
    static async Task TakeSnapshot(string url, ITelegramBotClient bot, Message message, CancellationToken token)
    {
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;

        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        
        // create a new browser page
        var page = await browser.NewPageAsync();

        try
        {
            await page.GotoAsync(url, new PageGotoOptions { Timeout = 10000 });
        }
        catch (Exception ex)
        {
            await bot.SendTextMessageAsync(message.Chat.Id, $"Invalid URL!", cancellationToken: token);
            return;
        }
        

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
        string sspath=$"screenshot_{timestamp}.png";
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = sspath});
        string pageText = await page.InnerTextAsync("body"); 
        string summary = GetSummaryFromPython(pageText);
        var metaDescription = await page.GetAttributeAsync("meta[name='description']", "content");

        await browser.CloseAsync();
        var output = new Output{
            URL=url,
            ScreenshotPath=sspath,
            SiteDescription=(metaDescription!=null?metaDescription:"NA").ToLower(),
            ShortText=summary.ToLower()
        };

        //elastic search
        var indexResponse = client.Index(output, i => i
        .Index("websites")
        .Id(timestamp)
);
        Console.WriteLine(indexResponse.DebugInformation);
        if (indexResponse.IsValid)
        {
            Console.WriteLine("Successful indexing!");
        }
        else
        {
            Console.WriteLine("Not successful indexing!");
        }

        var dbContext = new AppDbContext();
        dbContext.Outputs.Add(output);
        await dbContext.SaveChangesAsync();

        //checking indexes
//        var searchResponse = client.Search<Output>(s => s
//    .Index("websites")
//    .Size(10) 
//    .Query(q => q.MatchAll()) 
//);

//        if (searchResponse.IsValid)
//        {
//            Console.WriteLine("Documents in 'websites' index:");
//            foreach (var doc in searchResponse.Documents)
//            {
//                Console.WriteLine($"- URL: {doc.URL}");
//                Console.WriteLine($"  Description: {doc.SiteDescription}");
//                Console.WriteLine($"  Summary: {doc.ShortText}");
//                Console.WriteLine("-----------");
//            }
//        }
//        else
//        {
//            Console.WriteLine("Failed to retrieve documents.");
//            Console.WriteLine(searchResponse.DebugInformation);
//        }

        Console.WriteLine("Screenshot taken!");
        await bot.SendTextMessageAsync(message.Chat.Id, $"Screenshot of {url} taken! 😊", cancellationToken: token);
        
    }
    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { } message || message.Text is not { } messageText)
            return;

        Console.WriteLine($"Received: {messageText}");

        string[] parts = messageText.Split(' ');
        string command = parts[0].ToLower();
        string argument = parts.Length > 1 ? parts[1] : null;

        switch (command)
        {
            case "snapshot":
                await TakeSnapshot(argument,bot,message, token);
                break;
            case "getsnapshot":
                string[] searchTerms = messageText.Split(' ', 2);
                await Elasticsearch(searchTerms[1], bot,message);
                Console.WriteLine(searchTerms[1]);
                break;
            default:
                await bot.SendTextMessageAsync(message.Chat.Id, $"Command not recognized! ", cancellationToken: token);
                break;
        }
    }
    private static async Task Elasticsearch(string searchTerms, ITelegramBotClient bot, Message message)
    {
        var searchResponse = client.Search<Output>(s => s
            .Index("websites")  
            .Size(10)            
            .Query(q => q
                .Bool(b => b
                    .Should(
                        bs => bs.Match(m => m
                            .Field("shortText")
                            .Query(searchTerms)
                            .Fuzziness(Fuzziness.Auto)
                        ),
                        bs => bs.Match(m => m
                            .Field("siteDescription")
                            .Query(searchTerms)
                            .Fuzziness(Fuzziness.Auto)
                        )
                    )
                )
            ).Highlight(h => h
            .Fields(
                f => f.Field("shortText"),
                f => f.Field("siteDescription")
            )
        )
        );

        if (searchResponse.IsValid && searchResponse.Documents.Count > 0)
        {
            string response = "";
            foreach (var hit in searchResponse.Hits)
            {
                var highlightedShortText = hit.Highlight.ContainsKey("shortText")
               ? string.Join("...", hit.Highlight["shortText"])
               : hit.Source.ShortText;

                var highlightedSiteDescription = hit.Highlight.ContainsKey("siteDescription")
                    ? string.Join("...", hit.Highlight["siteDescription"])
                : hit.Source.SiteDescription;
                highlightedShortText = highlightedShortText.Replace("<em>", "👉").Replace("</em>", "👈");
                highlightedSiteDescription = highlightedSiteDescription.Replace("<em>", "👉").Replace("</em>", "👈");

                response = "Search results:\n";
                response += $"URL: {hit.Source.URL}\nDescription: {highlightedSiteDescription}\nSummary: {highlightedShortText}\nSnapshot 👇\n";
                await bot.SendTextMessageAsync(message.Chat.Id, response);

                string filePath = @$"C:\Users\neman\Desktop\Snapshot\PlaywrightScreenshotApp\bin\Debug\net6.0\{hit.Source.ScreenshotPath}";
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await bot.SendPhotoAsync(
                        chatId: message.Chat.Id, 
                        photo: stream 
                    );
                }
            }
        }
        else
        {
            await bot.SendTextMessageAsync(message.Chat.Id, $"No results found.");
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
    static string GetSummaryFromPython(string inputText)
    {
        string pythonScript = "C:\\Users\\neman\\Desktop\\Snapshot\\PlaywrightScreenshotApp\\lexrank_summary.py";
        string tempFilePath = Path.Combine(Path.GetTempPath(), "snapshot_input.txt");

        // save inputText to a temporary file
        File.WriteAllText(tempFilePath, inputText);
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "C:\\Users\\neman\\AppData\\Local\\Programs\\Python\\Python311\\python.exe",
            Arguments = $"\"{pythonScript}\" \"{tempFilePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8
        };
        Console.WriteLine("Using Python at: " + startInfo.FileName);

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                using (StreamReader errorReader = process.StandardError)
                {
                    string output = reader.ReadToEnd();
                    string errors = errorReader.ReadToEnd();

                    if (!string.IsNullOrEmpty(errors))
                    {
                        Console.WriteLine("Python error: " + errors);
                    }

                    return output;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return null;
        }
    }
}
