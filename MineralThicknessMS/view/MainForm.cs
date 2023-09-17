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
using MineralThicknessMS.config;
using System.Windows.Forms.DataVisualization.Charting;
using Grid = MineralThicknessMS.service.Grid;
using Org.BouncyCastle.Asn1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO.Ports;
using Status = MineralThicknessMS.entity.Status;
using System.Reflection;

namespace MineralThicknessMS
{
    public partial class MainForm : Form
    {
        public static MyServer myserver;
        private MySerialClient myserialclient;
        public static Instruction instruction;
        private MsgDecode msgDecode;
        private GMapOverlay overlay;//GMap图层
        private GMarkerGoogle scjMarker;//水采机标记
        private GMarkerGoogle csyMarker1;//第一个测深仪标记
        private GMarkerGoogle csyMarker2;//第二个测深仪标记                                  

        public MainForm()
        {
            InitializeComponent();
            GMapInit();//地图初始化 
            InitializeSerialPortComboBox();
            Task.Run(() => { GridInMapInit(); });
            instruction = new Instruction();
            //myserver = new MyServer();
            myserialclient = new MySerialClient();
            msgDecode = new MsgDecode();
            overlay = new GMapOverlay();
        }

        private void InitializeSerialPortComboBox()
        {
            List<string> availablePortNames = new(SerialPort.GetPortNames());
            serialPortComboBox.DataSource = availablePortNames;

            if (availablePortNames.Count == 0)
            {
                //MessageBox.Show("您的设备未开启任何串口！", "警告");
            }
            else
            {
                // 加载之前保存的用户选择的串口
                string LastSelectedSerialPort = Properties.Settings.Default.LastSelectedSerialPort;
                // 保存的串口不为空
                if (!string.IsNullOrEmpty(LastSelectedSerialPort))
                {
                    if (availablePortNames.Contains(LastSelectedSerialPort))
                    {
                        serialPortComboBox.SelectedItem = LastSelectedSerialPort;
                    }
                    // 之前保存的串口，当前查出来的串口列表里没有，默认选择串口列表第一个（USB转串口，拔掉串口就没有了）
                    else
                    {
                        serialPortComboBox.SelectedItem = availablePortNames.First();
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            gMapControl.MouseDoubleClick += gMapControl_MouseDoubleClick;

            //软件启动自动开启TCP服务
            myserver = new MyServer(10001);
            myserver.tsmiStart_Click(sender, e);

            //开启定时器1
            timer1.Interval = 10;
            timer1.Start();
            timer1.Tick += new System.EventHandler(time1_Tick);

            //开启定时器2
            timer2.Interval = 30000;
            timer2.Start();
            timer2.Tick += new System.EventHandler(time2_Tick);

            timer3.Interval = 1500;
            timer3.Start();
            timer3.Tick += new System.EventHandler(mapRealTimeRender);

            timer4.Interval = 3000;
            timer4.Start();
            timer4.Tick += new System.EventHandler(chPositionMTUpdate);

            //定时冲洗定时器
            timer5.Interval = 1000;
            timer5.Start();
            timer5.Tick += new System.EventHandler(autoWashTimer);
        }

        private async void autoWashTimer(object sender, EventArgs e)
        {
            if (myserver.server.IsRunning)
            {
                TimeSpan timeNow = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                if (timeNow == dateTimePickerAutoWash.Value.TimeOfDay)
                {
                    await Task.Run(() =>
                    {
                        autoWash(sender, e);
                    });
                }
            }
        }

        //定时冲洗函数
        private async void autoWash(object sender, EventArgs e)
        {
            try
            {
                //开启左侧冲水
                myserver.Client_OnDataSent_AutoWash(sender, e, instruction.getStartWashingL());
                await Task.Delay(1);
                //开启右侧冲水
                myserver.Client_OnDataSent_AutoWash(sender, e, instruction.getStartWashingR());
                await Task.Delay(1);
                //折叠左支架
                myserver.Client_OnDataSent_AutoWash(sender, e, instruction.getBracketLMoveDown());
                await Task.Delay(1);
                //折叠右支架
                myserver.Client_OnDataSent_AutoWash(sender, e, instruction.getBracketRMoveDown());

                //冲洗时长
                double delayMilliseconds = MsgDecode.StrConvertToDou(comboBoxWashTime.Text);
                await Task.Delay((int)(delayMilliseconds * 60 * 1000));

                //关闭左侧冲水
                myserver.Client_OnDataSent_AutoWash(sender, e, instruction.getStopWashingL());
                await Task.Delay(1);
                //关闭右侧冲水
                myserver.Client_OnDataSent_AutoWash(sender, e, instruction.getStopWashingR());

                //询问继续测量还是保持折叠状态
                AutoWashEndForm autoWashEndForm = new AutoWashEndForm();
                autoWashEndForm.ShowDialog();
            }
            catch (Exception ex)
            {

            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存用户选择到应用程序配置文件
            Properties.Settings.Default.LastSelectedSerialPort = serialPortComboBox.SelectedItem?.ToString();
            Properties.Settings.Default.Save();
        }

        //更新切割机位置矿厚
        private async void chPositionMTUpdate(object sender, EventArgs e)
        {
            //有经纬度表示GPS有数据传来，只有一个GPS数据不计算
            if (Status.latitude[0] != 0 && Status.longitude[0] != 0 && Status.latitude[1] != 0 && Status.longitude[1] != 0)
            {
                //0表示左后方测深仪(HGPS)，1表示右前方测深仪(QGPS)
                if (Status.waterwayId[0] != -1 && Status.rectangleId[0] != -1 && Status.waterwayId[1] != -1 && Status.rectangleId[1] != -1)
                {
                    double[] mt = await myserialclient.GetCHPositionMT();

                    BeginInvoke(new Action(() =>
                    {
                        if (Status.ori[0] < 90 && Status.ori[1] < 90)
                        {
                            ToolStripMenuItem8.Text = "北侧矿厚：" + Math.Round(mt[0], 2).ToString("0.00") + "m";
                            ToolStripMenuItem9.Text = "南侧矿厚：" + Math.Round(mt[1], 2).ToString("0.00") + "m";
                        }
                        else
                        {
                            ToolStripMenuItem8.Text = "北侧矿厚：" + Math.Round(mt[1], 2).ToString("0.00") + "m";
                            ToolStripMenuItem9.Text = "南侧矿厚：" + Math.Round(mt[0], 2).ToString("0.00") + "m";
                        }
                        ToolStripMenuItem8.ForeColor = Color.Red;
                        ToolStripMenuItem9.ForeColor = Color.Green;
                    }));
                }
            }
        }

        // GMap基础信息初始化
        private void GMapInit()
        {
            gMapControl = new GMapControl();
            gMapControl.Bearing = 0F;
            gMapControl.CanDragMap = true;
            gMapControl.Dock = DockStyle.Fill;
            gMapControl.EmptyTileColor = Color.Navy;
            gMapControl.GrayScaleMode = true;
            gMapControl.HelperLineOption = HelperLineOptions.DontShow;
            gMapControl.LevelsKeepInMemory = 5;
            gMapControl.Location = new Point(0, 0);
            gMapControl.Margin = new Padding(4);
            gMapControl.MarkersEnabled = true;
            gMapControl.MouseWheelZoomEnabled = true;
            gMapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            gMapControl.Name = "gMapControl";
            gMapControl.NegativeMode = false;
            gMapControl.PolygonsEnabled = true;
            gMapControl.RetryLoadTile = 0;
            gMapControl.RoutesEnabled = true;
            gMapControl.ScaleMode = ScaleModes.Fractional;
            gMapControl.SelectedAreaFillColor = Color.FromArgb(33, 65, 105, 225);
            gMapControl.ShowTileGridLines = false;
            gMapControl.Size = new Size(1859, 1362);
            gMapControl.TabIndex = 0;
            gMapControl.Zoom = 0D;

            gMapControl.Manager.Mode = AccessMode.ServerAndCache;
            //缓存位置
            gMapControl.CacheLocation = Environment.CurrentDirectory + "\\GMapCache\\";

            //gMapControl.MapProvider = AMapProvider.Instance; //高德地图
            gMapControl.MapProvider = GMapProviders.BingSatelliteMap;
            gMapControl.MinZoom = 2;  //最小比例
            gMapControl.MaxZoom = 22; //最大比例
            gMapControl.Zoom = 13;     //当前比例
            gMapControl.ShowCenter = false; //不显示中心十字点
            gMapControl.DragButton = MouseButtons.Left; //左键拖拽地图
            gMapControl.Position = new PointLatLng(40.42734887689348, 90.79702377319336); //地图中心位置
            splitContainer1.Panel2.Controls.Add(gMapControl);
        }

        // 双击获取网格所在航道，网格编号
        private void gMapControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 获取鼠标点击的屏幕坐标
            int mouseX = e.X;
            int mouseY = e.Y;

            // 获取鼠标点击位置的经纬度坐标
            PointLatLng point = gMapControl.FromLocalToLatLng(mouseX, mouseY);

            // point 现在包含了鼠标点击位置的经纬度坐标
            double latitude = point.Lat;
            double longitude = point.Lng;

            Grid grid = GridView.pointInGrid(Status.grids, point);
            if (grid.Id != 0)
            {
                //MessageBox.Show(latitude.ToString() + " " + longitude.ToString());
                MessageBox.Show("航道编号: " + grid.Column + "\n" + "网格编号: " + grid.Row, "航道-网格");
            }
        }

        //生成网格并绘制网格
        private async void GridInMapInit()
        {
            List<List<Grid>> gridList = await GridView.gridBuildAsync(BoundaryPoints.boundaryPointsList());
            Status.grids = gridList;

            List<Grid> list = new();
            gridList.ForEach(aChannelGrid =>
            {
                aChannelGrid.ForEach(aGrid =>
                {
                    list.Add(aGrid);
                });
            });

            List<Produce> produces = DataAnalysis.GetLatestProduceData(DateTime.Today);

            List<GMapPolygon> polygons = new();
            for (int i = 0; i < list.Count; i++)
            {
                GMapPolygon gridPolygon = new(list[i].PointLatLngs, "gridPolygon");
                gridPolygon.Stroke = new Pen(Color.Yellow, 2);
                double mt = produces[i].AverageElevation;
                if (mt < 0)
                {
                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.White));
                }
                else if (mt > 0 && mt <= 0.5)
                {
                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Cyan));
                }
                else if (mt > 0.5 && mt <= 1)
                {
                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.SkyBlue));
                }
                else if (mt > 1 && mt <= 1.5)
                {
                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.PaleGreen));
                }
                else if (mt > 1.5 && mt <= 2)
                {
                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Yellow));
                }
                else if (mt > 2 && mt <= 2.5)
                {
                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Orange));
                }
                else if (mt > 2.5 && mt <= 3)
                {
                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.OrangeRed));
                }
                else
                {
                    gridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Purple));
                }
                polygons.Add(gridPolygon);
            }

            // 绘制网格
            BeginInvoke(new Action<List<GMapPolygon>>((polygons) =>
            {
                foreach (GMapPolygon item in polygons)
                {
                    overlay.Polygons.Add(item);
                }
                GMapPolygon polygon = new(BoundaryPoints.getCorrectedPoints(), "polygon")
                {
                    Fill = new SolidBrush(Color.FromArgb(50, Color.ForestGreen)),
                    Stroke = new Pen(Color.Yellow, 3)
                };
                overlay.Polygons.Add(polygon);
                gMapControl.Overlays.Add(overlay);
                // 强制刷新 GMap 控件
                //gMapControl.Refresh();
                splitContainer1.SplitterWidth--;
                splitContainer1.Width--;
            }), polygons);
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

        private async Task<Dictionary<int, GMapPolygon>> getDrawnGrids(double[] amt)
        {
            Dictionary<int, GMapPolygon> drawnGrids = new();
            await Task.Run(() =>
            {
                //标记是否找到格子，找到直接终结循环
                bool[] flag = new bool[2] { false, false };
                for (int i = 0; i < amt.Length; i++)
                {
                    foreach (List<Grid> aChannelGrids in Status.grids)
                    {
                        if (flag[i])
                            break;
                        foreach (Grid grid in aChannelGrids)
                        {
                            //GPS无数据，循环结束
                            if (Status.latitude[i] == 0 && Status.longitude[i] == 0)
                                break;
                            else
                            {
                                if (grid.Column == Status.waterwayId[i] && grid.Row == Status.rectangleId[i])
                                {
                                    GMapPolygon targetGridPolygon = new(grid.PointLatLngs, "targetGridPolygon");
                                    if (amt[i] < 0)
                                    {
                                        targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.White));
                                    }
                                    else if (amt[i] > 0 && amt[i] <= 0.5)
                                    {
                                        targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Cyan));
                                    }
                                    else if (amt[i] > 0.5 && amt[i] <= 1)
                                    {
                                        targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.SkyBlue));
                                    }
                                    else if (amt[i] > 1 && amt[i] <= 1.5)
                                    {
                                        targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.PaleGreen));
                                    }
                                    else if (amt[i] > 1.5 && amt[i] <= 2)
                                    {
                                        targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Yellow));
                                    }
                                    else if (amt[i] > 2 && amt[i] <= 2.5)
                                    {
                                        targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Orange));
                                    }
                                    else if (amt[i] > 2.5 && amt[i] <= 3)
                                    {
                                        targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.OrangeRed));
                                    }
                                    else
                                    {
                                        targetGridPolygon.Fill = new SolidBrush(Color.FromArgb(255, Color.Purple));
                                    }
                                    targetGridPolygon.Stroke = new Pen(Color.Yellow, 2);
                                    drawnGrids.Add(i, targetGridPolygon);
                                    flag[i] = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            });
            return drawnGrids;
        }

        private async void mapRealTimeRender(object sender, EventArgs e)
        {
            double[] amt = new double[2];
            BeginInvoke(new Action(async () =>
            {
                if (Status.latitude[0] != 0 && Status.longitude[0] != 0)
                {
                    if (Status.waterwayId[0] != -1 && Status.rectangleId[0] != -1)
                    {
                        amt[0] = Status.mineDepth[0];
                        if (csyMarker1 != null)
                        {
                            overlay.Markers.Remove(csyMarker1);
                            csyMarker1.Dispose();
                        }
                        csyMarker1 = new GMarkerGoogle(new PointLatLng(Status.latitude[0], Status.longitude[0]), GMarkerGoogleType.arrow);
                        overlay.Markers.Add(csyMarker1);
                    }
                }

                if (Status.latitude[1] != 0 && Status.longitude[1] != 0)
                {
                    if (Status.waterwayId[1] != -1 && Status.rectangleId[1] != -1)
                    {
                        amt[1] = Status.mineDepth[1];
                        if (csyMarker2 != null && overlay.Markers.Contains(csyMarker2))
                        {
                            overlay.Markers.Remove(csyMarker2);
                            csyMarker2.Dispose();
                        }
                        csyMarker2 = new GMarkerGoogle(new PointLatLng(Status.latitude[1], Status.longitude[1]), GMarkerGoogleType.arrow);
                        overlay.Markers.Add(csyMarker2);
                    }
                }

                //标记水采机位置
                if (Status.latitude[0] != 0 && Status.longitude[0] != 0 && Status.latitude[1] != 0 && Status.longitude[1] != 0 &&
                    Status.waterwayId[0] != -1 && Status.rectangleId[0] != -1 && Status.waterwayId[1] != -1 && Status.rectangleId[1] != -1)
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
                Dictionary<int, GMapPolygon> drawGrids = await getDrawnGrids(amt);
                for (int i = 0; i < drawGrids.Count; ++i)
                {
                    if (drawGrids.Count == 1)
                    {
                        int key = drawGrids.Keys.First();
                        if (Status.GPSState[key] == "固定解" && Status.depth[key] >= 0.3)
                        {
                            overlay.Polygons.Add(drawGrids[key]);
                        }
                    }
                    else
                    {
                        if (Status.GPSState[i] == "固定解" && Status.depth[i] >= 0.3)
                        {
                            overlay.Polygons.Add(drawGrids[i]);
                        }
                    }
                }
            }));
            //double[] res = await GetCHPositionMT();
        }

        private void time2_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                DataAnalysis.updateMineAvg();
            });
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            btnSearch.Enabled = false;

            await Task.Run(() =>
            {
                DataAnalysis.subMine = DataAnalysis.mineTable(dateTimePickerBegin.Value, dateTimePickerEnd.Value);
                DataAnalysis.totalSubMine = DataAnalysis.SumAverageElevation(DataAnalysis.subMine);
            });

            labelTotalData.BeginInvoke(new Action(() =>
            {
                labelTotalData.Text = "时间段内本盐池采矿总量为：" + (Math.Round(DataAnalysis.totalSubMine, 2)).ToString() + "m³";
            }));

            dataGridView1.BeginInvoke(new Action(() =>
            {
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
            }));

            btnSearch.Enabled = true;
            MessageBox.Show("查询完成");
        }

        //private void btnSearch_Click(object sender, EventArgs e)
        //{
        //    DataAnalysis.subMine = DataAnalysis.mineTable(dateTimePickerBegin.Value, dateTimePickerEnd.Value);

        //    DataAnalysis.totalSubMine = DataAnalysis.SumAverageElevation(DataAnalysis.subMine);
        //    labelTotalData.Text = "时间段内本盐池采矿总量为:" + (Math.Round(DataAnalysis.totalSubMine, 2)).ToString() + "m³";

        //    dataGridView1.Rows.Clear();

        //    foreach (Produce produce in DataAnalysis.subMine)
        //    {
        //        DataGridViewRow row = new DataGridViewRow();
        //        row.CreateCells(dataGridView1);

        //        // 设置固定值到特定的单元格
        //        row.Cells[0].Value = produce.Channel;
        //        row.Cells[1].Value = produce.AverageElevation;

        //        // 设置 Produce 对象的其他属性值
        //        row.Cells[2].Value = dateTimePickerBegin.Value.ToShortDateString();
        //        row.Cells[3].Value = dateTimePickerEnd.Value.ToShortDateString();

        //        dataGridView1.Rows.Add(row);
        //    }
        //}

        private async void time1_Tick(object sender, EventArgs e)
        {
            // Run the task asynchronously to avoid blocking the UI thread
            await UpdateUIAsync();
        }

        // Create a separate method to perform the time-consuming updates
        private async Task UpdateUIAsync()
        {
            BeginInvoke(new Action(() =>
            {
                txtIp.Text = myserver.getIp();
                //服务端
                radioButton16.Checked = myserver.server.IsRunning;
                radioButton15.Checked = !(myserver.server.IsRunning);

                //串口通信客户端
                radioButton1.Checked = myserialclient.serialClient.IsConnected;
                radioButton2.Checked = !(myserialclient.serialClient.IsConnected);

                //水采机2
                radioButton14.Checked = Status.bracket[1];
                radioButton13.Checked = !(Status.bracket[1]);
                radioButton10.Checked = Status.soundMachine[1];
                radioButton9.Checked = !(Status.soundMachine[1]);
                label5.Text = "经度：" + GridView.degTodmsToString(Status.longitude[1]) + "E";
                label6.Text = "纬度：" + GridView.degTodmsToString(Status.latitude[1]) + "N";
                label7.Text = "水深：" + Math.Round((Status.depth[1]) * Status.measureCoefficient, 2) + "m";
                label8.Text = "矿厚：" + Math.Round(Status.mineDepth[1], 2) + "m";
                label11.Text = "GPS定位状态：" + Status.GPSState[1];
                label19.Text = "数据更新时间：" + Status.dataRefreshUTCTime[1].ToString();

                //水采机1
                radioButton18.Checked = Status.bracket[0];
                radioButton17.Checked = !(Status.bracket[0]);
                radioButton22.Checked = Status.soundMachine[0];
                radioButton21.Checked = !(Status.soundMachine[0]);
                label15.Text = "经度：" + GridView.degTodmsToString(Status.longitude[0]) + "E";
                label14.Text = "纬度：" + GridView.degTodmsToString(Status.latitude[0]) + "N";
                label13.Text = "水深：" + Math.Round((Status.depth[0]) * Status.measureCoefficient, 2) + "m";
                label12.Text = "矿厚：" + Math.Round(Status.mineDepth[0], 2) + "m";
                label16.Text = "GPS定位状态：" + Status.GPSState[0];
                label10.Text = "数据更新时间：" + Status.dataRefreshUTCTime[0].ToString();
            }));
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

        private void btnSonarPositionSelfCheck_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getSonarPositionSelfCheckL());
        }

        private void btnMoveSonarPositionToSelfCheck_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getMoveSonarPositionToSelfCheckL());
        }

        private void btnSonarPositionMoveUp_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getSonarPositionMoveUpL());
        }

        private void btnSonarPositionMoveDown_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getSonarPositionMoveDownL());
        }
        //结束


        //开启服务按钮
        private void btnStartService_Click(object sender, EventArgs e)
        {
            if (myserver.server.IsRunning)
            {
                MessageBox.Show("服务已经开启，若要重新开启请先关闭服务");
            }
            else if (MsgDecode.StrConvertToInt(txtPort.Text) == 0)
            {
                MessageBox.Show("请输入端口号");
            }
            else
            {
                myserver = new MyServer(MsgDecode.StrConvertToInt(txtPort.Text));
                myserver.tsmiStart_Click(sender, e);
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
            string strSClient;
            if (myserver.server.IsRunning)
                str1_1 = "服务端状态：开启";
            else
                str1_1 = "服务端状态：关闭";

            if (myserialclient.serialClient.IsConnected)
            {
                strSClient = "串口客户端状态：连接";
            }
            else
            {
                strSClient = "串口客户端状态：关闭";
            }

            if (Status.bracket[0])
                str2_1 = "支架状态：测量状态";
            else
                str2_1 = "支架状态：折叠状态";

            if (Status.soundMachine[0])
                str4_1 = "测深仪状态：深水状态";
            else
                str4_1 = "测深仪状态：浅水状态";

            str5_1 = "经度：" + Status.longitude[0].ToString() + "E";
            str6_1 = "纬度：" + Status.latitude[0].ToString() + "N";
            str7_1 = "水深：" + Status.depth[0].ToString() + "m";
            str8_1 = "矿厚：" + Status.mineDepth[0].ToString() + "m";
            str9_1 = "GPS定位状态：" + Status.GPSState[0];

            String str1 = str1_1 + "\r\n" + strSClient + "\r\n" + "左设备状态：" + "\r\n" + str2_1 + "\r\n" + str4_1 + "\r\n" + str5_1 + "\r\n" + str6_1
                + "\r\n" + str7_1 + "\r\n" + str8_1 + "\r\n" + str9_1;


            if (Status.bracket[1])
                str2_2 = "支架状态：测量状态";
            else
                str2_2 = "支架状态：折叠状态";


            if (Status.soundMachine[1])
                str4_2 = "测深仪状态：深水状态";
            else
                str4_2 = "测深仪状态：浅水状态";

            str5_2 = "经度：" + Status.longitude[1].ToString() + "E";
            str6_2 = "纬度：" + Status.latitude[1].ToString() + "N";
            str7_2 = "水深：" + Status.depth[1].ToString() + "m";
            str8_2 = "矿厚：" + Status.mineDepth[1].ToString() + "m";
            str9_2 = "GPS定位状态：" + Status.GPSState[1];

            String str2 = "右设备状态：" + "\r\n" + str2_2 + "\r\n" + str4_2 + "\r\n" + str5_2 + "\r\n" + str6_2
                + "\r\n" + str7_2 + "\r\n" + str8_2 + "\r\n" + str9_2;
            MessageBox.Show(str1 + "\r\n" + str2, "实时数据");
        }

        private void btnDataAnalysis_Click(object sender, EventArgs e)
        {
            MessageBox.Show("数据报表");
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
            if (DataAnalysis.radarProduce.Count != 0)
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

        private async void searchDayMine_Click(object sender, EventArgs e)
        {
            searchDayMine.Enabled = false;

            await Task.Run(() =>
            {
                DataAnalysis.dayTotalMine = DataAnalysis.dayMineTable(dateDayTimePicker.Value);
                DataAnalysis.dayMine = DataAnalysis.SumAverageElevation(DataAnalysis.dayTotalMine);
            });

            // 使用 BeginInvoke 在 UI 线程上更新 labelDayTotalMine 的文本
            labelDayTotalMine.BeginInvoke(new Action(() =>
            {
                labelDayTotalMine.Text = "此时间盐池矿量储量为:" + (Math.Round(DataAnalysis.dayMine, 2)).ToString() + "m³";
            }));

            dataGridView2.BeginInvoke(new Action(() =>
            {
                dataGridView2.Rows.Clear();
                foreach (Produce produce in DataAnalysis.dayTotalMine)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView2);

                    row.Cells[0].Value = produce.Channel;
                    row.Cells[1].Value = produce.AverageElevation;

                    row.Cells[2].Value = dateDayTimePicker.Value.ToShortDateString();

                    dataGridView2.Rows.Add(row);
                }
            }));

            searchDayMine.Enabled = true;
            MessageBox.Show("查询完成");
        }


        //private void searchDayMine_Click(object sender, EventArgs e)
        //{
        //    DataAnalysis.dayTotalMine = DataAnalysis.dayMineTable(dateDayTimePicker.Value);
        //    DataAnalysis.dayMine = DataAnalysis.SumAverageElevation(DataAnalysis.dayTotalMine);

        //    labelDayTotalMine.Text = "此时间盐池矿量储量为:" + (Math.Round(DataAnalysis.dayMine, 2)).ToString() + "m³";

        //    dataGridView2.Rows.Clear();

        //    foreach (Produce produce in DataAnalysis.dayTotalMine)
        //    {
        //        DataGridViewRow row = new DataGridViewRow();
        //        row.CreateCells(dataGridView2);

        //        // 设置固定值到特定的单元格
        //        row.Cells[0].Value = produce.Channel;
        //        row.Cells[1].Value = produce.AverageElevation;

        //        // 设置 Produce 对象的其他属性值
        //        row.Cells[2].Value = dateDayTimePicker.Value.ToShortDateString();

        //        dataGridView2.Rows.Add(row);
        //    }
        //}

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

        //导出每个网格平面坐标加高程数据
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
                            string line = $"{centerXY.X},{centerXY.Y},{Status.height2 + produce.AverageElevation}";
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

        //导出每个网格平面直角坐标系相对于第一个格子(4.5, 4.5)的位置加矿厚数据
        private void btnXYData_Click(object sender, EventArgs e)
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

                        int k = 0;
                        for (int i = 0; i < GridView.channelId; i++)
                        {
                            for (int j = 0; j < GridView.gridId; j++)
                            {
                                string line = $"{4.5 + 9 * i},{4.5 + 9 * j},{produces[k++].AverageElevation}";
                                lines.Add(line);
                            }
                        }

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
                ChartForm chartForm = new(DataAnalysis.dayTotalMine, dateDayTimePicker.Value.ToShortDateString()
                    + " " + Status.saltBoundId + "盐池矿量储量分析");
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
                ChartForm chartForm = new(DataAnalysis.subMine,
                    dateTimePickerBegin.Value.ToShortDateString() + "-" + dateTimePickerEnd.Value.ToShortDateString() + " " +
                    Status.saltBoundId + "盐池采矿量分析");
                chartForm.ShowDialog();
            }
        }

        /*        private async void GetCHPositionMTTimer(object sender, EventArgs e)
                {
                    double[] mt = await GetCHPositionMT();
                }*/

        //计算水采机前后切割机位置，返回切割机所在位置矿厚
        /*        private async Task<double[]> GetCHPositionMT()
                {
                    double[] result = new double[2] { -999, -999 };
                    //0表示左后方测深仪(HGPS)，1表示右前方测深仪(QGPS)
                    if ((Status.waterwayId[0] != -1 && Status.rectangleId[0] != -1) && (Status.waterwayId[1] != -1 && Status.rectangleId[1] != -1))
                    {
                        //左后GPS
                        PointLatLng gpsLA = new(GridView.degTodms(Status.latitude[0]), GridView.degTodms(Status.longitude[0]));
                        //右前GPS
                        PointLatLng gpsRB = new(GridView.degTodms(Status.latitude[1]), GridView.degTodms(Status.longitude[1]));

                        GpsXY.GetValueOfCoordSys();
                        GpsXY.GaussBL_XY(GpsXY.m_Cs84, gpsLA.Lat, gpsLA.Lng, out double X1, out double Y1);
                        GpsXY.GaussBL_XY(GpsXY.m_Cs84, gpsRB.Lat, gpsRB.Lng, out double X2, out double Y2);

                        //平面坐标
                        PointXY pointXY1 = new(X1, Y1);
                        PointXY pointXY2 = new(X2, Y2);

                        //高程
                        double elevation1 = Status.height2;
                        double elevation2 = Status.height2;

                        GPSCoor gpsCoorLA = new(gpsLA.Lat, gpsLA.Lng, pointXY1.X, pointXY1.Y, elevation1);
                        GPSCoor gpsCoorRB = new(gpsRB.Lat, gpsRB.Lng, pointXY2.X, pointXY2.Y, elevation2);
                        GPSCoord.IniCoordSysTag();

                        List<Grid> list = new();
                        //HGPS横滚小于等于30使用HGPS计算前后切割机位置
                        if (Status.rolling[0] <= 30)
                        {
                            double oriLA = Status.ori[0];
                            //计算垂足
                            GPSCoor footDownPoint = new();
                            GPSCoord.CalPnt(gpsCoorLA, oriLA + 90, Status.GPSToCenterAxisDis, footDownPoint);

                            //切割机位置D
                            GPSCoor calPointDBefore = new();
                            GPSCoor calPointDAfter = new();
                            //
                            GPSCoord.CalPnt(footDownPoint, oriLA, Status.HDisToCH[0], calPointDBefore);
                            GPSCoord.CalPnt(footDownPoint, oriLA + 180, Status.HDisToCH[1], calPointDAfter);
                            GpsXY.GaussXY_BL(GpsXY.m_Cs0, calPointDBefore.LocN, calPointDBefore.LocE, out double B1, out double L1);
                            GpsXY.GaussXY_BL(GpsXY.m_Cs0, calPointDAfter.LocN, calPointDAfter.LocE, out double B2, out double L2);
                            list.Add(GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), Status.grids, new PointLatLng(B1, L1)));
                            list.Add(GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), Status.grids, new PointLatLng(B2, L2)));

                            PointLatLng p1 = new(GridView.dmsTodeg(B1), GridView.dmsTodeg(L1));
                            PointLatLng p2 = new(GridView.dmsTodeg(B2), GridView.dmsTodeg(L2));
                            overlay.Markers.Add(new AMapMarker(p1, 12));
                            overlay.Markers.Add(new AMapMarker(p2, 12));
                        }
                        else
                        {
                            double oriRB = Status.ori[1];

                            //计算垂足
                            GPSCoor footDownPoint = new();
                            GPSCoord.CalPnt(gpsCoorRB, oriRB - 90, Status.GPSToCenterAxisDis, footDownPoint);

                            //切割机位置D
                            GPSCoor calPointDBefore = new();
                            GPSCoor calPointDAfter = new();
                            //
                            GPSCoord.CalPnt(footDownPoint, oriRB, Status.HDisToCH[0], calPointDBefore);
                            GPSCoord.CalPnt(footDownPoint, oriRB + 180, Status.HDisToCH[1], calPointDAfter);
                            GpsXY.GaussXY_BL(GpsXY.m_Cs0, calPointDBefore.LocN, calPointDBefore.LocE, out double B1, out double L1);
                            GpsXY.GaussXY_BL(GpsXY.m_Cs0, calPointDAfter.LocN, calPointDAfter.LocE, out double B2, out double L2);
                            list.Add(GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), Status.grids, new PointLatLng(B1, L1)));
                            list.Add(GridView.selectGrid(BoundaryPoints.getCorrectedPoints(), Status.grids, new PointLatLng(B2, L2)));

                            PointLatLng p1 = new(GridView.dmsTodeg(B1), GridView.dmsTodeg(L1));
                            PointLatLng p2 = new(GridView.dmsTodeg(B2), GridView.dmsTodeg(L2));
                            overlay.Markers.Add(new AMapMarker(p1, 12));
                            overlay.Markers.Add(new AMapMarker(p2, 12));
                        }

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].Id != 0)
                            {
                                //查数据库，查该格子矿厚
                                result[i] = await DataMapper.getGridMineThickness(list[i].Column, list[i].Row);
                            }
                        }
                    }
                    return result;
                }*/

        private void ToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            string regFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reg.ini");

            //文件中的激活码
            string regStr = ReadRegFile.regCode(regFullPath);

            DateTime endTime = SoftRegHelper.DecryptTimestamp(regStr.Substring(24));

            MessageBox.Show("订阅截至时间为：" + endTime.ToString(), "订阅信息");
        }

        private void btnConnectSerial_Click(object sender, EventArgs e)
        {
            if (myserialclient.serialClient.IsConnected)
            {
                MessageBox.Show("已连接串口，若要重连请先断开", "提示");
            }
            else
            {
                if (serialPortComboBox.Items.Count > 0)
                {
                    myserialclient = new MySerialClient(serialPortComboBox.SelectedItem.ToString());
                    myserialclient.serialConnect(sender, e);
                }
                else
                {
                    MessageBox.Show("您的设备没有可用的串口，请先开启串口！", "提示");
                }
            }
        }

        private void btnDisConnectSerial_Click(object sender, EventArgs e)
        {
            if (serialPortComboBox.Items.Count > 0)
            {
                MessageBox.Show("确定要断开串口？", "提示");
                myserialclient.serialDisconnect(sender, e);
            }
            else
            {
                MessageBox.Show("当前未连接串口！", "提示");
            }
        }

        private void btnInportOuterData_Click(object sender, EventArgs e)
        {
            btnInportOuterData.Enabled = false;

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
                points = DataAnalysis.ReadOuterAsciiFile(filePath, Status.height2);

                List<Produce> outerDataProduce = new List<Produce>();
                //将原始数据转换成 航道号-网格编号-平均高程
                outerDataProduce = DataAnalysis.produceList(points);


                //将平均高程减去盐池底板高度，得到平均矿厚 航道号-网格号-平均矿厚
                outerDataProduce = DataAnalysis.SubtractX(outerDataProduce, Status.height2);

                //将记录插入数据库
                DataAnalysis.dataInsert(outerDataProduce);

                MessageBox.Show("外部数据读取完成");
            }

            btnInportOuterData.Enabled = true;
        }

        private void btnSonarPositionSelfCheckR_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getSonarPositionSelfCheckR());
        }

        private void btnMoveSonarPositionToSelfCheckR_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getMoveSonarPositionToSelfCheckR());
        }

        private void btnSonarPositionMoveUpR_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getSonarPositionMoveUpR());
        }

        private void btnSonarPositionMoveDownR_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getSonarPositionMoveDownR());
        }

        private void btnSonarPositionMaintainR_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getSonarPositionMaintainR());
        }

        private void btnSonarPositionMaintainL_Click(object sender, EventArgs e)
        {
            myserver.Client_OnDataSent(sender, e, instruction.getSonarPositionMaintainL());
        }
    }
}
