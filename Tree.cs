#pragma warning disable CA2254

namespace SmartSync_Console
{
    class Tree<T> where T: new()
    {
        internal T root;
        public Tree()
        {
            root = new();
        }
    }
    class TreeNode<T> : IComparable<TreeNode<T>> where T: IComparable<T>, new()
    {
        private T _data;
        private readonly SortedSet<TreeNode<T>> treeNodes;
        public TreeNode()
        {
            _data = new();
            treeNodes = [];
        }
        public T Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }
        internal void SetData(T data)
        {
            this.Data = data;
        }
        public T GetData()
        {
            return Data;
        }
        public SortedSet<TreeNode<T>> GetList()
        {
            return treeNodes;
        }
        public int CompareTo(TreeNode<T>? obj)
        {
            if (obj == null) return 1;
            return _data.CompareTo(obj._data);
        }
    }
}
