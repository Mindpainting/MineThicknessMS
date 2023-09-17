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
    public class DataMapper
    {
        public async void addDataAsync(DataMsg dataMsg)
        {
            string sqlStr = "insert into test(data_time,latitude,longitude,gps_state,satellite,distance,depth,water_temperature,high," +
                           "velocity,boat_speed,navigation,guidance,rolling,level,temperature,device_state,mine_high,client_id,waterway_id,rectangle_id) " +
                           "values(@dataTime,@latitude,@longitude,@gpsState,@satellite,@distance,@depth,@waterTemperature,@high,@velocity," +
                           "@boatSpeed,@navigation,@guidance,@rolling,@level,@temperature,@deviceState,@mineHigh,@clientId,@waterwayId,@rectangleId)";

            MySqlParameter[] param = new MySqlParameter[]
            {
                new MySqlParameter("@dataTime",dataMsg.getDataTime()),
                new MySqlParameter("@latitude",dataMsg.getLatitude()),
                new MySqlParameter("@longitude",dataMsg.getLongitude()),
                new MySqlParameter("@gpsState",dataMsg.getGpsState()),
                new MySqlParameter("@satellite",dataMsg.getSatellite()),
                new MySqlParameter("@distance",dataMsg.getDistance()),
                new MySqlParameter("@depth",dataMsg.getDepth()),
                new MySqlParameter("@waterTemperature",dataMsg.getWaterTemperature()),
                new MySqlParameter("@high",dataMsg.getHigh()),
                new MySqlParameter("@velocity",dataMsg.getVelocity()),
                new MySqlParameter("@boatSpeed",dataMsg.getBoatSpeed()),
                new MySqlParameter("@navigation",dataMsg.getNavigation()),
                new MySqlParameter("@guidance",dataMsg.getGuidance()),
                new MySqlParameter("@rolling",dataMsg.getRolling()),
                new MySqlParameter("@level",dataMsg.getLevel()),
                new MySqlParameter("@mineHigh",dataMsg.getMineHigh()),
                new MySqlParameter("@temperature",dataMsg.getTemperature()),
                new MySqlParameter("@deviceState",dataMsg.getDeviceState()),
                new MySqlParameter("@clientId",dataMsg.getClientId()),
                new MySqlParameter("@waterwayId",dataMsg.getWaterwayId()),
                new MySqlParameter("@rectangleId",dataMsg.getRectangleId())
            };
            MySQLHelper.ExecSqlQuery(sqlStr, param);
        }
        
        //查固定航道固定网格内矿厚
        public static async Task<double> getGridMineThickness(int waterwayId, int rectangleId)
        {
            string sqlStr = "select avg_mine_depth from mproduce where waterway_id = @waterwayId and rectangle_id = @rectangleId order by date_time desc limit 1";

            MySqlParameter[] param = new MySqlParameter[]
            {
                new MySqlParameter("@waterwayId", waterwayId),
                new MySqlParameter("@rectangleId", rectangleId)
            };
            DataSet dataSet = MySQLHelper.ExecSqlQuery(sqlStr, param);

            double avg_mine_depth  = -999;
            try
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    avg_mine_depth = Convert.ToDouble(row["avg_mine_depth"]);
                }
            }
            catch (Exception)
            {
            }

            //返回-999，数据库中没有该网格数据
            return avg_mine_depth;
        }
    }
}
