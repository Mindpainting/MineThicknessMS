using MineralThicknessMS.service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineralThicknessMS.view
{
    public partial class RegForm : Form
    {
        public RegForm()
        {
            InitializeComponent();
        }

        SoftRegHelper softReg = new SoftRegHelper();

        private void GetMachineCode_Click(object sender, EventArgs e)
        {
            string MachineStr = softReg.GetMNum();
            txtMachineCode.Text = MachineStr;
        }

        private void activate_Click(object sender, EventArgs e)
        {
            //获取reg文件地址
            string regFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reg.ini");

            ModifyRegistrationCodeInTextFile(regFullPath, txtActivate.Text);


            SoftRegHelper softReg = new SoftRegHelper();
            string realRegStr = softReg.GetRNum(softReg.GetMNum());

            if (realRegStr == txtActivate.Text.Substring(0, 24)
                && DateTime.Now <= SoftRegHelper.DecryptTimestamp(txtActivate.Text.Substring(24)))
            {
                MessageBox.Show("激活成功，请关闭激活页面重启软件");
            }
            else
            {
                MessageBox.Show("激活失败");
            }
        }

        //
        private void ModifyRegistrationCodeInTextFile(string filePath, string newRegistrationCode)
        {
            try
            {
                // Read all lines from the text file
                string[] lines = File.ReadAllLines(filePath);

                // Search for the line containing "RegistrationCode="
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("RegistrationCode:"))
                    {
                        // Update the registration code after the equal sign
                        lines[i] = "RegistrationCode:" + newRegistrationCode;
                        break;
                    }
                }

                // Write the modified lines back to the file
                File.WriteAllLines(filePath, lines);
                Console.WriteLine("Registration code updated successfully.");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during file manipulation
                Console.WriteLine("Error modifying the file: " + ex.Message);
            }
        }
    }
}
