using MineralThicknessMS.entity;
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
    public partial class RadarDataInputForm : Form
    {
        public RadarDataInputForm()
        {
            InitializeComponent();
        }

        private void btnSetDepth_Click(object sender, EventArgs e)
        {
            Status.height2 = MsgDecode.StrConvertToDou(txtDepth.Text);
            this.Close();
        }
    }
}
