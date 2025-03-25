using System.Text.Json;
using Microsoft.Extensions.Logging;
#pragma warning disable IDE0079
#pragma warning disable CA2254

namespace SmartSync_Console
{
    class DataBaseCoder
    {
        private static readonly ILogger<DataBaseCoder> _logger;
        public static ILogger<DataBaseCoder> Logger => _logger;
        static DataBaseCoder()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<DataBaseCoder>();
        }
        public static void Encode(FileTree fileTree, string outputPath)
        {
            string jsonString = JsonSerializer.Serialize(fileTree);
            File.WriteAllText(outputPath, jsonString);
        }
        public static void Decode(out FileTree fileTree, string inputPath)
        {
            string jsonString = File.ReadAllText(inputPath);
            if (jsonString != null)
            {
                FileTree? tmp = JsonSerializer.Deserialize<FileTree>(jsonString);
                if (tmp == null)
                {
                    fileTree = new();
                    Logger.LogWarning("can not convert from database to filetree");
                }
                else
                {
                    fileTree = tmp;
                }
            }
            else
            {
                Logger.LogWarning("database is null");
                fileTree = new();
            }
        }
    }
}
