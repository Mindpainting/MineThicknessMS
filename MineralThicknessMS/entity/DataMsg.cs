using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineralThicknessMS.entity
{
    public class DataMsg
    {
        private string msgBegin;//消息头校验
        private string msgEnd;//消息尾校验

        private int id;
        private DateTime dataTime;   //时间
        private double latitude;     //纬度
        private double longitude;   //经度
        private int gpsState;    //gps状态
        private int satellite;   //使用卫星数量
        private double distance;    //激光测距值

        private double depth;       //深度

        private double waterTemperature; //水温

        private double high;     //大地高

        private double velocity; //声速
        private double boatSpeed;  //船速
        private double navigation;//惯导航向
        private double guidance;//惯导仰俯
        private double rolling;  //惯导横滚
        private double level;     //水罐液位
        private double temperature;//水罐温度
        private int deviceState;//设备状态
        private int clientId; //客户端编号
        public double mineHigh;//矿厚
        private int waterwayId; //航道号
        private int rectangleId;//网格编号

        public void setMsgBegin(string msgBegin)
        {
            this.msgBegin = msgBegin;
        }

        public void setMsgEnd(string msgEnd) {  this.msgEnd = msgEnd; }

        public string getMsgBegin()
        {
            return this.msgBegin;
        }

        public string getMsgEnd() { return this.msgEnd; }


        public void setMineHigh(double mineHigh)
        {
            this.mineHigh = mineHigh;
        }

        public double getMineHigh()
        {
            return this.mineHigh;
        }

        public void setRectangleId(int rectangleId)
        {
            this.rectangleId = rectangleId;
        }

        public int getRectangleId()
        {
            return this.rectangleId;
        }

        public void setWaterwayId(int waterwayId)
        {
            this.waterwayId = waterwayId;
        }

        public int getWaterwayId()
        {
            return this.waterwayId;
        }

        public void setWaterTemperature(double waterTemperature)
        {
            this.waterTemperature = waterTemperature;
        }

        public double getWaterTemperature()
        {
            return this.waterTemperature;
        }
        public DateTime getDataTime()
        {
            return this.dataTime;
        }
        public void setDataTime(DateTime dataTime)
        {
            this.dataTime = dataTime;
        }

        public double getLatitude()
        {
            return this.latitude;
        }
        public void setLatitude(double latitude)
        {
            this.latitude = latitude;
        }

        public double getLongitude()
        {
            return this.longitude;
        }
        public void setLongitude(double longitude)
        {
            this.longitude = longitude;
        }

        public int getGpsState()
        {
            return this.gpsState;
        }
        public void setGpsState(int gpsState)
        {
            this.gpsState = gpsState;
        }

        public int getSatellite()
        {
            return this.satellite;
        }
        public void setSatellite(int satellite)
        {
            this.satellite = satellite;
        }

        public double getDistance()
        {
            return this.distance;
        }
        public void setDistance(double distance)
        {
            this.distance = distance;
        }
        public double getDepth()
        {
            return this.depth;
        }
        public void setDepth(double depth)
        {
            this.depth = depth;
        }
        public double getHigh()
        {
            return this.high;
        }
        public void setHigh(double high)
        {
            this.high = high;
        }
        public double getVelocity()
        {
            return this.velocity;
        }
        public void setVelocity(double velocity)
        {
            this.velocity = velocity;
        }
        public int getClientId()
        {
            return this.clientId;
        }

        public void setClientId(int clientId)
        {
            this.clientId = clientId;
        }

        public void setBoatSpeed(double boatspeed)
        {
            this.boatSpeed = boatspeed;
        }

        public double getBoatSpeed()
        {
            return this.boatSpeed;
        }

        public void setNavigation(double navigation)
        {
            this.navigation = navigation;
        }

        public double getNavigation()
        {
            return this.navigation;
        }

        public void setGuidance(double guide)
        {
            this.guidance = guide;
        }

        public double getGuidance()
        {
            return this.guidance;
        }

        public void setRolling(double rolling)
        {
            this.rolling = rolling;
        }
        public double getRolling()
        {
            return this.rolling;
        }

        public void setLevel(double level)
        {
            this.level = level;
        }

        public double getLevel()
        {
            return this.level;
        }

        public void setTemperature(double temperature)
        {
            this.temperature = temperature;
        }

        public double getTemperature()
        {
            return this.temperature;
        }

        public void setDeviceState(int state)
        {
            this.deviceState = state;
        }
        public int getDeviceState()
        {
            return this.deviceState;
        }
    }
}
