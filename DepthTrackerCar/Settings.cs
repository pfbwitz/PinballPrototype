using System;

namespace DepthTracker
{
    public static class Settings
    {
        public static int X
        {
            get
            {
                return Convert.ToInt32(Properties.Settings.Default["X"]);
            }
            set
            {
                Properties.Settings.Default["X"] = value;
                Save();
            }
        }

        public static int Y
        {
            get
            {
                return Convert.ToInt32(Properties.Settings.Default["Y"]);
            }
            set
            {
                Properties.Settings.Default["Y"] = value;
                Save();
            }
        }

        public static int Width
        {
            get
            {
                return Convert.ToInt32(Properties.Settings.Default["Width"]);
            }
            set
            {
                Properties.Settings.Default["Width"] = value;
                Save();
            }
        }

        public static int Height
        {
            get
            {
                return Convert.ToInt32(Properties.Settings.Default["Height"]);
            }
            set
            {
                Properties.Settings.Default["Height"] = value;
                Save();
            }
        }

        public static int ZMin
        {
            get
            {
                return Convert.ToInt32(Properties.Settings.Default["ZMin"]);
            }
            set
            {
                Properties.Settings.Default["ZMin"] = value;
                Save();
            }
        }

        public static int ZMax
        {
            get
            {
                return Convert.ToInt32(Properties.Settings.Default["ZMax"]);
            }
            set
            {
                Properties.Settings.Default["ZMax"] = value;
                Save();
            }
        }

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}
