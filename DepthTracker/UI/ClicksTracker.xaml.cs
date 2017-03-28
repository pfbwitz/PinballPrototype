using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using DepthTracker.Settings;
using DepthTrackerClicks.Common;
using DepthTracker.Common.Worker;
using DepthTracker.Common.Interface;
using DepthTracker.Common.Enum;
using System.Windows.Media.Imaging;

namespace DepthTracker.UI
{
    public partial class ClicksTracker : Window, INotifyPropertyChanged, ITracker
    {
        #region properties 

        public WriteableBitmap DepthBitmap { get; set; }

        public bool TrackerMouseDown;

        private int _x;

        private int _y;

        public Window Instance { get { return this; } }

        public Button FlipButton { get { return BtnFlip; } }

        public Button SwitchButton { get { return BtnSwitch; } }

        public ImageSource ImageSource { get { return DepthBitmap; } }

        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        public TextBox XText { get { return xText; } }

        public TextBox YText { get { return yText; } }

        public TextBox ZMinText { get { return zMinText; } }

        public TextBox ZMaxText { get { return zMaxText; } }

        public TextBox WidthText { get { return widthText; } }

        public TextBox HeightText { get { return heightText; } }

        private readonly TrackerWorker<ClicksSettings> _trackerWorker;

        public string _statusText = string.Empty;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageBoxResult ShowMessage(string message, string title)
        {
            return MessageBox.Show(message, title);
        }

        #endregion

        public ClicksTracker()
        {
            _trackerWorker = TrackerWorker<ClicksSettings>.GetInstance(this);
        }

        public void PushButtons(int x, int y, bool detected, int lowestX, int highestX, int lowestY, int highestY)
        {
            var posXPercentage = (x - _trackerWorker.Rectangle.X) * 100 / _trackerWorker.Rectangle.Width;
            var posYPercentage = (y - _trackerWorker.Rectangle.Y) * 100 / _trackerWorker.Rectangle.Height;
            var c = false;
            if (_trackerWorker.Flip)
            {
                posXPercentage = 100 - posXPercentage;
                posYPercentage = 100 - posYPercentage;
            }

            _x = Convert.ToInt32(posXPercentage * SystemParameters.PrimaryScreenWidth / 100);
            _y = Convert.ToInt32(posYPercentage * SystemParameters.PrimaryScreenHeight / 100);

            if (!_trackerWorker.ClickHandled)
            {
                _trackerWorker.ClickHandled = detected;
                c = true;
            }

            if (!c)
                return;

            if (detected)
            {
                if (!_trackerWorker.Run)
                    return;

                //if(_x == lowestX || _x == highestX || _y == lowestY || _y == highestY)
                //{
                    //SetCursorPos(_x, _y);
                    if (!TrackerMouseDown)
                        MouseOperations.PushButton(ButtonDirection.Down, this);
                //}
            }
            else
            {
                if (!_trackerWorker.Run)
                    return;

                if (TrackerMouseDown)
                    MouseOperations.PushButton(ButtonDirection.Up, this);
            }
        }
    }
}
