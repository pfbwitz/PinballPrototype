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

        private const string RunningKey = "RunningPinball";

        private const string FlipYKey = "FlipYPinball";

        private const string FlipXKey = "FlipXPinball";

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
            get
            {
                return GetBoolByKey(RunningKey);
            }
            set
            {
                SaveSettingByKey(RunningKey, value);
            }
        }


        public bool FlipY
        {
            get
            {
                return GetBoolByKey(FlipYKey);
            }
            set
            {
                SaveSettingByKey(FlipYKey, value);
            }
        }

        public bool FlipX
        {
            get
            {
                return GetBoolByKey(FlipXKey);
            }
            set
            {
                SaveSettingByKey(FlipXKey, value);
            }
        }

        public void Save()
        {
            SaveSettings();
        }
    }
}
