// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace x01.Weiqi.Board
{
    public class InetBoard : BoardBase
    {
        int m_Col = -1, m_Row = -1, m_StepCount = -1;
        string m_Color = string.Empty;
        TcpClient m_Client = null;
        StreamWriter m_Writer = null;
        StreamReader m_Reader = null;
        int m_Count = 0;
        System.Windows.Forms.Timer m_Timer = new System.Windows.Forms.Timer();

        public InetBoard(int size = 38)
            : base(size)
        {
            m_Timer.Tick += new EventHandler(Timer_Tick);
            m_Timer.Start();

            InitClient();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            Draw();
        }

        void Draw()
        {
            int col = m_Col;
            int row = m_Row;
            if (NotIn(col, row))
            {
                return;
            }
            DrawChess(col, row);
            Eat(col, row);

            m_Count++;
        }

        void InitClient()
        {
            // 测试，取主机即可。实际，应为服务器域名
            m_Client = new TcpClient(Dns.GetHostName(), 51000);
            m_Writer = new StreamWriter(m_Client.GetStream(), Encoding.UTF8);
            m_Reader = new StreamReader(m_Client.GetStream(), Encoding.UTF8);

            Thread thread = new Thread(new ThreadStart(Receive));
            thread.Start();
        }

        void Receive()
        {
            while (true)
            {
                string data = m_Reader.ReadLine();
                string[] step = data.Split(',');
                m_Color = step[0];
                m_Col = int.Parse(step[1]);
                m_Row = int.Parse(step[2]);
                m_StepCount = int.Parse(step[3]);
            }
        }

        void Send(string data)
        {
            m_Writer.WriteLine(data);
            m_Writer.Flush();
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            
            if (m_Color == "black" && IsBlack != false)
            {
                return;
            }

            if (m_Color == "white" && IsBlack == false)
            {
                return;
            }

            int col = (int)e.GetPosition(this).X / ChessSize;
            int row = (int)e.GetPosition(this).Y / ChessSize;

            string data = string.Format("{0},{1},{2}", col, row, m_Count + 1);
            Send(data);
        }
    }
}
