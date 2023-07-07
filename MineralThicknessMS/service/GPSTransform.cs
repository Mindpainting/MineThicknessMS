using GMap.NET;

namespace MineralThicknessMS.service
{
/*    class EvilTransform
    {
        const double pi = 3.14159265358979324;
        const double a = 6378245.0;
        const double ee = 0.00669342162296594323;

        public static void transform(double wgLat, double wgLon, out double mgLat, out double mgLon)
        {
            if (outOfChina(wgLat, wgLon))
            {
                mgLat = wgLat;
                mgLon = wgLon;
                return;
            }
            double dLat = transformLat(wgLon - 105.0, wgLat - 35.0);
            double dLon = transformLon(wgLon - 105.0, wgLat - 35.0);
            double radLat = wgLat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            mgLat = wgLat + dLat;
            mgLon = wgLon + dLon;
        }

        static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0 * pi)) * 2.0 / 3.0;
            return ret;
        }
    }*/
/*    public class Gps
    {
        private double wgLat;
        private double wgLon;

        public Gps(double Lat, double Lon)
        {
            setWgLat(Lat);
            setWgLon(Lon);
        }
        public double getWgLat()
        {
            return wgLat;
        }
        public void setWgLat(double Lat)
        {
            this.wgLat = Lat;
        }
        public double getWgLon()
        {
            return wgLon;
        }
        public void setWgLon(double Lon)
        {
            this.wgLon = Lon;
        }
        public String toString()
        {
            return wgLat + "," + wgLon;
        }
    }*/
/*    public class PositionUtil
    {
        public static double pi = 3.1415926535897932384626;
        public static double a = 6378245.0;
        public static double ee = 0.00669342162296594323;
        public static int MapProviderSel = 0;
        public static double x_pi = pi * 3000.0 / 180.0;

        public static double dmsTodeg(double dms)
        {
            int dd, mm;
            double ss;
            double result;
            dd = (int)dms;
            mm = (int)((dms - dd) * 100.0);
            ss = (dms - dd - mm * 0.01) * 10000.0;
            if (ss >= 60)
            {
                mm = mm + 1;
                ss = 0;
            }
            if (mm >= 60)
            {
                dd = dd + 1;
                mm = 0;
            }
            result = dd + mm / 60.0 + ss / 3600.0;
            return result;
        }

        public static Gps gps84_To_Gcj02(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new Gps(lat, lon);
            }
            if (MapProviderSel < 9)
            {
                return new Gps(lat, lon);
            }
            else
            {
                double dLat = transformLat(lon - 105.0, lat - 35.0);
                double dLon = transformLon(lon - 105.0, lat - 35.0);
                double radLat = lat / 180.0 * pi;
                double magic = Math.Sin(radLat);
                magic = 1 - ee * magic * magic;
                double sqrtMagic = Math.Sqrt(magic);
                dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
                dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
                double mgLat = lat + dLat;
                double mgLon = lon + dLon;

                if (MapProviderSel > 9)
                {
                    Gps BPnt = gcj02_To_Bd09(mgLat, mgLon);
                    mgLat = BPnt.getWgLat();
                    mgLon = BPnt.getWgLon();
                }
                return new Gps(mgLat, mgLon);
            }

        }

        public static Gps gcj_To_Gps84(double lat, double lon)
        {
            if (MapProviderSel < 9)
            {
                return new Gps(lat, lon);
            }
            else
            {
                if (MapProviderSel > 9)
                {
                    double x = lon - 0.0065, y = lat - 0.006;
                    double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
                    double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
                    lon = z * Math.Cos(theta);
                    lat = z * Math.Sin(theta);
                }

                Gps gps = transform(lat, lon);
                double lontitude = lon * 2 - gps.getWgLon();
                double latitude = lat * 2 - gps.getWgLat();
                return new Gps(latitude, lontitude);
            }
        }

        public static Gps Gps84_ToBd09(double lat, double lon)
        {
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;

            Gps dgcj2bd = gcj02_To_Bd09(mgLat, mgLon);
            return dgcj2bd;
        }
        public static Gps gcj02_To_Bd09(double gg_lat, double gg_lon)
        {
            double x = gg_lon, y = gg_lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            double bd_lon = z * Math.Cos(theta) + 0.0065;
            double bd_lat = z * Math.Sin(theta) + 0.006;
            return new Gps(bd_lat, bd_lon);
        }

        public static Gps bd09_To_Gcj02(double bd_lat, double bd_lon)
        {
            double x = bd_lon - 0.0065, y = bd_lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            double gg_lon = z * Math.Cos(theta);
            double gg_lat = z * Math.Sin(theta);
            return new Gps(gg_lat, gg_lon);
        }

        public static Gps bd09_To_Gps84(double bd_lat, double bd_lon)
        {
            Gps gcj02 = PositionUtil.bd09_To_Gcj02(bd_lat, bd_lon);
            Gps map84 = PositionUtil.gcj_To_Gps84(gcj02.getWgLat(),
                    gcj02.getWgLon());
            return map84;

        }

        public static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        public static Gps transform(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new Gps(lat, lon);
            }
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            return new Gps(mgLat, mgLon);
        }

        public static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
                    + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y * pi / 12.0) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        public static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
                    * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x * pi / 12.0) + 300.0 * Math.Sin(x * pi / 30.0
                    )) * 2.0 / 3.0;
            return ret;
        }

        //获取dms(度分秒)=>deg(度)后的点集 
        public static List<PointLatLng> getCorrectedPoints(List<PointLatLng> boundaryPoints)
        {
            List<PointLatLng> correctedPoints = new();
            boundaryPoints.ForEach(point =>
            {
                point = new PointLatLng(PositionUtil.dmsTodeg(point.Lat), PositionUtil.dmsTodeg(point.Lng));
                correctedPoints.Add(point);
            });
            return correctedPoints;
        }

    }*/

    public class GpsXY
    {
        public const double PI = 3.1415926535897932;
        public const double rs = 1000000;
        public static CoordSys m_Cs0;                       //当前系统设定的坐标系统及相关参数，打开或者新建项目及修改坐标系统时赋值
        public static CoordSys m_Cs84;                      //WGS84坐标系统，打开或新建项目时获取相关参数
        public static CoorTranPara m_Cs0Para;               //坐标系统转换参数，打开或者新建项目及修改坐标系统时赋值 
        public static double CurNorthX, CurEastY, CurZ;     //当前北、东坐标
        public static double CurLat, CurLng, CurH;          //当前经纬度
        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        public static string strCoordSys = "WGS-84";                  //坐标系统
        public static double a = 6378137.0;                         //长半轴
        public static double f = 298.257223257;						//扁率	
        public static int iCoordTranMode = 0;                       //坐标转换方式，0：无，1：七参数转换，2：四参数转换
        //public static bool bFourPar=false;
        //public static bool bSevenPar=false;                       //四参数转换，七参数转换，check型相关变量
        public static int iProjection = 0;                            //投影方法：高斯、UTM、Mercator
        public static string L0 = "90°00′00.00″";
        public static string B0 = "00°00′00.00″";				//投影参数：中央子午线、基准维度、尺度比、X轴加常数、Y轴加常数
        public static double Fk = 1.0;
        public static double dX = 0;
        public static double dY = 500.000;
        public static double dX0 = 0;
        public static double dY0 = 0;
        public static double dZ0 = 0;                                 //坐标系统平移参数，单位：米
        public static double rX = 0;
        public static double rY = 0;
        public static double rZ = 0;					                //坐标系统旋转角度，单位：度分秒dd.mmssss
        public static double rsc = 1.0;					            //坐标系统尺度比
        public static double H0 = 0.0;                              //投影面高度
        public static bool m_bElevationFitting = false;            //是否开启高程拟合功能
        public static double ElevtorFittingX0 = 0;                  //高程拟合原点北坐标
        public static double ElevtorFittingY0 = 0;                  //高程拟合原点东坐标
        public static double[] ElevtorFittingA = new double[6];     //高程拟合6个系数
        public static double DeltaElevator = 0;                     //高程拟合计算出的改正数
        public static double BaseX0 = 0;//若坐标系统设计到墨卡托投影，则把基准点可以先算出来

        //点校正
        public static bool m_bCoordCorrect = false;
        public static double CoordCorrectNorth = 0;
        public static double CoordCorrectEast = 0;
        public static double CoordCorrectHeight = 0;
        public static bool m_bDataProcess = false;//是否是在数据处理过程中打开点校正设置


        public struct CoordSys   // first : Coordinate systeam struct
        {
            public char[] szCS;     //坐标系统名称
            public double A;		//坐标系统的椭球长半径；
            public double F;		//扁率
            public double E2;		//坐标系统的椭球第1偏心率的平方；
            public double L0;		//坐标系统的中央子午线的经度；
            public double B0;		//坐标系统的基准纬线；
            public double H0;		//坐标系统的投影面正常高；
            public double DN;		//平均高程异常；
            public double X0, Y0;	//坐标系统的加常数；
            public double UTM;      //投影比例，1/0.9996
            public int flag;        //投影方式，0：高斯投影；1：UTM投影；2：墨卡托投影；
        }
        public struct CoorTranPara
        {
            public double dX0;
            public double dY0;
            public double dZ0;
            public double rX;
            public double rY;
            public double rZ;
            public double rsc;
            public int flag;   //flag=1,四参数转换；flag=2,七参数转换；
        }
        public static double stringdms2Dms(string sdms)//字符串度分秒格式，带符号，转换为double dms
        {
            int dd, mm;
            int flag;
            flag = 1;
            double ss, dms;
            int pos1, pos2, pos3;
            string strTemp;
            pos1 = sdms.IndexOf("°");
            pos2 = sdms.IndexOf("′");
            pos3 = sdms.IndexOf("″");
            strTemp = sdms.Substring(0, pos1);
            dd = int.Parse(strTemp);
            strTemp = sdms.Substring(pos1 + 1, 2);
            mm = int.Parse(strTemp);
            strTemp = sdms.Substring(pos2 + 1, pos3 - pos2 - 1);
            ss = double.Parse(strTemp);
            if (dd < 0)
            {
                flag = -1;
            }
            //取整60进位
            if (Math.Abs((double)(ss - 60)) <= 0.0001 || ss == 60)
            {
                ss = 0;
                mm = mm + 1;
            }
            if (Math.Abs((int)(mm - 60)) <= 0.0001 || mm == 60)
            {
                mm = 0;
                dd = dd + 1 * flag;
            }
            dms = dd + flag * mm / 100.0 + flag * ss / 10000.0;
            return dms;
        }
        public static void GetValueOfCoordSys()//获取坐标系统转换各项参数
        {
            double b, e;
            b = a - a / f;
            e = Math.Sqrt(a * a - b * b) / a;

            m_Cs0.A = a;
            m_Cs0.F = 1 / f;
            m_Cs0.E2 = e * e;
            m_Cs0.L0 = stringdms2Dms(L0);
            m_Cs0.B0 = stringdms2Dms(B0);
            m_Cs0.UTM = Fk;
            m_Cs0.X0 = dX;
            m_Cs0.Y0 = dY;
            m_Cs0.DN = 0;
            m_Cs0.H0 = H0;
            m_Cs0.szCS = strCoordSys.ToCharArray(); ;
            m_Cs0.flag = iProjection;//投影方式

            m_Cs0Para.flag = 0;//标记为无参数转换
            if (iCoordTranMode == 1)
            {
                m_Cs0Para.dX0 = dX0;
                m_Cs0Para.dY0 = dY0;
                m_Cs0Para.dZ0 = dZ0;
                m_Cs0Para.rX = rX;
                m_Cs0Para.rY = rY;
                m_Cs0Para.rZ = rZ;
                m_Cs0Para.rsc = rsc;
                m_Cs0Para.flag = 2;//标记为七参数转换
            }
            else if (iCoordTranMode == 2)
            {
                m_Cs0Para.dX0 = dX0;
                m_Cs0Para.dY0 = dY0;
                m_Cs0Para.dZ0 = 0;
                m_Cs0Para.rX = rX;
                m_Cs0Para.rY = rX;
                m_Cs0Para.rZ = 0;
                m_Cs0Para.rsc = rsc;
                m_Cs0Para.flag = 1;//标记为四参数转换
            }
            //源坐标系统：WGS-84系统
            //应该修改为CGCS2000系
            m_Cs84.A = 6378137;
            m_Cs84.F = 1 / 298.257223563;
            m_Cs84.L0 = stringdms2Dms(L0);
            m_Cs84.B0 = stringdms2Dms(B0);
            m_Cs84.UTM = Fk;
            m_Cs84.X0 = dX;
            m_Cs84.Y0 = dY;
            m_Cs84.DN = 0;
            m_Cs84.H0 = H0;
            double b84, e84;
            b84 = m_Cs84.A - m_Cs84.A * m_Cs84.F;
            e84 = Math.Sqrt(m_Cs84.A * m_Cs84.A - b84 * b84) / m_Cs84.A;
            m_Cs84.E2 = e84 * e84;
        }
        public static void MercatorBL_XY(CoordSys Cs0, double B, double L, out double X, out double Y)
        {
            double e, ee, b, c, fe;
            double l;

            double radB, radL, radL0, radB0;

            dms2rad(B, out radB);
            dms2rad(L, out radL);
            dms2rad(Cs0.L0, out radL0);
            dms2rad(Cs0.B0, out radB0);

            c = Cs0.A / Math.Sqrt(1.0 - Cs0.E2);
            ee = Math.Sqrt(Cs0.E2 / (1 - Cs0.E2));

            b = Cs0.A - Cs0.F * Cs0.A;
            e = Math.Sqrt(Cs0.A * Cs0.A - b * b) / Cs0.A;
            fe = Math.Sqrt(Cs0.A * Cs0.A - b * b) / b;

            double r0, U, U0;


            U = (Math.Tan(Math.PI / 4.0 + radB / 2.0)) * Math.Pow(((1 - e * Math.Sin(radB)) / (1 + e * Math.Sin(radB))), e / 2.0);
            r0 = (Cs0.A * Cs0.A / b) * Math.Cos(radB0) / Math.Sqrt(1 + fe * fe * Math.Cos(radB0) * Math.Cos(radB0));
            X = r0 * Math.Log(U);
            //////////////////////////////////////////////////////////
            //计算基点坐标
            U0 = (Math.Tan(Math.PI / 4.0 + radB0 / 2.0)) * Math.Pow(((1 - e * Math.Sin(radB0)) / (1 + e * Math.Sin(radB0))), e / 2.0);
            BaseX0 = r0 * Math.Log(U0);

            X = X - BaseX0;
            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////////
            l = radL - radL0;
            Y = r0 * l;

            X = X + Cs0.X0 * 1000.0;
            Y = Y + Cs0.Y0 * 1000.0;

        }

        //高斯投影，BL-->XY
        public static void GaussBL_XY(CoordSys Cs0, double B, double L, out double X, out double Y)
        {
            double ee, c, x;
            double l, Bs, Bc, t, tt, W, V, N;

            double radB, radL, radL0, radl;
            double xx, yy, zz;
            double H;

            dms2rad(B, out radB);
            dms2rad(L, out radL);
            dms2rad(Cs0.L0, out radL0);

            radl = radL - radL0;

            c = Cs0.A / Math.Sqrt(1.0 - Cs0.E2);
            ee = Math.Sqrt(Cs0.E2 / (1 - Cs0.E2));

            double Rou = 180 * 3600 / PI;
            l = radTodms(radl);
            Bs = Math.Sin(radB);
            Bc = Math.Cos(radB);
            t = Math.Tan(radB);
            tt = ee * Bc;
            W = Math.Sqrt(1 - (Cs0.E2 * Bs * Bs));
            V = Math.Sqrt(1 + ee * ee * Bc * Bc);
            N = c / V;

            double a0, a2, a4, a6, a8;
            double m0, m2, m4, m6, m8;

            m0 = Cs0.A * (1 - Cs0.E2);
            m2 = 3.0 * Cs0.E2 * m0 / 2.0;
            m4 = 5.0 * Cs0.E2 * m2 / 4.0;
            m6 = 7.0 * Cs0.E2 * m4 / 6.0;
            m8 = 9.0 * Cs0.E2 * m6 / 8.0;

            a0 = m0 + m2 / 2.0 + 3.0 * m4 / 8.0 + 5.0 * m6 / 16.0 + 35.0 * m8 / 128.0;
            a2 = m2 / 2.0 + m4 / 2.0 + 15.0 * m6 / 32.0 + 7.0 * m8 / 16.0;
            a4 = m4 / 8.0 + 3.0 * m6 / 16.0 + 7.0 * m8 / 32.0;
            a6 = m6 / 32.0 + m8 / 16.0;
            a8 = m8 / 128.0;

            x = a0 * radB - a2 * Math.Sin((2 * radB)) / 2.0 + a4 * Math.Sin((4 * radB)) / 4.0 - a6 * Math.Sin((6 * radB)) / 6.0 + a8 * Math.Sin((8 * radB)) / 8.0;
            X = x + N * Bs * Bc * dmsTosec(l) * dmsTosec(l) / (2.0 * Rou * Rou) + N * Bs * Bc * Bc * Bc * (5.0 - t * t + 9.0 * tt * tt + 4.0 * tt * tt * tt * tt) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) / (24.0 * Rou * Rou * Rou * Rou) + N * Bs * Bc * Bc * Bc * Bc * Bc * (61.0 - 58.0 * t * t + t * t * t * t) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) / (720.0 * Rou * Rou * Rou * Rou * Rou * Rou);
            X = Cs0.X0 * 1000.0 + Cs0.UTM * X;
            Y = N * Bc * dmsTosec(l) / Rou + N * Bc * Bc * Bc * (1 - t * t + tt * tt) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) / (6.0 * Rou * Rou * Rou) + N * Bc * Bc * Bc * Bc * Bc * (5.0 - 18.0 * t * t + t * t * t * t + 14.0 * tt * tt - 58.0 * tt * tt * t * t) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) * dmsTosec(l) / (120.0 * Rou * Rou * Rou * Rou * Rou);
            Y = Cs0.Y0 * 1000.0 + Cs0.UTM * Y;

        }

        public static double dmsTosec(double dms)
        {
            double result;
            result = (dmsTodeg(dms)) * 3600.0;
            return result;
        }
        public static double dmsTodeg(double dms)
        {
            int dd, mm;
            double ss;
            double result;
            dd = (int)dms;
            mm = (int)((dms - dd) * 100.0);
            ss = (dms - dd - mm * 0.01) * 10000.0;
            if (ss >= 60)
            {
                mm = mm + 1;
                ss = 0;
            }
            if (mm >= 60)
            {
                dd = dd + 1;
                mm = 0;
            }
            result = dd + mm / 60.0 + ss / 3600.0;
            return result;
        }

        public static void UTMBL_XY(CoordSys Cs0, double B, double L, out double X, out double Y)
        {
            GaussBL_XY(Cs0, B, L, out X, out Y);
        }

        public static void Para4xy(double x1, double y1, CoorTranPara m_Para, out double x2, out double y2)
        {
            double radx;
            radx = m_Para.rX * PI / (3600 * 180);
            x2 = 0;
            y2 = 0;

            x2 = m_Para.dX0 + m_Para.rsc * Math.Cos(radx) * x1 - m_Para.rsc * Math.Sin(radx) * y1;
            y2 = m_Para.dY0 + m_Para.rsc * Math.Sin(radx) * x1 + m_Para.rsc * Math.Cos(radx) * y1;

        }

        public static void Para4TransBLToxy(double B, double L, CoordSys Cs0, CoordSys Cs1, CoorTranPara m_Para, out double x2, out double y2)
        {
            double x1 = 0, y1 = 0;
            //GaussBL_XY(Cs0,B,L,out x1,out y1);
            if (iProjection == 0)//高斯投影
            {
                GaussBL_XY(Cs0, B, L, out x1, out y1);
            }
            else if (iProjection == 1)// UTM投影
            {
                UTMBL_XY(Cs0, B, L, out x1, out y1);
            }
            else if (iProjection == 2)// 墨卡托投影
            {
                MercatorBL_XY(Cs0, B, L, out x1, out y1);
            }
            Para4xy(x1, y1, m_Para, out x2, out y2);

        }

        public static double CalFittingValue(double NorthX, double EastY)
        {
            double DeltaX = 0;
            double DeltaY = 0;
            double DeltaH = 0;
            DeltaX = NorthX - ElevtorFittingX0;
            DeltaY = EastY - ElevtorFittingY0;
            DeltaH = ElevtorFittingA[0] + ElevtorFittingA[1] * DeltaX + ElevtorFittingA[2] * DeltaY + ElevtorFittingA[3] * DeltaX * DeltaX + ElevtorFittingA[4] * DeltaY * DeltaY + ElevtorFittingA[5] * DeltaX * DeltaY;
            return DeltaH;
        }

        public static void Blh_xyz(double b, double l, double h, CoordSys Cs0, out double x, out double y, out double z)//大地坐标转化为空间直角坐标
        {
            double ni;
            double radb, radl;
            dms2rad(b, out radb);
            dms2rad(l, out radl);
            ni = (Cs0.A) / Math.Sqrt(1 - (Cs0.E2) * Math.Pow(Math.Sin(radb), 2));
            x = (ni + h) * Math.Cos(radb) * Math.Cos(radl);
            y = (ni + h) * Math.Cos(radb) * Math.Sin(radl);
            z = (ni - ni * (Cs0.E2) + h) * Math.Sin(radb);
        }

        public static void Xyz_blh(double x, double y, double z, CoordSys Cs0, out double b, out double l, out double h)//空间直角坐标转换为大地坐标
        {
            double s, b0, sq;

            s = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            b0 = Math.Atan(z / s);
            int j;
            double ni = 0;
            double bi = 0;
            for (j = 0; j < 10; j++)
            {
                sq = Math.Sqrt(1 - (Cs0.E2) * Math.Pow(Math.Sin(b0), 2));
                ni = (Cs0.A) / sq;
                bi = Math.Atan((z + ni * (Cs0.E2) * Math.Sin(b0)) / s);

                if ((Math.Abs(b0 - bi)) < 0.0000000001)
                {
                    break;
                }

                b0 = bi;
            }

            double Li, hi;
            Li = Math.Atan(y / x);
            hi = s / Math.Cos(bi) - ni;
            b = bi;  // %*180/pi;  * unit: radian
            l = Li; // %*180/pi;
            if (l < 0 && x < 0)
            {
                l = l + PI;
            }

            h = hi;

            b = radTodms(b);
            l = radTodms(l);

        }

        public static void brmul(double[] a, double[] b, int m, int n, int k, ref double[] c)//3,7,1
        {
            int i, j, l, u;
            //c = new double[3];
            for (i = 0; i <= m - 1; i++)//i=0,1,2
            {
                for (j = 0; j <= k - 1; j++)//j=0
                {
                    u = i * k + j; //u=0,1,2
                    c[u] = 0.0;
                    for (l = 0; l <= n - 1; l++)
                    {
                        c[u] = c[u] + a[i * n + l] * b[l * k + j];
                    }
                }
            }
        }

        public static void Para7xyz(double x1, double y1, double z1, CoorTranPara m_Para, out double x2, out double y2, out double z2)
        {
            int ntp = 1;

            double[] tB = new double[21];
            double[] dtx = new double[3];
            //first row
            tB[0] = 1;
            tB[1] = 0;
            tB[2] = 0;
            tB[3] = x1 / rs;
            tB[4] = 0;
            tB[5] = -z1 / rs;
            tB[6] = y1 / rs;
            //second row
            tB[7] = 0;
            tB[8] = 1;
            tB[9] = 0;
            tB[10] = y1 / rs;
            tB[11] = z1 / rs;
            tB[12] = 0;
            tB[13] = -x1 / rs;
            //third row
            tB[14] = 0;
            tB[15] = 0;
            tB[16] = 1;
            tB[17] = z1 / rs;
            tB[18] = -y1 / rs;
            tB[19] = x1 / rs;
            tB[20] = 0;
            //  2)get parameter
            double[] tc = new double[7];
            tc[0] = m_Para.dX0;
            tc[1] = m_Para.dY0;
            tc[2] = m_Para.dZ0;
            tc[3] = m_Para.rsc;
            tc[4] = m_Para.rX * rs * PI / 180.0 / 3600.0;
            tc[5] = m_Para.rY * rs * PI / 180.0 / 3600.0;
            tc[6] = m_Para.rZ * rs * PI / 180.0 / 3600.0;

            brmul(tB, tc, 3 * ntp, 7, 1, ref dtx); // matrix 

            x2 = dtx[0] + x1;
            y2 = dtx[1] + y1;
            z2 = dtx[2] + z1;

            tB = null;
            dtx = null;

        }

        public static void Para7TransBLHToxyz(double B, double L, double H, CoordSys Cs0, CoordSys Cs1, CoorTranPara m_Para, out double x2, out double y2, out double z2)
        {
            double x1, y1, z1;
            double radB, radL, H1;
            double B1, L1;
            Blh_xyz(B, L, H, Cs0, out x1, out y1, out z1);          //将WGS84经纬度BLH转换为WGS84空间直角坐标
            Para7xyz(x1, y1, z1, m_Para, out x2, out y2, out z2);   //利用输入的七参数，采用布尔莎模型，将84的空间直角坐标转换为54或其他坐标系下的空间直角坐标
            Xyz_blh(x2, y2, z2, Cs1, out radB, out radL, out H1);   //将空间直角坐标转换为对应坐标系统下的经纬度BLH
            B1 = radB;
            L1 = radL;

            if (iProjection == 0)//高斯投影
            {
                GaussBL_XY(Cs1, B1, L1, out x2, out y2);
            }
            else if (iProjection == 1)// UTM投影
            {
                UTMBL_XY(Cs1, B1, L1, out x2, out y2);
            }
            else if (iProjection == 2)// 墨卡托投影
            {
                MercatorBL_XY(Cs1, B1, L1, out x2, out y2);
            }
            z2 = H1;

        }

        //实时坐标转换---》原坐标BLH转化为XYZ
        public static void TransBLHToXYZ()
        {
            //double x1,y1,z1;
            if (m_Cs0Para.flag == 0)//不进行参数转换，即目标坐标系统为WGS-84系
            {
                if (m_Cs0.flag == 0)//高斯投影
                {
                    GaussBL_XY(m_Cs0, CurLat, CurLng, out CurNorthX, out CurEastY);
                    CurZ = CurH - m_Cs0.DN;
                }
                else if (m_Cs0.flag == 1)//UTM投影
                {
                    UTMBL_XY(m_Cs0, CurLat, CurLng, out CurNorthX, out CurEastY);
                    CurZ = CurH - m_Cs0.DN;
                }
                else if (m_Cs0.flag == 2)//墨卡托投影
                {
                    MercatorBL_XY(m_Cs0, CurLat, CurLng, out CurNorthX, out CurEastY);
                    CurZ = CurH - m_Cs0.DN;
                }
            }
            /*            else if (m_Cs0Para.flag == 1)//四参数坐标转换，即目标坐标系统不为WGS-84系
                        {
                            //Para4xy(x1, y1, m_Cs0Para, out CurNorthX, out CurEastY);
                            Para4TransBLToxy(CurLat, CurLng, m_Cs84, m_Cs0, m_Cs0Para, out CurNorthX, out CurEastY);
                            CurZ = CurH - m_Cs0.DN;
                            //加入代码，在四参数转换过程中，若选择高程拟合，则选择最近高程点作为拟合参考
                        }
                        else if (m_Cs0Para.flag == 2)//七参数坐标转换，即目标坐标系统不为WGS-84系
                        {
                            CurTransHeight = CurH;
                            Para7TransBLHToxyz(CurLat, CurLng, CurH, m_Cs84, m_Cs0, m_Cs0Para, out CurNorthX, out CurEastY, out CurZ);
                        }
            */
            //如果开启了高程拟合功能
            if (m_bElevationFitting)
            {
                double DeltaH = 0;
                DeltaH = CalFittingValue(CurNorthX, CurEastY);
                //CurZ = CurH- DeltaH;//当地平面高程=WGS-84高程-高程异常
                CurZ = CurZ - DeltaH;//当地平面高程=WGS-84高程-高程异常

            }
            //如果设置了点校正
            if (m_bCoordCorrect)
            {
                CurNorthX += CoordCorrectNorth;
                CurEastY += CoordCorrectEast;
                CurZ += CoordCorrectHeight;
            }
        }
        public static void MercatorXY_BL(CoordSys Cs0, double X, double Y, out double B, out double L)
        {
            double b, ee;
            double radB0, radL0;

            dms2rad(Cs0.B0, out radB0);
            dms2rad(Cs0.L0, out radL0);

            b = Cs0.A - Cs0.F * Cs0.A;
            ee = Math.Sqrt(Cs0.E2 / (1 - Cs0.E2));

            double fi, q, r0;
            double B2, B4, B6;

            double E2, E4, E6, E8;

            E2 = Cs0.E2; E4 = E2 * E2; E6 = E4 * E2; E8 = E4 * E4;

            B2 = E2 / 2.0 + 5 * E4 / 24.0 + E6 / 12.0;
            B4 = 3.53735 * E4 / 24.0 - E6 / 8.0;
            B6 = 7 * E6 / 120.0;


            r0 = Cs0.A * Cs0.A * Math.Cos(radB0) / (Math.Pow((1 + ee * ee * Math.Cos(radB0) * Math.Cos(radB0)), 0.5) * b);

            X = X - Cs0.X0 * 1000.0 + BaseX0;
            Y = Y - Cs0.Y0 * 1000.0;

            q = X / r0;
            fi = 2 * Math.Atan(Math.Pow(2.718281828459, q)) - PI / 2.0;
            B = fi + B2 * Math.Sin(2.0 * fi) + B4 * Math.Sin(4.0 * fi) + B6 * Math.Sin(6.0 * fi);
            L = Y / r0;
            B = radTodms(B);
            L = radTodms(L + radL0);

        }
        public static double radTodms(double rad)
        {
            int d;
            int m;
            double s;

            double dms = rad * 180 / PI; //弧度转换度
            int d1 = (int)dms;           //取出度
            d = d1;
            double m1 = (dms - d) * 60.0;
            int m2 = (int)m1;
            m = m2;
            double s1 = (m1 - m) * 60.0;
            s = s1;

            if (Math.Abs((s - 60)) <= 0.0001)
            {
                m = m + 1;
                s = 0;
            }
            if (Math.Abs((m - 60)) <= 0.0001)
            {
                m = 0;
                d++;
            }
            double result;
            result = d + m * 0.01 + s * 0.0001;
            return result;

        }

        public static void Para4Trans54XYTo84BL(double X54, double Y54, CoordSys Cs0, CoordSys Cs1, CoorTranPara m_Para, out double B84, out double L84)
        {
            //double X1,Y1,Z1;
            double X2, Y2;
            //double B54,L54,H54;
            B84 = 0;
            L84 = 0;
            CoorTranPara m_Para1;
            m_Para1.dX0 = -1 * m_Para.dX0;
            m_Para1.dY0 = -1 * m_Para.dY0;
            m_Para1.dZ0 = -1 * m_Para.dZ0;
            m_Para1.rX = -1 * m_Para.rX;
            m_Para1.rY = -1 * m_Para.rY;
            m_Para1.rZ = -1 * m_Para.rZ;
            m_Para1.rsc = 1 / m_Para.rsc;
            m_Para1.flag = m_Para.flag;
            Para4xy(X54, Y54, m_Para1, out X2, out Y2);
            //GaussXY_BL(Cs1,X2,Y2,out B84,out L84);
            if (m_Cs0.flag == 0)//高斯投影
            {
                GaussXY_BL(Cs1, X2, Y2, out B84, out L84);
            }
            else if (m_Cs0.flag == 1)//UTM投影
            {
                UTMXY_BL(Cs1, X2, Y2, out B84, out L84);
            }
            else if (m_Cs0.flag == 2)//墨卡托投影
            {
                MercatorXY_BL(Cs1, X2, Y2, out B84, out L84);
            }
        }

        public static void UTMXY_BL(CoordSys Cs0, double X, double Y, out double B, out double L)
        {
            GaussXY_BL(Cs0, X, Y, out B, out L);
        }

        public static void GaussXY_BL(CoordSys Cs0, double X, double Y, out double B, out double L)//高斯投影，XY-->BL,X：北(纵)坐标，Y：东(横)坐标
        {
            double c;
            double x2, x4, x6, x8, x10;

            Cs0.A = Cs0.A + Cs0.H0;//加入投影面高程            

            c = Cs0.A / Math.Sqrt(1.0 - Cs0.E2);

            x2 = Cs0.E2;
            x4 = x2 * x2;
            x6 = x2 * x4;
            x8 = x4 * x4;
            x10 = x2 * x8;

            double aa, bb, cc, dd, ee, ff;

            aa = 1.0 + 3.0 * x2 / 4.0 + 45.0 * x4 / 64.0 + 175.0 * x6 / 256.0;
            aa = aa + 11025.0 * x8 / 16384.0 + 43659.0 * x10 / 65536.0;
            bb = 3.0 * x2 / 4.0 + 15.0 * x4 / 16.0 + 525.0 * x6 / 512.0;
            bb = bb + 2205.0 * x8 / 2048.0 + 72765.0 * x10 / 65536.0;
            cc = 15.0 * x4 / 64.0 + 105.0 * x6 / 256.0;
            cc = cc + 2205.0 * x8 / 4096.0 + 10395.0 * x10 / 16384.0;
            dd = 35.0 * x6 / 512.0 + 315.0 * x8 / 2048.0 + 31185.0 * x10 / 13072.0;
            ee = 315.0 * x8 / 16384.0 + 3465.0 * x10 / 65536.0;
            ff = 693.0 * x10 / 131072.0;

            double a1, a2, a3, a4, a5, a6;
            a1 = aa * Cs0.A * (1.0 - Cs0.E2);
            a2 = -bb * Cs0.A * (1.0 - Cs0.E2) / 2.0;
            a3 = cc * Cs0.A * (1.0 - Cs0.E2) / 4.0;
            a4 = -dd * Cs0.A * (1.0 - Cs0.E2) / 6.0;
            a5 = ee * Cs0.A * (1.0 - Cs0.E2) / 8.0;
            a6 = -ff * Cs0.A * (1.0 - Cs0.E2) / 10.0;

            double r1, r2, r3;

            r1 = 2.0 * a2 + 4.0 * a3 + 6.0 * a4;
            r2 = -8.0 * a3 - 32.0 * a4;
            r3 = 32.0 * a4;

            double[] b11 = new double[4];
            double[] r11 = new double[4];
            double[] d11 = new double[4];

            b11[0] = -a2 / a1;
            r11[0] = -a3 / a1;
            d11[0] = -a4 / a1;
            for (int i = 0; i < 3; i++)
            {
                b11[i + 1] = b11[0] + b11[0] * r11[i];
                b11[i + 1] = b11[i + 1] - 2.0 * r11[0] * b11[i] - 3.0 * b11[0] * b11[i] * b11[i] / 2.0;
                r11[i + 1] = r11[0] + b11[0] * b11[i];
                d11[i + 1] = d11[0] + b11[0] * r11[i];
                d11[i + 1] = d11[i + 1] + 2.0 * r11[0] * b11[i] + b11[0] * b11[i] * b11[i] / 2.0;
            }

            double K1, K2, K3;
            K1 = 2.0 * b11[3] + 4.0 * r11[3] + 6.0 * d11[3];
            K2 = -8.0 * r11[3] - 32.0 * d11[3];
            K3 = 32.0 * d11[3];


            double Bf, Tf, Y2, Vf2, Nf, Ml, Mb;

            X = (X - Cs0.X0 * 1000.0) / Cs0.UTM;
            Y = (Y - Cs0.Y0 * 1000.0) / Cs0.UTM;
            Bf = X / a1;
            Bf = Bf + Math.Cos(Bf) * Math.Sin(Bf) * (K1 + Math.Sin(Bf) * Math.Sin(Bf) * (K2 + Math.Sin(Bf) * Math.Sin(Bf) * K3));
            Tf = Math.Tan(Bf);
            Y2 = x2 / (1.0 - x2) * Math.Cos(Bf) * Math.Cos(Bf);
            Vf2 = 1.0 + x2 / (1.0 - x2) * Math.Cos(Bf) * Math.Cos(Bf);
            Nf = c / Math.Sqrt(Vf2);
            Ml = Y / Nf;
            Mb = (Y / Nf) * (Y / Nf);
            B = Bf - Mb * Vf2 * Tf / 2.0 + Mb * Mb * Vf2 * Tf / 24.0 * (5.0 + 3.0 * Tf * Tf + Y2 - 9.0 * Y2 * Tf * Tf);
            B = B - Mb * Mb * Mb * Vf2 * Tf / 720.0 * (61.0 + (90.0 + 45.0 * Tf * Tf) * Tf * Tf);
            L = Ml / Math.Cos(Bf) - Ml * Ml * Ml / 6.0 / Math.Cos(Bf) * (1.0 + Y2 + 2.0 * Tf * Tf);
            L = L + Ml * Mb * Mb / 120.0 / Math.Cos(Bf) * (5 + (6 + 8 * Tf * Tf) * Y2 + (28 + 24 * Tf * Tf) * Tf * Tf);
            L = L + dmsTodeg(Cs0.L0) * PI / 180.0;

            B = radTodms(B);
            L = radTodms(L);

            double xx, yy, zz;
            double HH;
            Blh_xyz(B, L, 0, Cs0, out xx, out yy, out zz);
            Cs0.A = Cs0.A - Cs0.H0;
            Xyz_blh(xx, yy, zz, Cs0, out B, out L, out HH);

        }

        //格式：dd.mmss,角度化弧度
        public static void dms2rad(double dms, out double rad)
        {
            int nD = (int)dms;                                  //直接取出整数部分，赋值度
            int nM = (int)((dms - nD) * 100.0);
            double dS = (dms - nD) * 10000.0 - nM * 100.0;
            if (dS >= 99)//如果秒数大于60，证明dM值少了1
            {
                nM = nM + 1;
                dS = 0;
            }
            double dD = (double)(nD + nM / 60.0 + dS / 3600.0);
            rad = dD * PI / 180.0;
        }
    }
}
