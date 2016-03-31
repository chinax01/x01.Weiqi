using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using x01.Weiqi.Boards;
using x01.Weiqi.Core;

namespace x01.WeiqiTest
{
	[TestClass]
	public class HelperTest
	{
		[TestMethod]
		public void TesRoundPoses()
		{
			var expected = new List<Pos> {
				new Pos(3, 3), new Pos(3, 2), new Pos(2, 3), new Pos(3, 4), new Pos(4, 3),
				new Pos(2, 2), new Pos(4, 4), new Pos(2, 4), new Pos(4, 2) // fail case, new Pos(5,5)
			};
			var actual = Helper.RoundPoses(new Pos(3, 3));
			bool ok = true;
			foreach (var item in expected) {
				if (!actual.Contains(item)) {
					ok = false;
					break;
				}
			}
			Assert.IsTrue(ok);
		}

		[TestMethod]
		public void TestLinkPoses()
		{
			var expected = new List<Pos> {
				new Pos(3, 3), new Pos(3, 2), new Pos(2, 3), new Pos(3, 4), new Pos(4, 3)
				// next line is test number=2
				//, new Pos(3, 1), new Pos(1, 3), new Pos(3, 5), new Pos(5, 3)
			};
			var actual = Helper.LinkPoses(new Pos(3, 3));
			bool ok = true;
			foreach (var item in expected) {
				if (!actual.Contains(item)) {
					ok = false;
					break;
				}
			}
			Assert.IsTrue(ok);
		}
	}
}
