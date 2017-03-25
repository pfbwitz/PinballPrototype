namespace DepthTracker.Settings
{
    public class PinballSettings : Settings, ISettings
    {

        private const string XKey = "XPinball";

        private const string YKey = "YPinball";

        private const string WidthKey = "WidthPinball";

        private const string HeightKey = "HeightPinball";

        private const string ZMinKey = "ZMinPinball";

        private const string ZMaxKey = "ZMaxPinball";

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
            get { return GeneralZMin; }
            set { GeneralZMin = value; }
        }

        public int ZMax
        {
            get { return GeneralZMax; }
            set { GeneralZMax = value; }
        }

        public bool Run
        {
            get { return GeneralRun; }
            set { GeneralRun = value; }
        }

        public void Save()
        {
            SaveSettings();
        }
      
        public bool Flip
        {
            get { return GeneralFlip;  }
            set { GeneralFlip = value; }
        }
    }
}
