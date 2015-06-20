/**
 * Board.Pattern.cs (c) 2015 by x01
 * --------------------------------
 * 1.虽说记住定式，棋弱三分。但无论如何，每局棋都是从定式开始的。
 * 2.由于定式手数较多，Shape 中的方式将不再适用，故决定采用对比法，
 *   即先将定式的步数记录，然后对比之。
 * 3.定式的记录，采用四角八变换的办法，先记左上角，上方挂的变化，再
 *   变换之。
 * 4.定式太多，且在演进中，故只能择其要记录，且尚须不断修改增补。
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace x01.Weiqi.Boards
{
	/// <summary>
	/// Description of Board.Pattern.
	/// </summary>
	public partial class Board
	{
		Pos P_GetPos()
		{
			// 全局配合仍然缺乏！
			return GetPos_FromPatterns(Patterns);
		}

		List<List<Pos>> m_Patterns = null;
		List<List<Pos>> Patterns
		{
			get
			{
				if (m_Patterns != null) return m_Patterns;
				m_Patterns = new List<List<Pos>>();

				var patterns = new List<List<Pos>>();

				patterns.Add(P_LuStar_UpFlyOne);
				patterns.Add(P_LuStar_UpFlyOne_2);
				patterns.Add(P_LuStar_UpFlyOne_3);
				patterns.Add(P_LuStar_UpFlyOne_4);
				patterns.Add(P_LuStar_UpFlyOne_5);
				patterns.Add(P_LuStar_UpFlyOne_6);
				patterns.Add(P_LuStar_UpThree3);
				patterns.Add(P_LuStar_UpJumpOne);
				patterns.Add(P_LuStar_UpClip1);
				patterns.Add(P_LuStar_UpClip1L);
				patterns.Add(P_LuStar_UpClip2);
				patterns.Add(P_LuStar_UpClip2L);
				patterns.Add(P_LuStar_UpClip3);
				patterns.Add(P_LuStar_UpClip3_2);
				patterns.Add(P_LuStar_UpClip3_3);
				patterns.Add(P_LuStar_UpClip3_4);
				patterns.Add(P_LuStar_UpClip3L);

				patterns.Add(P_LuEye_UpJumpOne);
				patterns.Add(P_LuEye_UpJumpOne_2);
				patterns.Add(P_LuEye_UpJumpOne_3);
				patterns.Add(P_LuEye_UpClip1L);
				patterns.Add(P_LuEye_UpClip1L_2);
				patterns.Add(P_LuEye_UpClip2);
				patterns.Add(P_LuEye_UpClip2_2);

				patterns.Add(P_LuEyeH_UpJumpOne);
				patterns.Add(P_LuEyeH_UpJumpOne_2);

				patterns.Add(P_Lu33_Cusp);
				patterns.Add(P_Lu33_Cusp_2);
				patterns.Add(P_Lu33_FlyOne);
				patterns.Add(P_Lu33_FlyTwo);
				patterns.Add(P_Lu33_JumpTwo);

				patterns.Add(P_Lu42_FlyOne);
				patterns.Add(P_Lu42_FlyOne_2);

				m_Patterns = m_Patterns.Union(LeftUpPatterns(patterns))
					.Union(RightUpPatterns(patterns))
					.Union(LeftDownPatterns(patterns))
					.Union(RightDownPatterns(patterns)).ToList();

				var old = m_IsChangeStoneColor;
				m_IsChangeStoneColor = false;
				patterns.Clear();
				patterns.Add(P_LuStar_UpFlyOne_B13);
				patterns.Add(P_LuStar_UpFlyOne_B25);
				patterns.Add(P_LuStar_UpFlyOne_B35);
				patterns.Add(P_LuStar_UpFlyOne_B46);
				patterns.Add(P_LuStar_UpFlyOne_B92);
				patterns.Add(P_LuStar_UpFlyOne_B52);
				patterns.Add(P_LuStar_UpThree3_B23);

				patterns.Add(P_LuEye_UpJumpOne_B26);
				patterns.Add(P_LuEye_UpJumpOne_B11);
				patterns.Add(P_LuEye_Clip2_B15);

				m_Patterns = m_Patterns.Union(LeftUpPatterns(patterns))
					.Union(RightUpPatterns(patterns))
					.Union(LeftDownPatterns(patterns))
					.Union(RightDownPatterns(patterns)).ToList();

				m_IsChangeStoneColor = old;

				return m_Patterns;
			}
		}

		#region 星定式

		//    0 0 +
		//    0 +   +
		//    0 +
		//  0 +
		//  0 +   
		//    +
		//
		List<Pos> P_LuStar_UpThree3	// 点三三
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(4, 2));
				temp.Add(new Pos(4, 1));
				temp.Add(new Pos(5, 2));
				temp.Add(new Pos(5, 1));
				temp.Add(new Pos(6, 2));
				temp.Add(new Pos(1, 3));
				temp.Add(new Pos(1, 4));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(2, 5));
				return temp;
			}
		}
		//   +
		//   + 0 0           0
		//     +   0
		//
		//     +
		List<Pos> P_LuStar_UpJumpOne	// 一间高挂
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(3, 5));    // 第二步：一间高挂
				temp.Add(new Pos(5, 3));    // 第三步：一间守
				temp.Add(new Pos(2, 3));    // 第四步：托靠
				temp.Add(new Pos(2, 2));    // 第五步：三三守
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(2, 9));
				return temp;
			}
		}
		//     0
		//   +      0       0
		//     +
		//
		//   +
		List<Pos> P_LuStar_UpFlyOne
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(5, 2));    // 第三步：小飞守
				temp.Add(new Pos(1, 3));    // 第四步：小飞进角
				temp.Add(new Pos(2, 2));    // 第五步：三三守
				temp.Add(new Pos(2, 8));    // 第六步：拆二
				return temp;
			}
		}
		//     0
		//   +      0       0
		//     +
		//
		//     +
		//
		List<Pos> P_LuStar_UpFlyOne_2
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(5, 3));    // 第三步：一间跳
				temp.Add(new Pos(1, 3));    // 第四步：小飞进角
				temp.Add(new Pos(2, 2));    // 第五步：三三守
				temp.Add(new Pos(2, 8));    // 第六步：拆二
				return temp;
			}
		}
		//     
		//          0       
		//     +              0
		//
		//     +
		//
		List<Pos> P_LuStar_UpFlyOne_3
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(5, 3));    // 第三步：一间跳
				temp.Add(new Pos(3, 8));    // 第四步：大飞拆
				return temp;
			}
		}
		//     
		//          0       
		//     +              0
		//
		//   +
		//
		List<Pos> P_LuStar_UpFlyOne_4
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(5, 2));    // 第三步：小飞守
				temp.Add(new Pos(3, 8));    // 第四步：大飞拆
				return temp;
			}
		}
		//     
		//          0       
		//     +              0
		//
		//     +
		//
		List<Pos> P_LuStar_UpFlyOne_5
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(5, 3));    // 第三步：一间跳
				temp.Add(new Pos(3, 9));    // 第四步：超大飞拆
				return temp;
			}
		}
		//     
		//          0       
		//     +              0
		//
		//   +
		//
		List<Pos> P_LuStar_UpFlyOne_6
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(5, 2));    // 第三步：小飞守
				temp.Add(new Pos(3, 9));    // 第四步：超大飞拆
				return temp;
			}
		}
		//     
		//   0 0    0       
		//     +              +
		//   +
		//   0 + +
		//   + 0
		List<Pos> P_LuStar_UpClip3		// 三间高夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(3, 9));    // 第三步：三间高夹
				temp.Add(new Pos(5, 2));    // 第四步：双飞燕
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(6, 3));
				temp.Add(new Pos(5, 4));
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(4, 2));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(6, 2));
				return temp;
			}
		}
		//     
		//   0 0    0        +
		//     +              
		//   +
		//   0 + +
		//   + 0
		List<Pos> P_LuStar_UpClip3L		// 三间低夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(2, 9));    // 第三步：三间地夹
				temp.Add(new Pos(5, 2));    // 第四步：双飞燕
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(6, 3));
				temp.Add(new Pos(5, 4));
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(4, 2));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(6, 2));
				return temp;
			}
		}
		//   0 0 +
		//   0 + +  0       
		//   0 +    +         +
		//     +
		//   0 
		//   
		List<Pos> P_LuStar_UpClip3_2		// 三间高夹2
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(3, 9));    // 第三步：三间高夹
				temp.Add(new Pos(2, 2));    // 第四步：点三三
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(1, 3));
				temp.Add(new Pos(1, 4));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(5, 2));
				temp.Add(new Pos(3, 5));
				return temp;
			}
		}
		// + 0 0
		//   +     0       
		//     +             +
		//         0
		//     + 
		//   
		List<Pos> P_LuStar_UpClip3_3		// 三间高夹3
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(3, 9));    // 第三步：三间高夹
				temp.Add(new Pos(4, 5));    // 第四步：跳
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(1, 3));
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(1, 1));
				return temp;
			}
		}
		//     
		//   0 +    0       
		//   0 +              +
		// 0 + +
		//   0 + +
		// 0   0 +
		//     0
		List<Pos> P_LuStar_UpClip3_4        // 三间高夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(3, 9));    // 第三步：三间高夹
				temp.Add(new Pos(5, 2));    // 第四步：双飞燕
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(6, 3));
				temp.Add(new Pos(5, 4));
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(4, 2));
				temp.Add(new Pos(4, 1));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(6, 1));
				temp.Add(new Pos(6, 4));
				temp.Add(new Pos(7, 3));
				return temp;
			}
		}
		//   0 0 +
		//   0 + +  0       
		//   0 +           +
		//     +
		//   0 
		//   
		List<Pos> P_LuStar_UpClip2		// 二间高夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(3, 8));    // 第三步：二间高夹
				temp.Add(new Pos(2, 2));    // 第四步：点三三
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(1, 3));
				temp.Add(new Pos(1, 4));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(5, 2));
				return temp;
			}
		}
		//   0 0 +
		//   0 + +  0      + 
		//   0 +           
		//     +
		//   0 
		//   
		List<Pos> P_LuStar_UpClip2L		// 二间低夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(2, 8));    // 第三步：二间低夹
				temp.Add(new Pos(2, 2));    // 第四步：点三三
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(1, 3));
				temp.Add(new Pos(1, 4));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(5, 2));
				return temp;
			}
		}
		//   0 0 +
		//   0 + +  0     
		//   0 +         +  
		//     +
		//   0 
		//   
		List<Pos> P_LuStar_UpClip1		// 一间高夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(3, 7));    // 第三步：一间高夹
				temp.Add(new Pos(2, 2));    // 第四步：点三三
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(1, 3));
				temp.Add(new Pos(1, 4));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(5, 2));
				return temp;
			}
		}
		//   0 0 +
		//   0 + +  0    + 
		//   0 +           
		//     +
		//   0 
		//   
		List<Pos> P_LuStar_UpClip1L		// 一间低夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3));    // 第一步：左上星
				temp.Add(new Pos(2, 5));    // 第二步：小飞挂
				temp.Add(new Pos(2, 7));    // 第三步：一间低夹
				temp.Add(new Pos(2, 2));    // 第四步：点三三
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(1, 3));
				temp.Add(new Pos(1, 4));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(5, 2));
				return temp;
			}
		}

		#endregion

		#region 小目定式

		//   + 0 0            0
		//     +   0       
		//   +
		//       +
		List<Pos> P_LuEye_UpJumpOne		// 一间高挂
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2));    // 第一步：左上小目
				temp.Add(new Pos(3, 4));    // 第二步：一间高挂
				temp.Add(new Pos(5, 3));    // 第三步：小飞守
				temp.Add(new Pos(2, 2));    // 第四步：托三三
				temp.Add(new Pos(2, 1));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(4, 1));
				temp.Add(new Pos(2, 8));
				return temp;
			}
		}
		//       + + 0        0
		//     +   0 0       
		//   
		//     +
		List<Pos> P_LuEye_UpJumpOne_2		// 托退
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2));    // 第一步：左上小目
				temp.Add(new Pos(3, 4));    // 第二步：一间高挂
				temp.Add(new Pos(2, 4));    // 第三步：小飞托
				temp.Add(new Pos(2, 5));    // 第四步：扳
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 5));
				temp.Add(new Pos(5, 2));
				temp.Add(new Pos(2, 9));
				return temp;
			}
		}

		//       +
		//     +   0 0      
		//       + + 0
		//       
		//           0
		List<Pos> P_LuEye_UpJumpOne_3		// 外靠
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2));    // 第一步：左上小目
				temp.Add(new Pos(3, 4));    // 第二步：一间高挂
				temp.Add(new Pos(4, 4));
				temp.Add(new Pos(4, 5));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(3, 5));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(6, 5));
				return temp;
			}
		}
		//   0 0     0 +
		//   + + 0 0     +     
		//   
		//     
		//     +
		List<Pos> P_LuEye_UpClip1L		// 一间低夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2));    // 第一步：左上小目
				temp.Add(new Pos(3, 4));    // 第二步：一间高挂
				temp.Add(new Pos(2, 6));    // 第三步：一间低夹
				temp.Add(new Pos(2, 2));    // 第四步：托三三
				temp.Add(new Pos(6, 2));
				temp.Add(new Pos(3, 3));
				temp.Add(new Pos(3, 1));
				temp.Add(new Pos(2, 5));
				temp.Add(new Pos(3, 7));
				temp.Add(new Pos(2, 1));
				return temp;
			}
		}
		//	   +   + 0 +
		//   0 0 + + 0 +
		//     + 0 0 + +    
		//     0     0
		//         0
		//     
		List<Pos> P_LuEye_UpClip1L_2		// 一间低夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2));    // 第一步：左上小目
				temp.Add(new Pos(3, 4));    // 第二步：一间高挂
				temp.Add(new Pos(2, 6));    // 第三步：一间低夹
				temp.Add(new Pos(2, 2));    // 第四步：托三三
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 3));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(2, 5));
				temp.Add(new Pos(1, 2));
				temp.Add(new Pos(2, 1));
				temp.Add(new Pos(3, 5));
				temp.Add(new Pos(1, 5));
				temp.Add(new Pos(1, 4));
				temp.Add(new Pos(4, 2));
				temp.Add(new Pos(1, 6));
				temp.Add(new Pos(4, 5));
				temp.Add(new Pos(4, 6));
				temp.Add(new Pos(6, 4));
				return temp;
			}
		}
		//   + 0          
		//   + + 0 0     +  
		//   + 0 0
		//   +    
		//       0
		//     +
		//         0
		//
		List<Pos> P_LuEye_UpClip2		// 二间高夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2));    // 第一步：左上小目
				temp.Add(new Pos(3, 4));    // 第二步：一间高挂
				temp.Add(new Pos(3, 7));    // 第三步：二间高夹
				temp.Add(new Pos(4, 2));    // 第四步：外靠
				temp.Add(new Pos(4, 1));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(5, 1));
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(2, 1));
				temp.Add(new Pos(3, 3));
				temp.Add(new Pos(3, 1));
				temp.Add(new Pos(6, 3));
				temp.Add(new Pos(7, 2));
				temp.Add(new Pos(8, 4));
				return temp;
			}
		}
		//               0
		//     0   0       
		//   + +   0     +  
		//   + 0 0
		//   +    
		//       +
		List<Pos> P_LuEye_UpClip2_2		// 二间高夹
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2));    // 第一步：左上小目
				temp.Add(new Pos(3, 4));    // 第二步：一间高挂
				temp.Add(new Pos(3, 7));    // 第三步：二间高夹
				temp.Add(new Pos(4, 2));    // 第四步：外靠
				temp.Add(new Pos(4, 1));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(5, 1));
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(3, 1));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(1, 7));
				return temp;
			}
		}
		#endregion

		#region 高目定式
		//   0 + +            +
		//     0   +       
		//   0
		//       0
		List<Pos> P_LuEyeH_UpJumpOne		// 一间挂
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 4));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(2, 1));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(4, 1));
				temp.Add(new Pos(2, 8));
				temp.Add(new Pos(5, 3));
				return temp;
			}
		}
		//       0 0 +
		//     0   + +      
		//   
		//   0   +
		List<Pos> P_LuEyeH_UpJumpOne_2		// 一间挂
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 4));
				temp.Add(new Pos(3, 2));
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(2, 5));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 5));
				temp.Add(new Pos(5, 1));
				return temp;
			}
		}
		#endregion

		#region 三三定式

		//           +
		//     + +    
		//       0 0     0
		//   +
		//       
		//       0
		List<Pos> P_Lu33_Cusp
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(3, 3));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 4));
				temp.Add(new Pos(4, 1));
				temp.Add(new Pos(6, 3));
				temp.Add(new Pos(1, 5));
				temp.Add(new Pos(3, 7));
				return temp;
			}
		}
		//           
		//     + + 0        0
		//       0 0   
		//   +
		//       +
		//       
		List<Pos> P_Lu33_Cusp_2
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(3, 3));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 4));
				temp.Add(new Pos(4, 1));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(2, 8));
				return temp;
			}
		}
		//           
		//     +             0
		//          0   
		//       +
		//       
		List<Pos> P_Lu33_FlyOne
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(3, 4));
				temp.Add(new Pos(4, 3));
				temp.Add(new Pos(2, 8));
				return temp;
			}
		}
		//           
		//     +               0
		//           0   
		//   
		//       +
		//       
		List<Pos> P_Lu33_FlyTwo
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(3, 5));
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(2, 9));
				return temp;
			}
		}
		//           
		//     +     0      0
		//              
		//   
		//     +
		//       
		List<Pos> P_Lu33_JumpTwo
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(2, 2));
				temp.Add(new Pos(2, 5));
				temp.Add(new Pos(5, 2));
				temp.Add(new Pos(2, 8));
				return temp;
			}
		}
		#endregion

		#region 目外定式

		//     0 0   0
		//       + +     
		//   +
		//       
		//     
		//
		//
		//     +
		List<Pos> P_Lu42_FlyOne
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(4, 2));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(3, 4));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(3, 5));
				temp.Add(new Pos(2, 6));
				temp.Add(new Pos(9, 3));
				return temp;
			}
		}
		//     + 0         0
		//     + 0     
		//   +   0
		//     +  
		//          +
		List<Pos> P_Lu42_FlyOne_2
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(4, 2));
				temp.Add(new Pos(3, 4));
				temp.Add(new Pos(2, 3));
				temp.Add(new Pos(2, 4));
				temp.Add(new Pos(3, 3));
				temp.Add(new Pos(4, 4));
				temp.Add(new Pos(5, 3));
				temp.Add(new Pos(2, 8));
				temp.Add(new Pos(6, 5));
				return temp;
			}
		}
		#endregion

		#region m_isChangePatternColor == false

		//       +
		//     + 0 0       0
		// +   +   + 0
		// 0 +     +
		// 0 0 +      +
		//     0   0
		//
		//
		List<Pos> P_LuStar_UpFlyOne_B35		// 双飞燕
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3, StoneColor.Black));    // 第一步：左上星
				temp.Add(new Pos(2, 5, StoneColor.White));    // 第二步：小飞挂
				temp.Add(new Pos(5, 2, StoneColor.White));    
				temp.Add(new Pos(3, 5, StoneColor.Black));
				temp.Add(new Pos(3, 6, StoneColor.White));
				temp.Add(new Pos(4, 5, StoneColor.Black));
				temp.Add(new Pos(2, 4, StoneColor.White));
				temp.Add(new Pos(2, 3, StoneColor.Black));
				temp.Add(new Pos(6, 3, StoneColor.White));
				temp.Add(new Pos(4, 2, StoneColor.Black));
				temp.Add(new Pos(4, 1, StoneColor.White));
				temp.Add(new Pos(5, 3, StoneColor.Black));
				temp.Add(new Pos(5, 1, StoneColor.White));
				temp.Add(new Pos(3, 1, StoneColor.Black));
				temp.Add(new Pos(6, 5, StoneColor.White));
				temp.Add(new Pos(5, 6, StoneColor.Black));
				temp.Add(new Pos(2, 8, StoneColor.White));
				temp.Add(new Pos(1, 4, StoneColor.Black));
				return temp;
			}
		}
		//       + 0
		//     +
		//
		//   +
		List<Pos> P_LuStar_UpFlyOne_B13		// 尖顶
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3, StoneColor.Black));    // 第一步：左上星
				temp.Add(new Pos(2, 5, StoneColor.White));    // 第二步：小飞挂
				temp.Add(new Pos(5, 2, StoneColor.Black));    // 第三步：小飞守
				temp.Add(new Pos(2, 4, StoneColor.Black));    // 第四步：二路守角
				return temp;
			}
		}
		//   0
		// 0   0 0 + +
		// + 0 + +
		//   +
		List<Pos> P_LuStar_UpFlyOne_B25
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3, StoneColor.Black));
				temp.Add(new Pos(2, 5, StoneColor.Black));
				temp.Add(new Pos(2, 2, StoneColor.White));
				temp.Add(new Pos(3, 2, StoneColor.Black));
				temp.Add(new Pos(2, 3, StoneColor.White));
				temp.Add(new Pos(2, 4, StoneColor.Black));
				temp.Add(new Pos(3, 1, StoneColor.White));
				temp.Add(new Pos(4, 1, StoneColor.Black));
				temp.Add(new Pos(1, 1, StoneColor.White));
				temp.Add(new Pos(3, 0, StoneColor.Black));
				temp.Add(new Pos(2, 0, StoneColor.White));
				return temp;
			}
		}
		//          +
		//     +
		List<Pos> P_LuStar_UpFlyOne_B92
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3, StoneColor.Black));
				temp.Add(new Pos(2, 5, StoneColor.Black));
				temp.Add(new Pos(9, 2, StoneColor.Black));
				return temp;
			}
		}
		//          +
		//     0
		//       +
		//   +
		List<Pos> P_LuStar_UpFlyOne_B52
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3, StoneColor.White));
				temp.Add(new Pos(2, 5, StoneColor.Black));
				temp.Add(new Pos(5, 2, StoneColor.Black));
				temp.Add(new Pos(4, 4, StoneColor.Black));
				return temp;
			}
		}
		//      +
		//    +      +
		//      0               +
		//           +
		//    0
		//
		List<Pos> P_LuStar_UpFlyOne_B46
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3, StoneColor.White));
				temp.Add(new Pos(2, 5, StoneColor.Black));
				temp.Add(new Pos(5, 2, StoneColor.White));
				temp.Add(new Pos(3, 9, StoneColor.Black));
				temp.Add(new Pos(4, 6, StoneColor.Black));
				temp.Add(new Pos(1, 3, StoneColor.Black));
				temp.Add(new Pos(2, 2, StoneColor.Black));
				return temp;
			}
		}
		// + 0
		//   + 0 0     
		//     +   0       
		//   +        
		//     
		List<Pos> P_LuEye_UpJumpOne_B11		// 一间高挂
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 4, StoneColor.White));
				temp.Add(new Pos(3, 2, StoneColor.Black));
				temp.Add(new Pos(2, 2, StoneColor.White));
				temp.Add(new Pos(2, 1, StoneColor.Black));
				temp.Add(new Pos(2, 3, StoneColor.White));
				temp.Add(new Pos(1, 2, StoneColor.White));
				temp.Add(new Pos(1, 1, StoneColor.Black));
				return temp;
			}
		}
		//   + 0 0      +   
		//     +   0       
		//   +        +
		//       +
		List<Pos> P_LuEye_UpJumpOne_B26		// 一间高挂
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2, StoneColor.Black));    // 第一步：左上小目
				temp.Add(new Pos(3, 4, StoneColor.White));    // 第二步：一间高挂
				temp.Add(new Pos(5, 3, StoneColor.Black));    // 第三步：小飞守
				temp.Add(new Pos(2, 2, StoneColor.White));    // 第四步：托三三
				temp.Add(new Pos(2, 1, StoneColor.Black));
				temp.Add(new Pos(2, 3, StoneColor.White));
				temp.Add(new Pos(4, 1, StoneColor.Black));
				temp.Add(new Pos(2, 6, StoneColor.Black));
				temp.Add(new Pos(4, 5, StoneColor.Black));
				return temp;
			}
		}
		//       +      
		//   0 0   +     0
		//   0 + +
		//   + 
		List<Pos> P_LuEye_Clip2_B15
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 2, StoneColor.White));    // 第一步：左上小目
				temp.Add(new Pos(3, 4, StoneColor.Black));    // 第二步：一间高挂
				temp.Add(new Pos(3, 7, StoneColor.White));
				temp.Add(new Pos(4, 2, StoneColor.Black));
				temp.Add(new Pos(4, 1, StoneColor.White));
				temp.Add(new Pos(4, 3, StoneColor.Black));
				temp.Add(new Pos(5, 1, StoneColor.Black));
				temp.Add(new Pos(3, 1, StoneColor.White));
				temp.Add(new Pos(2, 3, StoneColor.Black));
				return temp;
			}
		}

		//    +
		//    + 0   
		//
		List<Pos> P_LuStar_UpThree3_B23	// 点三三
		{
			get
			{
				var temp = new List<Pos>();
				temp.Add(new Pos(3, 3, StoneColor.White));    // 第一步：左上星
				temp.Add(new Pos(2, 2, StoneColor.Black));
				temp.Add(new Pos(2, 3, StoneColor.Black));
				return temp;
			}
		}

		#endregion

		#region 四角八变换

		bool m_IsChangeStoneColor = true;
		List<Pos> Lu_Up(List<Pos> poses, bool firstIsBlack = true)	// lu：左上； Up: 上挂
		{
			return GetPattern(poses, firstIsBlack);
		}
		List<Pos> Lu_Left(List<Pos> poses, bool firstIsBlack = true)	// 左上，左挂
		{
			var result = new List<Pos>();
			var temp = Lu_Up(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(p.Col, p.Row, color: p.StoneColor));
			}
			return result;
		}
		List<Pos> Ru_Up(List<Pos> poses, bool firstIsBlack = true)	// NewCol = 9 + (9 - OldCol);    列变行不变
		{
			var result = new List<Pos>();
			var temp = Lu_Up(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(p.Row, 18 - p.Col, color: p.StoneColor));
			}
			return result;
		}
		List<Pos> Ru_Right(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Lu_Left(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(p.Row, 18 - p.Col, color: p.StoneColor));
			}
			return result;
		}
		List<Pos> Ld_Down(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Lu_Up(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(18 - p.Row, p.Col, color: p.StoneColor));
			}
			return result;
		}
		List<Pos> Ld_Left(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Lu_Left(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(18 - p.Row, p.Col, p.StoneColor));
			}
			return result;
		}
		List<Pos> Rd_Down(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Ld_Down(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(p.Row, 18 - p.Col, p.StoneColor));
			}
			return result;
		}
		List<Pos> Rd_Right(List<Pos> poses, bool firstIsBlack = true)
		{
			var result = new List<Pos>();
			var temp = Ru_Right(poses, firstIsBlack);
			foreach (var p in temp) {
				result.Add(new Pos(18 - p.Row, p.Col, p.StoneColor));
			}
			return result;
		}
		private List<Pos> GetPattern(List<Pos> poses, bool firstIsBlack = true)
		{
			if (!m_IsChangeStoneColor && firstIsBlack) {
				return poses;
			} else if (!m_IsChangeStoneColor && !firstIsBlack) {
				var t = new List<Pos>();
				foreach (var p in poses) {
					var pos = new Pos(p.Row, p.Col, p.StoneColor == StoneColor.Black ? StoneColor.White : StoneColor.Black);
					t.Add(pos);
				}
				return t;
			}

			var firstColor = firstIsBlack ? StoneColor.Black : StoneColor.White;
			var otherColor = firstColor == StoneColor.Black ? StoneColor.White : StoneColor.Black;
			int count = 0;
			var temp = new List<Pos>();
			foreach (var p in poses) {
				var color = count++ % 2 == 0 ? firstColor : otherColor;
				var t = new Pos(p.Row, p.Col, color);
				temp.Add(t);
			}
			return temp;
		}

		List<List<Pos>> LeftUpPatterns(List<List<Pos>> patterns)
		{
			List<List<Pos>> result = new List<List<Pos>>();
			foreach (var pattern in patterns) {
				result.Add(Lu_Left(pattern));
				result.Add(Lu_Left(pattern, false));
				result.Add(Lu_Up(pattern));
				result.Add(Lu_Up(pattern, false));
			}
			return result;
		}
		List<List<Pos>> RightUpPatterns(List<List<Pos>> patterns)
		{
			var result = new List<List<Pos>>();
			foreach (var pattern in patterns) {
				result.Add(Ru_Right(pattern));
				result.Add(Ru_Right(pattern, false));
				result.Add(Ru_Up(pattern));
				result.Add(Ru_Up(pattern, false));
			}
			return result;
		}
		List<List<Pos>> LeftDownPatterns(List<List<Pos>> patterns)
		{
			var result = new List<List<Pos>>();
			foreach (var pattern in patterns) {
				result.Add(Ld_Down(pattern));
				result.Add(Ld_Down(pattern, false));
				result.Add(Ld_Left(pattern));
				result.Add(Ld_Left(pattern, false));
			}
			return result;
		}
		List<List<Pos>> RightDownPatterns(List<List<Pos>> patterns)
		{
			var result = new List<List<Pos>>();
			foreach (var pattern in patterns) {
				result.Add(Rd_Down(pattern));
				result.Add(Rd_Down(pattern, false));
				result.Add(Rd_Right(pattern));
				result.Add(Rd_Right(pattern, false));
			}
			return result;
		}

		Pos GetPos_FromPatterns(List<List<Pos>> patterns)
		{
			Pos ltpos, rdpos;
			var all = BlackPoses.Union(WhitePoses).ToList();
			foreach (List<Pos> pattern in patterns) {
				ltpos = GetLeftTopPos(pattern);
				rdpos = GetRightDownPos(pattern);
				var poses = all.Intersect(GetArea(ltpos, rdpos)).OrderBy(p => GetStep(p).StepCount).ToList();
				int posCount = poses.Count;
				int count = pattern.Count;
				if (count >= posCount && posCount > 0) {
					var firstColor = GetStep(poses[0]).StoneColor;
					var otherColor = firstColor == StoneColor.Black ? StoneColor.White : StoneColor.Black;
					for (int i = 0; i < count; i++) {
						if (i > posCount - 1) {
							if (pattern[i].StoneColor == StoneColor.Black)
								return pattern[i];
							break;
						}
						if (poses[i] == pattern[i] && poses[i].StoneColor == pattern[i].StoneColor) {
							continue;
						} else {
							break;
						}
					}
				}
			}

			return m_InvalidPos;
		}

		Pos GetLeftTopPos(List<Pos> poses)
		{
			int row = int.MaxValue, col = int.MaxValue;
			foreach (var p in poses) {
				if (row > p.Row)
					row = p.Row;
				if (col > p.Col)
					col = p.Col;
			}
			return new Pos(row - 1, col - 1);
		}
		Pos GetRightDownPos(List<Pos> poses)
		{
			int row = int.MinValue, col = int.MinValue;
			foreach (var p in poses) {
				if (row < p.Row)
					row = p.Row;
				if (col < p.Col)
					col = p.Col;
			}
			return new Pos(row + 1, col + 1);
		}

		#endregion

	}
}