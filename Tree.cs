using System.IO;

namespace SmartSync_Console
{
    class Tree<T> where T: IComparable<T>, new()
    {
        internal TreeNode<T> root;
        public Tree()
        {
            root = new();
        }
    }
    class TreeNode<T> : IComparable<TreeNode<T>> where T: IComparable<T>, new()
    {
        private T _data;
        private SortedSet<TreeNode<T>> treeNodes;
        public TreeNode()
        {
            _data = new();
            treeNodes = new();
        }
        public T data
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
            this.data = data;
        }
        public T GetData()
        {
            return data;
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
