using GMap.NET.WindowsForms;
using GMap.NET;
using MineralThicknessMS.entity;
using MineralThicknessMS.service;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;
using System.Data;
using MineralThicknessMS.view;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
            timer2.Tick += new System.EventHandler(time2_Tick);

            GridInMapInit();

            //mapThread1 = new Thread(new ThreadStart(RenderMap1));
            //mapThread1.Start();

            //mapThread2 = new Thread(new ThreadStart(RenderMap2));
            //mapThread2.Start();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            GMapInit();//地图初始化
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
        //生成网格
        private void GridInMapInit()
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

            /*            correctedPoints.ForEach(point =>
                        {
                            //AMapMarker自定义地图标记点
                            overlay.Markers.Add(new AMapMarker(point, 8));

                        });*/
            BoundaryPoints.setBoundaryPoints(boundaryPoints);
            List<List<Grid>> gridList = GridView.gridBuild(boundaryPoints);
            Status.grids = gridList;

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

            // 创建多边形
            GMapPolygon polygon = new(BoundaryPoints.getCorrectedPoints(), "polygon")
            {
                // 设置多边形填充颜色
                Fill = new SolidBrush(Color.FromArgb(50, Color.ForestGreen)),
                // 设置多边形边界颜色和宽度
                Stroke = new Pen(Color.Yellow, 3)
            };
            overlay.Polygons.Add(polygon);

            gMapControl.Overlays.Add(overlay);
        }

        //绘制图例
        private void ChildLegendPanel_Paint(object sender, PaintEventArgs e)
        {
            // 在子容器的 Panel 的 Paint 事件中进行绘制
            Graphics graphics = e.Graphics;

            // 定义渐变颜色的起始颜色和结束颜色
            Color[] colors = { Color.Red, Color.OrangeRed, Color.Orange, Color.Yellow, Color.PaleGreen, Color.SkyBlue, Color.Cyan };

            // 定义每个渐变颜色的位置
            float[] positions = { 0.0f, 0.1666f, 0.332f, 0.498f, 0.664f, 0.83f, 1.0f };

            // 获取父容器的绘图上下文
            Graphics parentGraphics = parentLegendPanel.CreateGraphics();

            // 绘制渐变色矩形
            Rectangle rect = childLegendPanel.ClientRectangle;
            using LinearGradientBrush brush = new(rect, Color.White, Color.White, LinearGradientMode.Vertical);
            ColorBlend colorBlend = new()
            {
                Colors = colors,
                Positions = positions
            };
            brush.InterpolationColors = colorBlend;

            // 在子容器中绘制渐变色矩形
            graphics.FillRectangle(brush, rect);

            // 在父容器中绘制刻度线
            for (int i = 0; i < positions.Length; i++)
            {
                int x1 = childLegendPanel.Left;
                int x2 = childLegendPanel.Left - 10;
                int y = (int)(childLegendPanel.Top + childLegendPanel.Height * positions[i]);

                // 在父容器的绘图上下文中绘制刻度线
                parentGraphics.DrawLine(Pens.Black, x1, y, x2, y);
                // 绘制文本
                string text = $"{3 - 0.5 * i}m";
                SizeF textSize = parentGraphics.MeasureString(text, Font);
                PointF textPosition = new(x1 - childLegendPanel.Width * 2.5f, y - textSize.Height / 2);
                // 设置文本对齐方式为左对齐
                StringFormat format = new()
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };
                parentGraphics.DrawString(text, Font, Brushes.Black, textPosition);
            }
        }

        private void time2_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                DataAnalysis.updateMineAvg();
            });
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DataAnalysis.subMine = DataAnalysis.mineTable(dateTimePickerBegin.Value, dateTimePickerEnd.Value);


            DataAnalysis.totalSubMine = DataAnalysis.SumAverageElevation(DataAnalysis.subMine);
            labelTotalData.Text = "时间段内本盐池采矿总量为:" + (Math.Round(DataAnalysis.totalSubMine, 2)).ToString() + "m³";

            dataGridView1.Rows.Clear();

            foreach (Produce produce in DataAnalysis.subMine)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);

                // 设置固定值到特定的单元格
                row.Cells[0].Value = produce.Channel;
                row.Cells[1].Value = produce.AverageElevation;

                // 设置 Produce 对象的其他属性值
                row.Cells[2].Value = dateTimePickerBegin.Value.ToShortDateString();
                row.Cells[3].Value = dateTimePickerEnd.Value.ToShortDateString();

                dataGridView1.Rows.Add(row);
            }
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
                            else if (avgMT[i] > 0 && avgMT[i] <= 0.5)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Cyan));
                            }
                            else if (avgMT[i] > 0.5 && avgMT[i] <= 1)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.SkyBlue));
                            }
                            else if (avgMT[i] > 1 && avgMT[i] <= 1.5)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.PaleGreen));
                            }
                            else if (avgMT[i] > 1.5 && avgMT[i] <= 2)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Yellow));
                            }
                            else if (avgMT[i] > 2 && avgMT[i] <= 2.5)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Orange));
                            }
                            else if (avgMT[i] > 2.5 && avgMT[i] <= 3)
                            {
                                targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.OrangeRed));
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
            if (dataGridView2.Rows.Count == 1)
            {
                MessageBox.Show("请先查询报表!");
            }
            else
            {
                DataAnalysis.ExportToExcel(DataAnalysis.subMine, dateTimePickerBegin.Value, dateTimePickerEnd.Value, DataAnalysis.totalSubMine);
            }
        }

        //导入雷达数据按钮
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

                List<LaterPoint> points = new List<LaterPoint>();

                // 调用处理文件的方法，传入选定的文件路径
                //读出原始数据
                points = DataAnalysis.ReadRadarAsciiFile(filePath, Status.height2);

                DataAnalysis.radarProduce.Clear();
                DataAnalysis.radarProduce = DataAnalysis.produceList(points);

                //求出所有格子的矿厚
                MessageBox.Show("雷达数据读取完成");
            }
        }

        //导入摩托艇数据
        private void btnInportBoatData_Click(object sender, EventArgs e)
        {
            RadarDataInputForm form = new RadarDataInputForm();
            form.ShowDialog();

            if (DataAnalysis.motorProduce.Count != 0)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                // 设置对话框的标题和过滤器
                openFileDialog.Title = "选择DAT文件";
                openFileDialog.Filter = "DAT文件|*dat";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    List<LaterPoint> points = new List<LaterPoint>();

                    // 调用处理文件的方法，传入选定的文件路径
                    //读出原始数据
                    points = DataAnalysis.ReadMotorAsciiFile(filePath, Status.height2);

                    DataAnalysis.motorProduce.Clear();
                    //按航道号，网格编号，平均高程生成记录
                    DataAnalysis.motorProduce = DataAnalysis.produceList(points);

                    List<Produce> totalProduce = new List<Produce>();

                    int count = DataAnalysis.radarProduce.Count;

                    //合并
                    totalProduce = DataAnalysis.MergeAndCalculateAverage(DataAnalysis.radarProduce, DataAnalysis.motorProduce);

                    //将平均高程减去盐池底板高度，得到平均矿厚
                    totalProduce = DataAnalysis.SubtractX(totalProduce, Status.height2);

                    //将记录插入数据库
                    DataAnalysis.dataInsert(totalProduce);

                    MessageBox.Show("摩托艇数据读取完成");
                }
            }
            else
            {
                MessageBox.Show("请先导入雷达数据!");
            }

        }

        private void searchDayMine_Click(object sender, EventArgs e)
        {
            DataAnalysis.dayTotalMine = DataAnalysis.dayMineTable(dateDayTimePicker.Value);
            DataAnalysis.dayMine = DataAnalysis.SumAverageElevation(DataAnalysis.dayTotalMine);

            labelDayTotalMine.Text = "此时间盐池矿量储量为:" + (Math.Round(DataAnalysis.dayMine, 2)).ToString() + "m³";

            dataGridView2.Rows.Clear();

            foreach (Produce produce in DataAnalysis.dayTotalMine)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView2);

                // 设置固定值到特定的单元格
                row.Cells[0].Value = produce.Channel;
                row.Cells[1].Value = produce.AverageElevation;

                // 设置 Produce 对象的其他属性值
                row.Cells[2].Value = dateDayTimePicker.Value.ToShortDateString();

                dataGridView2.Rows.Add(row);
            }
        }

        private void btnExportDayExcel_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 1)
            {
                MessageBox.Show("请先查询报表!");
            }
            else
            {
                DataAnalysis.ExportDayMineToExcel(DataAnalysis.dayTotalMine, dateDayTimePicker.Value, DataAnalysis.dayMine);
            }
        }

        private void btn3DData_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 1)
            {
                MessageBox.Show("请先查询报表!");
            }
            else
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "文本文件|*.txt|所有文件|*.*", // 可以根据需求设置过滤器
                    Title = "选择导出文件的保存位置"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    try
                    {
                        List<string> lines = new();
                        List<Produce> produces = DataAnalysis.latestData;
                        produces.ForEach(produce =>
                        {
                            PointXY centerXY = GridView.getCenterXY(produce.Channel, produce.Grid);
                            string line = $"{centerXY.X},{centerXY.Y},777.666";
                            lines.Add(line);
                        });

                        using (StreamWriter writer = new(filePath, true))
                        {
                            foreach (string line in lines)
                            {
                                writer.WriteLine(line);
                            }
                        }
                        MessageBox.Show("数据导出成功！", "提示");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("数据导出失败：" + ex.Message, "错误");
                    }
                }
            }
        }

        //查看当日每个航道采矿量报表图
        private void btnChart1_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 1)
            {
                MessageBox.Show("请先查询报表!");
            }
            else
            {
                ChartForm chartForm = new(DataAnalysis.dayTotalMine);
                chartForm.ShowDialog();
            }
        }

        //查看一个时间段每个航道采矿量报表图
        private void btnChart2_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 1)
            {
                MessageBox.Show("请先查询报表!");
            }
            else
            {
                ChartForm chartForm = new(DataAnalysis.subMine);
                chartForm.ShowDialog();
            }
        }
    }
}
