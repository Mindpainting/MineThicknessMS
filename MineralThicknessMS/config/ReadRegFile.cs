using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineralThicknessMS.config
{
    public class ReadRegFile
    {
        public static string regCode(string filePath)
        {
            string registrationCode = null;

            try
            {
                // Read all lines from the text file
                string[] lines = File.ReadAllLines(filePath);

                // Search for the line containing "RegistrationCode="
                foreach (string line in lines)
                {
                    if (line.Contains("RegistrationCode:"))
                    {
                        // Extract the registration code after the equal sign
                        registrationCode = line.Split(':')[1].Trim();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during file reading
                Console.WriteLine("Error reading the file: " + ex.Message);
            }

            return registrationCode;
        }

    }
}
