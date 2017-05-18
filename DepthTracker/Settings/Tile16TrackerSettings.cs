namespace DepthTracker.Settings
{
    public class Tile16TrackerSettings : Settings, ISettings
    {

        private const string XKey = "X16Tile";

        private const string YKey = "Y16Tile";

        private const string WidthKey = "Width16Tile";

        private const string HeightKey = "Height16Tile";

        private const string ZMinKey = "ZMin16Tile";

        private const string ZMaxKey = "ZMax16Tile";

        private const string RunningKey = "Running16Tile";

        private const string FlipYKey = "FlipY16Tile";

        private const string FlipXKey = "FlipX16Tile";

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
