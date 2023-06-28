using MineralThicknessMS.config;
using MineralThicknessMS.entity;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineralThicknessMS.service
{
    public class DataAnalysis
    {
        public static double s = 81;
        public static double[] dayTotalMine = new double[7];
        public static double[] monthTotalMine = new double[3];

        //更新月差异数据函数
        public static void updateMonthDataAnalysis()
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            DateTime dateNow = currentTime;
            DateTime date1MAgo = dateNow.AddMonths(-2).AddDays(1 - dateNow.Day);

            string sqlStr = "SELECT SUM(avg_mine_depth),count(*) AS count FROM mproduce WHERE date_time " +
                "BETWEEN @dateTime1 and @dateTime2 GROUP BY YEAR(date_time), MONTH(date_time)";

            MySqlParameter[] param = new MySqlParameter[]
            {
                new MySqlParameter("@dateTime1",date1MAgo),
                new MySqlParameter("@dateTime2",dateNow),
            };

            DataSet ds = MySQLHelper.ExecSqlQuery(sqlStr, param);

            double[] month = new double[7];
            int count = 0;

            for (int i = ds.Tables[0].Rows.Count - 1; i >= 0; i--)
            {
                month[count] = (double)(ds.Tables[0].Rows[i][0]) * (DataAnalysis.s);
                count++;
            }

            DataAnalysis.monthTotalMine = month;
        }

        //更新日差异数据
        public static void updateDayDataAnalysis()
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            DateTime dateNow = currentTime;
            DateTime date7Ago = dateNow.AddDays(-6);

            string sqlStr = "SELECT SUM(avg_mine_depth), COUNT(*),date_time FROM mproduce " +
                "GROUP BY YEAR(date_time), MONTH(date_time), DAY(date_time)" +
                "having date_time BETWEEN @dateTime1 and @dateTime2";

            MySqlParameter[] param = new MySqlParameter[]
            {
                new MySqlParameter("@dateTime1",date7Ago),
                new MySqlParameter("@dateTime2",dateNow),
            };

            DataSet ds = MySQLHelper.ExecSqlQuery(sqlStr, param);

            double[] day = new double[7];
            int count = 0;

            for (int i = ds.Tables[0].Rows.Count - 1; i >= 0; i--)
            {
                day[count] = (double)(ds.Tables[0].Rows[i][0]) * (DataAnalysis.s);
                count++;
            }

            DataAnalysis.dayTotalMine = day;

        }

        //更新每个网格实时信息
        public static void updateMineAvg()
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            string sqlStr = "select * from test where data_time between @targetDate1 and @targetDate2";

            MySqlParameter[] param = new MySqlParameter[] {
                new MySqlParameter("@targetDate1",new DateTime(currentTime.Year,currentTime.Month,currentTime.Day,00,00,00)),
                new MySqlParameter("@targetDate2",new DateTime(currentTime.Year,currentTime.Month,currentTime.Day,23,59,59)),
            };

            DataSet ds = MySQLHelper.ExecSqlQuery(sqlStr, param);

            var originalDataTable = ds.Tables[0];// 原始的 DataTable 数据

            var groupedDataTables = new Dictionary<string, DataTable>();

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

                    double sum0 = 0, sum1 = 0;
                    foreach (DataMsg dataMsg in clusters[0])
                    {
                        sum0 += dataMsg.getMineHigh();
                    }
                    foreach (DataMsg dataMsg in clusters[1])
                    {
                        sum1 += dataMsg.getMineHigh();
                    }

                    string sqlStr2 = "SELECT count(*) id from mproduce WHERE waterway_id = @waterwayId and rectangle_id = @rectangleId and date_time " +
                        "BETWEEN @targetDate3 and @targetDate4";
                    MySqlParameter[] param2 = new MySqlParameter[] {
                        new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                        new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                        new MySqlParameter("@targetDate3",new DateTime(currentTime.Year,currentTime.Month,currentTime.Day,00,00,00)),
                        new MySqlParameter("@targetDate4",new DateTime(currentTime.Year,currentTime.Month,currentTime.Day,23,59,59)),
                    };

                    DataSet ds1 = MySQLHelper.ExecSqlQuery(sqlStr2, param2);
                    int sum = Convert.ToInt32(ds1.Tables[0].Rows[0][0]);

                    double avg = sum1 / clusters[1].Count - sum0 / clusters[0].Count;
                    double avgMineDepth = avg > 0 ? avg : -avg;

                    if (sum == 0)
                    {
                        string sqlStr1 = "insert into mproduce(date_time,avg_mine_depth,waterway_id,rectangle_id) " +
                        "values(@dateTime,@avgMineDepth,@waterwayId,@rectangleId)";

                        MySqlParameter[] param1 = new MySqlParameter[]
                        {
                            new MySqlParameter("@dateTime",new DateTime(currentTime.Year,currentTime.Month,currentTime.Day,currentTime.Hour,currentTime.Minute,currentTime.Second)),
                            new MySqlParameter("@avgMineDepth",avgMineDepth),
                            new MySqlParameter("@waterwayId",clusters[0][0].getWaterwayId()),
                            new MySqlParameter("@rectangleId",clusters[0][0].getRectangleId()),
                        };
                        MySQLHelper.ExecSqlQuery(sqlStr1, param1);
                    }

                }
                catch (Exception ex)
                {

                }
            }

            //Console.WriteLine("执行完成" + count);
        }
    }
}
