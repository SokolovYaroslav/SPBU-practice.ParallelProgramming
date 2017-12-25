using System;
using static System.Threading.Interlocked;

namespace BinaryTree
{
    public class BinarySearchTreeConcurrent<TK, TV> : ITree<TK, TV> where TK : IComparable
    {
        private Node<TK, TV> _root;

        public BinarySearchTreeConcurrent() {
            _root = null;
        }

        public TV Search(TK key) {
            var node = SearchNodes(key).Item2;
            return node == null || node.ChildrenM.Item3 ? default(TV) : node.Value;
        }

        public void Insert(TK key, TV value) {
            while (-1 == _insert(key, value)) { }
        }

        public void Delete(TK key) {
            while (-1 == _delete(key)) { }
        }

        private int _insert(TK key, TV value) {
            var newNode = new Node<TK, TV>(key, value);
            var nodes = SearchNodes(key);
            var previousNode = nodes.Item1;
            var currentNode = nodes.Item2;
            Tuple<Node<TK, TV>, Node<TK, TV>, bool> newChildren;

            if (currentNode != null) {
                if (!currentNode.ChildrenM.Item3) {
                    return 0;
                }
                newNode.ChildrenM = currentNode.ChildrenM;
            }
            if (previousNode == null) {
                if (null != CompareExchange(ref _root, newNode, null)) {
                    return -1;
                }
                return 0;
            }

            var oldChildren = previousNode.ChildrenM;
            switch (newNode.CompareTo(previousNode)) {
                case 1:
                    newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                        (oldChildren.Item1, newNode, oldChildren.Item3);
                    if (!ReferenceEquals(oldChildren, 
                        CompareExchange(ref previousNode.ChildrenM, newChildren, oldChildren))) {
                        return -1;
                    }
                    break;
                case -1:
                    newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                        (newNode, oldChildren.Item2, oldChildren.Item3);
                    if (!ReferenceEquals(oldChildren, 
                        CompareExchange(ref previousNode.ChildrenM, newChildren, oldChildren))) {
                        return -1;
                    }
                    break;
                default:
                    throw new ArgumentException("Unexpected return in Node's CompareTo");
            }
            return 0;
        }

        private int _delete(TK key) {
            var nodes = SearchNodes(key);
            var delParent = nodes.Item1;
            var delNode = nodes.Item2;
            if (delNode == null) {
                return 0;
            }

//            if (delParent == null) {
//                return -1;
//            }

            Tuple<Node<TK, TV>, Node<TK, TV>, bool> newChildren;
            Tuple<Node<TK, TV>, Node<TK, TV>, bool> oldChildren;
            
            if (delNode.ChildrenM.Item1 == null && delNode.ChildrenM.Item2 == null) {
                if (delNode == _root) _root = null;
                else {
                    switch (delNode.CompareTo(delParent)) {
                        case -1:
                            oldChildren = delParent.ChildrenM;
                            newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                                (null, oldChildren.Item2, oldChildren.Item3);
                            if (!ReferenceEquals(oldChildren, 
                                CompareExchange(ref delParent.ChildrenM, newChildren, oldChildren))) {
                                return -1;
                            }
                            break;
                        case 1:
                            oldChildren = delParent.ChildrenM;
                            newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                                (delParent.ChildrenM.Item1, null, oldChildren.Item3);
                            if (!ReferenceEquals(oldChildren, 
                                CompareExchange(ref delParent.ChildrenM, newChildren, oldChildren))) {
                                return -1;
                            }
                            break;
                        default:
                            throw new ArgumentException("Unexpected return in Node's CompareTo");
                    }
                }
            }
            else if (delNode.ChildrenM.Item1 == null) {
                if (delNode == _root) {
                    _root = delNode.ChildrenM.Item2;
                }
                else {
                    switch (delNode.CompareTo(delParent)) {
                        case -1:
                            oldChildren = delParent.ChildrenM;
                            newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                                (delNode.ChildrenM.Item2, delParent.ChildrenM.Item2, oldChildren.Item3);
                            if (!ReferenceEquals(oldChildren, 
                                CompareExchange(ref delParent.ChildrenM, newChildren, oldChildren))) {
                                return -1;
                            }
                            break;
                        case 1:
                            oldChildren = delParent.ChildrenM;
                            newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                                (delParent.ChildrenM.Item1, delNode.ChildrenM.Item2, oldChildren.Item3);
                            if (!ReferenceEquals(oldChildren, 
                                CompareExchange(ref delParent.ChildrenM, newChildren, oldChildren))) {
                                return -1;
                            }
                            break;
                        default:
                            throw new ArgumentException("Unexpected return in Node's CompareTo");
                    }
                }
            }
            else if (delNode.ChildrenM.Item2 == null) {
                if (delNode == _root) {
                    _root = delNode.ChildrenM.Item1;
                }
                else {
                    switch (delNode.CompareTo(delParent)) {
                        case -1:
                            oldChildren = delParent.ChildrenM;
                            newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                                (delNode.ChildrenM.Item1, delParent.ChildrenM.Item2, oldChildren.Item3);
                            if (!ReferenceEquals(oldChildren, 
                                CompareExchange(ref delParent.ChildrenM, newChildren, oldChildren))) {
                                return -1;
                            }
                            break;
                        case 1:
                            oldChildren = delParent.ChildrenM;
                            newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                                (delParent.ChildrenM.Item1, delNode.ChildrenM.Item1, oldChildren.Item3);
                            if (!ReferenceEquals(oldChildren, 
                                CompareExchange(ref delParent.ChildrenM, newChildren, oldChildren))) {
                                return -1;
                            }
                            break;
                        default:
                            throw new ArgumentException("Unexpected return in Node's CompareTo");   
                    }
                }
            }
            else if (delNode.ChildrenM.Item1 != null && delNode.ChildrenM.Item2 != null) {
                oldChildren = delNode.ChildrenM;
                newChildren = new Tuple<Node<TK, TV>, Node<TK, TV>, bool>
                    (delNode.ChildrenM.Item1, delNode.ChildrenM.Item2, true);
                if (!ReferenceEquals(oldChildren, 
                    CompareExchange(ref delNode.ChildrenM, newChildren, oldChildren))) {
                    return -1;
                }
            }
            return 0;
        }

        private Tuple<Node<TK, TV>, Node<TK, TV>> SearchNodes(TK key) {
            Node<TK, TV> previousNode = null; 
            var currentNode = _root;

            while (currentNode != null) {
                switch (key.CompareTo(currentNode.Key)) {
                    case 0:
                        return new Tuple<Node<TK, TV>, Node<TK, TV>>(previousNode, currentNode);
                    case 1:
                        previousNode = currentNode;
                        currentNode = currentNode.ChildrenM.Item2;
                        break;
                    case -1:
                        previousNode = currentNode;
                        currentNode = currentNode.ChildrenM.Item1;
                        break;
                    default:
                        throw new ArgumentException("Unexpected return in Node's CompareTo");
                }
            }
            return new Tuple<Node<TK, TV>, Node<TK, TV>>(previousNode, null);
        }
        
//        private static Node<TK, TV> MinNode(Node<TK, TV> subRoot) {
//            if (subRoot == null) {
//                return null;
//            }
//            var previousNode = subRoot;
//            var currentNode = subRoot;
//        
//            while (currentNode != null) {
//                previousNode = currentNode;
//                currentNode = currentNode.ChildrenM.Item1;
//            }
//
//            return previousNode;
//        }
    }
}