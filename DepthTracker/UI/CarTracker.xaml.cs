using System;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using WindowsInput.Native;
using System.Windows.Controls;
using DepthTracker.Common;
using DepthTracker.Settings;
using System.Windows.Media;

namespace DepthTracker.UI
{
    public partial class CarTracker : Window, ITracker, INotifyPropertyChanged
    {
        #region properties 

        public ImageSource ImageSource { get { return _prop?.DepthBitmap; } }

        public TextBox XText { get { return xText; } }

        public TextBox YText { get { return yText; } }

        public TextBox ZMinText { get { return zMinText; } }

        public TextBox ZMaxText { get { return zMaxText; } }

        public TextBox WidthText { get { return widthText; } }

        public TextBox HeightText { get { return heightText; } }

        private readonly TrackerProperties<CarSettings> _prop;

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

        public Dictionary<VirtualKeyCode, bool> Keys = new Dictionary<VirtualKeyCode, bool> {
            { VirtualKeyCode.LEFT, false},
            { VirtualKeyCode.RETURN, false},
            { VirtualKeyCode.RIGHT, false},
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageBoxResult ShowMessage(string message, string title)
        {
            return MessageBox.Show(message, title);
        }

        #endregion

        public CarTracker()
        {
            _prop = new TrackerProperties<CarSettings>(this);

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
                        _prop.Rectangle.X = value;
                        _prop.LowerXBound = value;
                        break;
                    case "yText":
                        _prop.Settings.Y = value;
                        _prop.Rectangle.Y = value;
                        _prop.LowerYBound = value;
                        break;
                    case "widthText":
                        _prop.Settings.Width = value;
                        _prop.Rectangle.Width = value;
                        _prop.UpperXBound = _prop.Rectangle.X + value;
                        break;
                    case "heightText":
                        _prop.Settings.Height = value;
                        _prop.Rectangle.Height = value;
                        _prop.UpperYBound = _prop.Rectangle.Y + value;
                        break;
                    case "zMinText":
                        _prop.Settings.ZMin = value;
                        _prop.ZCalibrated = true;
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

        public void PushButtons(int x, int y, bool detected)
        {
            #region determine button

            VirtualKeyCode keyCode = VirtualKeyCode.VK_0;
            if (x > _prop.Rectangle.X && x <= _prop.TileWidth + _prop.Rectangle.X)
            {
                if (_prop.Flip)
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.AHandled)
                        {
                            _prop.AHandled = detected;
                            keyCode = VirtualKeyCode.LEFT;
                        }
                    }
                    else
                    {
                        if (!_prop.QHandled)
                        {
                            _prop.QHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
                else
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.QHandled)
                        {
                            _prop.QHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_prop.AHandled)
                        {
                            _prop.AHandled = detected;
                            keyCode = VirtualKeyCode.LEFT;
                        }
                    }
                }
            }
            else if (x > _prop.TileWidth + _prop.Rectangle.X && x < _prop.TileWidth * 2 + _prop.Rectangle.X)
            {
                if (_prop.Flip)
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.DHandled)
                        {
                            _prop.DHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_prop.EHandled)
                        {
                            _prop.EHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
                else
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.EHandled)
                        {
                            _prop.EHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_prop.DHandled)
                        {
                            _prop.DHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
            }
            else if (x > _prop.TileWidth * 2 + _prop.Rectangle.X && x < _prop.TileWidth * 3 + _prop.Rectangle.X)
            {
                if (_prop.Flip)
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.JHandled)
                        {
                            _prop.JHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_prop.UHandled)
                        {
                            _prop.UHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
                else
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.UHandled)
                        {
                            _prop.UHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_prop.JHandled)
                        {
                            _prop.JHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
            }
            else if (x > _prop.TileWidth * 3 + _prop.Rectangle.X)
            {
                if (_prop.Flip)
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.LHandled)
                        {
                            _prop.LHandled = detected;
                            keyCode = VirtualKeyCode.RIGHT;
                        }
                    }
                    else
                    {
                        if (!_prop.OHandled)
                        {
                            _prop.OHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
                else
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.OHandled)
                        {
                            _prop.OHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_prop.LHandled)
                        {
                            _prop.LHandled = detected;
                            keyCode = VirtualKeyCode.RIGHT;
                        }
                    }
                }
            }

            #endregion

            if (keyCode == VirtualKeyCode.VK_0)
                return;

            if (detected)
                DoKeyDown(keyCode);
            else
            {
                if (keyCode == VirtualKeyCode.LEFT && !detected)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.RETURN && !detected)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.RIGHT && !detected)
                    DoKeyUp(keyCode);
            }
        }

        public void DoKeyDown(VirtualKeyCode key)
        {
            if (!_prop.Run)
                return;

            try
            {
                if (_prop.DownCount == int.MaxValue)
                    _prop.DownCount = 0;

                _prop.DownCount = checked(_prop.DownCount + 1);
            }
            catch (OverflowException)
            {
                _prop.DownCount = 0;
            }

            if (_prop.DownCount % _prop.ButtonTrigger != 0)
                return;

            PushButton(key, ButtonDirection.Down);
        }

        public void DoKeyUp(VirtualKeyCode key)
        {
            PushButton(key, ButtonDirection.Up);
        }

        public void PushButton(VirtualKeyCode key, ButtonDirection buttonDirection)
        {
            switch (buttonDirection)
            {
                case ButtonDirection.Up:
                    Keys[key] = false;
                    _prop.InputSimulator.Keyboard.KeyUp(key);
                    break;
                case ButtonDirection.Down:
                    _prop.InputSimulator.Keyboard.KeyDown(key);
                    break;
            }
        }

        #endregion
    }
}
