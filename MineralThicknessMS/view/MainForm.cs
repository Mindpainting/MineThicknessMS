using GMap.NET.WindowsForms;
using GMap.NET;
using MineralThicknessMS.entity;
using MineralThicknessMS.service;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;
using OxyPlot.Series;
using OxyPlot;
using OxyPlot.Axes;
using System.Data;
using System.Windows.Forms;
using MineralThicknessMS.view;

namespace MineralThicknessMS
{
    public partial class MainForm : Form
    {
        private MyServer myserver;
        private Instruction instruction;
        private DataMapper dataMapper;
        private MsgDecode msgDecode;
        private GMapOverlay overlay;//GMap图层
        private GMarkerGoogle scjMarker;//水采机标记
        private GMarkerGoogle csyMarker1;//第一个测深仪标记
        private GMarkerGoogle csyMarker2;//第二个测深仪标记
        //private Thread mapThread1;//生成网格
        //private Thread mapThread2;//实时网格填色，标记水采机，测深仪

        public MainForm()
        {
            InitializeComponent();
            myserver = new MyServer(txtPort.Text.ToString());
            instruction = new Instruction();
            msgDecode = new MsgDecode();
            overlay = new GMapOverlay();
            dataMapper = new DataMapper();
            gMapControl = new GMapControl();

            timer1.Interval = 10;
            timer1.Start();
            timer1.Tick += new System.EventHandler(time1_Tick);

            timer2.Interval = 3000;
            timer2.Start();
            //timer2.Tick += new System.EventHandler(time2_Tick);

            gridInMapInit();

            //mapThread1 = new Thread(new ThreadStart(RenderMap1));
            //mapThread1.Start();

            //mapThread2 = new Thread(new ThreadStart(RenderMap2));
            //mapThread2.Start();
        }

        //生成网格
        private void gridInMapInit()
        {
            // 调接口获取延迟边界数据，绘制边界线

            // 边界点y45
/*            List<PointLatLng> boundaryPoints = new()
            {
                new PointLatLng(40.2810614738, 90.4917205106),
                new PointLatLng(40.2745895561, 90.4850289188),
                new PointLatLng(40.2647281553, 90.5022922085),
                new PointLatLng(40.2712197832, 90.5048896228),
            };*/


            //y31
            List<PointLatLng> boundaryPoints = new()
            {
                new PointLatLng(40.2531571096, 90.4901935097),
                new PointLatLng(40.2507185251, 90.4834874450),
                new PointLatLng(40.2425903988, 90.4939599525),
                new PointLatLng(40.2450507356, 90.5006782956)
            };
            BoundaryPoints.setBoundaryPoints(boundaryPoints);
            List<List<Grid>> gridList = GridView.gridBuild(boundaryPoints);
            Status.grids = gridList;

/*            BoundaryPoints.getCorrectedPoints().ForEach(point =>
            {
                //AMapMarker自定义地图标记点
                overlay.Markers.Add(new AMapMarker(point, 8));

            });*/

            //遍历每一个格子，在地图上绘制
            gridList.ForEach(aChannelGrid =>
            {
                aChannelGrid.ForEach(aGrid =>
                {
                    overlay.Polygons.Add(new(aGrid.PointLatLngs, "gridPolygon")
                    {
                        Stroke = new Pen(Color.Yellow, 2)
                    });
                });
            });
            /*            gridList.ForEach(aChannelGrid =>
                        {
                            aChannelGrid.ForEach(aGrid =>
                            {
                                overlay.Polygons.Add(new(aGrid.PointLatLngs, "gridPolygon")
                                {
                                    Stroke = new Pen(Color.Yellow, 2)
                                });
                                GMapPolygon gridPolygon = new(aGrid.PointLatLngs, "gridPolygon")
                                {
                                    Stroke = new Pen(Color.Yellow, 2),
                                };
                                Random random = new Random();
                                int randomNumber = random.Next(2, 10);
                                if (randomNumber >= 2 && randomNumber <= 4)
                                {
                                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(225, Color.YellowGreen));
                                }
                                else if (randomNumber > 4 && randomNumber <= 6)
                                {
                                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(225, Color.SkyBlue));
                                }
                                else if (randomNumber > 6 && randomNumber <= 8)
                                {
                                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(225, Color.Orange));
                                }
                                else
                                {
                                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(225, Color.MediumPurple));
                                }
                                overlay.Polygons.Add(gridPolygon);
                            });
                        });*/

            // 创建多边形
/*            GMapPolygon polygon = new(BoundaryPoints.getCorrectedPoints(), "polygon")
            {
                // 设置多边形填充颜色
                Fill = new SolidBrush(Color.FromArgb(50, Color.ForestGreen)),
                // 设置多边形边界颜色和宽度
                Stroke = new Pen(Color.Yellow, 3)
            };
            overlay.Polygons.Add(polygon);*/

            gMapControl.Overlays.Add(overlay);
        }

        private void time2_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                DataAnalysis.updateMineAvg();
            });
            System.Data.DataTable ds = DataAnalysis.mineTable();
            double sum = 0;
            foreach (DataRow row in ds.Rows)
            {
                double x = MsgDecode.StrConvertToDou(row[1]);
                sum += x;
            }

            labelTotalData.Text = "当前本盐池采矿总量为:" + (Math.Round(sum, 2)).ToString() + "m³";
            dataGridView1.DataSource = ds;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            GMapInit();//地图初始化
            Task.Run(() =>
            {
                System.Data.DataTable result = DataAnalysis.mineTable();

                dataGridView1.Invoke(new Action(() =>
                {
                    dataGridView1.DataSource = result;
                }));
            });
        }
        // GMap基础信息初始化
        public void GMapInit()
        {
            gMapControl.Bearing = 0F;
            gMapControl.CanDragMap = true;
            gMapControl.Dock = DockStyle.Fill;
            gMapControl.EmptyTileColor = Color.Navy;
            gMapControl.GrayScaleMode = true;
            gMapControl.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            gMapControl.LevelsKeepInMemory = 5;
            gMapControl.Location = new Point(0, 0);
            gMapControl.Margin = new Padding(4);
            gMapControl.MarkersEnabled = true;
            gMapControl.MaxZoom = 2;
            gMapControl.MinZoom = 2;
            gMapControl.MouseWheelZoomEnabled = true;
            gMapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            gMapControl.Name = "gMapControl";
            gMapControl.NegativeMode = false;
            gMapControl.PolygonsEnabled = true;
            gMapControl.RetryLoadTile = 0;
            gMapControl.RoutesEnabled = true;
            gMapControl.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional;
            gMapControl.SelectedAreaFillColor = Color.FromArgb(33, 65, 105, 225);
            gMapControl.ShowTileGridLines = false;
            gMapControl.Size = new Size(1859, 1362);
            gMapControl.TabIndex = 0;
            splitContainer1.Panel2.Controls.Add(gMapControl);
            gMapControl.Zoom = 0D;

            //
            gMapControl.Manager.Mode = AccessMode.ServerAndCache;
            gMapControl.CacheLocation = Environment.CurrentDirectory + "\\GMapCache\\"; //缓存位置
            //gMapControl.MapProvider = AMapProvider.Instance; //高德地图
            gMapControl.MapProvider = GMapProviders.BingSatelliteMap;
            gMapControl.MinZoom = 2;  //最小比例
            gMapControl.MaxZoom = 22; //最大比例
            gMapControl.Zoom = 13;     //当前比例
            gMapControl.ShowCenter = false; //不显示中心十字点
            gMapControl.DragButton = MouseButtons.Left; //左键拖拽地图
            gMapControl.Position = new PointLatLng(40.42734887689348, 90.79702377319336); //地图中心位置
        }

        private void time1_Tick(object sender, EventArgs e)
        {
            txtIp.Text = myserver.getIp();

            //服务端
            radioButton16.Checked = myserver.openFlag;
            radioButton15.Checked = !(myserver.openFlag);

            //水采机2
            radioButton14.Checked = Status.bracket[1];
            radioButton13.Checked = !(Status.bracket[1]);
            radioButton10.Checked = Status.soundMachine[1];
            radioButton9.Checked = !(Status.soundMachine[1]);
            label5.Text = "经度：" + Status.longitude[1] + "E";
            label6.Text = "纬度：" + Status.latitude[1] + "N";
            label7.Text = "水深：" + Status.depth[1] + "m";
            label8.Text = "矿厚：" + Status.mineDepth[1] + "m";
            label11.Text = "GPS定位状态：" + Status.GPSState[1];

            //水采机1
            radioButton18.Checked = Status.bracket[0];
            radioButton17.Checked = !(Status.bracket[0]);
            radioButton22.Checked = Status.soundMachine[0];
            radioButton21.Checked = !(Status.soundMachine[0]);
            label15.Text = "经度：" + Status.longitude[0] + "E";
            label14.Text = "纬度：" + Status.latitude[0] + "N";
            label13.Text = "水深：" + Status.depth[0] + "m";
            label12.Text = "矿厚：" + Status.mineDepth[0] + "m";
            label16.Text = "GPS定位状态：" + Status.GPSState[0];

            //渲染实时采集的数据
            //查询水采机所在位置两侧格子内所有数据
            List<DataMsg> list1 = new();
            List<DataMsg> list2 = new();
            List<List<DataMsg>> res1 = new();
            List<List<DataMsg>> res2 = new();
            //存放每个格子内分出的两组数据的平均矿厚
            double[] avgMT = new double[2];
            try
            {
                if (Status.waterwayId[0] != 0 && Status.rectangleId[0] != 0)
                {
                    if (csyMarker1 != null)
                    {
                        overlay.Markers.Remove(csyMarker1);
                        csyMarker1.Dispose();
                    }
                    csyMarker1 = new GMarkerGoogle(new PointLatLng(Status.latitude[0], Status.longitude[0]), GMarkerGoogleType.arrow);
                    overlay.Markers.Add(csyMarker1);
                    list1 = dataMapper.getRealtimeRenderData(Status.waterwayId[0], Status.rectangleId[0]);
                    res1 = KMeansClustering.KMeansCluster(list1, 2, 100);
                }
            }
            catch (Exception ex)
            {
                avgMT[0] = Status.mineDepth[0];
            }

            try
            {
                if (Status.waterwayId[1] != 0 && Status.rectangleId[1] != 0)
                {
                    if (csyMarker2 != null)
                    {
                        overlay.Markers.Remove(csyMarker2);
                        csyMarker2.Dispose();
                    }
                    csyMarker2 = new GMarkerGoogle(new PointLatLng(Status.latitude[1], Status.longitude[1]), GMarkerGoogleType.arrow);
                    overlay.Markers.Add(csyMarker2);
                    list2 = dataMapper.getRealtimeRenderData(Status.waterwayId[1], Status.rectangleId[1]);
                    res2 = KMeansClustering.KMeansCluster(list2, 2, 100);
                }
            }
            catch (Exception ex)
            {
                avgMT[1] = Status.mineDepth[1];
            }
            //临时存放每个格子平均矿厚
            double[] tempAvg = new double[2];
            //计算第一个格子内所有数据的平均矿厚
            for (int i = 0; i < res1.Count; i++)
            {
                double sum = 0.0;
                foreach (DataMsg item in res1[i])
                {
                    sum += item.mineHigh;
                }
                tempAvg[i] = sum / res1.Count * 1.0;
            }
            if (avgMT[0] == 0)
            {
                avgMT[0] = tempAvg[0] < tempAvg[1] ? tempAvg[0] : tempAvg[1];
            }

            //计算第二个格子内所有数据的平均矿厚
            for (int i = 0; i < res2.Count; i++)
            {
                double sum = 0.0;
                foreach (DataMsg item in res2[i])
                {
                    sum += item.mineHigh;
                }
                tempAvg[i] = sum / res2.Count * 1.0;
            }
            if (avgMT[1] == 0)
            {
                avgMT[1] = tempAvg[0] < tempAvg[1] ? tempAvg[0] : tempAvg[1];
            }

            //标记当前水采机位置
            //有两个点的时候才标记水采机位置，一个或没有标记无意义
            if ((Status.latitude[0] != 0 && Status.latitude[1] != 0) && (Status.longitude[0] != 0 && Status.longitude[1] != 0))
            {
                if (scjMarker != null)
                {
                    overlay.Markers.Remove(scjMarker);
                    scjMarker.Dispose();
                }
                double avgCenterLat = (Status.latitude[0] + Status.latitude[1]) / 2.0;
                double avgCenterLng = (Status.longitude[0] + Status.longitude[1]) / 2.0;
                //取两侧测深仪经纬度平均值，求水采机中心位置经纬度
                PointLatLng avgCenterLatLng = new(avgCenterLat, avgCenterLng);
                scjMarker = new(avgCenterLatLng, GMarkerGoogleType.orange);
                overlay.Markers.Add(scjMarker);
            }

            //渲染颜色
            for (int i = 0; i < avgMT.Length; i++)
            {
                Status.grids.ForEach(aChannelGrids =>
                {
                    aChannelGrids.ForEach(grid =>
                    {
                        if (grid.Column == Status.waterwayId[i] && grid.Row == Status.rectangleId[i])
                        {
                            GMapPolygon targetGridPolygon = new(grid.PointLatLngs, "targetGridPolygon");
                            if (avgMT[i] <= 0)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.White));
                            }
                            else if (avgMT[i] > 0 && avgMT[i] <= 5)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Green));
                            }
                            else if (avgMT[i] > 5 && avgMT[i] <= 10)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.YellowGreen));
                            }
                            else if (avgMT[i] > 10 && avgMT[i] <= 15)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Blue));
                            }
                            else
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Purple));
                            }
                            targetGridPolygon.Stroke = new Pen(Color.Yellow, 2);
                            overlay.Polygons.Add(targetGridPolygon);
                        }
                    });
                });
            }
        }

        //开始
        private void btnLBup_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getBracketLMoveUp());
        }

        private void btnLBdown_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getBracketLMoveDown());
        }

        private void btnLBstop_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getBracketLMoveStop());
        }

        private void btnRBup_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getBracketRMoveUp());
        }

        private void btnRBdown_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getBracketRMoveDown());
        }

        private void btnRBstop_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getBracketRMoveStop());
        }

        private void btnLTup_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getTransducerLMoveUp());
        }

        private void btnLTdown_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getTransducerLMoveDown());
        }

        private void btnLTstop_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getTransducerLMoveStop());
        }

        private void btnRTup_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getTransducerRMoveUp());
        }

        private void btnRTdown_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getTransducerRMoveDown());
        }

        private void btnRTstop_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getTransducerRMoveStop());
        }

        private void btnLWstart_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getStartWashingL());
        }

        private void btnLWstop_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getStopWashingL());
        }

        private void btnRWstart_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getStartWashingR());
        }

        private void btnRWstop_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getStopWashingR());
        }

        private void btnLHstart_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getStartTankHeatingL());
        }

        private void btnLHstop_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getStopTankHeatingL());
        }

        private void btnRHstart_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getStartTankHeatingR());
        }

        private void btnRHstop_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getStopTankHeatingR());
        }
        //结束


        //开启服务按钮
        private void btnStartService_Click(object sender, EventArgs e)
        {
            if (MsgDecode.StrConvertToDou(txtHeight1.Text) <= 0 || MsgDecode.StrConvertToDou(txtHeight2.Text) <= 0
                || txtHeight1.Enabled == true || txtHeight2.Enabled == true
                )
            {
                MessageBox.Show("请先提交下方的支架高度和盐池底板高度！", "服务开启失败");
            }
            else
            {
                if (myserver.openFlag)
                {
                    MessageBox.Show("服务已经开启，若要重新开启请先关闭服务");
                }
                else
                {
                    myserver = new MyServer(txtPort.Text.ToString());
                    myserver.tsmiStart_Click(sender, e);
                }
            }
        }
        //关闭服务按钮
        private void btnCloseService_Click(object sender, EventArgs e)
        {
            MessageBox.Show("确认要关闭服务？");
            myserver.tsmiStop_Click(sender, e);
        }

        //工具栏 侧边栏最小化
        private void tSBtnAllowLeft_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
        }

        //工具栏 侧边栏展开
        private void tSBtnAllowRight_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = false;
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            string str1_1;
            string str2_1;
            string str2_2;
            string str4_1;
            string str4_2;
            string str5_1;
            string str5_2;
            string str6_1;
            string str6_2;
            string str7_1;
            string str7_2;
            string str8_1;
            string str8_2;
            string str9_1;
            string str9_2;
            if (myserver.openFlag)
                str1_1 = "服务端状态：开启";
            else
                str1_1 = "服务端状态：关闭";

            if (Status.bracket[0])
                str2_1 = "支架状态：展开状态";
            else
                str2_1 = "支架状态：折叠状态";

            if (Status.soundMachine[0])
                str4_1 = "测深仪状态：测量状态";
            else
                str4_1 = "测深仪状态：冲洗状态";

            str5_1 = "经度：" + Status.longitude[0].ToString() + "E";
            str6_1 = "纬度：" + Status.latitude[0].ToString() + "N";
            str7_1 = "水深：" + Status.depth[0].ToString() + "m";
            str8_1 = "矿厚：" + Status.mineDepth[0].ToString() + "m";
            str9_1 = "GPS定位状态：" + Status.GPSState[0];

            String str1 = str1_1 + "\r\n" + "设备1状态：" + "\r\n" + str2_1 + "\r\n" + str4_1 + "\r\n" + str5_1 + "\r\n" + str6_1
                + "\r\n" + str7_1 + "\r\n" + str8_1 + "\r\n" + str9_1;


            if (Status.bracket[1])
                str2_2 = "支架状态：展开状态";
            else
                str2_2 = "支架状态：折叠状态";


            if (Status.soundMachine[1])
                str4_2 = "测深仪状态：测量状态";
            else
                str4_2 = "测深仪状态：冲洗状态";

            str5_2 = "经度：" + Status.longitude[1].ToString() + "E";
            str6_2 = "纬度：" + Status.latitude[1].ToString() + "N";
            str7_2 = "水深：" + Status.depth[1].ToString() + "m";
            str8_2 = "矿厚：" + Status.mineDepth[1].ToString() + "m";
            str9_2 = "GPS定位状态：" + Status.GPSState[1];

            String str2 = "设备2状态：" + "\r\n" + str2_2 + "\r\n" + str4_2 + "\r\n" + str5_2 + "\r\n" + str6_2
                + "\r\n" + str7_2 + "\r\n" + str8_2 + "\r\n" + str9_2;
            MessageBox.Show(str1 + "\r\n" + str2, "实时数据");
        }

        private void btnDataAnalysis_Click(object sender, EventArgs e)
        {
            MessageBox.Show("数据报表");
        }

        private void btnDataClean_Click(object sender, EventArgs e)
        {
            txtHeight1.Text = null;
            txtHeight2.Text = null;
            txtHeight1.Enabled = true;
            txtHeight2.Enabled = true;
        }

        private void btnDataSub_Click(object sender, EventArgs e)
        {
            if (MsgDecode.StrConvertToDou(txtHeight1.Text) <= 0 || MsgDecode.StrConvertToDou(txtHeight2.Text) <= 0)
            {
                MessageBox.Show("请先输入支架高度和盐池底板高度！");
            }
            else
            {
                Status.height1 = MsgDecode.StrConvertToDou(txtHeight1.Text);
                Status.height2 = MsgDecode.StrConvertToDou(txtHeight2.Text);
                txtHeight1.Enabled = false;
                txtHeight2.Enabled = false;
            }
        }

        private void btn_Excel_Click(object sender, EventArgs e)
        {
            DataAnalysis.ExportToExcel(DataAnalysis.mineTable());
        }

        //导入浅滩数据按钮
        private void btnInputData_Click(object sender, EventArgs e)
        {
            RadarDataInputForm form = new RadarDataInputForm();
            form.ShowDialog();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 设置对话框的标题和过滤器
            openFileDialog.Title = "选择ASC文件";
            openFileDialog.Filter = "ASC文件|*asc";
            // 显示对话框并检查用户是否点击了“确定”按钮
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                // 调用处理文件的方法，传入选定的文件路径
                //读出原始数据
                List<LaterPoint> points = DataAnalysis.ReadRadarAsciiFile(filePath);

                List<Produce> radarProduce = new List<Produce>();
                radarProduce = DataAnalysis.produceList(points);

                radarProduce = DataAnalysis.SubtractX(radarProduce, Status.height2);
                DataAnalysis.radarDataInsert(radarProduce);

                //求出所有格子的矿厚
                MessageBox.Show("雷达数据读取完成");
            }
        }

        //矿量日报表折线图
        //private void addLineChart()
        //{
        //    plotView.Controller = new PlotController();
        //    //禁用鼠标滚轮放大缩小折线图
        //    plotView.Controller.UnbindMouseWheel();

        //    //创建图标模型
        //    var plotModel = new PlotModel()
        //    {
        //        Title = "矿量日趋势",
        //        TitleFontSize = 16,
        //        TitleFontWeight = FontWeights.Bold,
        //        TitleColor = OxyColors.Black,
        //    };

        //    //x轴
        //    var xAxis = new DateTimeAxis()
        //    {
        //        Position = AxisPosition.Bottom,
        //        StringFormat = "MM/dd",
        //        Minimum = DateTimeAxis.ToDouble(new DateTime(2023, 6, 22)),
        //        Maximum = DateTimeAxis.ToDouble(new DateTime(2023, 6, 28)),
        //        //按天间隔
        //        IntervalType = DateTimeIntervalType.Days,
        //        //刻度间隔，默认60
        //        IntervalLength = 30,
        //        //label旋转一定角度
        //        Angle = -45,
        //        //Unit = "2023"
        //    };

        //    //y轴
        //    var yAxis = new LinearAxis()
        //    {
        //        Position = AxisPosition.Left,
        //        AxislineColor = OxyColors.Black,
        //        AxislineThickness = 2,
        //        Minimum = 0,
        //        Maximum = 1000,
        //        MajorStep = 100,
        //        MinorStep = 100,
        //        Unit = "立方米",
        //    };
        //    plotModel.Axes.Add(xAxis);
        //    plotModel.Axes.Add(yAxis);

        //    //创建数据列
        //    var dataSeries = new LineSeries()
        //    {
        //        Title = "Series",
        //        Color = OxyColors.YellowGreen,
        //        Points = {
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 6, 22)), DataAnalysis.dayTotalMine[6])},
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 6, 23)), DataAnalysis.dayTotalMine[5])},
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 6, 24)), DataAnalysis.dayTotalMine[4])},
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 6, 25)), DataAnalysis.dayTotalMine[3])},
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 6, 26)), DataAnalysis.dayTotalMine[2])},
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 6, 27)), DataAnalysis.dayTotalMine[1])},
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 6, 28)), DataAnalysis.dayTotalMine[0])},
        //        }
        //    };
        //    plotModel.Series.Add(dataSeries);

        //    plotView.Model = plotModel;
        //}

        ////矿量月报表折线图
        //private void addLineChart1()
        //{
        //    plotView1.Controller = new PlotController();
        //    plotView1.Controller.UnbindMouseWheel();

        //    var plotModel = new PlotModel()
        //    {
        //        Title = "矿量月趋势",
        //        TitleFontSize = 16,
        //        TitleFontWeight = FontWeights.Bold,
        //        TitleColor = OxyColors.Black,
        //    };

        //    var xAxis = new DateTimeAxis()
        //    {
        //        Position = AxisPosition.Bottom,
        //        StringFormat = "yyyy/MM",
        //        Minimum = DateTimeAxis.ToDouble(new DateTime(2023, 4, 1)),
        //        Maximum = DateTimeAxis.ToDouble(new DateTime(2023, 6, 1)),
        //        //按月间隔
        //        IntervalType = DateTimeIntervalType.Months,
        //        IntervalLength = 30,
        //        Angle = 0,
        //    };

        //    var yAxis = new LinearAxis()
        //    {
        //        Position = AxisPosition.Left,
        //        AxislineColor = OxyColors.Black,
        //        //AxislineThickness = 2,
        //        Minimum = 0,
        //        Maximum = 5000,
        //        MajorStep = 1000,
        //        MinorStep = 1000,
        //        //y轴单位
        //        Unit = "立方米"
        //        //ExtraGridlineStyle = LineStyle.None,
        //    };
        //    plotModel.Axes.Add(xAxis);
        //    plotModel.Axes.Add(yAxis);

        //    //创建数据列
        //    var dataSeries = new LineSeries()
        //    {
        //        Title = "Series",
        //        Color = OxyColors.Orange,
        //        Points = {
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 4, 1)), DataAnalysis.monthTotalMine[2]) },
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 5, 1)), DataAnalysis.monthTotalMine[1]) },
        //            { new DataPoint(DateTimeAxis.ToDouble(new DateTime(2023, 6, 1)), DataAnalysis.monthTotalMine[0]) },
        //        }
        //    };
        //    plotModel.Series.Add(dataSeries);

        //    plotView1.Model = plotModel;
        //}
    }
}
