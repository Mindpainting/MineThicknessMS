using MineralThicknessMS.service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineralThicknessMS.view
{
    public partial class AutoWashEndForm : Form
    {
        public AutoWashEndForm()
        {
            InitializeComponent();
        }

        //继续进行测量
        private async void btnContinueMeasuring_Click(object sender, EventArgs e)
        {
            //展开左支架
            MainForm.myserver.Client_OnDataSent_AutoWash(sender, e, MainForm.instruction.getBracketLMoveUp());
            await Task.Delay(1);
            //展开右支架
            MainForm.myserver.Client_OnDataSent_AutoWash(sender, e, MainForm.instruction.getBracketRMoveUp());
            //关闭页面
            this.Close();
        }

        //保持折叠状态
        private void btnKeepFolding_Click(object sender, EventArgs e)
        {
            //关闭页面
            this.Close();
        }
    }
}
