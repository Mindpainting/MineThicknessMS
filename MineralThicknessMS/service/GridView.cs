using GMap.NET;
using MineralThicknessMS.entity;

namespace MineralThicknessMS.service
{
    public class GridView
    {
        //航道总数
        public static int channelId = 0;
        //每个航道格子总数
        public static int gridId = 0;

        public static string degTodmsToString(double deg)
        {
            int dd, mm;
            double ss;
            dd = (int)deg;
            mm = (int)((deg - dd) * 60);
            ss = Math.Round(((deg - dd) * 60.0 - mm) * 60.0, 1);
            return $"{dd + "°" + mm + "′" + ss + "″"}";
        }

        public static double degTodms(double deg)
        {
            int dd, mm;
            double ss;
            dd = (int)deg;
            mm = (int)((deg - dd) * 60);
            ss = ((deg - dd) * 60 - mm) * 60;
            return dd + mm / 100.0 + ss / 10000.0;
        }

        public static List<List<Grid>> gridBuild(List<PointLatLng> boundaryPoints)
        {
            //边界点平面坐标
            List<PointXY> pointXY = new();
            GpsXY.GetValueOfCoordSys();
            boundaryPoints.ForEach(p =>
            {
                GpsXY.GaussBL_XY(GpsXY.m_Cs84, p.Lat, p.Lng, out double X, out double Y);
                pointXY.Add(new PointXY(X, Y));
            });

            PointXY LeftDown = pointXY[0];
            PointXY LeftUp = pointXY[1];
            PointXY RightUp = pointXY[2];
            PointXY RightDown = pointXY[3];

            double disx = Math.Sqrt(Math.Pow(RightDown.X - LeftDown.X, 2) + Math.Pow(RightDown.Y - LeftDown.Y, 2));
            double disy = Math.Sqrt(Math.Pow(LeftUp.X - LeftDown.X, 2) + Math.Pow(LeftUp.Y - LeftDown.Y, 2));

            // 9米格
            double gridSize = 9;
            int XGridCount = (int)(disx / gridSize);
            int YGridCount = (int)(disy / gridSize);
            channelId = XGridCount;
            gridId = YGridCount;

            double kx = (RightDown.Y - LeftDown.Y) / (RightDown.X - LeftDown.X);//网格x方向斜率
            double ky = (LeftUp.Y - LeftDown.Y) / (LeftUp.X - LeftDown.X);//网格y方向斜率
            double Xwidth = Math.Sqrt((disx / XGridCount * disx / XGridCount) / (kx * kx + 1));
            double Ywidth = kx * Xwidth;
            double Xheight = Math.Sqrt((disy / YGridCount * disy / YGridCount) / (ky * ky + 1));
            double Yheight = ky * Xheight;

            // 生成的每个网格（按编号）
            List<Grid> gridList = new();
            //所有航道格子（按航道分组）
            List<List<Grid>> gridsGroupByChannel = new();
            for (int i = 0; i < XGridCount; i++)
            {
                //每个航道所有格子
                List<Grid> aChannelGrids = new();
                for (int j = 0; j < YGridCount; j++)
                {
                    if (kx < 0 && ky > 0)
                    {
                        double ldx = -i * Xwidth + j * Xheight + LeftDown.X;
                        double ldy = -i * Ywidth + j * Yheight + LeftDown.Y;

                        double lux = -i * Xwidth + (j + 1) * Xheight + LeftDown.X;
                        double luy = -i * Ywidth + (j + 1) * Yheight + LeftDown.Y;

                        double rux = -(i + 1) * Xwidth + (j + 1) * Xheight + LeftDown.X;
                        double ruy = -(i + 1) * Ywidth + (j + 1) * Yheight + LeftDown.Y;

                        double rdx = -(i + 1) * Xwidth + j * Xheight + LeftDown.X;
                        double rdy = -(i + 1) * Ywidth + j * Yheight + LeftDown.Y;

                        Grid grid = new()
                        {
                            Id = i * YGridCount + j + 1,
                            CenterX = (lux + rux + ldx + rdx) / 4,
                            CenterY = (luy + ruy + ldy + rdy) / 4
                        };

                        PointXY lu = new(lux, luy);
                        PointXY ld = new(ldx, ldy);
                        PointXY rd = new(rdx, rdy);
                        PointXY ru = new(rux, ruy);

                        List<PointXY> gridPoints = new()
                            {
                                ld, lu, ru, rd
                            };

                        //把每个格子的平面坐标转换为经纬度，再转换为deg格式，存入pointLatLngs
                        List<PointLatLng> pointLatLngs = new();
                        gridPoints.ForEach(p =>
                        {
                            GpsXY.GaussXY_BL(GpsXY.m_Cs0, p.X, p.Y, out double B, out double L);
                            pointLatLngs.Add(new PointLatLng(dmsTodeg(B), dmsTodeg(L)));
                        });

                        grid.PointLatLngs = pointLatLngs;
                        grid.Column = (grid.Id - 1) / YGridCount;
                        grid.Row = (grid.Id - 1) % YGridCount;
                        gridList.Add(grid);
                        aChannelGrids.Add(grid);
                    }
                }
                //第i+1个航道添加第j+1个格子
                gridsGroupByChannel.Add(aChannelGrids);
            }
            return gridsGroupByChannel;
        }

        //给一个盐池的边界点（dms），返回按航道分组的格子
        public async static Task<List<List<Grid>>> gridBuildAsync(List<PointLatLng> boundaryPoints)
        {
            // 生成的每个网格（按编号）
            List<Grid> gridList = new();
            //所有航道格子（按航道分组）
            List<List<Grid>> gridsGroupByChannel = new();
            await Task.Run(() => 
            {
                //边界点平面坐标
                List<PointXY> pointXY = new();
                GpsXY.GetValueOfCoordSys();
                boundaryPoints.ForEach(p =>
                {
                    GpsXY.GaussBL_XY(GpsXY.m_Cs84, p.Lat, p.Lng, out double X, out double Y);
                    pointXY.Add(new PointXY(X, Y));
                });

                PointXY LeftDown = pointXY[0];
                PointXY LeftUp = pointXY[1];
                PointXY RightUp = pointXY[2];
                PointXY RightDown = pointXY[3];

                double disx = Math.Sqrt(Math.Pow(RightDown.X - LeftDown.X, 2) + Math.Pow(RightDown.Y - LeftDown.Y, 2));
                double disy = Math.Sqrt(Math.Pow(LeftUp.X - LeftDown.X, 2) + Math.Pow(LeftUp.Y - LeftDown.Y, 2));

                // 9米格
                double gridSize = 9;
                int XGridCount = (int)(disx / gridSize);
                int YGridCount = (int)(disy / gridSize);
                channelId = XGridCount;
                gridId = YGridCount;

                double kx = (RightDown.Y - LeftDown.Y) / (RightDown.X - LeftDown.X);//网格x方向斜率
                double ky = (LeftUp.Y - LeftDown.Y) / (LeftUp.X - LeftDown.X);//网格y方向斜率
                double Xwidth = Math.Sqrt((disx / XGridCount * disx / XGridCount) / (kx * kx + 1));
                double Ywidth = kx * Xwidth;
                double Xheight = Math.Sqrt((disy / YGridCount * disy / YGridCount) / (ky * ky + 1));
                double Yheight = ky * Xheight;

                for (int i = 0; i < XGridCount; i++)
                {
                    //每个航道所有格子
                    List<Grid> aChannelGrids = new();
                    for (int j = 0; j < YGridCount; j++)
                    {
                        if (kx < 0 && ky > 0)
                        {
                            double ldx =  -i* Xwidth +j * Xheight + LeftDown.X;
                            double ldy =  -i* Ywidth +j * Yheight + LeftDown.Y;

                            double lux = -i * Xwidth + (j + 1) * Xheight + LeftDown.X;
                            double luy = -i * Ywidth + (j + 1) * Yheight + LeftDown.Y;

                            double rux = -(i + 1) * Xwidth + (j + 1) * Xheight + LeftDown.X;
                            double ruy = -(i + 1) * Ywidth + (j + 1) * Yheight + LeftDown.Y;

                            double rdx = -(i + 1) * Xwidth + j * Xheight + LeftDown.X;
                            double rdy = -(i + 1) * Ywidth + j * Yheight + LeftDown.Y;

                            Grid grid = new()
                            {
                                Id = i * YGridCount + j + 1,
                                CenterX = (lux + rux + ldx + rdx) / 4,
                                CenterY = (luy + ruy + ldy + rdy) / 4
                            };

                            PointXY ld = new(ldx, ldy);
                            PointXY lu = new(lux, luy);
                            PointXY ru = new(rux, ruy);
                            PointXY rd = new(rdx, rdy);
                            
                            List<PointXY> gridPoints = new()
                            {
                                ld, lu, ru, rd
                            };

                            //把每个格子的平面坐标转换为经纬度，再转换为deg格式，存入pointLatLngs
                            List<PointLatLng> pointLatLngs = new();
                            gridPoints.ForEach(p =>
                            {
                                GpsXY.GaussXY_BL(GpsXY.m_Cs0, p.X, p.Y, out double B, out double L);
                                pointLatLngs.Add(new PointLatLng(dmsTodeg(B), dmsTodeg(L)));
                            });

                            grid.PointLatLngs = pointLatLngs;
                            grid.Column = (grid.Id - 1) / YGridCount;
                            grid.Row = (grid.Id - 1) % YGridCount;
                            gridList.Add(grid);
                            aChannelGrids.Add(grid);
                        }
                    }
                    //第i个航道添加第j个格子
                    gridsGroupByChannel.Add(aChannelGrids);
                }
            });          
            return gridsGroupByChannel;
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

        //给一个盐池四个角点(deg)，按航道分组的格子，一个dms经纬度点，返回该点所在格子，id不为零在格子内，否则不在
        public static Grid selectGrid(List<PointLatLng> correctedPoints, List<List<Grid>> gridsGroupByChannel, PointLatLng dmsPoint)
        {
            PointLatLng degPoint = new(dmsTodeg(dmsPoint.Lat), dmsTodeg(dmsPoint.Lng));          
            return pointInGrid(correctedPoints, gridsGroupByChannel, degPoint);
        }

        //计算叉积
        public static double getCross(PointLatLng p1, PointLatLng p2, PointLatLng p)
        {
            return (p2.Lat - p1.Lat) * (p.Lng - p1.Lng) - (p.Lat - p1.Lat) * (p2.Lng - p1.Lng);
        }

        //判断一个点是否在 bPoints(deg)围成的矩形内，point(deg)
        public static bool pointInRectangle(List<PointLatLng> bPoints, PointLatLng point)
        {
            if ((getCross(bPoints[1], bPoints[0], point) * getCross(bPoints[3], bPoints[2], point) >= 0)
                  && (getCross(bPoints[0], bPoints[3], point) * getCross(bPoints[2], bPoints[1], point) >= 0))
            {
                return true;
            }
            return false;
        }
       
        //给一个盐池四个角点(deg)，按航道分组的格子，一个ded经纬度点，返回该点所在格子，id不为0在格子内，否则不在
        public static Grid pointInGrid(List<PointLatLng> correctedPoints, List<List<Grid>> gridsGroupByChannel, PointLatLng degPoint)
        {
            Grid targetGrid = new() { Id = 0 };
            //点在盐池内，然后再判断在哪个格子
            if (pointInRectangle(correctedPoints, degPoint))
            {
                gridsGroupByChannel.ForEach(aChannelGrids =>
                {
                    //当前航道第一个格子
                    Grid first = aChannelGrids.First();
                    //当前航道最后个格子
                    Grid last = aChannelGrids.Last();
                    List<PointLatLng> cPoints = new()
                    {
                        first.PointLatLngs[0],
                        last.PointLatLngs[1],
                        last.PointLatLngs[2],
                        first.PointLatLngs[3],
                    };
                    if (pointInRectangle(cPoints, degPoint))
                    {
                        aChannelGrids.ForEach(grid =>
                        {
                            if ((getCross(grid.PointLatLngs[1], grid.PointLatLngs[0], degPoint) * getCross(grid.PointLatLngs[3], grid.PointLatLngs[2], degPoint) >= 0)
                             && (getCross(grid.PointLatLngs[0], grid.PointLatLngs[3], degPoint) * getCross(grid.PointLatLngs[2], grid.PointLatLngs[1], degPoint) >= 0))
                            {
                                targetGrid = grid;
                            }
                        });
                    }
                });
            }
            return targetGrid;
        }

        //给一个盐池按航道分组的格子，一个deg经纬度点，返回该点所在格子，id不为0在格子内，否则不在
        public static Grid pointInGrid(List<List<Grid>> gridsGroupByChannel, PointLatLng degPoint)
        {
            Grid targetGrid = new() { Id = 0 };
            //点在盐池内，然后再判断在哪个格子
            if (pointInRectangle(BoundaryPoints.getCorrectedPoints(), degPoint))
            {
                gridsGroupByChannel.ForEach(aChannelGrids =>
                {
                    //当前航道第一个格子
                    Grid first = aChannelGrids.First();
                    //当前航道最后个格子
                    Grid last = aChannelGrids.Last();
                    List<PointLatLng> cPoints = new()
                    {
                        first.PointLatLngs[0],
                        last.PointLatLngs[1],
                        last.PointLatLngs[2],
                        first.PointLatLngs[3],
                    };
                    if (pointInRectangle(cPoints, degPoint))
                    {
                        aChannelGrids.ForEach(grid =>
                        {
                            if ((getCross(grid.PointLatLngs[1], grid.PointLatLngs[0], degPoint) * getCross(grid.PointLatLngs[3], grid.PointLatLngs[2], degPoint) >= 0)
                             && (getCross(grid.PointLatLngs[0], grid.PointLatLngs[3], degPoint) * getCross(grid.PointLatLngs[2], grid.PointLatLngs[1], degPoint) >= 0))
                            {
                                targetGrid = grid;
                            }
                        });
                    }
                });
            }
            return targetGrid;
        }

        //给一个平面坐标点，返回一个经纬度点(dms)
        public static PointLatLng pointXYToBL(PointXY point)
        {
            GpsXY.GetValueOfCoordSys();
            GpsXY.GaussXY_BL(GpsXY.m_Cs0, point.X, point.Y, out double B, out double L);
            return new PointLatLng(B, L);
        }

        //返回给定航道网格的中心点平面坐标
        public static PointXY getCenterXY(int channelId, int gridId)
        {
            PointXY xy = new(0, 0);
            Status.grids.ForEach(aChannelGrids =>
            {
                if (aChannelGrids.First().Column == channelId)
                {
                    aChannelGrids.ForEach(grid => {
                        if (grid.Row == gridId)
                        {
                            xy.X = grid.CenterX;
                            xy.Y = grid.CenterY;
                        }
                    });
                }
            });
            return xy;
        }
    }

    public class Grid
    {
        private int id;//格子编号，记录有多少个格子
        private int row;//行（格子编号）
        private int column;//列（航道编号）
        private double centerX;
        private double centerY;
        private List<PointLatLng> pointLatLngs = new();//格子四个角点（deg）
        public Grid() { }

        public int Id { get => id; set => id = value; }
        public int Row { get => row; set => row = value; }
        public int Column { get => column; set => column = value; }
        public List<PointLatLng> PointLatLngs { get => pointLatLngs; set => pointLatLngs = value; }
        public double CenterX { get => centerX; set => centerX = value; }
        public double CenterY { get => centerY; set => centerY = value; }
    }

    //盐池四个角点类
    public class BoundaryPoints
    {
        public static PointLatLng LeftDown; //左下角点 1
        public static PointLatLng LeftUp;   //左上角点 2      
        public static PointLatLng RightUp;  //右上角点 3
        public static PointLatLng RightDown;//右下角点 4

        public static void setBoundaryPoints(List<PointLatLng> points)
        {
            LeftDown = points[0];
            LeftUp = points[1];
            RightUp = points[2];
            RightDown = points[3];
        }

        public static List<PointLatLng> boundaryPointsList()
        {   
            return new List<PointLatLng>()
            {
                LeftDown,
                LeftUp,
                RightUp,
                RightDown,
            };
        }

        //获取dms(度分秒)=>deg(度)后的角点 
        public static List<PointLatLng> getCorrectedPoints()
        {
            return new List<PointLatLng>()
            {
                new PointLatLng(GridView.dmsTodeg(LeftDown.Lat), GridView.dmsTodeg(LeftDown.Lng)),
                new PointLatLng(GridView.dmsTodeg(LeftUp.Lat), GridView.dmsTodeg(LeftUp.Lng)),
                new PointLatLng(GridView.dmsTodeg(RightUp.Lat), GridView.dmsTodeg(RightUp.Lng)),
                new PointLatLng(GridView.dmsTodeg(RightDown.Lat), GridView.dmsTodeg(RightDown.Lng)),
            };
        }
    }
    
    //平面坐标
    public class PointXY
    {
        private double x;
        private double y;

        public PointXY(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
    }
}
