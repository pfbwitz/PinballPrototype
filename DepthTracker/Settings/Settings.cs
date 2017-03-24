using System;

namespace DepthTracker.Settings
{
    public abstract class Settings
    {
        protected static int GetIntByKey(string key)
        {
            return Convert.ToInt32(Properties.Settings.Default[key]);
        }

        protected static void SaveSettingByKey(string key, object value)
        {
            Properties.Settings.Default[key] = value;
            SaveSettings();
        }

        protected static void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }
    }
}
