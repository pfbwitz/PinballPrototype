using DepthTracker.Common;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using DepthTracker.Common.Interface;

namespace DepthTracker.UI
{
    public partial class TrackerPicker : Window
    {
        public TrackerPicker()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var content = ((Button)sender).Content.ToString();
            Process process = new Process();
            ITracker window;
           
            switch (content)
            {
                case "Pinball":
                    process.StartInfo.FileName = string.Format(ConfigurationManager.AppSettings["GameRootPath"], "Pinball");
                    window = new PinballTracker();
                    break;
                case "Car":
                    process.StartInfo.FileName = string.Format(ConfigurationManager.AppSettings["GameRootPath"], "Racing");
                    window = new CarTracker();
                    break;
                case "Kings":
                    process.StartInfo.FileName = string.Format(ConfigurationManager.AppSettings["GameRootPath"], "Kings");
                    window = new ClicksTracker();
                    break;
                case "Never":
                    process.StartInfo.FileName = string.Format(ConfigurationManager.AppSettings["GameRootPath"], "NeverHaveIEver");
                    window = new ClicksTracker();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
#if !DEBUG
            process.Start();
#endif
            window.Instance.ShowDialog();
        }
    }
}
