using MineralThicknessMS.service;

namespace MineralThicknessMS.entity
{
    public class Status
    {
        //盐池编号
        public static string saltBoundId = "";

        //测深系数
        public static double measureCoefficient = 0;

        //支架状态 true：展开 false：折叠
        public static bool[] bracket = new bool[2];

        //测声仪状态 
        public static bool[] soundMachine = new bool[2];

        //纬度
        public static double[] latitude = new double[2];

        //经度
        public static double[] longitude = new double[2];

        //航道编号
        public static int[] waterwayId = new int[2];

        //网格编号
        public static int[] rectangleId = new int[2];

        //水深
        public static double[] depth = new double[2];

        //矿厚
        public static double[] mineDepth = new double[2];

        //支架高度
        public static double height1 = 0;

        //盐池底板高度
        public static double height2 = 0;

        //GPS定位状态
        public static string[] GPSState = new string[2];

        //两GPS至船中轴线距离
        public static double GPSToCenterAxisDis;
       
        //GPS惯导航向
        public static double[] ori = new double[2];

        //惯导横滚
        public static double[] rolling = new double[2];  

        //QGPS(右前方测深仪) 中轴线点至前，后切割机距离
        public static double[] QDisToCH = new double[2];

        //HGPS(左后方测深仪) 中轴线点至前，后切割机距离
        public static double[] HDisToCH = new double[2];

        //每一个小网格集合，按航道分组
        public static List<List<Grid>> grids = new();

        //网格底面积
        public static double s = 81;

        //数据更新时间(UTC时间)
        public static DateTime[] dataRefreshUTCTime = new DateTime[2];

        public void setStatus(DataMsg dataMsg)
        {
            try
            {
                int clientId = dataMsg.getClientId() - 1;
                int i = dataMsg.getDeviceState();
                if (i == 0)
                {
                    bracket[clientId] = false;
                    soundMachine[clientId] = false;

                }
                if (i == 1)
                {
                    bracket[clientId] = false;
                    soundMachine[clientId] = true;

                }
                if (i == 2)
                {
                    bracket[clientId] = true;
                    soundMachine[clientId] = false;

                }
                if (i == 3)
                {
                    bracket[clientId] = true;
                    soundMachine[clientId] = true;

                }
                latitude[clientId] = dataMsg.getLatitude();
                longitude[clientId] = dataMsg.getLongitude();
                waterwayId[clientId] = dataMsg.getWaterwayId();
                rectangleId[clientId] = dataMsg.getRectangleId();
                depth[clientId] = dataMsg.getDepth();
                mineDepth[clientId] = Math.Round(dataMsg.getHigh() - height1 - (dataMsg.getDepth())*measureCoefficient - height2,2);
                ori[clientId] = dataMsg.getNavigation();
                rolling[clientId] = dataMsg.getRolling();
                dataRefreshUTCTime[clientId] = dataMsg.getDataTime();
                switch (dataMsg.getGpsState())
                {
                    case 0:
                        GPSState[clientId] = "初始化";
                        break;
                    case 1:
                        GPSState[clientId] = "单点定位";
                        break;
                    case 2:
                        GPSState[clientId] = "码差分";
                        break;
                    case 3:
                        GPSState[clientId] = "无效PPS";
                        break;
                    case 4:
                        GPSState[clientId] = "固定解";
                        break;
                    case 5:
                        GPSState[clientId] = "浮点解";
                        break;
                    case 6:
                        GPSState[clientId] = "正在估算";
                        break;
                    case 7:
                        GPSState[clientId] = "人工输入固定值";
                        break;
                    case 8:
                        GPSState[clientId] = "模拟模式";
                        break;
                    case 9:
                        GPSState[clientId] = "WAAS差分";
                        break;
                }
            }catch  (Exception e)
            {
            }
        }
        
    }
}
