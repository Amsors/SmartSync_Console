using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
#pragma warning disable IDE0079
#pragma warning disable CA2254

namespace SmartSync_Console
{
    [Serializable]
    class FileTree
    {
        private FileData root = new();
        public FileData Root
        {
            get { return root; }
            set { root = value; }
        }
        public readonly string rootDirectory = "";

        private FileMap _fileMap = new();
        //[JsonInclude]
        //public FileMap GeneralFileMap => _fileMap;
        public FileMap GeneralFileMap
        {
            get { return _fileMap; }
            set { _fileMap = value; }
        }

        private static readonly ILogger<FileTree> _logger;
        [JsonIgnore]
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
            FileOperator.RecursiveInitialize(this, name, root, noFather);
            root = GeneralFileMap.NameDataPairs[name];
        }
        [JsonConstructor]
        public FileTree() { }
    }

    [method: JsonConstructor]
    class FileData() : IComparable<FileData>
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
        private FileMap _subFileMap = new();
        //[JsonInclude]
        //public FileMap SubFileMap => _subFileMap;
        public FileMap SubFileMap
        {
            get { return _subFileMap; }
            set { _subFileMap = value; }
        }
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
            get{ return _kind; }
            set{ _kind = value; }
        }
        private static readonly ILogger<FileData> _logger;
        [JsonIgnore]
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

    [method: JsonConstructor]
    class FileMap()
    {
        private SortedList<string, FileData> _nameDataPairs = [];
        //public SortedList<string, FileData> NameDataPairs => _nameDataPairs;
        public SortedList<string, FileData> NameDataPairs
        {
            get { return _nameDataPairs; }
            set { _nameDataPairs = value; }
        }
        private static readonly ILogger<FileMap> _logger;
        [JsonIgnore]
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
    }
}
