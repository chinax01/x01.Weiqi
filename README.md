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