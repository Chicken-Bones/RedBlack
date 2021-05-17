using System;
using System.Collections.Generic;

namespace CodeChicken.RedBlack
{
	public class ComparableRedBlackTree<Node> : BaseRedBlackTree<Node> where Node : RedBlackNode<Node>, IComparable<Node>
	{
		public bool Insert(Node node) {
			if (Root == null) {
				InsertAt(null, false, node);
				return true;
			}

			Node loc = Closest(node, out int c);
			if (c == 0)
				return false;

			InsertAt(loc, c > 0, node);
			return true;
		}

		public override void Add(Node item) => Insert(item);
		
		public override bool Contains(Node item) => item != null && Find(item.CompareTo) != null;

		public Node Closest(Node node, out int c) => Closest(node.CompareTo, out c);

		protected override void OrderConsistencyCheck(Node left, Node right) {
			if (left != null && right != null && left.CompareTo(right) >= 0)
				throw new InvalidOperationException($"Comparison contract violated by supplied arguments {left} < {right}");
		}
		
		/// <summary>
		/// Faster insert for a set of ascending nodes
		/// </summary>
		/// <param name="nodes"></param>
		public void InsertRange(IEnumerable<Node> nodes) {
			Node loc = null;
			foreach (var node in nodes) {
				if (loc == null) { //find insertion location
					loc = Closest(node, out int c);
					if (c < 0)
						loc = loc?.Prev;
				}

				OrderConsistencyCheck(loc, node);
				InsertAt(loc, true, node);
				loc = node;
			}
			OrderConsistencyCheck(loc, loc.Next);
		}
	}
}