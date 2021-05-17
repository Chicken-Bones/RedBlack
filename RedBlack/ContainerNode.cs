using System;

namespace CodeChicken.RedBlack
{
	public class ContainerNode<T, Node> : RedBlackNode<Node>, IComparable<Node>
		where T : IComparable<T> where Node : ContainerNode<T, Node>
	{
		public T value;

		public ContainerNode(T value) {
			this.value = value;
		}

		public int CompareTo(Node other) => value.CompareTo(other.value);

		public override string ToString() => $"{(Red ? 'R' : 'B')}: {value}";
	}
}