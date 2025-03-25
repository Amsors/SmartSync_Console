using System.IO;
using Microsoft.Extensions.Logging;
#pragma warning disable CA2254

namespace SmartSync_Console
{
    [Serializable]
    class FileTree : Tree<TreeNode<FileData>>
    {
        [NonSerialized]
        public readonly string rootDirectory = "";
        private readonly FileMap _fileMap = new();
        public FileMap GeneralFileMap => _fileMap; 

        [NonSerialized]
        private static readonly ILogger<FileTree> _logger;
        public static ILogger<FileTree> Logger => _logger;
        static FileTree()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<FileTree>();
        }
        public FileTree(string name, string repoDirectory)
        {
            this.rootDirectory = repoDirectory;
            FileData noFather = new() { Kind = FileData.KIND.NOFATHER };
            this.root.Data.Path = name;
            FileOperator.RecursiveInitialize(this, name, root, noFather);
        }
    }

    class FileData : IComparable<FileData>
    {
        //private string _name = "";
        //public string Name => _name;
        private long _fileSize;
        public long FileSize{
            get{ return _fileSize; }
            set{ _fileSize = value; }
        }
        private DateTime _lastWriteTime;
        public DateTime LastWriteTime
        {
            get { return _lastWriteTime; }
            set { _lastWriteTime = value; }
        }
        private DateTime _lastAccessTime;
        public DateTime LastAccessTime
        {
            get { return _lastAccessTime; }
            set { _lastAccessTime = value; }
        }
        private FileAttributes _fileAttributes;
        public FileAttributes FileAttributes
        {
            get { return _fileAttributes; }
            set { _fileAttributes = value; }
        }
        private string _path="";
        public string Path{
            get { return _path; }
            set { _path = value; }
        }
        private bool _hashed = false;
        public bool Hashed{
            get { return _hashed; }
            set { _hashed = value; }
        }
        private string _hashValue = "";
        public string HashValue
        {
            get { return _hashValue; }
            set { _hashValue = value; }
        }
        private readonly FileMap _subFileMap = new();
        public FileMap SubFileMap => _subFileMap;
        public enum KIND:int
        {
            NA=0,
            FILE=1,
            DIRECTORY=2,
            NULL=-1,
            NOFATHER=-2
        }
        private KIND _kind=KIND.NA;
        public KIND Kind
        {
            get
            {
                return _kind;
            }
            set
            {
                _kind = value;
            }
        }
        [NonSerialized]
        private static readonly ILogger<FileData> _logger;
        public static ILogger<FileData> Logger => _logger;
        static FileData()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<FileData>();
        }
        public FileData() { }
        //public FileData(string absolutePath)
        //{
        //    if (File.Exists(absolutePath))
        //    {
        //        //string relevantPath=FileOperator.GetRelevantPath()
        //        Logger.LogTrace($"Initializing {absolutePath}");
        //        _kind = KIND.FILE;
        //        this._path = absolutePath;

        //        FileInfo fileInfo = new(absolutePath);
        //        _fileSize = fileInfo.Length;
        //        _lastWriteTime = fileInfo.LastWriteTime;
        //        _lastAccessTime = fileInfo.LastAccessTime;

        //        _fileAttributes = File.GetAttributes(absolutePath);
        //    }
        //    if (Directory.Exists(absolutePath))
        //    {
        //        Logger.LogTrace($"Initializing {absolutePath}");
        //        _kind = KIND.DIRECTORY;
        //        this._path = absolutePath;

        //        DirectoryInfo directoryInfo = new(absolutePath);
        //        _lastWriteTime = directoryInfo.LastWriteTime;
        //        _lastAccessTime = directoryInfo.LastAccessTime;
        //    }
        //}
        public int CompareTo(FileData? fileData)
        {
            if (fileData == null) return 1;
            return Path.CompareTo(fileData.Path);
        }
        public void AddSubFile(string path, FileData fileData)
        {
            if (_subFileMap.NameDataPairs.ContainsKey(path))
            {
                Logger.LogWarning($"File {path} already exist, unable to add");
                return;
            }
            _subFileMap.NameDataPairs.Add(path, fileData);
        }
        public void DeleteSubFile(string path)
        {
            if (_subFileMap.NameDataPairs.ContainsKey(path)==false)
            {
                Logger.LogWarning($"File {path} does not exist, unable to delete");
                return;
            }
            _subFileMap.NameDataPairs.Remove(path);
        }
    }

    class FileMap
    {
        private readonly SortedList<string, FileData> _nameDataPairs;
        public SortedList<string, FileData> NameDataPairs => _nameDataPairs;
        private static readonly ILogger<FileMap> _logger;
        public static ILogger<FileMap> Logger => _logger;
        static FileMap()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<FileMap>();
        }
        public void AddPair(string key, FileData value)
        {
            if (_nameDataPairs.ContainsKey(key))
            {
                Logger.LogWarning($"File {key} already exist when trying to add it to FileMap");
                return;
            }
            _nameDataPairs.Add(key, value);
        }
        public void DeletePair(string key)
        {
            if (_nameDataPairs.ContainsKey(key)==false)
            {
                Logger.LogWarning($"File {key} does not exist when trying to delete it in FileMap");
                return;
            }
            _nameDataPairs.Remove(key);
        }
        public FileMap()
        {
            _nameDataPairs = [];
        }
    }
}
