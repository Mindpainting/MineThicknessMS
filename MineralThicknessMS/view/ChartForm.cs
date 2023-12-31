﻿using MineralThicknessMS.service;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MineralThicknessMS.view
{
    public partial class ChartForm : Form
    {
        private string chartTitle;

        public ChartForm(List<Produce> produces,string title)
        {
            InitializeComponent();
            this.chartTitle = title;
            ChartInit(produces, chartTitle);
            btn_saveAsImage.BringToFront();
        }

        public void ChartInit(List<Produce> produces,string chartTitle)
        {
            //标题
            chart1.Titles.Add(chartTitle);
            chart1.Titles[0].Font = new Font("微软雅黑", 16, FontStyle.Bold);
            chart1.Titles[0].Alignment = ContentAlignment.MiddleCenter;
            //副标题
            chart1.Titles.Add("单位：m³");
            //chart1.Titles[1].Docking = Docking.Left;
            chart1.Titles[1].Alignment = ContentAlignment.TopLeft;
            chart1.Titles[1].Font = new Font("微软雅黑", 12);

            //图表类型及数据
            chart1.Series.Clear();
            chart1.Series.Add("Series1");
            chart1.Series["Series1"].ChartType = SeriesChartType.Column;

            // 添加数据点
            foreach (Produce produce in produces)
            {
                chart1.Series["Series1"].Points.AddXY(produce.Channel, produce.AverageElevation);
            }

            //x轴
            chart1.ChartAreas[0].AxisX.Title = "航道编号";
            chart1.ChartAreas[0].AxisX.TitleFont = new Font("微软雅黑", 12);
            chart1.ChartAreas[0].AxisX.TitleForeColor = Color.Black;
            //设置刻度值和间隔
            chart1.ChartAreas[0].AxisX.Minimum = -1;
            chart1.ChartAreas[0].AxisX.Maximum = GridView.channelId + 1;
            chart1.ChartAreas[0].AxisX.Interval = 5; // 设置 X 轴间隔
            //chart1.ChartAreas[0].AxisX.MinorTickMark.Interval = 0.2; // 设置次刻度线的间隔
            //设置刻度倾斜角度
            chart1.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chart1.ChartAreas[0].AxisX.IntervalOffset = 1; // 设置刻度间隔偏移
            //解决倾斜后字体变换
            chart1.ChartAreas[0].AxisX.LabelStyle.Font = new Font("微软雅黑", 8);

            //y轴
            //chart1.ChartAreas[0].AxisY.Title = "单位：m³";
            //chart1.ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Far; //单位在轴线的位置
            //chart1.ChartAreas[0].AxisY.TextOrientation = TextOrientation.Horizontal;
            //chart1.ChartAreas[0].AxisY.TitleFont = new Font("微软雅黑", 10);
            //chart1.ChartAreas[0].AxisY.TitleForeColor = Color.Black;
            //刻度值和间隔
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 30000;
            chart1.ChartAreas[0].AxisY.Interval = 5000;


            // 隐藏水平网格线
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            // 隐藏 Y 轴线
            chart1.ChartAreas[0].AxisY.LineWidth = 0;
            // 设置水平网格线的样式
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            // 隐藏 Y 轴刻度线
            chart1.ChartAreas[0].AxisY.MajorTickMark.Enabled = false;

            //这里写会出问题，到设计器的属性里面找到对应的属性修改
            //chart1.Series["Series1"].CustomProperties = "PointWidth=0.1";
            //chart1.Series[0]["PixelPointWidth"] = "10"; // 设置柱子之间的距离

            // 清除所有图例
            chart1.Legends.Clear();
        }

        public void SaveAsImage()
        {
            // 创建 SaveFileDialog 对象
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            // 设置文件过滤器
            saveFileDialog.Filter = "PNG 图片|*.png|JPEG 图片|*.jpeg|BMP 图片|*.bmp";
            saveFileDialog.Title = "选择保存路径";

            // 显示 SaveFileDialog 对话框
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取用户选择的文件路径
                string filePath = saveFileDialog.FileName;

                // 导出为图片
                chart1.SaveImage(filePath, ChartImageFormat.Png);
                MessageBox.Show("图片保存成功！", "提示");
            }
        }

        private void Btn_SaveAsImage_Click(object sender, EventArgs e)
        {
            try
            {
                SaveAsImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("图片保存成功：" + ex.Message, "错误");
            }
        }
    }
}
