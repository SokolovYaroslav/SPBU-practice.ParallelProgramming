using System;

namespace BinaryTree
{
    public class Node<TK, TV> : IComparable where TK : IComparable
    {
        public readonly TK Key;
        public readonly TV Value;
        
        public Node(TK key, TV value) {
            Key = key;
            Value = value;
        }

        // Specific field for concurrent tree
        public Tuple<Node<TK, TV>, Node<TK, TV>, bool> ChildrenM = 
            new Tuple<Node<TK, TV>, Node<TK, TV>, bool>(null, null, false);

        public int CompareTo(object obj) {
            if (obj == null) 
                throw new ArgumentException("Node is null");

            var otherTemperature = obj as Node<TK, TV>;
            if (otherTemperature != null) 
                return Key.CompareTo(otherTemperature.Key);
            
            throw new ArgumentException("Object is not a Node");
        }
    }
}