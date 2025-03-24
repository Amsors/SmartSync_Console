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
            });

            ILogger logger = factory.CreateLogger<MainProgram>();

            string TrackDirectory = @"C:\Users\fmz2024\Desktop\TEST";
            FileTree fileTree = new(TrackDirectory);
            Tracker tracker = new(fileTree);
            tracker.TrackDirectory();


        }
    }
    
}