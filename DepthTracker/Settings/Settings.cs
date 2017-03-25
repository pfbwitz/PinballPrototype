using System;

namespace DepthTracker.Settings
{
    public abstract class Settings
    {
        protected static int GetIntByKey(string key)
        {
            return Convert.ToInt32(Properties.Settings.Default[key]);
        }

        protected static bool GetBoolByKey(string key)
        {
            return Convert.ToBoolean(Properties.Settings.Default[key]);
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

        public bool GeneralRun
        {
            get
            {
                return GetBoolByKey("Run");
            }
            set
            {
                SaveSettingByKey("Run", value);
            }
        }

        public bool GeneralFlip
        {
            get
            {
                return GetBoolByKey("Flip");
            }
            set
            {
                SaveSettingByKey("Flip", value);
            }
        }

        public int GeneralZMin
        {
            get
            {
                return GetIntByKey("ZMin");
            }
            set
            {
                SaveSettingByKey("ZMin", value);
            }
        }

        public int GeneralZMax
        {
            get
            {
                return GetIntByKey("ZMax");
            }
            set
            {
                SaveSettingByKey("ZMax", value);
            }
        }
    }
}
