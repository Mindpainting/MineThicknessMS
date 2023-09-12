using MineralThicknessMS.entity;
using MineralThicknessMS.service;
using GMap.NET;
using System.IO.Ports;

namespace MineralThicknessMS.config
{
    public class RWIniFile
    {
/*        private static string baseDirectory = Directory.GetCurrentDirectory();
        private static string rootPath = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\"));
        private static string filePath = Path.Combine(rootPath, @"Release\net6.0-windows\init.ini");*/

        //C:\Users\Lenovo\Desktop\MainBranch\MineralThicknessMS\bin\Debug\net6.0-windows\init.ini
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "init.ini");

        private static readonly Dictionary<string, string> initData = new();

        // 写入初始化数据
        public static void WriteIniFile()
        {
            string[] lines = new string[]
            {
            "# 盐池编号",
            "SaltBoundId=Y45",
            "",
            "# 原Y45盐池四个角点",
            "LeftDownLat=40.27459093",
            "LeftDownLng=90.48501327",
            "LeftUpLat=40.28105912",
            "LeftUpLng=90.49171266",
            "RightUpLat=40.27120563",
            "RightUpLng=90.50493119",
            "RightDownLat=40.26472789",
            "RightDownLng=90.50224275",
            "",
            "# 盐池底板高度",
            "FloorHeight=722.933",
            "",
            "# 支架高度",
            "BracketHeight=2.31",
            "",
            "# 测深系数",
            "MeasureCoefficient=1.3225",
            "",
            "#两GPS至船中轴线距离",
            "GPSToCenterAxisDis=4.47",
            "",
            "# QGPS 中轴线点至前切割机距离",
            "QGPS_BeforeDis=7.4",
            "# QGPS 中轴线点至后切割机距离",
            "QGPS_AfterDis=40.2",
            "",
            "# HGPS 中轴线点至前切割机距离",
            "HGPS_BeforeDis=40.7",
            "# HGPS 中轴线点至后切割机距离",
            "HGPS_AfterDis=6.9"

            //"# y31盐池四个角点",
            //"LeftUpLat=40.2531571096",
            //"LeftUpLng=90.4901935097",
            //"LeftDownLat=40.2507185251",
            //"LeftDownLng=90.4834874450",
            //"RightDownLat=40.2425903988",
            //"RightDownLng=90.4939599525",
            //"RightUpLat=40.2450507356",
            //"RightUpLng=90.5006782956",
                
                             
            //"# 现Y45盐池四个角点",
            //"LeftUpLat=40.28105912",
            //"LeftUpLng=90.49171266",
            //"LeftDownLat=40.27459093",
            //"LeftDownLng=90.48501327",
            //"RightDownLat=40.26472789",
            //"RightDownLng=90.50224275",
            //"RightUpLat=40.27120563",
            //"RightUpLng=90.50493119",
            };
            File.WriteAllLines(filePath, lines);
        }

        // 根据key，返回一个value
        public static string ReadInitFileByKey(string key)
        {
            string value = "";
            // 读取文件内容
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // 忽略空行和注释行
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#"))
                            continue;

                        // 查找指定键的行
                        if (line.StartsWith(key + "="))
                        {
                            // 获取键值对的值部分
                            value = line.Substring(line.IndexOf("=") + 1).Trim();
                            break;
                        }
                    }
                }
            }
            return value;
        }

        /*        //读取初始化文件所有数据
                public static void GetAllInitData()
                {
                    using (StreamReader reader = new(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.Trim();

                            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            {
                                // 忽略空行和注释行
                                continue;
                            }

                            // 解析变量和值
                            int equalsIndex = line.IndexOf("=");
                            if (equalsIndex != -1)
                            {
                                string key = line.Substring(0, equalsIndex).Trim();
                                string value = line.Substring(equalsIndex + 1).Trim();
                                initData[key] = value;
                            }
                        }
                    }
                }*/

        // 把读取的数据赋给项目中的变量
        public static void InitData() 
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                WriteIniFile();
            }
            using (StreamReader reader = new(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    {
                        // 忽略空行和注释行
                        continue;
                    }

                    // 解析变量和值
                    int equalsIndex = line.IndexOf("=");
                    if (equalsIndex != -1)
                    {
                        string key = line.Substring(0, equalsIndex).Trim();
                        string value = line.Substring(equalsIndex + 1).Trim();
                        initData[key] = value;
                    }
                }
            }

            Status.saltBoundId = initData["SaltBoundId"];
            Status.height1 = Convert.ToDouble(initData["BracketHeight"]);
            Status.height2 = Convert.ToDouble(initData["FloorHeight"]);
            Status.measureCoefficient = Convert.ToDouble(initData["MeasureCoefficient"]);
            Status.GPSToCenterAxisDis = Convert.ToDouble(initData["GPSToCenterAxisDis"]);

            Status.QDisToCH[0] = Convert.ToDouble(initData["QGPS_BeforeDis"]);
            Status.QDisToCH[1] = Convert.ToDouble(initData["QGPS_AfterDis"]);
            Status.HDisToCH[0] = Convert.ToDouble(initData["HGPS_BeforeDis"]);
            Status.HDisToCH[1] = Convert.ToDouble(initData["HGPS_AfterDis"]);

            BoundaryPoints.LeftDown = new PointLatLng(Convert.ToDouble(initData["LeftDownLat"]), Convert.ToDouble(initData["LeftDownLng"]));
            BoundaryPoints.LeftUp = new PointLatLng(Convert.ToDouble(initData["LeftUpLat"]), Convert.ToDouble(initData["LeftUpLng"]));
            BoundaryPoints.RightUp = new PointLatLng(Convert.ToDouble(initData["RightUpLat"]), Convert.ToDouble(initData["RightUpLng"]));
            BoundaryPoints.RightDown = new PointLatLng(Convert.ToDouble(initData["RightDownLat"]), Convert.ToDouble(initData["RightDownLng"]));

/*            string[] str = initData["SerialPort"].Split(",");
            foreach (string s in str)
            {
                MySerialClient.serialPortIni.Add(new SerialPort(s));
            }*/
        }
    }
}
