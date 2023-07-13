using MineralThicknessMS.entity;
using MineralThicknessMS.service;
using GMap.NET;

namespace MineralThicknessMS.config
{
    public class RWIniFile
    {
        //获取init.ini路径
/*        private static string baseDirectory = Directory.GetCurrentDirectory();
        private static string rootPath = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\"));
        private static string filePath = Path.Combine(rootPath, @"config\init.ini");*/

        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "init.ini");

        private static readonly Dictionary<string, string> initData = new();

        // 写入初始化数据
        public static void WriteIniFile()
        {
            string[] lines = new string[]
            {
            "# 盐池编号",
            "SaltBoundId=Y31",
            "",
            "# 盐池四个角点",
            "LeftUpLat=40.2531571096",
            "LeftUpLng=90.4901935097",
            "LeftDownLat=40.2507185251",
            "LeftDownLng=90.4834874450",
            "RightDownLat=40.2425903988",
            "RightDownLng=90.4939599525",
            "RightUpLat=40.2450507356",
            "RightUpLng=90.5006782956",
            "",
            "# 盐池底板高度",
            "FloorHeight=766.32",
            "",
            "# 支架高度",
            "BracketHeight=1.5",
            "",
            "# 测深系数",
            "MeasureCoefficient=2.0"
                /* 
                            "LeftUpLat=40.2810614738",
                            "LeftUpLng=90.4917205106",
                            "LeftDownLat=40.2745895561",
                            "LeftDownLng=90.4850289188",
                            "RightDownLat=40.2647281553",
                            "RightDownLng=90.5022922085",
                            "RightUpLat=40.2712197832",
                            "RightUpLng=90.5048896228",
                */
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

            BoundaryPoints.LeftUp = new PointLatLng(Convert.ToDouble(initData["LeftUpLat"]), Convert.ToDouble(initData["LeftUpLng"]));
            BoundaryPoints.LeftDown = new PointLatLng(Convert.ToDouble(initData["LeftDownLat"]), Convert.ToDouble(initData["LeftDownLng"]));
            BoundaryPoints.RightDown = new PointLatLng(Convert.ToDouble(initData["RightDownLat"]), Convert.ToDouble(initData["RightDownLng"]));
            BoundaryPoints.RightUp = new PointLatLng(Convert.ToDouble(initData["RightUpLat"]), Convert.ToDouble(initData["RightUpLng"]));
        }
    }
}
