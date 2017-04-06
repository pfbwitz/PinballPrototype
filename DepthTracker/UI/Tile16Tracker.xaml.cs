﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using WindowsInput.Native;
using System.Windows.Controls;
using System.Threading.Tasks;
using DepthTracker.Settings;
using System.Windows.Media;
using DepthTracker.Common.Worker;
using DepthTracker.Common.Interface;
using DepthTracker.Common.Enum;
using System.Windows.Media.Imaging;

namespace DepthTracker.UI
{
    public partial class Tile16Tracker : Window, ITracker, INotifyPropertyChanged
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

        private readonly TrackerWorker<Tile16TrackerSettings> _trackerWorker;

        public string _statusText = string.Empty;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                if(PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
            }
        }

        public Dictionary<VirtualKeyCode, bool> Keys = new Dictionary<VirtualKeyCode, bool> {
            //outer rows
            { VirtualKeyCode.VK_Q, false}, //0,0
            { VirtualKeyCode.VK_A, false}, //0, 1
            { VirtualKeyCode.VK_E, false}, //1, 0
            { VirtualKeyCode.VK_D, false}, //1, 1
            { VirtualKeyCode.VK_U, false}, // 0, 2
            { VirtualKeyCode.VK_J, false}, // 1, 2
            { VirtualKeyCode.VK_O, false}, // 0, 3
            { VirtualKeyCode.VK_L, false}, // 1, 3
            //middlerows
            { VirtualKeyCode.RETURN, false},
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public MessageBoxResult ShowMessage(string message, string title)
        {
            return MessageBox.Show(message, title);
        }

        #endregion

        public Tile16Tracker()
        {
            _trackerWorker = TrackerWorker<Tile16TrackerSettings>.GetInstance(this, 4);
        }

        public void PushButtons(int x, int y, bool detected)
        {
            #region determine button

            VirtualKeyCode keyCode = VirtualKeyCode.VK_Y;
            if (x > _trackerWorker.Rectangle.X && x <= _trackerWorker.TileWidth + _trackerWorker.Rectangle.X)
            {
                if (_trackerWorker.Flip)
                {
                    if (y >= _trackerWorker.Rectangle.Y && y <= _trackerWorker.TileHeight + _trackerWorker.Rectangle.Y)
                    {
                        if (!_trackerWorker.AHandled)
                        {
                            _trackerWorker.AHandled = detected;
                            keyCode = VirtualKeyCode.VK_A;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight && 
                        y <= _trackerWorker.TileHeight * 2 + _trackerWorker.Rectangle.Y)
                    {
                        //row 2
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y * 2 + _trackerWorker.TileHeight &&
                        y <= _trackerWorker.TileHeight * 3 + _trackerWorker.Rectangle.Y)
                    {
                        // row 3
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.QHandled)
                        {
                            _trackerWorker.QHandled = detected;
                            keyCode = VirtualKeyCode.VK_Q;
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
                            keyCode = VirtualKeyCode.VK_Q;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight && 
                        y <= _trackerWorker.TileHeight * 2 + _trackerWorker.Rectangle.Y)
                    {
                        //row 2
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight * 2 &&
                      y <= _trackerWorker.TileHeight * 3 + _trackerWorker.Rectangle.Y)
                    {
                        //row 3
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.AHandled)
                        {
                            _trackerWorker.AHandled = detected;
                            keyCode = VirtualKeyCode.VK_A;
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
                            keyCode = VirtualKeyCode.VK_D;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight &&
                       y <= _trackerWorker.TileHeight * 2 + _trackerWorker.Rectangle.Y)
                    {
                        // row 2
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y * 2 + _trackerWorker.TileHeight &&
                        y <= _trackerWorker.TileHeight * 3 + _trackerWorker.Rectangle.Y)
                    {
                        // row 3
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.EHandled)
                        {
                            _trackerWorker.EHandled = detected;
                            keyCode = VirtualKeyCode.VK_E;
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
                            keyCode = VirtualKeyCode.VK_E;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight &&
                       y <= _trackerWorker.TileHeight * 2 + _trackerWorker.Rectangle.Y)
                    {
                        // row 2
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight * 2 &&
                      y <= _trackerWorker.TileHeight * 3 + _trackerWorker.Rectangle.Y)
                    {
                        // row 3
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.DHandled)
                        {
                            _trackerWorker.DHandled = detected;
                            keyCode = VirtualKeyCode.VK_D;
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
                            keyCode = VirtualKeyCode.VK_J;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight &&
                       y <= _trackerWorker.TileHeight * 2 + _trackerWorker.Rectangle.Y)
                    {
                        // row 2
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y * 2 + _trackerWorker.TileHeight &&
                        y <= _trackerWorker.TileHeight * 3 + _trackerWorker.Rectangle.Y)
                    {
                        // row 3
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.UHandled)
                        {
                            _trackerWorker.UHandled = detected;
                            keyCode = VirtualKeyCode.VK_U;
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
                            keyCode = VirtualKeyCode.VK_U;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight &&
                       y <= _trackerWorker.TileHeight * 2 + _trackerWorker.Rectangle.Y)
                    {
                        // row 2
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight * 2 &&
                      y <= _trackerWorker.TileHeight * 3 + _trackerWorker.Rectangle.Y)
                    {
                        // row 3
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.JHandled)
                        {
                            _trackerWorker.JHandled = detected;
                            keyCode = VirtualKeyCode.VK_J;
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
                            keyCode = VirtualKeyCode.VK_L;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight &&
                       y <= _trackerWorker.TileHeight * 2 + _trackerWorker.Rectangle.Y)
                    {
                        // row 2
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y * 2 + _trackerWorker.TileHeight &&
                        y <= _trackerWorker.TileHeight * 3 + _trackerWorker.Rectangle.Y)
                    {
                        // row 3
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.OHandled)
                        {
                            _trackerWorker.OHandled = detected;
                            keyCode = VirtualKeyCode.VK_O;
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
                            keyCode = VirtualKeyCode.VK_O;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight &&
                       y <= _trackerWorker.TileHeight * 2 + _trackerWorker.Rectangle.Y)
                    {
                        // row 2
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else if (y >= _trackerWorker.Rectangle.Y + _trackerWorker.TileHeight * 2 &&
                      y <= _trackerWorker.TileHeight * 3 + _trackerWorker.Rectangle.Y)
                    {
                        // row 3
                        if (!_trackerWorker.ReturnHandled)
                        {
                            _trackerWorker.ReturnHandled = detected;
                            keyCode = VirtualKeyCode.RETURN;
                        }
                    }
                    else
                    {
                        if (!_trackerWorker.LHandled)
                        {
                            _trackerWorker.LHandled = detected;
                            keyCode = VirtualKeyCode.VK_L;
                        }
                    }
                }
            }

            #endregion

            if (keyCode == VirtualKeyCode.VK_Y)
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

                try
                {
                    System.Diagnostics.Debug.WriteLine(ButtonDirection.Down + ": " + keyCode.ToString());
                    _trackerWorker.InputSimulator.Keyboard.KeyDown(keyCode);
                    Task.Run(async () =>
                    {
                        await Task.Delay(200);
                        await Dispatcher.BeginInvoke(new Action(() => _trackerWorker.InputSimulator.Keyboard.KeyUp(keyCode)));
                    });
                }
                catch
                {
                }
            }
        }
    }
}
