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

        public MyServer() 
        {
            server = new TcpServer();
        }

        public MyServer(int port)
        {
            dataMapper = new DataMapper();
            msgDecode = new MsgDecode();
            Control.CheckForIllegalCrossThreadCalls = false;
            server = new TcpServer();
            status = new entity.Status();
            server.Port = port;

            server.ClientConnected += Server_ClientConnected;
        }

        //开启服务
        public void tsmiStart_Click(object sender, EventArgs e)
        {
            server.StartAsync();
        }

        //关闭服务
        public void tsmiStop_Click(object sender, EventArgs e)
        {
            server.CloseAsync();
        }


        public void Server_ClientConnected(object sender, STTech.BytesIO.Tcp.Entity.ClientConnectedEventArgs e)
        {
            e.Client.OnDataReceived += Client_OnDataReceived;
        }

        public async void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            await Task.Run(() =>
            {
                TcpClient tcpClient = (TcpClient)sender;
                String str = e.Data.EncodeToString();

                DataMsg dataMsg = new DataMsg();
                dataMsg = msgDecode.msgSplit(str);

                status.setStatus(dataMsg);

                if (dataMsg.getMsgBegin() == "$GPGGA" && dataMsg.getMsgEnd() == "*5F"
                    && dataMsg.getWaterwayId() >= 0 && dataMsg.getRectangleId() >= 0
                    && dataMsg.getMineHigh() >= 0 && dataMsg.getDepth() >= 0.3 && dataMsg.getGpsState() == 4
                )
                {
                    // Assuming dataMapper.addDataAsync method exists and it's an asynchronous operation.
                    dataMapper.addDataAsync(dataMsg); // ConfigureAwait(false) to avoid capturing UI context.
                }
            });
        }

        //public void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        //{
        //    TcpClient tcpClient = (TcpClient)sender;
        //    String str = e.Data.EncodeToString();

        //    DataMsg dataMsg = new DataMsg();
        //    dataMsg = msgDecode.msgSplit(str);

        //    status.setStatus(dataMsg);

        //    if (dataMsg.getMsgBegin() == "$GPGGA" && dataMsg.getMsgEnd() == "*5F"
        //        && dataMsg.getWaterwayId() != 0 && dataMsg.getRectangleId() != 0
        //        && dataMsg.getMineHigh() >= 0 && dataMsg.getDepth() >= 0.3 && dataMsg.getGpsState() == 4
        //        )
        //    {
        //        dataMapper.addDataAsync(dataMsg);
        //    }
        //}

        //发送消息给服务端
        public void Client_OnDataSent(object sender, EventArgs e, string msg)
        {
            try
            {
                if (server.Clients.Last() == server.Clients.First())
                {
                    server.Clients.Last().SendAsync(msg.GetBytes());
                }
                else
                {
                    server.Clients.Last().SendAsync(msg.GetBytes());
                    server.Clients.First().SendAsync(msg.GetBytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("未开启服务或无水采机连接服务", "指令发送失败");
            }
        }

        //发送消息给服务端
        public void Client_OnDataSent_AutoWash(object sender, EventArgs e, string msg)
        {
            try
            {
                if (server.Clients.Last() == server.Clients.First())
                {
                    server.Clients.Last().SendAsync(msg.GetBytes());
                }
                else
                {
                    server.Clients.Last().SendAsync(msg.GetBytes());
                    server.Clients.First().SendAsync(msg.GetBytes());
                }
            }
            catch (Exception ex)
            {

            }
        }

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
