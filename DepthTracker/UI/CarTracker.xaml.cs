using System;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using WindowsInput.Native;
using System.Windows.Controls;
using DepthTracker.Common;
using DepthTracker.Settings;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace DepthTracker.UI
{
    public partial class CarTracker : ITracker, INotifyPropertyChanged
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
                if (_statusText != value)
                {
                    _statusText = value;
                    if (PropertyChanged != null)
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

        public KinectSensor KinectSensor = null;

        public DepthFrameReader DepthFrameReader = null;

        public FrameDescription DepthFrameDescription = null;

        public CarTracker()
        {
            //KinectSensor = KinectSensor.GetDefault();
            //KinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

            //DepthFrameReader = KinectSensor.DepthFrameSource.OpenReader();
            //DepthFrameReader.FrameArrived += Reader_FrameArrived;
            //DepthFrameDescription = KinectSensor.DepthFrameSource.FrameDescription;

            _trackerWorker = TrackerWorker<CarSettings>.GetInstance(this);
        }

        public void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            var bounds = _trackerWorker.ZBounds;
            var depthFrameProcessed = false;
            using (var depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    using (var depthBuffer = depthFrame.LockImageBuffer())
                    {
                        if (((DepthFrameDescription.Width * DepthFrameDescription.Height) == (depthBuffer.Size / DepthFrameDescription.BytesPerPixel)) &&
                            (DepthFrameDescription.Width == DepthBitmap.PixelWidth) && (DepthFrameDescription.Height == DepthBitmap.PixelHeight))
                        {
                            _trackerWorker.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, bounds[0], bounds[1]);

                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            foreach (var b in _trackerWorker.DepthPixels)
            {
                if (b > 100)
                    ShowMessage("b", b.ToString());
                break;
            }

            if (depthFrameProcessed)
            {
                DepthBitmap.WritePixels(
                     new Int32Rect(0, 0, DepthBitmap.PixelWidth, DepthBitmap.PixelHeight),
                     _trackerWorker.DepthPixels,
                     DepthBitmap.PixelWidth,
                     0
                );
            }
        }

        public void PushButtons(int x, int y, bool detected, int lowestX, int highestX, int lowestY, int highestY)
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
            }
            else
            {
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
