using System;
using System.Windows;
using System.Windows.Controls;
using DepthTracker.Common.Interface;

namespace DepthTracker.UI
{
    public partial class TrackerPicker : Window
    {
        public ITracker Tracker { get; private set; }

        public TrackerPicker()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
#if !DEBUG
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = string.Format(System.Configuration.ConfigurationManager.AppSettings["GameRootPath"], 
                ((Button)sender).Content.ToString());
            process.Start();
#endif
            if (Tracker != null)
            {
                Tracker.Instance.Close();
                Tracker = null;
            }

            Tracker = GetTrackerWindow(((Button)sender).Content.ToString());
            Tracker.Instance.ShowDialog();
        }

        private ITracker GetTrackerWindow(string content)
        {
            switch (content)
            {
                case "Pinball":
                    return new PinballTracker();
                case "Racing":
                    return new CarTracker();
                case "Kings":
                    return new ClicksTracker();
                case "NeverHaveIEver":
                    return new ClicksTracker();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
