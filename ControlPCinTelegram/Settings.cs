using System.Diagnostics;
using Newtonsoft.Json;

namespace ControlPCinTelegram 
{
    internal class Settings
    {
        public bool isFirstSettings = false;
        private const string nameFile = "settings.ini";
        private Parameters parameters = new Parameters();

        public bool isAutoStart
        {
            get
            {
                return parameters.isAutoStart;    // возвращаем значение свойства
            }
            set
            {
                parameters.isAutoStart = value;
                WriteSavedSettings();
            }
        }
        public bool isBlockTaskManager
        {
            get
            {
                return parameters.isBlockTaskManager;    // возвращаем значение свойства
            }
            set
            {
                parameters.isBlockTaskManager = value;
                WriteSavedSettings();
            }
        }
        public string tokenBot
        {
            get
            {
                return parameters.tokenBot;    // возвращаем значение свойства
            }
            set
            {
                parameters.tokenBot = value;
                WriteSavedSettings();
            }
        }
        public int chatIdClient
        {
            get
            {
                return parameters.chatIdClient;    // возвращаем значение свойства
            }
            set
            {
                parameters.chatIdClient = value;
                WriteSavedSettings();
            }
        }
        public Settings()
        {           
            ReadSavedSettings();
        }
        public void SaveListProceses()
        {
            Process[] processes = Process.GetProcesses();
            parameters.allowedProcesses = processes.ToList<Process>();
            WriteSavedSettings();
        }
        private bool ReadSavedSettings()
        {
            FileInfo fileInf = new FileInfo(nameFile);

            if (File.Exists(fileInf.FullName)&& fileInf.Length!=0)
            {
                try
                {
                    string json = File.ReadAllText(fileInf.Name);
                    parameters = JsonConvert.DeserializeObject<Parameters>(json);
                    isFirstSettings = true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                isFirstSettings = false;
                WriteSavedSettings();
                return false;
            }
            return true;
        }
        private void WriteSavedSettings()
        {
            FileInfo fileInf = new FileInfo(nameFile);

            if (!fileInf.Exists)
            {
                using (File.Create(nameFile)) { }
            }
            string json = JsonConvert.SerializeObject(parameters); // Сериализация в JSON-строку
            File.WriteAllText(nameFile, json); // Запись JSON-строки в файл

            return;
        }
    }
    class Parameters
    {
        public string tokenBot = null;
        public int chatIdClient = -1;
        public bool isAutoStart = false;
        public bool isBlockTaskManager = true;
        public List<Process> allowedProcesses;
    }
}
