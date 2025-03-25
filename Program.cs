using System.Text.Json;
using Microsoft.Extensions.Logging;
#pragma warning disable IDE0079
#pragma warning disable CA2254

namespace SmartSync_Console
{
    class MainProgram
    {
        static void Main()
        {

            using ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            ILogger logger = factory.CreateLogger<MainProgram>();

            FileTree fileTree = new("root", @"C:\Users\fmz2024\Desktop\TEST");
            Tracker tracker = new(@"root", fileTree);

            DataBaseCoder.Encode(fileTree, "data.json");
            DataBaseCoder.Decode(out FileTree another, "data.json");

            tracker.TrackDirectory();
        }
    }
    
}