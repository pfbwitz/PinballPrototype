using DepthTracker.UI;
using System;
using System.Collections.Generic;
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
            switch (content)
            {
                case "Pinball":
                    new PinballTracker().ShowDialog();
                    
                    break;
                case "Car":
                    new CarTracker().ShowDialog();
                    break;
                case "Clicks":
                    new ClicksTracker().ShowDialog();
                    break;
            }
        }
    }
}
