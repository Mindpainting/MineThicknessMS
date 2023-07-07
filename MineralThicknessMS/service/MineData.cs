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

namespace MineralThicknessMS.service
{
    public class DataAnalysis
    {

        //更新每个网格实时信息 => mproduce表
        public static void updateMineAvg()
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            string sqlStr = "SELECT * FROM test WHERE data_time >= CURDATE() AND data_time <= NOW()";
            DataSet ds = MySQLHelper.ExecSqlQuery(sqlStr);

            var originalDataTable = ds.Tables[0];// 原始的 DataTable 数据

            var groupedDataTables = new Dictionary<string, System.Data.DataTable>();

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

            //for (int i = 0; i < groupedDataTables.Count; i++)
            //{
            //    foreach (DataRow row in groupedDataTables.ElementAt(i).Value.Rows)
            //    {
            //        foreach (DataColumn column in groupedDataTables.ElementAt(i).Value.Columns)
            //        {
            //            Console.Write(row[column] + " ");
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine("\r\n");
            //}
            //int count = 0;
            for (int i = 0; i < groupedDataTables.Count; i++)
            {
                try
                {
                    List<DataMsg> data = new List<DataMsg>();
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
                        //foreach (DataColumn column in groupedDataTables.ElementAt(i).Value.Columns)
                        //{
                        //    Console.Write(row[column] + " ");
                        //}
                        //Console.WriteLine();
                    }

                    List<List<DataMsg>> clusters = KMeansClustering.KMeansCluster(data, 2, 100);

                    //count++;

                    double clusterSum0 = 0, clusterSum1 = 0;
                    foreach (DataMsg dataMsg in clusters[0])
                    {
                        clusterSum0 += dataMsg.getMineHigh();
                    }
                    foreach (DataMsg dataMsg in clusters[1])
                    {
                        clusterSum1 += dataMsg.getMineHigh();
                    }
                    string sqlStr2 = "SELECT count(*) from mproduce WHERE waterway_id = @waterwayId and rectangle_id = @rectangleId and " +
                        "date_time >= CURDATE() AND date_time <= NOW()";
                    MySqlParameter[] param2 = new MySqlParameter[] {
                        new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                        new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                    };

                    DataSet ds1 = MySQLHelper.ExecSqlQuery(sqlStr2, param2);
                    //sum代表今天开采有无该网格信息
                    int sum = Convert.ToInt32(ds1.Tables[0].Rows[0][0]);

                    //采矿量
                    double avg = clusterSum1 / clusters[1].Count - clusterSum0 / clusters[0].Count;
                    double avgSub = avg > 0 ? avg : -avg;

                    //矿量储量
                    double avgAdd = (clusterSum1 + clusterSum0) / (clusters[1].Count + clusters[0].Count);

                    //今天没有该网格信息，且今天有采前采后数据(avgSub >= 0.3)，则添加
                    //将差值设置为采矿量（avg_mine_produce）
                    if (sum == 0 && avgSub >= 0.3)
                    {
                        string sqlStr1 = "insert into mproduce(date_time,avg_mine_produce,waterway_id,rectangle_id,flag) " +
                        "values(@dateTime,@avgMineProduce,@waterwayId,@rectangleId,@flag)";

                        MySqlParameter[] param1 = new MySqlParameter[]
                        {
                            new MySqlParameter("@dateTime",new DateTime(currentTime.Year,currentTime.Month,currentTime.Day,currentTime.Hour,currentTime.Minute,currentTime.Second)),
                            new MySqlParameter("@avgMineProduce",avgSub),
                            new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                            new MySqlParameter("@flag",1),
                        };
                        MySQLHelper.ExecSql(sqlStr1, param1);
                    }
                    //今天有该网格信息，且今天有采前采后数据(avgSub >= 0.3),则更新
                    else if (sum > 0 && avgSub >= 0.3)
                    {
                        string sqlStr1 = "update mproduce set avg_mine_produce = @avgMineProduce,date_time = @dateTime,flag = @flag " +
                            "where waterway_id = @waterwayId and rectangle_id = @rectangleId";
                        MySqlParameter[] param1 = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineProduce",avgSub),
                            new MySqlParameter("@dateTime",new DateTime(currentTime.Year,currentTime.Month,currentTime.Day,currentTime.Hour,currentTime.Minute,currentTime.Second)),
                            new MySqlParameter("@flag",1),
                            new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                        };
                        MySQLHelper.ExecSqlQuery(sqlStr1, param1);
                    }
                    //
                    //今天没有该网格信息，采前采后数据今天不同时存在(avgSub < 0.3)
                    else if (sum == 0 && avgSub < 0.3)
                    {
                        string sqlStr1 = "SELECT Date(data_time) FROM test WHERE waterway_id = 1 and rectangle_id = 1 AND DATE(`data_time`) < CURDATE() ORDER BY `data_time` DESC LIMIT 1";

                        DataSet ds2 = MySQLHelper.ExecSqlQuery(sqlStr1);

                        DateTime mineBeginTime = (DateTime)ds2.Tables[0].Rows[0][0];

                        string sqlStr4 = "SELECT count(*) from mproduce WHERE date_time BETWEEN @mineBeginTime and NOW() and waterway_id = @waterwayId " +
                            "and rectangle_id = @rectangleId";
                        MySqlParameter[] param4 = new MySqlParameter[]
                        {
                            new MySqlParameter("@mineBeginTime",mineBeginTime),
                            new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                        };

                        DataSet ds3 = MySQLHelper.ExecSqlQuery(sqlStr4, param4);

                        int count = Convert.ToInt32(ds3.Tables[0].Rows[0][0]);

                        //代表本期还没采过，将数据记录到今天
                        if (count == 0)
                        {
                            string sqlStr3 = "insert into mproduce(date_time,avg_mine_reserve,waterway_id,rectangle_id,flag) " +
                            "values(NOW(),@avgMineReserves,@waterwayId,@rectangleId,0)";
                            MySqlParameter[] param1 = new MySqlParameter[]
                            {
                                new MySqlParameter("@avgMineReserves",avgAdd),
                                new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                                new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                            };
                            MySQLHelper.ExecSqlQuery(sqlStr3, param1);
                        }
                        //代表本期采过，将数据更新至历史
                        else if (count == 1)
                        {
                            string sqlStr5 = "select avg_mine_reserve from mproduce where waterway_id = @waterwayId and rectangle_id = @rectangleId";
                            MySqlParameter[] param3 = new MySqlParameter[]
                            {
                                 new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                                new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                            };

                            DataSet ds4 = MySQLHelper.ExecSqlQuery(sqlStr5, param3);

                            double avgMineReserve = (double)ds4.Tables[0].Rows[0][0];

                            double avgMineProduce = avgMineReserve - avgAdd;

                            string sqlStr3 = "update mproduce set avg_mine_produce = @avgMineProduce, flag = @flag where waterway_id = " +
                                "@waterwayId and rectangle_id = @rectangleId ";
                            MySqlParameter[] param1 = new MySqlParameter[]
                            {
                                new MySqlParameter("@avgMineProduce",avgMineProduce),
                                new MySqlParameter("@flag",1),
                                new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                                new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                            };
                            MySQLHelper.ExecSql(sqlStr3, param1);
                        }
                    }
                    //今天有该网格信息，采前采后数据今天不同时存在(avgSub < 0.3),则更新
                    else if (sum > 0 && avgSub < 0.3)
                    {
                        string sqlStr1 = "update mproduce set avg_mine_reserve = @avgMineReserve,date_time = NOW(),flag = 0 " +
                        "where waterway_id = @waterwayId and rectangle_id = @rectangleId";
                        MySqlParameter[] param1 = new MySqlParameter[]
                        {
                            new MySqlParameter("@avgMineReserve",avgAdd),
                            new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                        };
                        MySQLHelper.ExecSql(sqlStr1, param1);
                    }

                }
                catch (Exception ex)
                {

                }
            }

            //Console.WriteLine("执行完成" + count);
        }

        public static System.Data.DataTable mineTable()
        {
            string sqlStr = "SELECT date_time FROM mproduce WHERE waterway_id=1 and rectangle_id=1 " +
                "ORDER BY date_time DESC LIMIT 1;";
            DataSet ds = MySQLHelper.ExecSqlQuery(sqlStr);

            DateTime dt = (DateTime)ds.Tables[0].Rows[0][0];

            string sqlStr1 = "SELECT waterway_id," +
                "SUM(CASE WHEN flag = 1 THEN avg_mine_produce ELSE 0 END) + SUM(CASE WHEN flag = 0 THEN avg_mine_reserve ELSE 0 END) AS total_sum," +
                "MAX(date_time) AS max_e,MIN(date_time) AS min_e " +
                "FROM mproduce WHERE date_time >= @dateTime GROUP BY waterway_id";
            MySqlParameter[] param1 = new MySqlParameter[]
            {
                new MySqlParameter("@dateTime",dt),
            };

            DataSet dataSet = MySQLHelper.ExecSqlQuery(sqlStr1, param1);

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                double originalValue = Convert.ToDouble(row["total_sum"]);
                double multipliedValue = originalValue * 81;
                row["total_sum"] = multipliedValue;
            }

            return dataSet.Tables[0];
        }


        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //导出excel
        public static void ExportToExcel(System.Data.DataTable dataTable)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveFileDialog.DefaultExt = "xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                        // 设置列名
                        for (int col = 0; col < dataTable.Columns.Count; col++)
                        {
                            worksheet.Cells[1, col + 1].Value = dataTable.Columns[col].ColumnName;
                        }

                        // 将 DataTable 数据写入工作表
                        for (int row = 0; row < dataTable.Rows.Count; row++)
                        {
                            for (int col = 0; col < dataTable.Columns.Count; col++)
                            {
                                // 检查是否为日期列
                                if (dataTable.Columns[col].DataType == typeof(DateTime))
                                {
                                    DateTime dateTimeValue = (DateTime)dataTable.Rows[row][col];
                                    worksheet.Cells[row + 2, col + 1].Value = dateTimeValue.ToString("yyyy-MM-dd");
                                }
                                else
                                {
                                    worksheet.Cells[row + 2, col + 1].Value = dataTable.Rows[row][col];
                                }
                            }
                        }

                        // 保存 Excel 文件
                        FileInfo file = new FileInfo(filePath);
                        package.SaveAs(file);

                        MessageBox.Show("文件导出成功！");
                    }
                }
            }
        }

        //public static double s = 81;
        //public static double[] dayTotalMine = new double[7];
        //public static double[] monthTotalMine = new double[3];

        //更新月差异数据函数
        //public static void updateMonthDataAnalysis()
        //{
        //    System.DateTime currentTime = new System.DateTime();
        //    currentTime = System.DateTime.Now;

        //    DateTime dateNow = currentTime;
        //    DateTime date1MAgo = dateNow.AddMonths(-2).AddDays(1 - dateNow.Day);

        //    string sqlStr = "SELECT SUM(avg_mine_depth),count(*) AS count FROM mproduce WHERE date_time " +
        //        "BETWEEN @dateTime1 and @dateTime2 GROUP BY YEAR(date_time), MONTH(date_time)";

        //    MySqlParameter[] param = new MySqlParameter[]
        //    {
        //        new MySqlParameter("@dateTime1",date1MAgo),
        //        new MySqlParameter("@dateTime2",dateNow),
        //    };

        //    DataSet ds = MySQLHelper.ExecSqlQuery(sqlStr, param);

        //    double[] month = new double[7];
        //    int count = 0;

        //    for (int i = ds.Tables[0].Rows.Count - 1; i >= 0; i--)
        //    {
        //        month[count] = (double)(ds.Tables[0].Rows[i][0]) * (DataAnalysis.s);
        //        count++;
        //    }

        //    DataAnalysis.monthTotalMine = month;
        //}

        //更新日差异数据
        //public static void updateDayDataAnalysis()
        //{
        //    System.DateTime currentTime = new System.DateTime();
        //    currentTime = System.DateTime.Now;

        //    DateTime dateNow = currentTime;
        //    DateTime date7Ago = dateNow.AddDays(-6);

        //    string sqlStr = "SELECT SUM(avg_mine_depth), COUNT(*),date_time FROM mproduce " +
        //        "GROUP BY YEAR(date_time), MONTH(date_time), DAY(date_time)" +
        //        "having date_time BETWEEN @dateTime1 and @dateTime2";

        //    MySqlParameter[] param = new MySqlParameter[]
        //    {
        //        new MySqlParameter("@dateTime1",date7Ago),
        //        new MySqlParameter("@dateTime2",dateNow),
        //    };

        //    DataSet ds = MySQLHelper.ExecSqlQuery(sqlStr, param);

        //    double[] day = new double[7];
        //    int count = 0;

        //    for (int i = ds.Tables[0].Rows.Count - 1; i >= 0; i--)
        //    {
        //        day[count] = (double)(ds.Tables[0].Rows[i][0]) * (DataAnalysis.s);
        //        count++;
        //    }

        //    DataAnalysis.dayTotalMine = day;

        //}
    }
}
