using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeChicken.RedBlack
{
	/// <summary>
	/// Base type for RedBlackTrees with nodes that contain a single value
	/// The Node type parameter is still specifiable to allow extension
	/// </summary>
	public abstract class SimpleRedBlackTree<T, Node> : ComparableRedBlackTree<Node>, ICollection<T>
		where T : IComparable<T> where Node : ContainerNode<T, Node>
	{
		protected abstract Node NewNode(T value);
		public Node Find(T value) => Find(n => value.CompareTo(n.value));

		public void Insert(T value) => Insert(NewNode(value));
		public bool Contains(T value) => Find(value) != null;
		public bool Remove(T value) => Remove(Find(value));

		public void Add(T item) => Insert(item);
		public void Add(IEnumerable<T> items) {
			foreach (var t in items)
				Add(t);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			foreach (var t in this)
				array[arrayIndex++] = t;
		}

		public new IEnumerator<T> GetEnumerator() => ((ICollection<Node>) this).Select(n => n.value).GetEnumerator();
		public void BuildFrom(IEnumerable<T> list) => BuildFrom(list.Select(NewNode).ToList());
	}
}
