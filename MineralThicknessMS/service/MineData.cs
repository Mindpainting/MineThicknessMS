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
using Google.DataTable.Net.Wrapper;
using Org.BouncyCastle.Math.EC.Multiplier;
using GMap.NET;
using System.Reflection.Metadata.Ecma335;
using OfficeOpenXml.Drawing.Chart;
using System.Drawing.Imaging;
using System.Reflection;
using OfficeOpenXml.Style;

namespace MineralThicknessMS.service
{
    public class DataAnalysis
    {
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

                    double avgSub = 0, avgAdd;

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

                    //mproduce今天没有该网格信息，且今天有采前采后数据(avgSub >= 0.3)，则添加
                    //将差值设置为采矿量（avg_mine_produce）
                    if (sum == 0 && avgSub >= 0.3)
                    {
                        string sqlStrInsertProduceToday = "insert into mproduce(date_time,avg_mine_produce,waterway_id,rectangle_id,flag,update_time) " +
                        "values(NOW(),@avgMineProduce,@waterwayId,@rectangleId,1,NOW())";

                        MySqlParameter[] paramInsertProduceToday = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineProduce",avgSub),
                            new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                        };
                        MySQLHelper.ExecSql(sqlStrInsertProduceToday, paramInsertProduceToday);
                    }
                    //mproduce今天有该网格信息，且今天有采前采后数据(avgSub >= 0.3),则更新
                    else if (sum > 0 && avgSub >= 0.3)
                    {
                        string sqlStrUpdateProduceToday = "update mproduce set avg_mine_produce = @avgMineProduce,flag = 1,update_time = NOW() " +
                            "where waterway_id = @waterwayId and rectangle_id = @rectangleId";
                        MySqlParameter[] paramUpdateProduceToday = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineProduce",avgSub),
                            new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                        };
                        MySQLHelper.ExecSqlQuery(sqlStrUpdateProduceToday, paramUpdateProduceToday);
                    }
                    //
                    //mproduce今天没有该网格信息，采前采后数据今天不同时存在(avgSub < 0.3)
                    else if (sum == 0 && avgSub < 0.3)
                    {
                        //找历史中是否有该网格的数据
                        string sqlStrSelectHisData = "SELECT count(*) from mproduce WHERE waterway_id = @waterwayId " +
                            "and rectangle_id = @rectangleId";
                        MySqlParameter[] paramSelectHisData = new MySqlParameter[]
                        {
                            new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                        };

                        DataSet ds3 = MySQLHelper.ExecSqlQuery(sqlStrSelectHisData, paramSelectHisData);

                        int count = Convert.ToInt32(ds3.Tables[0].Rows[0][0]);

                        //代表没有该网格的历史记录，将数据记录到今天
                        if (count == 0)
                        {
                            string sqlStr3 = "insert into mproduce(date_time,avg_mine_reserve,waterway_id,rectangle_id,flag,update_time) " +
                            "values(NOW(),@avgMineReserves,@waterwayId,@rectangleId,0,NOW())";
                            MySqlParameter[] param1 = new MySqlParameter[]
                            {
                                new MySqlParameter("@avgMineReserves",avgAdd),
                                new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                                new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                            };
                            MySQLHelper.ExecSqlQuery(sqlStr3, param1);
                        }
                        //代表有该网格的历史记录
                        else if (count == 1)
                        {
                            //看历史记录中的flag
                            string sqlStrFlagUpdateTime = "SELECT flag,Date(update_time),CURDATE(),avg_mine_reserve FROM mproduce WHERE waterway_id = @waterwayId " +
                                "and rectangle_id = @rectangleId ORDER BY date_time DESC LIMIT 1";
                            MySqlParameter[] paramFlagUpdateTime = new MySqlParameter[]
                            {
                                new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                                new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                            };
                            DataSet dsFlagUpdateTime = MySQLHelper.ExecSqlQuery(sqlStrFlagUpdateTime, paramFlagUpdateTime);

                            int flag = (int)dsFlagUpdateTime.Tables[0].Rows[0][0];
                            bool isUpdateToday = (DateTime)dsFlagUpdateTime.Tables[0].Rows[0][1] == (DateTime)dsFlagUpdateTime.Tables[0].Rows[0][2];
                            double avgMineReserve = (double)dsFlagUpdateTime.Tables[0].Rows[0][3];

                            //flag不是今天修改的，将数据记录到今天
                            if (flag == 1 && isUpdateToday == false)
                            {
                                string sqlStrUpdateHisData = "insert into mproduce(date_time,avg_mine_reserve,waterway_id,rectangle_id,flag,update_time) " +
                                "values(NOW(),@avgMineReserves,@waterwayId,@rectangleId,0,NOW())";
                                MySqlParameter[] paramUpdateHisData = new MySqlParameter[]
                                {
                                new MySqlParameter("@avgMineReserves",avgAdd),
                                new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                                new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                                };
                                MySQLHelper.ExecSqlQuery(sqlStrUpdateHisData, paramUpdateHisData);
                            }

                            //flag为0(之前没有采后的数据)，或采后的数据是今天更新的，继续更新
                            if (flag == 0 || isUpdateToday == true)
                            {
                                double avgMineProduce = avgMineReserve - avgAdd;

                                string sqlStrReUpdateHisData = "update mproduce set avg_mine_produce = @avgMineProduce, flag = 1,update_time = NOW() where waterway_id = " +
                                    "@waterwayId and rectangle_id = @rectangleId ";
                                MySqlParameter[] paramReUpdateHisData = new MySqlParameter[]
                                {
                                    new MySqlParameter("@avgMineProduce",avgMineProduce),
                                    new MySqlParameter("@waterwayId",data[0].getWaterwayId()),
                                    new MySqlParameter("@rectangleId",data[0].getRectangleId()),
                                };
                                MySQLHelper.ExecSql(sqlStrReUpdateHisData, paramReUpdateHisData);
                            }
                        }
                    }
                    //mproduce今天有该网格信息，采前采后数据今天不同时存在(avgSub < 0.3),则更新
                    else if (sum > 0 && avgSub < 0.3)
                    {
                        string sqlStrUpdateToday = "update mproduce set avg_mine_reserve = @avgMineReserve,date_time = NOW(),flag = 0,update_time = NOW() " +
                        "where waterway_id = @waterwayId and rectangle_id = @rectangleId";
                        MySqlParameter[] paramUpdateToday = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineReserve",avgAdd),
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

        //以航道号，采矿量，结束时间，开始时间生成DataTable
        public static System.Data.DataTable mineTable(DateTime dateTimeBegin,DateTime dateTimeEnd)
        {
            try
            {
                dateTimeBegin = new DateTime(dateTimeBegin.Year, dateTimeBegin.Month, dateTimeBegin.Day);

                dateTimeEnd = new DateTime(dateTimeEnd.Year, dateTimeEnd.Month, dateTimeEnd.Day);
                dateTimeEnd = dateTimeEnd.AddDays(1);
                //mproduce表中相同格子有重复不影响
                string sqlStr1 = "SELECT waterway_id," +
                    "SUM(CASE WHEN flag = 1 THEN avg_mine_produce ELSE 0 END) + SUM(CASE WHEN flag = 0 THEN avg_mine_reserve ELSE 0 END) AS total_sum," +
                    "MAX(date_time) AS max_e,MIN(date_time) AS min_e " +
                    "FROM mproduce WHERE date_time >= @dateTimeBegin AND date_time <= @dateTimeEnd GROUP BY waterway_id";
                MySqlParameter[] param1 = new MySqlParameter[]
                {
                    new MySqlParameter("@dateTimeBegin",dateTimeBegin),
                    new MySqlParameter("@dateTimeEnd",dateTimeEnd),
                };

                DataSet dataSet = MySQLHelper.ExecSqlQuery(sqlStr1, param1);

                //将取出的采矿深度和更新为采矿量
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    double originalValue = Convert.ToDouble(row["total_sum"]);
                    //将矿厚乘底面积
                    double multipliedValue = originalValue * Status.s;
                    row["total_sum"] = multipliedValue;
                }

                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        ////以航道号，采矿量，结束时间，开始时间生成DataTable
        //public static System.Data.DataTable mineTable()
        //{
        //    try
        //    {
        //        //3.
        //        //找出本次开采开始时间
        //        string sqlStr = "SELECT date_time FROM mproduce WHERE waterway_id=0 and rectangle_id=0 " +
        //            "ORDER BY date_time DESC LIMIT 1;";
        //        DataSet ds = MySQLHelper.ExecSqlQuery(sqlStr);

        //        DateTime dt = (DateTime)ds.Tables[0].Rows[0][0];

        //        //mproduce表中相同格子有重复不影响
        //        string sqlStr1 = "SELECT waterway_id," +
        //            "SUM(CASE WHEN flag = 1 THEN avg_mine_produce ELSE 0 END) + SUM(CASE WHEN flag = 0 THEN avg_mine_reserve ELSE 0 END) AS total_sum," +
        //            "MAX(date_time) AS max_e,MIN(date_time) AS min_e " +
        //            "FROM mproduce WHERE date_time >= @dateTime AND waterway_id > 0 GROUP BY waterway_id";
        //        MySqlParameter[] param1 = new MySqlParameter[]
        //        {
        //        new MySqlParameter("@dateTime",dt),
        //        };

        //        DataSet dataSet = MySQLHelper.ExecSqlQuery(sqlStr1, param1);

        //        //将取出的采矿深度和更新为采矿量
        //        foreach (DataRow row in dataSet.Tables[0].Rows)
        //        {
        //            double originalValue = Convert.ToDouble(row["total_sum"]);
        //            //将矿厚乘底面积
        //            double multipliedValue = originalValue * Status.s;
        //            row["total_sum"] = multipliedValue;
        //        }

        //        return dataSet.Tables[0];
        //    }catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        //输入雷达asc文件路径，提取平面坐标加高程数据
        public static List<LaterPoint> ReadRadarAsciiFile(string filePath)
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
                            if (double.Parse(fields[2]) >= Status.height2)
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

        //将雷达,摩托艇原始数据转换为有效数据，航道-网格-平均高程
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

                int channel = grid1.Column;
                int grid = grid1.Row;

                if (grid1.Id == 0)
                {
                    continue;
                }

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

        //雷达数据插入数据库
        public static void radarDataInsert(List<Produce> radarProduce)
        {
            //将时间为今天的记录删除(防止重复导入数据test,mproduce)
            string sqlStrCleanTest = "delete from test where date_time >= CURDATE()";
            MySQLHelper.ExecSql(sqlStrCleanTest);

            string sqlStrCleanMproduce = "delete from mproduce where date_time >= CURDATE()";
            MySQLHelper.ExecSql(sqlStrCleanMproduce);

            //将雷达数据插入数据库
            string sqlStr = "insert into mproduce(date_time,avg_mine_reserve,waterway_id,rectangle_id,flag,update_time) " +
                "values(NOW(),@avgMine,@waterWayId,@RectanglewayId,0,NOW())";
            foreach (Produce radarData in radarProduce)
            {
                MySqlParameter[] param = new MySqlParameter[]
                {
                    new MySqlParameter("@avgMine",radarData.AverageElevation),
                    new MySqlParameter("@waterWayId",radarData.Channel),
                    new MySqlParameter("@RectanglewayId",radarData.Grid),
                };
                MySQLHelper.ExecSql(sqlStr, param);
            }
        }

        //模板
        public static void ExportToExcel(System.Data.DataTable dataTable,DateTime dateTimeBegin,DateTime dateTimeEnd)
        {
           ///获取模板文件的相对路径
            string templateFilePath = @"..\..\..\Resource\template.xlsx";

            // 获取模板文件的绝对路径
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string templateFullPath = Path.GetFullPath(Path.Combine(currentDirectory, templateFilePath));

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

                double sum = 0;
                foreach (DataRow row in dataTable.Rows)
                {
                    double x = MsgDecode.StrConvertToDou(row[1]);
                    sum += x;
                }

                worksheet.Cells[5,6].Value = sum;

                // 从第9行开始写入数据
                int startRow = 9;

                // 设置起始序号
                int sequence = 1;

                // 使用DataTable的数据填充Excel表格
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    // 写入序号
                    worksheet.Cells[startRow + row, 1].Value = sequence++;

                    // 写入航道编号
                    worksheet.Cells[startRow + row, 2].Value = dataTable.Rows[row][0].ToString();

                    // 写入采矿量
                    worksheet.Cells[startRow + row, 3].Value = dataTable.Rows[row][1].ToString();

                    // 写入结束时间
                    worksheet.Cells[startRow + row, 4].Value = dataTable.Rows[row][2].ToString();

                    // 写入开始时间
                    worksheet.Cells[startRow + row, 5].Value = dataTable.Rows[row][3].ToString();
                }

                // 获取数据范围
                ExcelRange dataRange = worksheet.Cells[startRow, 1, startRow + dataTable.Rows.Count - 1, 6];

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

        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //导出excel有图
        //public static void ExportToExcel(System.Data.DataTable dataTable)
        //{
        //    try
        //    {
        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        //        {
        //            saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
        //            saveFileDialog.DefaultExt = "xlsx";

        //            if (saveFileDialog.ShowDialog() == DialogResult.OK)
        //            {
        //                string filePath = saveFileDialog.FileName;

        //                using (ExcelPackage package = new ExcelPackage())
        //                {
        //                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

        //                    // 设置列名
        //                    worksheet.Cells[1, 1].Value = "航道编号";
        //                    worksheet.Cells[1, 2].Value = "采矿量(m³)";
        //                    worksheet.Cells[1, 3].Value = "结束时间";
        //                    worksheet.Cells[1, 4].Value = "开始时间";

        //                    double sum = 0;
        //                    foreach (DataRow row in dataTable.Rows)
        //                    {
        //                        double x = MsgDecode.StrConvertToDou(row[1]);
        //                        sum += x;
        //                    }

        //                    // 将 DataTable 数据写入工作表
        //                    for (int row = 0; row < dataTable.Rows.Count; row++)
        //                    {
        //                        for (int col = 0; col < dataTable.Columns.Count; col++)
        //                        {
        //                            // 检查是否为日期列
        //                            if (dataTable.Columns[col].DataType == typeof(DateTime))
        //                            {
        //                                DateTime dateTimeValue = (DateTime)dataTable.Rows[row][col];
        //                                worksheet.Cells[row + 2, col + 1].Value = dateTimeValue.ToString("yyyy-MM-dd hh:MM:ss");
        //                            }
        //                            else
        //                            {
        //                                worksheet.Cells[row + 2, col + 1].Value = dataTable.Rows[row][col];
        //                            }
        //                        }
        //                    }
        //                    worksheet.Cells[dataTable.Rows.Count + 2, 1].Value = "总量(m³)";
        //                    worksheet.Cells[dataTable.Rows.Count + 2, 2].Value = sum.ToString();
        //                    // 添加柱状统计图
        //                    ExcelChart chart = (ExcelChart)worksheet.Drawings.AddChart("Chart", eChartType.ColumnClustered);
        //                    chart.SetSize(1500, 750);
        //                    chart.SetPosition(4, 0, 6, 0);
        //                    chart.Title.Text = "盐池采矿量分析(航道编号-采矿量)";
        //                    chart.XAxis.Title.Text = "航道编号";
        //                    chart.YAxis.Title.Text = "采矿量(单位:m³)";

        //                    // 设置统计图的数据范围
        //                    ExcelRange dataRange = worksheet.Cells[2, 2, dataTable.Rows.Count + 1, 2];
        //                    ExcelChartSerie series = chart.Series.Add(dataRange, worksheet.Cells[2, 1, dataTable.Rows.Count + 1, 1]);
        //                    series.Header = worksheet.Cells[1, 2].Value.ToString();


        //                    // 保存 Excel 文件
        //                    FileInfo file = new FileInfo(filePath);
        //                    package.SaveAs(file);

        //                    MessageBox.Show("文件导出成功！");
        //                }
        //            }
        //        }
        //    }catch(Exception e)
        //    {
        //        MessageBox.Show("文件导出失败！");
        //    }
        //}

        //无图
        ////ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        ////导出excel
        //public static void ExportToExcel(System.Data.DataTable dataTable)
        //{
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        //    {
        //        saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
        //        saveFileDialog.DefaultExt = "xlsx";

        //        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        //        {
        //            string filePath = saveFileDialog.FileName;

        //            using (ExcelPackage package = new ExcelPackage())
        //            {
        //                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

        //                // 设置列名
        //                worksheet.Cells[1, 1].Value = "航道编号";
        //                worksheet.Cells[1, 2].Value = "采矿量";
        //                worksheet.Cells[1, 3].Value = "结束时间";
        //                worksheet.Cells[1, 4].Value = "开始时间";

        //                // 将 DataTable 数据写入工作表
        //                for (int row = 0; row < dataTable.Rows.Count; row++)
        //                {
        //                    for (int col = 0; col < dataTable.Columns.Count; col++)
        //                    {
        //                        // 检查是否为日期列
        //                        if (dataTable.Columns[col].DataType == typeof(DateTime))
        //                        {
        //                            DateTime dateTimeValue = (DateTime)dataTable.Rows[row][col];
        //                            worksheet.Cells[row + 2, col + 1].Value = dateTimeValue.ToString("yyyy-MM-dd");
        //                        }
        //                        else
        //                        {
        //                            worksheet.Cells[row + 2, col + 1].Value = dataTable.Rows[row][col];
        //                        }
        //                    }
        //                }

        //                // 保存 Excel 文件
        //                FileInfo file = new FileInfo(filePath);
        //                package.SaveAs(file);

        //                MessageBox.Show("文件导出成功！");
        //            }
        //        }
        //    }
        //}
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
    }
}
