using System;

namespace CodeChicken.RedBlack
{
	/// <summary>
	/// Concrete implementation of SimpleRedBlackTree. Just specifies the type of ContainerNode
	/// </summary>
	public class RedBlackTree<T> : SimpleRedBlackTree<T, RedBlackTree<T>.Node> where T : IComparable<T>
	{
		public class Node : ContainerNode<T, Node>
		{
			public Node(T value) : base(value) {}
		}

		protected override Node NewNode(T value) => new Node(value);
	}
}
