using Microsoft.Extensions.Logging;
#pragma warning disable CA2254

namespace SmartSync_Console
{
    class Tracker
    {
        private readonly string directory = "";
        private readonly FileTree trackedFileTree;
        private static readonly ILogger<Tracker> _logger;
        public static ILogger<Tracker> Logger => _logger;
        static Tracker()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<Tracker>();
        }
        public Tracker(string directory, FileTree fileTree)
        {
            this.directory = fileTree.rootDirectory + @"\" + directory;
            trackedFileTree = fileTree;
        }
        public void TrackDirectory()
        {
            FileSystemWatcher watcher = new()
            {
                Path = directory,
                //Filter = "*",
                NotifyFilter = NotifyFilters.LastWrite
                    | NotifyFilters.FileName
                    | NotifyFilters.DirectoryName
            };

            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnRenamed;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.ReadKey();
        }
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            Logger.LogInformation($"文件或文件夹创建：{e.FullPath}");
            FileOperator.AddFile(trackedFileTree, e.FullPath);
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            Logger.LogInformation($"文件或文件夹删除：{e.FullPath}");
            FileOperator.DeleteFile(trackedFileTree, e.FullPath);
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Logger.LogInformation($"文件或文件夹更改：{e.FullPath}");
            FileOperator.UpdateFile(trackedFileTree, e.FullPath);
        }
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Logger.LogInformation($"文件或文件夹重命名：从 {e.OldFullPath} 到 {e.FullPath}");
            FileOperator.RenameFile(trackedFileTree, e.OldFullPath, e.FullPath);
        }
    }
}
