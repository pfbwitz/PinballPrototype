using System.Windows;
using System.Windows.Controls;

namespace DepthTracker.UI
{
    public interface ITracker
    {
        TextBox XText { get; }

        TextBox YText { get; }

        TextBox ZMinText { get; }

        TextBox ZMaxText { get; }

        TextBox WidthText { get; }

        TextBox HeightText { get; }

        string StatusText { get; set; }

        MessageBoxResult ShowMessage(string message, string title);

        void PushButtons(int x, int y, bool detected);

        void SetTitle(string title);
    }
}
