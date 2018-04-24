using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using RedBlack;

namespace RedBlackTests
{
	[TestClass]
	public class TestRedBlackTree
	{
		public static void Verify<Node>(BaseRedBlackTree<Node> tree) where Node : RedBlackNode<Node> {
			if (tree.Root != null)
				Assert.IsTrue(tree.Root.Parent == null, "Root node has non-null parent");

			Verify(tree.Root);
		}

		public static void Verify<T>(RedBlackList<T> tree) where T : IComparable<T> {
			Verify((BaseRedBlackTree<RedBlackList<T>.Node>)tree);
			Assert.IsTrue(VerifyCounts(tree.Root) == tree.Count);
		}

		private static int VerifyCounts<T>(RedBlackList<T>.Node tree) where T : IComparable<T> {
			if (tree == null)
				return 0;

			Assert.IsTrue(tree.Count == VerifyCounts(tree.Left) + VerifyCounts(tree.Right) + 1, "Count Violation");
			return tree.Count;
		}

		private static bool IsBlack<T>(RedBlackNode<T> node) where T : RedBlackNode<T> => node == null || node.Black;

		private static int Verify<T>(RedBlackNode<T> tree) where T : RedBlackNode<T> {
			if (tree == null)
				return 0;

			if (tree is IComparable<T> comp) {
				if (tree.Left != null)
					Assert.IsTrue(comp.CompareTo(tree.Left) > 0, "Order Violation");
				if (tree.Right != null)
					Assert.IsTrue(comp.CompareTo(tree.Right) < 0, "Order Violation");
			}

			if (tree.Left != null)
				Assert.IsTrue(tree.Left.Parent == tree, "Parent Child ref Mismatch");
			if (tree.Right != null)
				Assert.IsTrue(tree.Right.Parent == tree, "Parent Child ref Mismatch");

			if (tree.Red)
				Assert.IsTrue(IsBlack(tree.Left) && IsBlack(tree.Right), "Red Violation");

			int height;
			Assert.AreEqual(height = Verify(tree.Left), Verify(tree.Right), "Black Violation");

			if (tree.Black)
				height++;

			return height;
		}

		private static T[] Shuffle<T>(T[] array, Random rand) {
			for (int i = 0; i < array.Length; i++) {
				int r = i + rand.Next(array.Length - i);
				T t = array[r];
				array[r] = array[i];
				array[i] = t;
			}
			return array;
		}

		[TestMethod]
		public void TestConstructSeq() {
			var tree = new RedBlackTree<int>();
			Verify(tree);
			for (int i = 0; i < 1000; i++) {
				tree.Add(i);
				Verify(tree);
			}
		}

		[TestMethod]
		public void TestConstructShuffled() {
			var rand = new Random(0);
			for (int i = 0; i < 1000; i++) {
				var arr = Enumerable.Range(0, i).ToArray();
				for (int j = 0; j < 3; j++) {
					Shuffle(arr, rand);
					Verify(new RedBlackTree<int> { arr });
				}
			}
		}


		[TestMethod]
		public void TestBuildFrom() {
			for (int i = 0; i < 1000; i++) {
				var tree = new RedBlackTree<int>();
				tree.BuildFrom(Enumerable.Range(0, i));
				Verify(tree);
			}
		}

		[TestMethod]
		public void TestInsert() {
			for (int i = 0; i < 1000; i++) {
				var tree = new RedBlackTree<float> { Enumerable.Range(0, 1000).Select(e => (float)e) };
				tree.InsertAt(tree.Find(i), true, new RedBlackTree<float>.Node(i + 0.5f));
				Verify(tree);
			}
		}

		[TestMethod]
		public void TestDelete() {
			for (int i = 0; i < 1000; i++) {
				var tree = new RedBlackTree<int> { Enumerable.Range(0, 1000) };
				tree.Remove(i);
				Verify(tree);
			}
		}

		[TestMethod]
		public void TestModifications() {
			var rand = new Random(0);
			var tree = new RedBlackTree<int> { Enumerable.Range(0, 1000).Select(i => i * 100) };
			var list = new List<int>();
			list.AddRange(tree);

			for (int i = 0; i < 100; i++) {
				bool insert = tree.Count < 100 || tree.Count < 2000 && rand.Next(2) == 0;
				int n = rand.Next(1, insert ? tree.Count : tree.Count / 2);
				for (int j = 0; j < n; j++) {
					if (insert) {
						int r = rand.Next(tree.Count + 1);
						int a = r == 0 ? int.MinValue : list[r - 1] + 1;
						int b = r == list.Count ? int.MaxValue : list[r];
						if (a == b)
							continue;
						int v = rand.Next(a, b);
						list.Insert(r, v);
						tree.Insert(v);
					}
					else {
						int r = rand.Next(tree.Count);
						int v = list[r];
						list.RemoveAt(r);
						tree.Remove(v);
					}

					Verify(tree);
					Assert.IsTrue(Enumerable.SequenceEqual(list, tree), "list and tree not equal");
				}
				Logger.LogMessage("Size {0}", tree.Count);
			}
		}

		[TestMethod]
		public void TestModificationsList() {
			var rand = new Random(0);
			var list = new RedBlackList<int> { Enumerable.Range(0, 1000).Select(i => i * 100) };

			for (int i = 0; i < 100; i++) {
				bool insert = list.Count < 100 || list.Count < 2000 && rand.Next(2) == 0;
				int n = rand.Next(1, insert ? list.Count : list.Count / 2);
				for (int j = 0; j < n; j++) {
					if (insert) {
						int r = rand.Next(list.Count + 1);
						int a = r == 0 ? int.MinValue : list[r - 1] + 1;
						int b = r == list.Count ? int.MaxValue : list[r];
						if (a == b)
							continue;

						list.Insert(rand.Next(a, b));
					} else {
						list.RemoveAt(rand.Next(list.Count));
					}

					Verify(list);
				}
				Logger.LogMessage("Size {0}", list.Count);
			}
		}

		[TestMethod]
		public void TestListIndex() {
			var rbList = new RedBlackList<int> { Enumerable.Range(0, 1000) };
			var list = new List<int>();
			list.AddRange(rbList);

			for (int i = 0; i < list.Count; i++)
				Assert.IsTrue(list[i] == rbList[i]);
		}

		[TestMethod]
		public void TestList() {
			var rand = new Random(0);
			var list = new UnorderedRedBlackList<int> { Enumerable.Repeat(0, 1000) };

			for (int i = 0; i < 50; i++) {
				bool insert = list.Count < 100 || list.Count < 2000 && rand.Next(2) == 0;
				int n = rand.Next(1, insert ? list.Count : list.Count / 2);
				for (int j = 0; j < n; j++) {
					if (insert) {
						list.Insert(rand.Next(list.Count + 1), 0);
					} else {
						list.RemoveAt(rand.Next(list.Count));
					}

					Verify(list);
				}
				Logger.LogMessage("Size {0}", list.Count);
			}
		}

		[TestMethod]
		public void TestPerformance() {
			var rand = new Random(0);
			IList<object> list = new UnorderedRedBlackList<object> { Enumerable.Repeat<object>(null, 10000) };
			var alist = new List<object>();
			alist.AddRange(list);

			int ops = 0;
			var sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < 100; i++) {
				bool insert = list.Count < 5000 || list.Count < 20000 && rand.Next(2) == 0;
				int n = rand.Next(1, insert ? list.Count : list.Count / 2);
				for (int j = 0; j < n; j++) {
					if (insert) {
						list.Insert(rand.Next(list.Count + 1), null);
					} else {
						list.RemoveAt(rand.Next(list.Count));
					}
				}
				ops += n;
			}
			long t1 = sw.ElapsedMilliseconds;

			list = alist;
			sw.Restart();
			for (int i = 0; i < 100; i++) {
				bool insert = list.Count < 5000 || list.Count < 20000 && rand.Next(2) == 0;
				int n = rand.Next(1, insert ? list.Count : list.Count / 2);
				for (int j = 0; j < n; j++) {
					if (insert) {
						list.Insert(rand.Next(list.Count + 1), null);
					} else {
						list.RemoveAt(rand.Next(list.Count));
					}
				}
			}
			long t2 = sw.ElapsedMilliseconds;

			Logger.LogMessage("Op Count : {0}", ops);
			Logger.LogMessage("Red-Black: {0}ms", t1);
			Logger.LogMessage("List     : {0}ms", t2);
			Assert.IsTrue(t1*2 < t2, "Not fast enough. {0}ms vs {1}ms", t1, t2);

		}
	}
}
