using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeChicken.RedBlack
{
	public class BaseRedBlackTree<Node> : ICollection<Node> where Node : RedBlackNode<Node>
	{
		private Node root;
		private int _version;

		public Node Root {
			get => root;
			private set {
				root = value;
				value?.MakeRoot();
			}
		}

		#region Public Functions
		public int Count { get; private set; }

		public Node LeftMost => Root?.LeftMost;
		public Node RightMost => Root?.RightMost;

		public void InsertAt(Node loc, bool right, Node node) {
			Debug.Assert(node != null);
			Count++;
			_version++;

			//null insert location means start or end
			if (loc == null) {
				loc = Root?.Most(right);
				if (loc == null) {
					Root = node;
					return;
				}
			}

			//ensures insertion location is a leaf, otherwise finds the successor (or predecessor) which is guaranteed to be a leaf
			Node at = loc.Child(right);
			if (at != null) {
				right = !right;
				loc = at.Most(right);
				at = loc.Child(right);
			}

#if DEBUG
			Node prev = right ? loc : loc.Prev;
			Node next = right ? loc.Next : loc;
			OrderConsistencyCheck(prev, node);
			OrderConsistencyCheck(node, next);
#endif

			loc.Assign(right, node);
			node.Red = true;
			node.Parent?.ChildrenChanged();

			FixInsertion(node);
		}

		public Node Find(Func<Node, int> comp) {
			Node node = Closest(comp, out int c);
			return c == 0 ? node : null;
		}

		public Node Closest(Func<Node, int> comp, out int c) {
			if (Root == null) {
				c = 1;
				return null;
			}

			Node node = Root;
			while (true) {
				c = comp(node);
				if (c == 0)
					return node;

				Node next = node.Child(c > 0);
				if (next == null)
					return node;

				node = next;
			}
		}

		public bool Remove(Node node) {
			if (node == null)
				return false;

			Debug.Assert(node.Root == Root);
			Count--;
			_version++;

			Node del = node;
			//if node has two children, identify successor and delete it instead
			if (node.Left != null && node.Right != null)
				del = node.Right.LeftMost;

			Debug.Assert(del.Left == null || del.Right == null); //node has at most one child
			Node child = del.Left ?? del.Right; //node child could be null

			//if deleted node is Root
			if (del.Parent == null) {
				Root = child;
				return true;
			}

			//save location of deleted node for tree fixing, because deleted node may be a successor instead
			bool doubleBlack = del.Black && IsBlack(child);
			Node deletedFrom = del.Parent;
			bool deletedSide = del.Side;
			ReplaceWith(del, child); //replace node with child
			if (child != null)
				child.Black = true;

			if (del != node) { //if deleted node was successor, replace the original node with the deleted successor
				ReplaceWith(node, del);
				del.Red = node.Red;
				del.Left = node.Left;
				del.Right = node.Right;

				if (deletedFrom == node)
					deletedFrom = del;
			}

			deletedFrom?.ChildrenChanged();
			node.Parent?.ChildrenChanged();

			if (doubleBlack)
				FixRemoval(deletedFrom, deletedSide);

			return true;

		}

		public void Replace(Node loc, Node node) {
			Debug.Assert(loc.Root == Root);
#if DEBUG
			OrderConsistencyCheck(loc.Prev, node);
			OrderConsistencyCheck(node, loc.Next);
#endif
			ReplaceWith(loc, node);
			node.Red = loc.Red;
			node.Left = loc.Left;
			node.Right = loc.Right;

			node.ChildrenChanged();
			node.Parent?.ChildrenChanged();
		}

		#endregion
		#region Aliases and Helpers
		public bool IsReadOnly => false;

		public virtual void Add(Node item) => throw new NotImplementedException("Add(Node) called but Node is not comparable. Try ComparableRedBlackTree");

		public void Add(IEnumerable<Node> items) {
			foreach (var node in items)
				Add(node);
		}

		public void Clear() {
			Root = null;
			Count = 0;
			_version++;
		}

		public virtual bool Contains(Node item) => throw new NotImplementedException("Contains(Node) called but Node is not comparable. Try ComparableRedBlackTree");

		public void CopyTo(Node[] array, int arrayIndex) {
			foreach (var n in this)
				array[arrayIndex++] = n;
		}

		public IEnumerator<Node> GetEnumerator() {
			int version = _version;
			Node node = LeftMost;
			while (node != null) {
				if (version != _version)
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				yield return node;
				node = node.Next;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Faster insert for a set of ascending nodes
		/// </summary>
		public void InsertRange(Node loc, IEnumerable<Node> nodes) {
			foreach (var node in nodes) {
				OrderConsistencyCheck(loc, node);
				InsertAt(loc, true, node);
				loc = node;
			}
			OrderConsistencyCheck(loc, loc.Next);
		}

		public void RemoveRange(Node first, Node last) {
			OrderConsistencyCheck(first, last);

			Node node = first;
			while (true) {
				Node next = node.Next;
				Remove(node);
				if (node == last)
					return;

				node = next;
			}
		}

		public void BuildFrom(IReadOnlyList<Node> nodes) {
			if (Root != null)
				throw new InvalidOperationException("BuildFrom called on non-empty tree");

			int bh = 0;
			for (int i = nodes.Count + 1; i > 1; i >>= 1)
				bh++;

			Root = BuildFrom(nodes, 0, nodes.Count, bh);
			Count = nodes.Count;
		}

		private Node BuildFrom(IReadOnlyList<Node> nodes, int a, int b, int bh) {
			if (b == a)
				return null;

			int c = (b + a) / 2;
			Node p = nodes[c];
			p.Black = bh > 0;
			p.Left = BuildFrom(nodes, a, c, bh - 1);
			p.Right = BuildFrom(nodes, c + 1, b, bh - 1);
			OrderConsistencyCheck(p.Left, p);
			OrderConsistencyCheck(p, p.Right);
			p.ChildrenChanged();
			return p;
		}

		/// <summary>
		/// Consistency check to ensure that comparability is maintained.
		/// Throws an exception if left > right
		/// Either left or right could be null, in which case this method should not throw.
		/// Only implemented in comparable subclasses.
		/// May not be called in release builds, so do not rely on it
		/// </summary>
		protected virtual void OrderConsistencyCheck(Node left, Node right) {}

		#endregion
		#region Internal Functions
		private static bool IsBlack(Node node) => node == null || node.Black;

		/// <summary>
		/// Only assigns parent (or root if no parent), does not copy children
		/// </summary>
		private void ReplaceWith(Node node, Node replacement) {
			if (node.Parent == null)
				Root = replacement;
			else
				node.Parent.Assign(node.Side, replacement);
		}

		private void FixInsertion(Node node) {
			Debug.Assert(node.Red);

			Node p = node.Parent;
			if (IsBlack(p)) //adding red nodes to black parents is always fine
				return;

			if (p == Root && p.Red) {
				p.Black = true;
				return;
			}

			Node g = p.Parent;
			Node u = p.Sibling; //uncle
			if (IsBlack(u)) {
				//g has only one red child p
				bool nside = node.Side;
				bool pside = p.Side;
				if (nside != pside) //node is on the inside of p, relative to g
					Rotate(p, !nside); //rotate node up to parent

				//rotate black node g to make p the new black parent and recolor g to red
				Rotate(g, !pside);
				//the node in place of g now has two red children
				return;
			}

			//grandparent has 2 red children
			//swap colors and propogate up
			p.Black = true;
			u.Black = true;
			g.Red = true;
			FixInsertion(g);
		}

		//recolouring and rotations for the case where
		//the node on the shortSide of p is black and is one short of the black height requirements of the tree
		//this imples that the node on the longSide of p has a blackHeight of at least 1
		private void FixRemoval(Node p, bool shortSide) {
			Node s = p.Child(!shortSide);
			Debug.Assert(IsBlack(p.Child(shortSide)));
			Debug.Assert(s != null); //the long side must have nodes

			if (s.Red) {
				//if the sibling is red, it must have two black children (to have a black height > 0)
				//the parent must also be black
				//rotate so the parent becomes a red node on the short side
				Rotate(p, shortSide);
				//reacquire new (black) sibling, previously inner nephew
				s = p.Child(!shortSide);
				Debug.Assert(s.Black);
			}

			//obtain outer nephew
			Node oNeph = s.Child(!shortSide);
			if (IsBlack(oNeph)) {
				Node iNeph = s.Child(shortSide);
				if (IsBlack(iNeph)) {
					//both nephews are black
					//color s red, making the black height equal, but short on both sides
					s.Red = true;
					if (p.Red)
						p.Black = true; //just make p black, restoring desired black height
					else if (p != Root)
						FixRemoval(p.Parent, p.Side); //report p as short to its parent

					return;
				}

				//inner nephew is red, rotate it up to s and change to black
				//change s (now a nephew) to red, all black heights preserved
				Rotate(s, !shortSide);
				iNeph.Black = true;
				oNeph = s; // oNeph.red = true; (overwritten later)

				Debug.Assert(IsBlack(p.Child(!shortSide)));
				Debug.Assert(oNeph == p.Child(!shortSide).Child(!shortSide));
			}

			//have a red outer nephew, rotate p to gain a black (s is black)
			//s takes the color of p, preserving height
			//outer nephew had a black parent s, which is now moved to the short side, oNeph is coloured black to compensate
			//inner nephew is moved to short side (which is no-longer short) and retains height
			Rotate(p, shortSide);
			oNeph.Black = true;
		}

		private void Rotate(Node node, bool right) {
			Node inc = node.Child(!right);
			Debug.Assert(inc != null); //can't rotate a null node into position

			ReplaceWith(node, inc);//move inc to node slot
			node.Assign(!right, inc.Child(right)); //pass orphan across
			inc.Assign(right, node); //set node as child of inc

			//preserve red-black
			if (node.Red != inc.Red) {
				node.Red = !node.Red;
				inc.Red = !inc.Red;
			}

			node.ChildrenChanged();
		}
		#endregion
	}
}