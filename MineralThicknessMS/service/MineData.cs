using MineralThicknessMS.config;
using MineralThicknessMS.entity;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using Org.BouncyCastle.Math.EC.Multiplier;
using GMap.NET;
using System.Reflection.Metadata.Ecma335;
using OfficeOpenXml.Drawing.Chart;
using System.Drawing.Imaging;
using System.Reflection;
using OfficeOpenXml.Style;
using System.Threading.Channels;

namespace MineralThicknessMS.service
{
    public class DataAnalysis
    {
        public static List<Produce> motorProduce = new List<Produce>();
        public static List<Produce> radarProduce = new List<Produce>();

        //按时间段查询到的数据
        public static List<Produce> subMine = new List<Produce>();
        public static double totalSubMine = 0;

        //按时间查询到的数据
        public static List<Produce> dayTotalMine = new List<Produce>();
        public static List<Produce> latestData = new List<Produce>();
        public static double dayMine = 0;

        //更新每个网格实时信息 => mproduce表
        public static void updateMineAvg()
        {
            //找出test表中今天的所有数据
            string sqlStrDayTotalData = "SELECT * FROM test WHERE DATE(data_time) = CURDATE();";
            DataSet ds = MySQLHelper.ExecSqlQuery(sqlStrDayTotalData);

            var originalDataTable = ds.Tables[0];// 原始的 DataTable 数据

            var groupedDataTables = new Dictionary<string, System.Data.DataTable>();

            //将上面找出的数据以相同的航道号网格号分DateTable
            foreach (DataRow row in originalDataTable.Rows)
            {
                var key = row["waterway_id"].ToString() + "_" + row["rectangle_id"].ToString();

                if (!groupedDataTables.ContainsKey(key))
                {
                    var newDataTable = originalDataTable.Clone();
                    groupedDataTables.Add(key, newDataTable);
                }
                groupedDataTables[key].Rows.Add(row.ItemArray);
            }

            //遍历每一个DataTable
            for (int i = 0; i < groupedDataTables.Count; i++)
            {
                try
                {
                    List<DataMsg> data = new List<DataMsg>();
                    //遍历每个DataTable内的记录，将其添加到对应集合中
                    foreach (DataRow row in groupedDataTables.ElementAt(i).Value.Rows)
                    {
                        DataMsg dataMsg = new DataMsg();
                        dataMsg.setDataTime((DateTime)row["data_time"]);
                        dataMsg.setLatitude((double)row["latitude"]);
                        dataMsg.setLongitude((double)row["longitude"]);

                        dataMsg.setMineHigh((double)row["mine_high"]);

                        dataMsg.setWaterwayId((int)row["waterway_id"]);
                        dataMsg.setRectangleId((int)row["rectangle_id"]);
                        data.Add(dataMsg);
                    }

                    double avgSub = 0, avgAdd = 0, avgMinGroup = 0;

                    try
                    {
                        //将每一个集合使用KMeans分类器分成两个集合
                        List<List<DataMsg>> clusters = KMeansClustering.KMeansCluster(data, 2, 100);

                        //分别求两个集合的总和
                        double clusterSum0 = 0, clusterSum1 = 0;
                        foreach (DataMsg dataMsg in clusters[0])
                        {
                            clusterSum0 += dataMsg.getMineHigh();
                        }
                        foreach (DataMsg dataMsg in clusters[1])
                        {
                            clusterSum1 += dataMsg.getMineHigh();
                        }

                        //采矿量
                        double avg = clusterSum1 / clusters[1].Count - clusterSum0 / clusters[0].Count;
                        avgSub = avg > 0 ? avg : -avg;
                        avgMinGroup = clusterSum1 / clusters[1].Count < clusterSum0 / clusters[0].Count ? clusterSum1 / clusters[1].Count : clusterSum0 / clusters[0].Count;      

                        //矿量储量
                        avgAdd = (clusterSum1 + clusterSum0) / (clusters[1].Count + clusters[0].Count);
                    }
                    catch (Exception e)
                    {
                        avgAdd = data[0].getMineHigh();
                    }

                    //在mproduce表中查看今天有无该网格信息
                    string sqlStrIsToday = "SELECT count(*) from mproduce WHERE waterway_id = @waterwayId and rectangle_id = @rectangleId and " +
                        "DATE(date_time) = CURDATE()";
                    MySqlParameter[] paramIsToday = new MySqlParameter[] {
                        new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                        new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                    };

                    DataSet dsIsToday = MySQLHelper.ExecSqlQuery(sqlStrIsToday, paramIsToday);
                    //sum代表今天开采有无该网格信息(mproduce)
                    int sum = Convert.ToInt32(dsIsToday.Tables[0].Rows[0][0]);

                    //mproduce今天没有该网格信息，且今天有采前采后数据(avgSub >= 0.3)，则添加较小组的平均值(采后的)
                    if (sum == 0 && avgSub >= 0.3)
                    {
                        string sqlStrInsertProduceToday = "insert into mproduce(date_time,avg_mine_depth,waterway_id,rectangle_id) " +
                        "values(NOW(),@avgMineDepth,@waterwayId,@rectangleId)";

                        MySqlParameter[] paramInsertProduceToday = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineDepth",avgMinGroup),
                            new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                        };
                        MySQLHelper.ExecSql(sqlStrInsertProduceToday, paramInsertProduceToday);
                    }
                    //mproduce今天有该网格信息，且今天有采前采后数据(avgSub >= 0.3),则继续使用较小组的平均值更新
                    else if (sum > 0 && avgSub >= 0.3)
                    {
                        string sqlStrUpdateProduceToday = "update mproduce set avg_mine_depth = @avgMineDepth " +
                            "where waterway_id = @waterwayId and rectangle_id = @rectangleId and Date(date_time)=CURDATE()";
                        MySqlParameter[] paramUpdateProduceToday = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineDepth",avgMinGroup),
                            new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                        };
                        MySQLHelper.ExecSqlQuery(sqlStrUpdateProduceToday, paramUpdateProduceToday);
                    }
                    //mproduce今天没有该网格信息，采前采后数据今天不同时存在，直接添加平均值代表矿厚
                    else if (sum == 0 && avgSub < 0.3)
                    {
                        string sqlStrInsertProduceToday = "insert into mproduce(date_time,avg_mine_depth,waterway_id,rectangle_id) " +
                        "values(NOW(),@avgMineDepth,@waterwayId,@rectangleId)";

                        MySqlParameter[] paramInsertProduceToday = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineDepth",avgAdd),
                            new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                        };
                        MySQLHelper.ExecSql(sqlStrInsertProduceToday, paramInsertProduceToday);
                    }
                    //mproduce今天有该网格信息，采前采后数据今天不同时存在(avgSub < 0.3),则继续使用平均值更新代表矿厚
                    else if (sum > 0 && avgSub < 0.3)
                    {
                        string sqlStrUpdateToday = "update mproduce set avg_mine_depth = @avgMineDepth " +
                        "where waterway_id = @waterwayId and rectangle_id = @rectangleId and Date(date_time)=CURDATE()";
                        MySqlParameter[] paramUpdateToday = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineDepth",avgAdd),
                            new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                        };
                        MySQLHelper.ExecSql(sqlStrUpdateToday, paramUpdateToday);
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        //输入雷达asc文件路径，提取平面坐标加高程数据,depth判断数据是否有误
        public static List<LaterPoint> ReadRadarAsciiFile(string filePath,double depth)
        {
            List<LaterPoint> points = new List<LaterPoint>();

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] fields = line.Split(',');


                        if (fields.Length >= 3)
                        {
                            if (double.Parse(fields[2]) >= depth)
                            {
                                double x = double.Parse(fields[1]);
                                double y = double.Parse(fields[0]);
                                double elevation = double.Parse(fields[2]);

                                LaterPoint point = new LaterPoint(x, y, elevation);
                                points.Add(point);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            return points;
        }

        //输入外部asc文件路径，提取平面坐标加高程数据,depth判断数据是否有误
        public static List<LaterPoint> ReadOuterAsciiFile(string filePath, double depth)
        {
            List<LaterPoint> points = new List<LaterPoint>();

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] fields = line.Split(',');

                        if (fields.Length >= 3)
                        {
                            if (double.Parse(fields[4]) >= depth)
                            {
                                double x = double.Parse(fields[3]);
                                double y = double.Parse(fields[2]);
                                double elevation = double.Parse(fields[4]);

                                LaterPoint point = new LaterPoint(x, y, elevation);
                                points.Add(point);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            return points;
        }

        //输入摩托艇asc文件路径，提取平面坐标加高程数据,depth判断数据是否有误
        public static List<LaterPoint> ReadMotorAsciiFile(string filePath, double depth)
        {
            List<LaterPoint> points = new List<LaterPoint>();

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] fields = line.Split(',');

                        if (fields.Length >= 3)
                        {
                            if (double.Parse(fields[4]) >= depth)
                            {
                                double x = double.Parse(fields[3]);
                                double y = double.Parse(fields[2]);
                                double elevation = double.Parse(fields[4]);

                                LaterPoint point = new LaterPoint(x, y, elevation);
                                points.Add(point);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            return points;
        }

        //将雷达/摩托艇原始数据转换为有效数据，航道-网格-平均高程
        public static List<Produce> produceList(List<LaterPoint> points)
        {
            List<List<Grid>> grids = GridView.gridBuild(BoundaryPoints.boundaryPointsList());
            Dictionary<(int, int), List<double>> averages = new Dictionary<(int, int), List<double>>();
            foreach (LaterPoint point in points)
            {
                double i = point.X;
                double j = point.Y;

                Grid grid1 = new Grid();
                PointXY xy = new PointXY(point.X, point.Y);
                PointLatLng latlng = GridView.pointXYToBL(xy);
                grid1 = GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), grids, latlng);


                if (grid1.Id == 0)
                {
                    continue;
                }

                int channel = grid1.Column;
                int grid = grid1.Row;


                if (!averages.ContainsKey((channel, grid)))
                {
                    averages[(channel, grid)] = new List<double>();
                }

                averages[(channel, grid)].Add(point.Elevation);
            }
            List<Produce> produce = new List<Produce>();

            foreach (var item in averages)
            {
                double averageElevation = item.Value.Count > 0 ? item.Value.Average() : 0;

                Produce produceItem = new Produce
                {
                    Channel = item.Key.Item1,
                    Grid = item.Key.Item2,
                    AverageElevation = averageElevation
                };

                produce.Add(produceItem);
            }
            return produce;
        }

        //将两个集合中航道号和网格编号相同的记录合并，高程取平均值
        public static List<Produce> MergeAndCalculateAverage(List<Produce> list1, List<Produce> list2)
        {
            var mergedList = list1.Concat(list2) // 合并两个集合
                .GroupBy(p => new { p.Channel, p.Grid }) // 根据 Channel 和 Grid 分组
                .Select(g => new Produce
                {
                    Channel = g.Key.Channel,
                    Grid = g.Key.Grid,
                    AverageElevation = g.Average(p => p.AverageElevation) // 计算平均值
                })
                .ToList();

            return mergedList;
        }

        //将雷达高程减去底板高度
        public static List<Produce> SubtractX(List<Produce> list, double value)
        {
            List<Produce> modifiedList = new List<Produce>();

            foreach (Produce item in list)
            {
                item.AverageElevation -= value;
                modifiedList.Add(item);
            }

            return modifiedList;
        }

        //事务分配插入，快，准
        public static void dataInsert(List<Produce> produces)
        {
            // 将时间为今天的记录删除（防止重复导入数据mproduce）
            string sqlStrCleanMproduce = "DELETE FROM mproduce WHERE DATE(date_time) = CURDATE()";
            MySQLHelper.ExecSql(sqlStrCleanMproduce);

            // 将雷达数据插入数据库
            string sqlStr = "INSERT INTO mproduce (date_time, avg_mine_depth, waterway_id, rectangle_id) VALUES (@dateTime, @avgMine, @waterWayId, @rectangleId)";

            int batchSize = 1000; // 调整批次大小
            int totalRecords = produces.Count;
            int batchCount = (int)Math.Ceiling((double)totalRecords / batchSize);

            using (var connection = new MySqlConnection(MySQLHelper.connStr))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new MySqlCommand(sqlStr, connection))
                        {
                            command.Parameters.Add("@dateTime", MySqlDbType.DateTime);
                            command.Parameters.Add("@avgMine", MySqlDbType.Double);
                            command.Parameters.Add("@waterWayId", MySqlDbType.Int32);
                            command.Parameters.Add("@rectangleId", MySqlDbType.Int32);

                            for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
                            {
                                int startIndex = batchIndex * batchSize;
                                int endIndex = Math.Min(startIndex + batchSize, totalRecords);

                                for (int index = startIndex; index < endIndex; index++)
                                {
                                    Produce produce = produces[index];

                                    command.Parameters["@dateTime"].Value = DateTime.Now.AddDays(-1);
                                    command.Parameters["@avgMine"].Value = produce.AverageElevation;
                                    command.Parameters["@waterWayId"].Value = produce.Channel;
                                    command.Parameters["@rectangleId"].Value = produce.Grid;

                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw; // 可根据需要处理事务提交失败的情况
                    }
                }
            }
        }

        //根据时间选择最新的数据(航道编号-网格编号-矿厚-xx)
        public static List<Produce> GetLatestProduceData(DateTime dateTime)
        {
            List<Produce> produces = new List<Produce>();

            using (var conn = new MySqlConnection(MySQLHelper.connStr))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "SELECT date_time, avg_mine_depth FROM mproduce WHERE date_time < @dateTime " +
                        "AND waterway_id = @waterWayId AND rectangle_id = @rectangleId ORDER BY date_time desc LIMIT 1";

                    for (int waterWayId = 0; waterWayId < GridView.channelId; waterWayId++)
                    {
                        for (int rectangleId = 0; rectangleId < GridView.gridId; rectangleId++)
                        {
                            command.Parameters.Clear(); // 清除已定义的参数

                            command.Parameters.AddWithValue("@dateTime", dateTime);
                            command.Parameters.AddWithValue("@waterWayId", waterWayId);
                            command.Parameters.AddWithValue("@rectangleId", rectangleId);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    object averageElevationObj = reader[1];
                                    double averageElevation = averageElevationObj is DBNull ? 0 : Convert.ToDouble(averageElevationObj);

                                    object latestTimeObj = reader[0];
                                    DateTime latestTime = latestTimeObj is DBNull ? dateTime : Convert.ToDateTime(latestTimeObj);

                                    Produce produce = new Produce
                                    {
                                        Channel = waterWayId,
                                        Grid = rectangleId,
                                        AverageElevation = averageElevation,
                                        time = latestTime,
                                    };
                                    produces.Add(produce);
                                }
                                else
                                {
                                    Produce produce = new Produce
                                    {
                                        Channel = waterWayId,
                                        Grid = rectangleId,
                                        AverageElevation = 0,
                                        time = dateTime,
                                    };
                                    produces.Add(produce);
                                }
                            }
                        }
                    }
                }
            }

            return produces;
        }

        //使用list2的矿厚减list1的矿厚(航道编号-网格编号-矿厚差-xx)
        public static List<Produce> MergeAndCalculateDifference(List<Produce> list1, List<Produce> list2)
        {
            List<Produce> mergedList = new List<Produce>();

            var produceDict = list2.ToDictionary(p => new { p.Channel, p.Grid });

            foreach (var produce in list1)
            {
                var key = new { produce.Channel, produce.Grid };
                if (produceDict.ContainsKey(key))
                {
                    double difference = produceDict[key].AverageElevation - produce.AverageElevation;
                    Produce mergedProduce = new Produce
                    {
                        Channel = produce.Channel,
                        Grid = produce.Grid,
                        AverageElevation = difference,
                        time = produce.time
                    };
                    mergedList.Add(mergedProduce);
                }
            }

            return mergedList;
        }

        //将航道号相同的网格的矿厚相加，结果(航道编号-xx-总矿厚-xx)
        public static List<Produce> AggregateAverageElevation(List<Produce> produces)
        {
            List<Produce> aggregatedProduces = new List<Produce>();

            var groupedProduces = produces.GroupBy(p => p.Channel);

            foreach (var group in groupedProduces)
            {
                int channel = group.Key;
                double totalAverageElevation = group.Sum(p => p.AverageElevation);

                Produce aggregatedProduce = new Produce
                {
                    Channel = channel,
                    AverageElevation = totalAverageElevation
                };

                aggregatedProduces.Add(aggregatedProduce);
            }

            return aggregatedProduces;
        }

        //将矿厚转换成体积，结果(航道编号-xx-总体积-xx)
        public static List<Produce> MultiplyAverageElevation(List<Produce> produces, double area)
        {
            List<Produce> multipliedProduces = produces.Select(p => new Produce
            {
                Channel = p.Channel,
                Grid = p.Grid,
                AverageElevation = p.AverageElevation * area,
                time = p.time
            }).ToList();

            return multipliedProduces;
        }

        //将每条记录AverageElevation求和
        public static double SumAverageElevation(List<Produce> produces)
        {
            double sum = produces.Sum(p => p.AverageElevation);
            return sum;
        }


        //以航道号，采矿量，
        public static List<Produce> mineTable(DateTime dateTimeBegin, DateTime dateTimeEnd)
        {
            dateTimeBegin = new DateTime(dateTimeBegin.Year,dateTimeBegin.Month,dateTimeBegin.Day,00,00,00);
            dateTimeEnd = new DateTime(dateTimeEnd.AddDays(1).Year, dateTimeEnd.AddDays(1).Month, dateTimeEnd.AddDays(1).Day,00,00,00);

            List<Produce> producesBegin = new List<Produce>();
            List<Produce> producesEnd = new List<Produce>();

            //分别找出两个时刻最新的数据
            producesBegin = GetLatestProduceData(dateTimeBegin);
            producesEnd = GetLatestProduceData(dateTimeEnd);

            //求出矿厚差(航道编号-网格编号-矿厚差-xx)
            List<Produce> subProduce = new List<Produce>();
            subProduce = MergeAndCalculateDifference(producesEnd, producesBegin);

            //将航道号相同的网格的矿厚相加，结果(航道编号-xx-总矿厚差-xx)
            subProduce = AggregateAverageElevation(subProduce);

            //将矿厚转换成体积，结果(航道编号-xx-总体积-xx)
            subProduce = MultiplyAverageElevation(subProduce,Status.s);

            return subProduce;
        }

        //以航道号，采矿量，
        public static List<Produce> dayMineTable(DateTime dayTime)
        {
            dayTime = new DateTime(dayTime.AddDays(1).Year, dayTime.AddDays(1).Month, dayTime.AddDays(1).Day, 00, 00, 00);

            latestData.Clear();

            latestData = GetLatestProduceData(dayTime);

            List<Produce> produce = new List<Produce>();

            //将航道号相同的网格的矿厚相加，结果(航道编号-xx-总矿厚差-xx)
            produce = AggregateAverageElevation(latestData);

            //将矿厚转换成体积，结果(航道编号-xx-总体积-xx)
            produce = MultiplyAverageElevation(produce, Status.s);

            return produce;

        }

        //时间段模板
        public static void ExportToExcel(List<Produce> produces,DateTime dateTimeBegin,DateTime dateTimeEnd,double sum)
        {
           /////获取模板文件的相对路径
           // string templateFilePath = @"..\..\..\Resource\template.xlsx";

            // // 获取模板文件的绝对路径
            // string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // string templateFullPath = Path.GetFullPath(Path.Combine(currentDirectory, templateFilePath));

            string templateFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "template.xlsx");

            // 验证模板文件是否存在
            if (!File.Exists(templateFullPath))
            {
                MessageBox.Show("模板文件不存在！");
                return;
            }
            // 设置EPPlus的LicenseContext为NonCommercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // 加载模板文件
            FileInfo templateFile = new FileInfo(templateFullPath);

            // 创建新的Excel包
            using (ExcelPackage excelPackage = new ExcelPackage(templateFile))
            {
                // 获取工作表
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["Sheet1"]; // 根据工作表的名称访问（示例中假设工作表名称为 "Sheet1"）

                worksheet.Cells[3, 3].Value = dateTimeBegin.ToShortDateString();
                worksheet.Cells[3, 6].Value = dateTimeEnd.ToShortDateString();
                worksheet.Cells[5, 4].Value = Status.saltBoundId;
                worksheet.Cells[5,6].Value = Math.Round(sum,2);

                // 从第9行开始写入数据
                int startRow = 9;

                // 设置起始序号
                int sequence = 1;

                // 使用DataTable的数据填充Excel表格
                for (int row = 0; row < produces.Count; row++)
                {
                    // 写入序号
                    worksheet.Cells[startRow + row, 1].Value = sequence++;

                    // 写入航道编号
                    worksheet.Cells[startRow + row, 2].Value = produces[row].Channel.ToString();

                    // 写入采矿量
                    worksheet.Cells[startRow + row, 3].Value = Math.Round(produces[row].AverageElevation,2).ToString();

                    // 写入结束时间
                    worksheet.Cells[startRow + row, 4].Value = dateTimeBegin.ToShortDateString();

                    // 写入开始时间
                    worksheet.Cells[startRow + row, 5].Value = dateTimeEnd.ToShortDateString();

                }
                

                // 获取数据范围
                ExcelRange dataRange = worksheet.Cells[startRow, 1, startRow + produces.Count - 1, 6];

                // 设置边框样式
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // 自动调整列宽以便更好地显示内容
                worksheet.Cells.AutoFitColumns();

                // 保存导出的Excel文件
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel 文件|*.xlsx";
                saveFileDialog.Title = "保存Excel文件";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    excelPackage.SaveAs(new FileInfo(saveFileDialog.FileName));
                    MessageBox.Show("Excel文件保存成功！");
                }
            }
        }

        //时间模板
        public static void ExportDayMineToExcel(List<Produce> produces,DateTime dateTime,double sum)
        {
            /////获取模板文件的相对路径
            //string templateFilePath = @"..\..\..\Resource\dayTemplate.xlsx";

            //// 获取模板文件的绝对路径
            //string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //string templateFullPath = Path.GetFullPath(Path.Combine(currentDirectory, templateFilePath));

            string templateFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dayTemplate.xlsx");

            // 验证模板文件是否存在
            if (!File.Exists(templateFullPath))
            {
                MessageBox.Show("模板文件不存在！");
                return;
            }
            // 设置EPPlus的LicenseContext为NonCommercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // 加载模板文件
            FileInfo templateFile = new FileInfo(templateFullPath);

            // 创建新的Excel包
            using (ExcelPackage excelPackage = new ExcelPackage(templateFile))
            {
                // 获取工作表
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["Sheet1"]; // 根据工作表的名称访问（示例中假设工作表名称为 "Sheet1"）

                worksheet.Cells[3, 4].Value = dateTime.ToShortDateString();

                worksheet.Cells[5, 4].Value = Status.saltBoundId;

                worksheet.Cells[5, 6].Value = Math.Round(sum, 2);

                // 从第9行开始写入数据
                int startRow = 9;

                // 设置起始序号
                int sequence = 1;

                // 使用DataTable的数据填充Excel表格
                for (int row = 0; row < produces.Count; row++)
                {
                    // 写入序号
                    worksheet.Cells[startRow + row, 1].Value = sequence++;

                    // 写入航道编号
                    worksheet.Cells[startRow + row, 2].Value = produces[row].Channel.ToString();

                    // 写入采矿量
                    worksheet.Cells[startRow + row, 3].Value = Math.Round(produces[row].AverageElevation,2).ToString();

                    // 写入结束时间
                    worksheet.Cells[startRow + row, 4].Value = dateTime.ToShortDateString();

                    // 合并第五列和第六列的单元格
                    worksheet.Cells[startRow + row, 5, startRow + row, 6].Merge = true;
                }

                // 获取数据范围
                ExcelRange dataRange = worksheet.Cells[startRow, 1, startRow + produces.Count - 1, 6];

                // 设置边框样式
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // 自动调整列宽以便更好地显示内容
                worksheet.Cells.AutoFitColumns();

                // 保存导出的Excel文件
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel 文件|*.xlsx";
                saveFileDialog.Title = "保存Excel文件";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    excelPackage.SaveAs(new FileInfo(saveFileDialog.FileName));
                    MessageBox.Show("Excel文件保存成功！");
                }
            }
        }
    }
    //原始数据提取结构
    public class LaterPoint
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Elevation { get; set; }

        public LaterPoint(double x, double y, double elevation)
        {
            X = x;
            Y = y;
            Elevation = elevation;
        }
    }
    public class Produce
    {
        public int Channel { get; set; }
        public int Grid { get; set; }
        public double AverageElevation { get; set; }

        public DateTime time { get; set; }
    }
}
