using GMap.NET;
using MineralThicknessMS.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineralThicknessMS.service
{
    public class MsgDecode
    {
        GridView gridView = new GridView();

        public DataMsg msgSplit(String msg)
        {
            try
            {
                DataMsg data = new DataMsg();
                string[] strArry = msg.Split(',');
                data.setMsgBegin(strArry[0]);
                data.setDataTime(ConvertIntDatetime(strArry[1]));
                //
                data.setLatitude(ConvertToLat(strArry[2]));
                //
                data.setLongitude(ConvertToLat(strArry[4]));
                data.setGpsState(StrConvertToInt(strArry[6]));
                data.setSatellite(StrConvertToInt(strArry[7]));
                data.setDistance(StrConvertToDou(strArry[8]));
                data.setDepth(StrConvertToDou(strArry[9]));
                data.setWaterTemperature(StrConvertToDou(strArry[10]));
                data.setHigh(StrConvertToDou(strArry[11]));
                data.setVelocity(StrConvertToDou(strArry[12]));
                data.setBoatSpeed(StrConvertToDou(strArry[13]));
                data.setNavigation(StrConvertToDou(strArry[14]));
                data.setGuidance(StrConvertToDou(strArry[15]));
                data.setRolling(StrConvertToDou(strArry[16]));
                data.setLevel(StrConvertToDou(strArry[17]));
                data.setTemperature(StrConvertToDou(strArry[18]));
                data.setDeviceState(StrConvertToInt(strArry[19]));
                data.setClientId(StrConvertToInt(strArry[20].Substring(0, 1)));
                data.setMsgEnd(strArry[20].Substring(1, 3));

                PointLatLng point = new PointLatLng(data.getLatitude(), data.getLongitude());
                Grid grid = GridView.pointInGrid(Status.grids, point);
                data.setWaterwayId(grid.Column);
                data.setRectangleId(grid.Row);

                return data;
            }
            catch (Exception e)
            {
                return new DataMsg();
            }
        }



        //string转int
        public int StrConvertToInt(object o)
        {
            int result = 0;
            if (o != null)
            {
                int.TryParse(o.ToString(), out result);
            }
            return result;
        }

        //string转double
        public static double StrConvertToDou(Object o)
        {
            double result = 0;
            if (o != null)
            {
                double.TryParse(o.ToString(), out result);
            }
            return result;
        }

        //浮点UTC时间转换成DateTime时间
        public DateTime ConvertIntDatetime(string utc)
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            utc = utc.Substring(0, 6);
            string utcH = utc.Substring(0, 2);
            string utcM = utc.Substring(2, 2);
            string utcS = utc.Substring(4, 2);

            int h = (StrConvertToInt(utcH) + 8) % 24;
            int m = StrConvertToInt(utcM);
            int s = StrConvertToInt(utcS);

            return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, h, m, s);
        }

        //ddmm.mmmmmm转换成dd
        public static double ConvertToLat(string inStr)//GGA 字符串中DM格式转换为度
        {
            double latitude;
            int Deg;
            double Min;
            double Dm;
            Double.TryParse(inStr, out Dm);
            Deg = (int)(Dm / 100);//获取度
            Min = Dm - Deg * 100;
            latitude = (double)(Deg + Min / 60.0);
            return latitude;
        }
    }
}
