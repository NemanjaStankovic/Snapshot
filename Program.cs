using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System.Diagnostics;
using System;
using System.Linq;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics.Analysis;
using RestSharp;
using Newtonsoft.Json.Linq;  // If you are using JSON parsing
using System.Text.RegularExpressions;


class Program
{
    private static string openAiApiKey = "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";

    static async Task Main(string[] args)
    {
        // Launch the browser (Chromium in this case)
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var dbContext = new AppDbContext();
        // Create a new browser page (tab)
        var page = await browser.NewPageAsync();
        var url = "https://www.hcl.hr/vijest/vraca-se-minimalisticka-gradnja-grada-strategiju-islanders-new-shores-226004/";
        // Navigate to a URL
        await page.GotoAsync(url);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
        string sspath=$"screenshot.png_{timestamp}.png";
        // Take a screenshot of the page
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = sspath });

        string pageText = await page.InnerTextAsync("body"); // Extract text from the entire body
        string summary = GetSummaryFromPython(pageText);

        var metaDescription = await page.GetAttributeAsync("meta[name='description']", "content");

        // Close the browser
        await browser.CloseAsync();
        var output = new Output{
            URL=url,
            ScreenshotPath=sspath,
            SiteDescription=metaDescription!=null?metaDescription:"NA",
            ShortText=summary
        };
        dbContext.Outputs.Add(output);
        await dbContext.SaveChangesAsync();
        Console.WriteLine("Screenshot taken!");
    }
    static string GetSummaryFromPython(string inputText)
    {
        string pythonScript = "C:\\Users\\neman\\Desktop\\Snapshot\\PlaywrightScreenshotApp\\lexrank_summary.py";  // Putanja do Python skripte
        // Kreiramo proces koji poziva Python
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "C:\\Users\\neman\\AppData\\Local\\Programs\\Python\\Python311\\python.exe",  // Path to Python executable
            Arguments = $"\"{pythonScript}\" \"{inputText}\"",  // Pass the escaped input text
            RedirectStandardOutput = true,
            RedirectStandardError = true,  // Capture errors
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Console.WriteLine("Using Python at: " + startInfo.FileName);

        try
        {
            // Start the Python process
            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                using (StreamReader errorReader = process.StandardError)
                {
                    string output = reader.ReadToEnd();  // Capture standard output
                    string errors = errorReader.ReadToEnd();  // Capture standard error

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
            // Handle exceptions (e.g., if the Python script doesn't run)
            Console.WriteLine("Error: " + ex.Message);
            return null;
        }
    }
}
