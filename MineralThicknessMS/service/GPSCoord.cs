using static MineralThicknessMS.service.GpsXY;

namespace MineralThicknessMS.service
{
    public class GPSCoor
    {
        public double Lat;
        public double Lon;
        public double LocN;
        public double LocE;
        public double LocH;

        public GPSCoor()
        {
            Lat = 0;
            Lon = 0;
            LocN = 0;
            LocE = 0;
            LocH = 0;
        }

        public GPSCoor(double lat, double lon, double locN, double locE, double locH)
        {
            Lat = lat;
            Lon = lon;
            LocN = locN;
            LocE = locE;
            LocH = locH;
        }
    }
    public class GPSCoord
    {
        const double PI = 3.14159265358979324;
        public static CoordSys UsingCoordSysTag;

        public static bool IniCoordSysTag()
        {
            //UsingCoordSysTag=RCoordSysTag;
            double a = 6378137;
            double f = 298.257223563;
            double b, e;
            b = a - a / f;
            e = Math.Sqrt(a * a - b * b) / a;
            UsingCoordSysTag.A = a;
            UsingCoordSysTag.F = 1 / f;
            UsingCoordSysTag.E2 = e * e;
            UsingCoordSysTag.B0 = 0;
            UsingCoordSysTag.L0 = 114.0000;
            UsingCoordSysTag.H0 = 0;
            UsingCoordSysTag.DN = 0;
            UsingCoordSysTag.X0 = 0;
            UsingCoordSysTag.Y0 = 500;
            UsingCoordSysTag.flag = 1;
            //UsingCoordSysTag.UTM=1.0004001600640256102440976390556;
            //UsingCoordSysTag.UTM=0.9996;
            UsingCoordSysTag.UTM = 1;
            //memset(UsingCoordSysTag.szCS, NULL, 80);
            //strcpy(UsingCoordSysTag.szCS, "WGS_84");
            return true;
        }

        //计算距离
        public static double CalDis(GPSCoor GPSPntN, GPSCoor GPSPntL)
        {
            double NorthOne = GPSPntN.LocN;
            double EastOne = GPSPntN.LocE;
            double NorthTwo = GPSPntL.LocN;
            double EastTwo = GPSPntL.LocE;

            double Dis = Math.Sqrt((NorthOne - NorthTwo) * (NorthOne - NorthTwo) + (EastOne - EastTwo) * (EastOne - EastTwo));
            return Dis;
        }

        //计算方位角
        public static double CalOri(GPSCoor GPSPntN, GPSCoor GPSPntL)
        {
            double NorthOne = GPSPntN.LocN;
            double EastOne = GPSPntN.LocE;
            double NorthTwo = GPSPntL.LocN;
            double EastTwo = GPSPntL.LocE;

            double Orient = -9999.0;
            if (NorthOne == NorthTwo)
            {
                if (EastOne > EastTwo)
                    return 270;
                else
                    return 90;
            }
            if (EastOne == EastTwo)
            {
                if (NorthOne > NorthTwo)
                    return 180;
                else
                    return 0;
            }
            Orient = Math.Atan((EastOne - EastTwo) / (NorthOne - NorthTwo));
            Orient = Rad2d(Orient);
            if ((NorthTwo - NorthOne) < 0)
                Orient = Orient + 180;
            if (Orient < 0)
                Orient = Orient + 360;
            return Orient;
        }

        public static double Rad2d(double Rad)
        {
            double deg;
            deg = Rad / PI * 180.0;
            return deg;
        }

        //已知一点，一方向，距离，计算线上另一点，CurOri单位为度
        public static bool CalPnt(GPSCoor CurPnt, double TargetOri, double Dis, GPSCoor CalPnt)
        {
            TargetOri = TargetOri / 180.0 * PI;
            CalPnt.LocN = CurPnt.LocN + Dis * Math.Cos(TargetOri);
            CalPnt.LocE = CurPnt.LocE + Dis * Math.Sin(TargetOri);
            CalPnt.LocH = CurPnt.LocH;
            Plan2WGS84(CalPnt);
            return false;
        }

        // 平面转WGS84
        public static bool Plan2WGS84(GPSCoor GPSPnt)
        {
            double X = GPSPnt.LocN;
            double Y = GPSPnt.LocE;
            double Z = GPSPnt.LocH;

            GetValueOfCoordSys();
            GaussXY_BL(UsingCoordSysTag, X, Y, out GPSPnt.Lat, out GPSPnt.Lon);
            GPSPnt.LocH = Z + UsingCoordSysTag.DN;
            return true;
        }

        public static double CalDetaAngle(double CurOri, double TarOri)//CurOri:当前方向      TarOri:目标方向
        {
            bool Left = false;
            double DetaOri = TarOri - CurOri;
            //判断左右
            if (CurOri >= 0 && CurOri <= 180)//当前方向在第一、二象限
            {
                if (DetaOri >= 0 && DetaOri <= 180)//目标在右
                {
                    Left = false;
                }
                else//目标在左
                {
                    Left = true;
                }
            }
            else//当前方向在第三、四象限
            {
                if (DetaOri <= 0 && DetaOri >= -180)//目标在左
                {
                    Left = true;
                }
                else//目标在右
                {
                    Left = false;
                }
            }
            DetaOri = Math.Abs(DetaOri);
            if (DetaOri >= 180)
                DetaOri = 360 - DetaOri;
            if (Left)
                DetaOri = -1 * Math.Abs(DetaOri);
            else
                DetaOri = Math.Abs(DetaOri);
            return DetaOri;
        }

        public static double CalDisPntToLine(GPSCoor Pnt, GPSCoor LineStartPnt, GPSCoor LineEndPnt)
        {
            double DisP2L;
            double Ds = CalDis(Pnt, LineStartPnt);
            double AngAP = CalOri(LineStartPnt, Pnt);
            double AngAB = CalOri(LineStartPnt, LineEndPnt);
            double AngBAP = CalDetaAngle(AngAB, AngAP);
            DisP2L = Ds * Math.Sin(AngBAP / 180 * PI);
            if (AngBAP < 0)
                DisP2L = -1 * Math.Abs(DisP2L);
            else
                DisP2L = Math.Abs(DisP2L);
            return DisP2L;
        }
    }
}
