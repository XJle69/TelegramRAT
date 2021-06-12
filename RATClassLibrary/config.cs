using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace TelegramRAT
{
    public sealed class config
    {
        // Telegram settings.
        private static string telegramToken = "";
        private static string telegramChatID = "";
        private static int telegramCommandCheckDelay = 1;

        public static string TelegramToken { get => telegramToken; set => telegramToken = value; }
        public static string TelegramChatID { get => telegramChatID; set => telegramChatID = value; }
        public static string TelegramCommandCheckDelay { get => telegramCommandCheckDelay.ToString(); set => telegramCommandCheckDelay = Convert.ToInt32(value); }

        public static void Save(string file)
        {
            List<object> values = new List<object>();
            List<Type> types = new List<Type>();
            types.Add(typeof(string));

            //найдем все статические поля
            FieldInfo[] vars = typeof(config).GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                    );

            //занесем все в коллекцию и найдем все возможные типы
            foreach (FieldInfo x in vars)
            {
                values.Add(x.Name);
                values.Add(x.GetValue(null));
                if (!types.Contains(x.FieldType)) types.Add(x.FieldType);
            }

            //сериализуем коллекцию в файл 
            XmlSerializer ser = new XmlSerializer(typeof(List<object>), types.ToArray());

            Stream s = new FileStream(file, FileMode.Create);

            using (s)
            {
                ser.Serialize(s, values);
            }
        }

        //Считывает состояние всех статических полей текущего класса из XML-файла
        public static void Load(string file)
        {
            List<object> values;
            List<Type> types = new List<Type>();
            types.Add(typeof(string));

            //найдем все статические поля
            FieldInfo[] vars = typeof(config).GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
                    );

            //найдем все возможные типы
            foreach (FieldInfo x in vars)
            {
                if (!types.Contains(x.FieldType)) types.Add(x.FieldType);
            }

            //загружаем коллекцию из файла
            XmlSerializer ser = new XmlSerializer(typeof(List<object>), types.ToArray());
            Stream s = new FileStream(file, FileMode.Open);

            using (s)
            {
                values = (List<object>)ser.Deserialize(s);
            }

            for (int i = 0; i < values.Count; i += 2)
            {
                var item = vars.Where(x => x.Name == (string)values[i]).First(); //найдем поле по имени

                if (item != null)
                {
                    item.SetValue(null, values[i + 1]); //установим значение поля
                }
            }

        }
    }
}
