using STTech.BytesIO.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STTech.BytesIO.Core;
using System.Drawing.Drawing2D;
using MineralThicknessMS.entity;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net;

namespace MineralThicknessMS.service
{
    public class MyServer
    {

        public TcpServer server;
        private DataMapper dataMapper;
        private MsgDecode msgDecode;
        private entity.Status status;
        public bool openFlag = false;

        public MyServer(string port)
        {
            dataMapper = new DataMapper();
            msgDecode = new MsgDecode();
            Control.CheckForIllegalCrossThreadCalls = false;
            server = new TcpServer();
            status = new entity.Status();
            server.Port = int.Parse(port);
            server.ClientConnected += Server_ClientConnected;
        }

        //开启服务
        public void tsmiStart_Click(object sender, EventArgs e)
        {
            server.StartAsync();
            openFlag = true;
        }

        //关闭服务
        public void tsmiStop_Click(object sender, EventArgs e)
        {
            server.CloseAsync();
            openFlag = false;
        }


        public void Server_ClientConnected(object sender, STTech.BytesIO.Tcp.Entity.ClientConnectedEventArgs e)
        {
            e.Client.OnDataReceived += Client_OnDataReceived;
        }

        private void Server_ClientDisconnected(object sender, STTech.BytesIO.Tcp.Entity.ClientDisconnectedEventArgs e)
        {

        }

        public void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            TcpClient tcpClient = (TcpClient)sender;
            String str = e.Data.EncodeToString();
            DataMsg dataMsg = new DataMsg();
            dataMsg = msgDecode.msgSplit(str);
            if(dataMsg.getMsgBegin() == "$GPGGA" && dataMsg.getMsgEnd() == "*5F"
                && dataMsg.getWaterwayId() != 0 && dataMsg.getRectangleId() !=0
                && dataMsg.getMineHigh() >= 0
                )
            {
                dataMapper.addData(dataMsg);
                status.setStatus(dataMsg);
            }
        }

        //发送消息给服务端
        public void Client_OnDataSent(object sender, EventArgs e, string msg)
        {
            try
            {
                if (server.Clients.Last() == server.Clients.First())
                {
                    server.Clients.Last().Send(msg.GetBytes());
                }
                else
                {
                    server.Clients.Last().Send(msg.GetBytes());
                    server.Clients.First().Send(msg.GetBytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("未开启服务或无水采机连接服务", "指令发送失败");
            }
        }

        ////获取本机局域网ip
        //public string getIp()
        //{
        //    IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        //    return iPHostEntry.AddressList[1].ToString();
        //}
        public string getIp()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            if (localhost != null)
            {
                foreach (IPAddress item in localhost.AddressList)
                {
                    //判断是否是内网IPv4地址
                    if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return item.MapToIPv4().ToString();
                    }
                }
            }
            return "127.0.0.1";
        }
    }


}
