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
#if !DEBUG
            var process = new Process();
            process.StartInfo.FileName = string.Format(ConfigurationManager.AppSettings["GameRootPath"], ((Button)sender).Content.ToString());
            process.Start();
#endif
            GetTrackerWindow(((Button)sender).Content.ToString()).Instance.ShowDialog();
        }

        private ITracker GetTrackerWindow(string content)
        {
            switch (content)
            {
                case "Pinball":
                    return new PinballTracker();

                case "Car":
                    return new CarTracker();
                case "Kings":
                    return new ClicksTracker();
                case "Never":
                    return new ClicksTracker();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
