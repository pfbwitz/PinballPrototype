using System;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using WindowsInput.Native;
using System.Windows.Controls;
using System.Threading.Tasks;
using DepthTracker.Common;
using DepthTracker.Settings;
using System.Windows.Media;

namespace DepthTracker.UI
{
    public partial class PinballTracker : Window, INotifyPropertyChanged, ITracker
    {
        #region properties 

        public ImageSource ImageSource { get { return _prop?.DepthBitmap; } }

        public TextBox XText { get { return xText; } }

        public TextBox YText { get { return yText; } }

        public TextBox ZMinText { get { return zMinText; } }

        public TextBox ZMaxText { get { return zMaxText; } }

        public TextBox WidthText { get { return widthText; } }

        public TextBox HeightText { get { return heightText; } }

        private readonly TrackerProperties<PinballSettings> _prop;

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
            { VirtualKeyCode.VK_Q, false},
            { VirtualKeyCode.VK_A, false},
            { VirtualKeyCode.VK_E, false},
            { VirtualKeyCode.VK_D, false},
            { VirtualKeyCode.VK_U, false},
            { VirtualKeyCode.VK_J, false},
            { VirtualKeyCode.VK_O, false},
            { VirtualKeyCode.VK_L, false},
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageBoxResult ShowMessage(string message, string title)
        {
            return MessageBox.Show(message, title);
        }

        #endregion

        public PinballTracker()
        {
            _prop = new TrackerProperties<PinballSettings>(this);

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

        #region methods

        public void SetTitle(string title)
        {
            Title = title;
        }

        public void SetStatusText()
        {
            StatusText = _prop.KinectSensor.IsAvailable ? Properties.Resources.RunningStatusText :
              Properties.Resources.NoSensorStatusText;
        }

        public void PushButtons(int x, int y, bool detected)
        {
            #region determine button

            VirtualKeyCode keyCode = VirtualKeyCode.RETURN;
            if (x > _prop.Rectangle.X && x <= _prop.TileWidth + _prop.Rectangle.X)
            {
                if (_prop.Flip)
                {
                    if (y >= _prop.Rectangle.Y && y <= _prop.TileHeight + _prop.Rectangle.Y)
                    {
                        if (!_prop.AHandled)
                        {
                            _prop.AHandled = detected;
                            keyCode = VirtualKeyCode.VK_A;
                        }
                    }
                    else
                    {
                        if (!_prop.QHandled)
                        {
                            _prop.QHandled = detected;
                            keyCode = VirtualKeyCode.VK_Q;
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
                            keyCode = VirtualKeyCode.VK_Q;
                        }
                    }
                    else
                    {
                        if (!_prop.AHandled)
                        {
                            _prop.AHandled = detected;
                            keyCode = VirtualKeyCode.VK_A;
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
                            keyCode = VirtualKeyCode.VK_D;
                        }
                    }
                    else
                    {
                        if (!_prop.EHandled)
                        {
                            _prop.EHandled = detected;
                            keyCode = VirtualKeyCode.VK_E;
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
                            keyCode = VirtualKeyCode.VK_E;
                        }
                    }
                    else
                    {
                        if (!_prop.DHandled)
                        {
                            _prop.DHandled = detected;
                            keyCode = VirtualKeyCode.VK_D;
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
                            keyCode = VirtualKeyCode.VK_J;
                        }
                    }
                    else
                    {
                        if (!_prop.UHandled)
                        {
                            _prop.UHandled = detected;
                            keyCode = VirtualKeyCode.VK_U;
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
                            keyCode = VirtualKeyCode.VK_U;
                        }
                    }
                    else
                    {
                        if (!_prop.JHandled)
                        {
                            _prop.JHandled = detected;
                            keyCode = VirtualKeyCode.VK_J;
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
                            keyCode = VirtualKeyCode.VK_L;
                        }
                    }
                    else
                    {
                        if (!_prop.OHandled)
                        {
                            _prop.OHandled = detected;
                            keyCode = VirtualKeyCode.VK_O;
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
                            keyCode = VirtualKeyCode.VK_O;
                        }
                    }
                    else
                    {
                        if (!_prop.LHandled)
                        {
                            _prop.LHandled = detected;
                            keyCode = VirtualKeyCode.VK_L;
                        }
                    }
                }
            }

            #endregion

            if (keyCode == VirtualKeyCode.RETURN)
                return;

            if (detected)
                DoKeyDown(keyCode);
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

        public void PushButton(VirtualKeyCode key, ButtonDirection buttonDirection)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(buttonDirection + ": " + key.ToString());
                switch (buttonDirection)
                {
                    case ButtonDirection.Up:
                        Keys[key] = false;
                        _prop.InputSimulator.Keyboard.KeyUp(key);
                        break;
                    case ButtonDirection.Down:
                        _prop.InputSimulator.Keyboard.KeyDown(key);
                        Task.Run(async () =>
                        {
                            await Task.Delay(200);
                            await Dispatcher.BeginInvoke(new Action(() => {
                                _prop.InputSimulator.Keyboard.KeyUp(key);
                            }));
                        });
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
