using Microsoft.Extensions.Logging;

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
            tracker.TrackDirectory();


        }
    }
    
}