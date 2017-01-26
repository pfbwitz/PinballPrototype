using System.Windows;
using System.Windows.Input;

namespace DepthTracker.UI
{
    public partial class CalibrationTemplateWindow : Window
    {
        public CalibrationTemplateWindow()
        {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.None;
        }
    }
}
