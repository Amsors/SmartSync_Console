namespace SmartSync_Console
{
    class Tracker
    {
        private string directory = "";
        private FileTree onTracked;
        public Tracker(FileTree fileTree)
        {
            directory = fileTree.root.GetData().path;
            onTracked = fileTree;
        }
        public void TrackDirectory()
        {
            FileSystemWatcher watcher = new();
            watcher.Path = directory;
            watcher.Filter = "*.*";
            watcher.NotifyFilter = NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.DirectoryName;

            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnRenamed;

            watcher.EnableRaisingEvents = true;

            Console.ReadKey();
        }
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"文件创建：{e.FullPath}");
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"文件删除：{e.FullPath}");
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"文件更改：{e.FullPath}");
        }
        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine($"文件重命名：从 {e.OldFullPath} 到 {e.FullPath}");
        }
    }

    class Finder
    {
        private FileData nullFileData = new() { Kind = FileData.KIND.NULL };
        public FileData FindNode(FileTree fileTree, string name)
        {
            if (fileTree.fileMap.nameDataPairs.TryGetValue(name, out FileData? value))
            {
                return value;
            }
            else
            {
                return nullFileData;
            }
                
        }
    }
}
