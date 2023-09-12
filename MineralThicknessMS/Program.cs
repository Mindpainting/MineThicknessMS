using MineralThicknessMS.config;
using MineralThicknessMS.service;
using MineralThicknessMS.view;

namespace MineralThicknessMS
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            //无激活码
            RWIniFile.InitData();//初始化数据
            Application.Run(new MainForm());

            //有激活码
            //获取reg文件地址
            //string regFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reg.ini");
            ////文件中的激活码
            //string regStr = ReadRegFile.regCode(regFullPath);

            //SoftRegHelper softReg = new SoftRegHelper();
            //string realRegStr = softReg.GetRNum(softReg.GetMNum());

            //if (regStr.Substring(0, 24) == realRegStr && SoftRegHelper.DecryptTimestamp(regStr.Substring(24)) >= DateTime.Now)
            //{
            //    RWIniFile.InitData();//初始化数据
            //    Application.Run(new MainForm());
            //}
            //else
            //{
            //    Application.Run(new RegForm());
            //}
        }
    }
}