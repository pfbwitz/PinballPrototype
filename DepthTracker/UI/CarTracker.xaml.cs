using System;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using WindowsInput.Native;
using System.Windows.Controls;
using DepthTracker.Settings;
using System.Windows.Media;
using DepthTracker.Common.Interface;
using DepthTracker.Common.Enum;
using DepthTracker.Common.Worker;
using System.Windows.Media.Imaging;

namespace DepthTracker.UI
{
    public partial class CarTracker : ITracker, INotifyPropertyChanged, IButtonTrackerWindow
    {
        #region properties 

        public WriteableBitmap DepthBitmap { get; set; }

        public Window Instance { get { return this; } }

        public Button FlipButton { get { return BtnFlip; } }

        public Button SwitchButton { get { return BtnSwitch; } }

        public ImageSource ImageSource { get { return DepthBitmap; } }

        public TextBox XText { get { return xText; } }

        public TextBox YText { get { return yText; } }

        public TextBox ZMinText { get { return zMinText; } }

        public TextBox ZMaxText { get { return zMaxText; } }

        public TextBox WidthText { get { return widthText; } }

        public TextBox HeightText { get { return heightText; } }

        private readonly TrackerWorker<CarSettings> _trackerWorker;

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

        public Dictionary<VirtualKeyCode, bool> Keys
        {
            get
            {
                return new Dictionary<VirtualKeyCode, bool>
                {
                    { VirtualKeyCode.LEFT, false},
                    { VirtualKeyCode.RETURN, false},
                    { VirtualKeyCode.RIGHT, false}
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageBoxResult ShowMessage(string message, string title)
        {
            return MessageBox.Show(message, title);
        }

        #endregion

        public CarTracker()
        {
            _trackerWorker = TrackerWorker<CarSettings>.GetInstance(this);
        }

        public void PushButtons(int x, int y, bool detected)
        {
            #region determine button

            VirtualKeyCode keyCode = VirtualKeyCode.VK_0;
            if (x > _trackerWorker.Rectangle.X && x <= _trackerWorker.TileWidth + _trackerWorker.Rectangle.X)
            {
                if (_trackerWorker.Flip)
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.AHandled)
                        {
                            _trackerWorker.AHandled = detected;
                            keyCode = VirtualKeyCode.LEFT;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.QHandled)
                        {
                            _trackerWorker.QHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
                else
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.QHandled)
                        {
                            _trackerWorker.QHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.AHandled)
                        {
                            _trackerWorker.AHandled = detected;
                            keyCode = VirtualKeyCode.LEFT;
                        }
                    }
                }
            }
            else if (x > _trackerWorker.TileWidth + _trackerWorker.Rectangle.X && x < _trackerWorker.TileWidth * 2 + _trackerWorker.Rectangle.X)
            {
                if (_trackerWorker.Flip)
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.DHandled)
                        {
                            _trackerWorker.DHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.EHandled)
                        {
                            _trackerWorker.EHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
                else
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.EHandled)
                        {
                            _trackerWorker.EHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.DHandled)
                        {
                            _trackerWorker.DHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
            }
            else if (x > _trackerWorker.TileWidth * 2 + _trackerWorker.Rectangle.X && x < _trackerWorker.TileWidth * 3 + _trackerWorker.Rectangle.X)
            {
                if (_trackerWorker.Flip)
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.JHandled)
                        {
                            _trackerWorker.JHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.UHandled)
                        {
                            _trackerWorker.UHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
                else
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.UHandled)
                        {
                            _trackerWorker.UHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.JHandled)
                        {
                            _trackerWorker.JHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
            }
            else if (x > _trackerWorker.TileWidth * 3 + _trackerWorker.Rectangle.X)
            {
                if (_trackerWorker.Flip)
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.LHandled)
                        {
                            _trackerWorker.LHandled = detected;
                            keyCode = VirtualKeyCode.RIGHT;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.OHandled)
                        {
                            _trackerWorker.OHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                }
                else
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.OHandled)
                        {
                            _trackerWorker.OHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.LHandled)
                        {
                            _trackerWorker.LHandled = detected;
                            keyCode = VirtualKeyCode.RIGHT;
                        }
                    }
                }
            }

            #endregion

            if (keyCode == VirtualKeyCode.VK_0)
                return;

            if (detected)
            {
                if (!_trackerWorker.Run)
                    return;

                try
                {
                    if (_trackerWorker.DownCount == int.MaxValue)
                        _trackerWorker.DownCount = 0;

                    _trackerWorker.DownCount = checked(_trackerWorker.DownCount + 1);
                }
                catch (OverflowException)
                {
                    _trackerWorker.DownCount = 0;
                }

                if (_trackerWorker.DownCount % _trackerWorker.ButtonTrigger != 0)
                    return;

                PushButton(keyCode, ButtonDirection.Down);
                //this.PushButton(keyCode, ButtonDirection.Down, _trackerWorker.InputSimulator);
            }
            else
            {
                //if (keyCode == VirtualKeyCode.LEFT && !detected)
                //    this.PushButton(keyCode, ButtonDirection.Up, _trackerWorker.InputSimulator);
                //else if (keyCode == VirtualKeyCode.RETURN && !detected)
                //    this.PushButton(keyCode, ButtonDirection.Up, _trackerWorker.InputSimulator);
                //else if (keyCode == VirtualKeyCode.RIGHT && !detected)
                //    this.PushButton(keyCode, ButtonDirection.Up, _trackerWorker.InputSimulator);

                if (keyCode == VirtualKeyCode.LEFT && !detected)
                    PushButton(keyCode, ButtonDirection.Up);
                else if (keyCode == VirtualKeyCode.RETURN && !detected)
                    PushButton(keyCode, ButtonDirection.Up);
                else if (keyCode == VirtualKeyCode.RIGHT && !detected)
                    PushButton(keyCode, ButtonDirection.Up);
            }
        }

        public void PushButton(VirtualKeyCode key, ButtonDirection buttonDirection)
        {
            if (key != VirtualKeyCode.RETURN && key != VirtualKeyCode.LEFT && key != VirtualKeyCode.RIGHT)
                return;

            switch (buttonDirection)
            {
                case ButtonDirection.Up:
                    Keys[key] = false;
                    _trackerWorker.InputSimulator.Keyboard.KeyUp(key);
                    break;
                case ButtonDirection.Down:
                    _trackerWorker.InputSimulator.Keyboard.KeyDown(key);
                    break;
            }
        }
    }
}
