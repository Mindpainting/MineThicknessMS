using MineralThicknessMS.config;

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
            //RWIniFile.WriteIniFile();
            RWIniFile.InitData();//��ʼ������
            Application.Run(new MainForm());
        }
    }
}