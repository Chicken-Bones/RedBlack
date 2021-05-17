using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeChicken.RedBlack
{
	/// <summary>
	/// Helper methods for implementing list-like variants of RedBlackTree
	/// The node -> count field is supplied as a function to allow for node implementations with multiple indexers
	/// Nodes can have a count of any value
	/// </summary>
	public static class RedBlackCountAccesor<Node> where Node : RedBlackNode<Node>
	{
		public delegate int GetCount(Node node);

		public static Node GetByIndex(BaseRedBlackTree<Node> tree, int index, GetCount getCount) {
			if (index < 0 || index >= tree.Count)
				throw new ArgumentException($"Index out of range {index} [0-{tree.Count})");

			Node node = tree.Root;
			while (true) {
				if (node.Left != null && index < getCount(node.Left)) {
					node = node.Left;
					continue;
				}

				if (node.Right == null)
					return node;

				index -= getCount(node) - getCount(node.Right);
				if (index < 0)
					return node;

				node = node.Right;
			}
		}

		public static int IndexOf(BaseRedBlackTree<Node> tree, Node node, GetCount getCount) {
			if (node == null)
				return -1;

			Debug.Assert(node.Root == node);
			int index = node.Left != null ? getCount(node.Left) : 0;

			while (node.Parent != null) {
				if (node.Side) //if we're on the right side of a parent, add all the left sum
					index += getCount(node.Parent) - getCount(node);

				node = node.Parent;
			}
			return index;
		}
	}

	/// <summary>
	/// Extension of SimpleRedBlackTree where nodes are augmented with indexing capability
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class RedBlackList<T> : SimpleRedBlackTree<T, RedBlackList<T>.Node>, IList<T>
		where T : IComparable<T>
	{
		public class Node : ContainerNode<T, Node>
		{
			public int Count { get; private set; } = 1;

			public Node(T value) : base(value) { }

			public override void ChildrenChanged() {
				int _Count = 1;
				if (Left != null)
					_Count += Left.Count;
				if (Right != null)
					_Count += Right.Count;

				if (_Count != Count) {
					Count = _Count;
					Parent?.ChildrenChanged();
				}
			}
		}

		protected override Node NewNode(T value) => new Node(value);
		
		public Node NodeAt(int index) => RedBlackCountAccesor<Node>.GetByIndex(this, index, n => n.Count);
		public int IndexOf(Node node) => RedBlackCountAccesor<Node>.IndexOf(this, node, n => n.Count);

		public void Insert(int index, Node node) {
			if (index == Count)
				InsertAt(null, true, node);
			else
				InsertAt(NodeAt(index), false, node);
		}

		public T this[int index] {
			get => NodeAt(index).value;
			set => Replace(NodeAt(index), NewNode(value));
		}

		public int IndexOf(T item) => IndexOf(Find(item));
		public void Insert(int index, T value) => Insert(index, NewNode(value));
		public void RemoveAt(int index) => Remove(NodeAt(index));
	}
}
