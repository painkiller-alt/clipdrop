using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace clipdrop
{
    public class SettingsData
    {
        public bool TopMost { get; set; } = false;
        public bool AutoStart { get; set; } = false;
    }

    public static class SettingsManager
    {
        private static readonly string _settingsPath = Path.Combine(Application.StartupPath, "settings.json");
        private static SettingsData _data;

        public static SettingsData Load()
        {
            if (_data != null) return _data;

            if (File.Exists(_settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsPath);
                    _data = JsonConvert.DeserializeObject<SettingsData>(json);
                }
                catch { }
            }

            if (_data == null)
                _data = new SettingsData();

            // Синхронизируем автозагрузку с реестром (на случай ручного изменения)
            _data.AutoStart = AutoStartManager.IsAutoStartEnabled();

            return _data;
        }

        public static void Save(SettingsData data)
        {
            _data = data;
            try
            {
                string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static bool GetTopMost()
        {
            return Load().TopMost;
        }

        public static void SetTopMost(bool value)
        {
            var data = Load();
            data.TopMost = value;
            Save(data);
        }

        public static bool GetAutoStart()
        {
            return Load().AutoStart;
        }

        public static void SetAutoStart(bool value)
        {
            var data = Load();
            data.AutoStart = value;
            Save(data);
            AutoStartManager.SetAutoStart(value); // обновляем реестр
        }
    }
}