using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MineralThicknessMS.service
{
    public class SoftRegHelper
    {
        #region 变量
        public int[] IntCode = new int[127]; //存储密钥
        public char[] CharCode = new char[25]; //存储ASCII码
        public int[] IntNumber = new int[25]; //存储ASCII码值
        #endregion

        #region 方法
        /// <summary>
        /// 初始化存储密钥
        /// </summary>
        public void SetIntCode()
        {
            for (int i = 1; i < IntCode.Length; i++)
            {
                IntCode[i] = i % 9;
            }
        }

        ///<summary>
        /// 获取硬盘卷标号
        ///</summary>
        public string GetDiskVolumeSerialNumber()
        {
            //ManagementClass mc = new ManagementClass("win32_NetworkAdapterConfiguration");
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        ///<summary>
        /// 获取CPU序列号
        ///</summary>
        public string GetCpu()
        {
            string strCpu = null;
            ManagementClass myCpu = new ManagementClass("win32_Processor");
            ManagementObjectCollection myCpuCollection = myCpu.GetInstances();
            foreach (ManagementObject myObject in myCpuCollection)
            {
                strCpu = myObject.Properties["Processorid"].Value.ToString();
            }
            return strCpu;
        }

        ///<summary>
        /// 生成机器码(机器码由CPU序列号+硬盘卷标号合成)----可扩展
        ///</summary>
        public string GetMNum()
        {
            string strNum = GetCpu() + GetDiskVolumeSerialNumber();
            string strMNum = strNum.Substring(0, 24); //截取前24位作为机器码
            return strMNum;
        }

        ///<summary>
        /// 生成注册码（根据本机机器码生成注册码）
        ///</summary>
        public string GetRNum()
        {
            SetIntCode();
            string strMNum = GetMNum();
            for (int i = 1; i < CharCode.Length; i++) //存储机器码
            {
                CharCode[i] = Convert.ToChar(strMNum.Substring(i - 1, 1));
            }
            for (int j = 1; j < IntNumber.Length; j++) //改变ASCII码值
            {
                IntNumber[j] = Convert.ToInt32(CharCode[j]) + IntCode[Convert.ToInt32(CharCode[j])];
            }
            string strAsciiName = ""; //注册码
            for (int k = 1; k < IntNumber.Length; k++) //生成注册码
            {

                if ((IntNumber[k] >= 48 && IntNumber[k] <= 57) || (IntNumber[k] >= 65 && IntNumber[k]
                                                                   <= 90) || (IntNumber[k] >= 97 && IntNumber[k] <= 122))
                //判断如果在0-9、A-Z、a-z之间
                {
                    strAsciiName += Convert.ToChar(IntNumber[k]).ToString();
                }
                else if (IntNumber[k] > 122) //判断如果大于z
                {
                    strAsciiName += Convert.ToChar(IntNumber[k] - 10).ToString();
                }
                else
                {
                    strAsciiName += Convert.ToChar(IntNumber[k] - 9).ToString();
                }
            }
            return strAsciiName;
        }

        ///<summary>
        /// 生成注册码（根据传入的机器码生成注册码）
        ///</summary>
        ///<returns>机器码</returns>
        public string GetRNum(string machineStr)
        {
            SetIntCode();
            string strMNum = machineStr;
            for (int i = 1; i < CharCode.Length; i++) //存储机器码
            {
                CharCode[i] = Convert.ToChar(strMNum.Substring(i - 1, 1));
            }
            for (int j = 1; j < IntNumber.Length; j++) //改变ASCII码值
            {
                IntNumber[j] = Convert.ToInt32(CharCode[j]) + IntCode[Convert.ToInt32(CharCode[j])];
            }
            string strAsciiName = ""; //注册码
            for (int k = 1; k < IntNumber.Length; k++) //生成注册码
            {

                if ((IntNumber[k] >= 48 && IntNumber[k] <= 57) || (IntNumber[k] >= 65 && IntNumber[k]
                                                                   <= 90) || (IntNumber[k] >= 97 && IntNumber[k] <= 122))
                //判断如果在0-9、A-Z、a-z之间
                {
                    strAsciiName += Convert.ToChar(IntNumber[k]).ToString();
                }
                else if (IntNumber[k] > 122) //判断如果大于z
                {
                    strAsciiName += Convert.ToChar(IntNumber[k] - 10).ToString();
                }
                else
                {
                    strAsciiName += Convert.ToChar(IntNumber[k] - 9).ToString();
                }
            }
            return strAsciiName;
        }
        #endregion

        // 256位固定密钥，用于加密和解密
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("ThisIsA256BitFixedKeyForEncrypti");

        // 解密时间戳
        public static DateTime DecryptTimestamp(string encryptedTimestamp)
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedTimestamp);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;

                byte[] iv = new byte[aes.BlockSize / 8];
                Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, iv.Length, encryptedData.Length - iv.Length);
                    }

                    byte[] decryptedBytes = ms.ToArray();
                    long timestampTicks = BitConverter.ToInt64(decryptedBytes, 0);
                    return new DateTime(timestampTicks);
                }
            }
        }

    }
}
