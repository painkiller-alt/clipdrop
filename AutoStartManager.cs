using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace clipdrop
{
    public static class AutoStartManager
    {
        private const string AppName = "clipdrop";

        public static void SetAutoStart(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (enable)
                    {
                        string appPath = $"\"{Application.ExecutablePath}\"";
                        key.SetValue(AppName, appPath);
                    }
                    else
                    {
                        if (key.GetValue(AppName) != null)
                            key.DeleteValue(AppName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    if (key == null) return false;
                    var value = key.GetValue(AppName);
                    return value != null && value.ToString() == $"\"{Application.ExecutablePath}\"";
                }
            }
            catch
            {
                return false;
            }
        }
    }
}