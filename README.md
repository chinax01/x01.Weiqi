# x01.Weiqi
围棋程序，初步实现了提子算法，保存棋谱，网络对弈，人机大战，显示步数，自动重绘等功能。
<h3>2015年5月2日: </h3>
1.使用 Code First 来操作数据库。但窗口大小改变时，StepBoard 仍是个问题。<br />
2.使用 GitHub for windows，Sync 时 VS 老跑出来 debug。改用 shell：git init => git push 后问题解决。
<h3>2015年5月4日：</h3>
1.改正了 StepBoard 重绘时的问题。<br />
2.git 时出现 fast-forward 问题，采用移除本地仓库，Clone 远程仓库，修改后重新 git push，可谓笨也！
<h3>2015年5月5日：</h3>
1.为在 Windows XP 上运行，只能采取 .Net 4.0，安装 Sql Express 2008，故作了些调整。<br />
2.开发过程，可参见：http://www.cnblogs.com/china_x01/category/685502.html
<h3>2015年5月6日：</h3>
1.为操作的方便，作了些调整，如中文显示菜单，快捷键等。<br />
2.删除 StepService 类，因为数据操作很少，没有必要加个中间层。
<h3>2015年5月7日：</h3>
1.Go 大约是 Game 加一个圈，表示围棋的意思。但在表音的语言里，添加象形的东西，多少有些奇怪。
围棋术语，大都采用日语的音译，如：aji，不知何云？名不正则言不顺，在围棋这一特定场景下，统一术语，
是有必要的。当然，有些术语是不需要的，如见合等，因为在程序中尚无具体实现。简化术语如下：<br />
	围棋：weiqi；	 棋盘：board；	棋子：stone；	棋局：chess；	棋步：step；	
	吃：eat；	飞：fly；	跳：jump；	尖：sharp；	长：grow；	征：levy；
<br />
2.牵一发而动全身，小作修改，也能导致许多问题，一动不如一静也。<br />
3.解决 BackOne() 时死子不能显示数字的问题。