﻿using System.Diagnostics;
using System.Configuration;
using System.Text.Json;
using System.Linq;
using System;
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
                fileInf.Create();                
            }
            string json = JsonConvert.SerializeObject(parameters); // Сериализация в JSON-строку

            File.WriteAllText(nameFile, json); // Запись JSON-строки в файл

            return;
        }
    }
    class Parameters
    {
        public bool isAutoStart = false;
        public bool isBlockTaskManager = true;
        public List<Process> allowedProcesses;
    }
}
