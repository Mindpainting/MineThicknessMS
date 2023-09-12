using GMap.NET;
using MineralThicknessMS.entity;
using STTech.BytesIO.Serial;
using System.IO.Ports;

namespace MineralThicknessMS.service
{
    public class MySerialClient
    {
        // 串口连接客户端
        public SerialClient serialClient;

        public MySerialClient() { 
            serialClient = new SerialClient();
        }

        public MySerialClient(string serialClientName)
        {
            serialClient = new SerialClient();
            serialClient.PortName = serialClientName;

            // 监听连接成功事件
            serialClient.OnConnectedSuccessfully += Client_OnConnectedSuccessfully;
            // 监听连接失败事件
            serialClient.OnConnectionFailed += Client_OnConnectionFailed;

            // 监听接收数据事件
            //serialClient.OnDataReceived += Client_OnDataReceived;
        }

        private async void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            //收到的消息
            string msgStr = e.Data.EncodeToString();

            double[] mt = await GetCHPositionMT();

            //要发送的消息
            await serialClient.SendAsync("data".GetBytes());
        }

        private void Client_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {

        }

        private void Client_OnConnectionFailed(object sender, STTech.BytesIO.Core.ConnectionFailedEventArgs e)
        {
            List<string> portNames = new(SerialPort.GetPortNames());

            foreach (string portName in portNames)
            {
                if(e.Exception.Message.Contains(portName))
                {
                    MessageBox.Show(portName + "串口被占用！", "提示");
                }
            }         
        }

        public async void serialConnect(object sender, EventArgs e)
        {
            serialClient.Connect();
            while (true)
            {
                double[] mt = await GetCHPositionMT();
                //要发送的消息
                await serialClient.SendAsync((mt[0].ToString() + "," + mt[1].ToString()).GetBytes());
                
                //await serialClient.SendAsync(mt[1].ToString().GetBytes());

                await Task.Delay(3000);
            }
        }

        public void serialDisconnect(object sender, EventArgs e)
        {
            serialClient.Disconnect();
        }

        //计算水采机前后切割机位置，返回切割机所在位置矿厚
        public async Task<double[]> GetCHPositionMT()
        {
            //GPS没有数据或不在盐池内，矿厚都为零
            double[] result = new double[2] { 0, 0 };
            //有经纬度表示GPS有数据传来，只有一个GPS数据不计算
            if (Status.latitude[0] != 0 && Status.longitude[0] != 0 && Status.latitude[1] != 0 && Status.longitude[1] != 0)
            {
                //0表示左后方测深仪(HGPS)，1表示右前方测深仪(QGPS)
                if (Status.waterwayId[0] != -1 && Status.rectangleId[0] != -1 && Status.waterwayId[1] != -1 && Status.rectangleId[1] != -1)
                {
                    //左后GPS
                    PointLatLng gpsLA = new(GridView.degTodms(Status.latitude[0]), GridView.degTodms(Status.longitude[0]));
                    //右前GPS
                    PointLatLng gpsRB = new(GridView.degTodms(Status.latitude[1]), GridView.degTodms(Status.longitude[1]));

                    GpsXY.GetValueOfCoordSys();
                    GpsXY.GaussBL_XY(GpsXY.m_Cs84, gpsLA.Lat, gpsLA.Lng, out double X1, out double Y1);
                    GpsXY.GaussBL_XY(GpsXY.m_Cs84, gpsRB.Lat, gpsRB.Lng, out double X2, out double Y2);

                    //平面坐标
                    PointXY pointXY1 = new(X1, Y1);
                    PointXY pointXY2 = new(X2, Y2);

                    //高程
                    double elevation1 = Status.height2;
                    double elevation2 = Status.height2;

                    GPSCoor gpsCoorLA = new(gpsLA.Lat, gpsLA.Lng, pointXY1.X, pointXY1.Y, elevation1);
                    GPSCoor gpsCoorRB = new(gpsRB.Lat, gpsRB.Lng, pointXY2.X, pointXY2.Y, elevation2);
                    GPSCoord.IniCoordSysTag();

                    List<Grid> list = new();
                    //HGPS横滚小于等于30使用HGPS计算前后切割机位置
                    if (Status.rolling[0] <= 30)
                    {
                        double oriLA = Status.ori[0];
                        //计算垂足
                        GPSCoor footDownPoint = new();
                        GPSCoord.CalPnt(gpsCoorLA, oriLA + 90, Status.GPSToCenterAxisDis, footDownPoint);

                        //切割机位置D
                        GPSCoor calPointDBefore = new();
                        GPSCoor calPointDAfter = new();
                        //
                        GPSCoord.CalPnt(footDownPoint, oriLA, Status.HDisToCH[0], calPointDBefore);
                        GPSCoord.CalPnt(footDownPoint, oriLA + 180, Status.HDisToCH[1], calPointDAfter);
                        GpsXY.GaussXY_BL(GpsXY.m_Cs0, calPointDBefore.LocN, calPointDBefore.LocE, out double B1, out double L1);
                        GpsXY.GaussXY_BL(GpsXY.m_Cs0, calPointDAfter.LocN, calPointDAfter.LocE, out double B2, out double L2);
                        list.Add(GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), Status.grids, new PointLatLng(B1, L1)));
                        list.Add(GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), Status.grids, new PointLatLng(B2, L2)));
                    }
                    else
                    {
                        double oriRB = Status.ori[1];
                        //计算垂足
                        GPSCoor footDownPoint = new();
                        GPSCoord.CalPnt(gpsCoorRB, oriRB - 90, Status.GPSToCenterAxisDis, footDownPoint);

                        //切割机位置D
                        GPSCoor calPointDBefore = new();
                        GPSCoor calPointDAfter = new();
                        //
                        GPSCoord.CalPnt(footDownPoint, oriRB, Status.HDisToCH[0], calPointDBefore);
                        GPSCoord.CalPnt(footDownPoint, oriRB + 180, Status.HDisToCH[1], calPointDAfter);
                        GpsXY.GaussXY_BL(GpsXY.m_Cs0, calPointDBefore.LocN, calPointDBefore.LocE, out double B1, out double L1);
                        GpsXY.GaussXY_BL(GpsXY.m_Cs0, calPointDAfter.LocN, calPointDAfter.LocE, out double B2, out double L2);
                        list.Add(GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), Status.grids, new PointLatLng(B1, L1)));
                        list.Add(GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), Status.grids, new PointLatLng(B2, L2)));
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        //计算的切割头的位置在盐池内，查数据库
                        if (list[i].Id != 0)
                        {
                            //查该格子矿厚
                            result[i] = await DataMapper.getGridMineThickness(list[i].Column, list[i].Row);
                        }
                    }
                }
            }
            return result;
        }
    }
}
