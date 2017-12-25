using System;

namespace BinaryTree
{
    public interface ITree<TK, TV> where TK : IComparable
    {
        void Insert(TK key, TV value);
        TV Search(TK key);
        void Delete(TK key);
    }
}