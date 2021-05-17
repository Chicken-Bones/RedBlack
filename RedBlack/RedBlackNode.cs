using System.Collections.Generic;

namespace CodeChicken.RedBlack
{
	public abstract class RedBlackNode<Node> where Node : RedBlackNode<Node>
	{
		private Node left, right;
		public bool Red { get; internal set; }
		public Node Parent { get; private set; }

		public bool Black {
			get => !Red;
			internal set => Red = !value;
		}

		public Node Left {
			get => left;
			internal set {
				left = value;
				if (value != null)
					value.Parent = (Node)this;
			}
		}

		public Node Right {
			get => right;
			internal set {
				right = value;
				if (value != null)
					value.Parent = (Node)this;
			}
		}
		
		internal void MakeRoot() {
			Parent = null;
		}

		public Node Root => Parent?.Root ?? (Node)this;

		public bool Side => Parent.right == this;

		public Node Sibling => Parent?.Child(!Side);

		public Node Child(bool r) => r ? right : left;

		public void Assign(bool r, Node n) {
			if (r) Right = n;
			else Left = n;
		}

		public Node LeftMost => left?.LeftMost ?? (Node)this;
		public Node RightMost => right?.RightMost ?? (Node)this;

		public Node Most(bool r) => r ? RightMost : LeftMost;

		public Node Next => Closest(true);
		public Node Prev => Closest(false);

		public Node Closest(bool r) {
			Node next = Child(r)?.Most(!r);
			if (next != null)
				return next;

			Node cur = (Node)this;
			while (cur.Parent != null && cur.Side == r)
				cur = cur.Parent;

			return cur.Parent;
		}

		public IEnumerable<Node> To(Node last) {
			Node node = (Node)this;
			yield return node;
			while (node != last) {
				node = node.Next;
				yield return node;
			}
		}

		public virtual void ChildrenChanged() {}
	}
}
