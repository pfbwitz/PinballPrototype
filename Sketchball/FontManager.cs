using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sketchball
{
    public class FontManager
    {
        private static FontManager _instance = null;

        private FontFamily mechanikBold;
        private FontFamily mechanikLight;
        private FontFamily mechanikRegular;
        private System.Windows.Media.FontFamily mechanik;

        private FontManager()
        {
            PrivateFontCollection fontCollection = new PrivateFontCollection();

            // Add fonts
            fontCollection.AddFontFile(Path.Combine(Application.ExecutablePath, "..", "Resources", "mechanik-Bold.ttf"));
            fontCollection.AddFontFile(Path.Combine(Application.ExecutablePath, "..", "Resources", "mechanik-Light.ttf"));
            fontCollection.AddFontFile(Path.Combine(Application.ExecutablePath, "..", "Resources", "mechanik-Regular.ttf"));
            mechanikBold = fontCollection.Families[0];
            //mechanikLight = fontCollection.Families[1];
            //mechanikRegular = fontCollection.Families[2];

            mechanik = new System.Windows.Media.FontFamily(new Uri(Path.Combine(Application.ExecutablePath, "..", "Resources", "mechanik-Bold.ttf")), "mechanik");
        }

        private static FontManager Instance()
        {
            if (_instance == null) _instance = new FontManager();
            return _instance;
        }

        public static FontFamily pathMechanikBold
        {
            get
            {
                return Instance().mechanikBold;
            }
        }

        public static FontFamily pathMechanikLight
        {
            get
            {
                return Instance().mechanikLight;
            }
        }

        public static FontFamily pathMechanikRegular
        {
            get
            {
                return Instance().mechanikRegular;
            }
        }

        public static System.Windows.Media.FontFamily Mechanik
        {
            get {
                //var font = new Font(pathMechanikRegular, 40);
                //return new System.Windows.Media.FontFamily(font.Name);
                return Instance().mechanik;
            }
        }
    }
}
