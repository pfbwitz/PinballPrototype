using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DepthTracker.Common
{
    public interface ITracker
    {
        TextBox XText { get; }

        TextBox YText { get; }

        TextBox ZMinText { get; }

        TextBox ZMaxText { get; }

        TextBox WidthText { get; }

        TextBox HeightText { get; }

        Button FlipButton { get; }

        Button SwitchButton { get; }

        string StatusText { get; set; }

        MessageBoxResult ShowMessage(string message, string title);

        void PushButtons(int x, int y, bool detected);

        Window Instance { get; }

        WriteableBitmap DepthBitmap { get; set; }
    }
}
