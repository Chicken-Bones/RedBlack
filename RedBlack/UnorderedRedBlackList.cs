using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeChicken.RedBlack
{
	/// <summary>
	/// Example composition implementation of IList<T> with Log(n) Insert(index) and Remove(index)
	/// This does not require T to implement IComparable, so items can be inserted at any location as opposed to RedBlackList<T>
	/// As a consequence, IndexOf(T), Contains(T) and Remove(T) are not implmented as they would run in O(n) and should be avoioded
	/// If these are needed just subclass and override, or use LINQ with IEnumerable<T>
	/// </summary>
	/// <typeparam name="T">The element type of this list</typeparam>
	public class UnorderedRedBlackList<T> : BaseRedBlackTree<UnorderedRedBlackList<T>.Node>, IList<T>
	{
		/// <summary>
		/// A copy of RedBlackList.Node without IComparable<T>
		/// </summary>
		public class Node : RedBlackNode<Node>
		{
			public T value;
			public int Count { get; private set; } = 1;

			public Node(T value) {
				this.value = value;
			}

			public override void ChildrenChanged() {
				int c = 1;
				if (Left != null)
					c += Left.Count;
				if (Right != null)
					c += Right.Count;

				if (c != Count) {
					Count = c;
					Parent?.ChildrenChanged();
				}
			}

			public override string ToString() => $"{(Red ? 'R' : 'B')}: {value}";
		}

		// functions copied from RedBlackList
		#region RedBlackList
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
			set => Replace(NodeAt(index), new Node(value));
		}
		

		public void Insert(int index, T value) => Insert(index, new Node(value));
		public void RemoveAt(int index) => Remove(NodeAt(index));
		#endregion

		public virtual int IndexOf(T item) => throw new NotImplementedException("IndexOf(T) would be O(n), consider ComparableRedBlackTree");
		public virtual bool Contains(T value) => throw new NotImplementedException("Contains(T) would be O(n), consider ComparableRedBlackTree");
		public virtual bool Remove(T value) => throw new NotImplementedException("Remove(T) would be O(n), consider ComparableRedBlackTree");

		public void Add(T item) => InsertAt(null, true, new Node(item));
		public void Add(IEnumerable<T> items) {
			foreach (var t in items)
				Add(t);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			foreach (var t in this)
				array[arrayIndex++] = t;
		}

		public new IEnumerator<T> GetEnumerator() => ((ICollection<Node>)this).Select(n => n.value).GetEnumerator();
	}
}
