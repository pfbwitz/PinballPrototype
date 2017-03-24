namespace DepthTracker.Settings
{
    public class ClicksSettings : Settings, ISettings
    {

        private const string XKey = "XClicks";

        private const string YKey = "YClicks";

        private const string WidthKey = "WidthClicks";

        private const string HeightKey = "HeightClicks";

        private const string ZMinKey = "ZMinClicks";

        private const string ZMaxKey = "ZMaxClicks";

        public int X
        {
            get
            {
                return GetIntByKey(XKey);
            }
            set
            {
                SaveSettingByKey(XKey, value);
            }
        }

        public int Y
        {
            get
            {
                return GetIntByKey(YKey);
            }
            set
            {
                SaveSettingByKey(YKey, value);
            }
        }

        public int Width
        {
            get
            {
                return GetIntByKey(WidthKey);
            }
            set
            {
                SaveSettingByKey(WidthKey, value);
            }
        }

        public int Height
        {
            get
            {
                return GetIntByKey(HeightKey);
            }
            set
            {
                SaveSettingByKey(HeightKey, value);
            }
        }

        public int ZMin
        {
            get
            {
                return GetIntByKey(ZMinKey);
            }
            set
            {
                SaveSettingByKey(ZMinKey, value);
            }
        }

        public int ZMax
        {
            get
            {
                return GetIntByKey(ZMaxKey);
            }
            set
            {
                SaveSettingByKey(ZMaxKey, value);
            }
        }

        public void Save()
        {
            SaveSettings();
        }
      
    }
}
