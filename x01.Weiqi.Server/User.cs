// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Server
{
    class User
    {
        public TcpClient Client { get; private set; }
        public StreamReader Reader { get; private set; }
        public StreamWriter Writer { get; private set; }
        public User(TcpClient client)
        {
            this.Client = client;
            NetworkStream netStream = client.GetStream();
            Reader = new StreamReader(netStream, System.Text.Encoding.UTF8);
            Writer = new StreamWriter(netStream, System.Text.Encoding.UTF8);
        }
    }
}
