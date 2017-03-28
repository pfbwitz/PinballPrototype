using DepthTracker.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DepthTracker.UI
{
    /// <summary>
    /// Interaction logic for TrackerPicker.xaml
    /// </summary>
    public partial class TrackerPicker : Window
    {
        public TrackerPicker()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var content = ((Button)sender).Content.ToString();
            Process Pinballgame = new Process();
            Pinballgame.StartInfo.FileName = @"C:\CLoudabit\Pinball.lnk";
            Process CarGame = new Process();
            CarGame.StartInfo.FileName = @"C:\CLoudabit\Racing.lnk";
            Process KingsGame = new Process();
            KingsGame.StartInfo.FileName = @"C:\CLoudabit\Kings.lnk";
            Process NeverGame = new Process();
            NeverGame.StartInfo.FileName = @"C:\CLoudabit\NeverHaveIEver.lnk";

            switch (content)
            {

                case "Pinball":
                    Pinballgame.Start();
                    new PinballTracker().ShowDialog();
                    break;
                case "Car":
                    CarGame.Start();
                    new CarTracker().ShowDialog();
                    break;
                case "Kings":
                    KingsGame.Start();
                    new ClicksTracker().ShowDialog();
                    break;
                case "Never":
                    NeverGame.Start();
                    new ClicksTracker().ShowDialog();
                    break;
            }
        }
    }
}
