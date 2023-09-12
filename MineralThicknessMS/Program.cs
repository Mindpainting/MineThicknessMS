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

            //�޼�����
            RWIniFile.InitData();//��ʼ������
            Application.Run(new MainForm());

            //�м�����
            //��ȡreg�ļ���ַ
            //string regFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reg.ini");
            ////�ļ��еļ�����
            //string regStr = ReadRegFile.regCode(regFullPath);

            //SoftRegHelper softReg = new SoftRegHelper();
            //string realRegStr = softReg.GetRNum(softReg.GetMNum());

            //if (regStr.Substring(0, 24) == realRegStr && SoftRegHelper.DecryptTimestamp(regStr.Substring(24)) >= DateTime.Now)
            //{
            //    RWIniFile.InitData();//��ʼ������
            //    Application.Run(new MainForm());
            //}
            //else
            //{
            //    Application.Run(new RegForm());
            //}
        }
    }
}