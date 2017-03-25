using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Threading.Tasks;
using DepthTracker.Common;
using DepthTracker.Settings;
using DepthTrackerClicks.Common;
using DepthTracker.UI;

namespace DepthTrackerClicks.UI
{
    public partial class ClicksTracker : Window, INotifyPropertyChanged, ITracker
    {
        #region properties 

        public ImageSource ImageSource { get { return _prop?.DepthBitmap; } }

        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        public TextBox XText { get { return xText; } }

        public TextBox YText { get { return yText; } }

        public TextBox ZMinText { get { return zMinText; } }

        public TextBox ZMaxText { get { return zMaxText; } }

        public TextBox WidthText { get { return widthText; } }

        public TextBox HeightText { get { return heightText; } }

        private readonly TrackerProperties<ClicksSettings> _prop;

        public string _statusText = null;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                }
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
            _prop = new TrackerProperties<ClicksSettings>(this);

            _prop.Setup();

            DataContext = this;

            InitializeComponent();

            xText.Text = _prop.Rectangle.X.ToString();
            yText.Text = _prop.Rectangle.Y.ToString();
            zMinText.Text = _prop.Settings.ZMin.ToString();
            zMaxText.Text = _prop.Settings.ZMax.ToString();
            widthText.Text = _prop.Settings.Width.ToString();
            heightText.Text = _prop.Rectangle.Height.ToString();

            _prop.Sensor_IsAvailableChanged(null, null);

            Closing += _prop.Window_Closing;
            BtnFlip.Click += _prop.BtnFlip_Click;
            BtnSwitch.Click += _prop.BtnSwitch_Click;

            xText.TextChanged += _prop.TextBox_TextChanged;
            yText.TextChanged += _prop.TextBox_TextChanged;
            widthText.TextChanged += _prop.TextBox_TextChanged;
            heightText.TextChanged += _prop.TextBox_TextChanged;
            zMinText.TextChanged += _prop.TextBox_TextChanged;
            zMaxText.TextChanged += _prop.TextBox_TextChanged;

            BtnFlip.Content = _prop.Flip ? "FLIP OFF" : "FLIP ON";
            BtnSwitch.Content = _prop.Run ? "ON" : "OFF";
        }

        #region handlers

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            int value;
            _prop.ZCalibrated = false;
            if (int.TryParse(box.Text, out value))
            {
                switch (box.Name)
                {
                    case "xText":
                        _prop.Settings.X = value;
                        _prop.LowerXBound = value;
                        break;
                    case "yText":
                        _prop.Settings.Y = value;
                        _prop.LowerYBound = value;
                        break;
                    case "widthText":
                        _prop.Settings.Width = value;
                        _prop.UpperXBound = _prop.Rectangle.X + value;
                        break;
                    case "heightText":
                        _prop.Settings.Height = value;
                        _prop.UpperYBound = _prop.Rectangle.Y + value;
                        break;
                    case "zMinText":
                        _prop.Settings.ZMin = value;
                        //zCalibrated = true;
                        break;
                    case "zMaxText":
                        _prop.Settings.ZMax = value;
                        _prop.ZCalibrated = true;
                        break;
                }
            }

            _prop.TileWidth = _prop.Rectangle.Width / 4;
            _prop.TileHeight = _prop.Rectangle.Height / 2;
        }

        #endregion

        #region methods

        public void SetTitle(string title)
        {
            Title = title;
        }

        public void SetStatusText()
        {
            StatusText = _prop.KinectSensor.IsAvailable ? "Running" : "Not connected";
        }

        private int _x;
        private int _y;

        public void PushButtons(int x, int y, bool detected)
        {
            var posXPercentage = (x - _prop.Rectangle.X) * 100 / _prop.Rectangle.Width;
            var posYPercentage = (y - _prop.Rectangle.Y) * 100 / _prop.Rectangle.Height;
            var c = false;
            if (_prop.Flip)
            {
                posXPercentage = 100 - posXPercentage;
                posYPercentage = 100 - posYPercentage;
            }

            _x = Convert.ToInt32(posXPercentage * SystemParameters.PrimaryScreenWidth / 100);
            _y = Convert.ToInt32(posYPercentage * SystemParameters.PrimaryScreenHeight / 100);

            if (!_prop.ClickHandled)
            {
                _prop.ClickHandled = detected;
                c = true;
            }

            if (!c)
                return;

            if (detected)
                DoKeyDown();
            else
                DoKeyUp();
        }

        public void DoKeyDown()
        {
            if (!_prop.Run)
                return;

            if(!_mouseDown)
            {
                SetCursorPos(_x, _y);
                PushButton(ButtonDirection.Down);
            }
            else
            {
                SetCursorPos(_x, _y);
            }
        }

        private bool _mouseDown;

        public void DoKeyUp()
        {
            if (!_prop.Run)
                return;

            if(_mouseDown)
                PushButton(ButtonDirection.Up);
        }

        public void PushButton(ButtonDirection buttonDirection)
        {
            try
            {
                switch (buttonDirection)
                {
                    case ButtonDirection.Up:
                        //_mouseDown = false;
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
                        Task.Run(async() => {
                            await Task.Delay(1000);
                            await Dispatcher.BeginInvoke(new Action(() => _mouseDown = false));
                        });
                        break;
                    case ButtonDirection.Down:
                        _mouseDown = true;
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
