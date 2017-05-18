namespace DepthTracker.Settings
{
    public class CarSettings : Settings, ISettings
    {

        private const string XKey = "XCar";

        private const string YKey = "YCar";

        private const string WidthKey = "WidthCar";

        private const string HeightKey = "HeightCar";

        private const string ZMinKey = "ZMinCar";

        private const string ZMaxKey = "ZMaxCar";

        private const string RunningKey = "RunningCar";

        private const string FlipYKey = "FlipYCar";

        private const string FlipXKey = "FlipXCar";

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
