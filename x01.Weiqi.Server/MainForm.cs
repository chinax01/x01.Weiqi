// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace Server
{
	public partial class MainForm : Form
	{
		TcpListener m_TcpListener;
		List<User> m_Users = new List<User>();  //为简化，只作 2 想

		public MainForm()
		{
			InitializeComponent();
		}

		private void ButtonStartServer_Click(object sender, EventArgs e)
		{
			IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName())[0];
			m_TcpListener = new TcpListener(ip, 51000);
			try {
				m_TcpListener.Start();
			} catch (Exception) { }

			ThreadStart threadStart = new ThreadStart(ListenClient);
			Thread thread = new Thread(threadStart);
			thread.Start();

			MessageBox.Show("Start server success!");
		}

		void ListenClient()
		{
			while (true) {
				TcpClient client = null;
				try {
					client = m_TcpListener.AcceptTcpClient();
				} catch {
					break;  // 以此退出？
				}
				User user = new User(client);
				m_Users.Add(user);

				ParameterizedThreadStart ptStart = new ParameterizedThreadStart(ReceiveData);
				Thread thread = new Thread(ptStart);
				thread.Start(user);
			}
		}

		void ReceiveData(object u)
		{
			User user = u as User;
			if (user == null) return;
			while (true) {
				try {
					string data = user.Reader.ReadLine();
					for (int i = 0; i < 2; i++) {
						string color = i % 2 == 0 ? "black" : "white";
						m_Users[i].Writer.WriteLine(string.Format("{0},{1}", color, data));
						m_Users[i].Writer.Flush();
					}
				} catch (Exception) { }
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			foreach (var item in m_Users) {
				item.Client.Close();
			}
			m_TcpListener.Stop();
		}
	}
}
