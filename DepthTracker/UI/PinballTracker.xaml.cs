using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WindowsInput;
using WindowsInput.Native;
using System.Drawing;
using System.Windows.Controls;
using System.Threading.Tasks;
using DepthTracker.Common;
using DepthTracker.Settings;

namespace DepthTracker.UI
{
    public partial class PinballTracker : Window, INotifyPropertyChanged
    {
        #region attr

        public readonly ISettings Settings = SettingsHelper<PinballSettings>.GetInstance();

        //private PipeClient _pipeClient;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private const int _buttonTrigger = 3;

        private int _upCount;

        private int _downCount;

        ushort[] _zBounds;
        public ushort[] ZBounds
        {
            get
            {
                try
                {
                    _zBounds = new ushort[] { Convert.ToUInt16(zMinText.Text), Convert.ToUInt16(zMaxText.Text) };
                }
                catch { }
                return _zBounds;
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

        private bool _qHandled = false;

        private bool _aHandled = false;

        private bool _eHandled = false;

        private bool _dHandled = false;

        private bool _jHandled = false;

        private bool _oHandled = false;

        private bool _uHandled = false;

        private bool _lHandled = false;

        bool zCalibrated = false;

        public Rectangle Rectangle;

        private int _lowerXBound;

        private int _upperXBound;

        private int _lowerYBound;

        private int _upperYBound;

        private bool _run;
        private bool _flip;

        private PixelMap[,] _mapping;

        public ImageSource ImageSource { get { return _depthBitmap; } }

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

        public int Z = 0;

        private const int MapDepthToByte = 8000 / 256; // Map depth range to byte range

        private KinectSensor _kinectSensor = null;

        private DepthFrameReader _depthFrameReader = null;

        private FrameDescription _depthFrameDescription = null;

        private WriteableBitmap _depthBitmap = null;

        private byte[] _depthPixels = null;

        private string _statusText = null;

        public event PropertyChangedEventHandler PropertyChanged;

        private int _tileWidth;

        private int _tileHeight;

        InputSimulator _inputSimulator;

        #endregion

        public PinballTracker()
        {
            _kinectSensor = KinectSensor.GetDefault();
            _kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

            _depthFrameReader = _kinectSensor.DepthFrameSource.OpenReader();
            _depthFrameReader.FrameArrived += Reader_FrameArrived;
            _depthFrameDescription = _kinectSensor.DepthFrameSource.FrameDescription;

            _mapping = new PixelMap[_depthFrameDescription.Width, _depthFrameDescription.Height];

            _inputSimulator = new InputSimulator();
            _tileWidth = Rectangle.Width / 4;
            _tileHeight = Rectangle.Height / 2;

            _depthPixels = new byte[_depthFrameDescription.Width * _depthFrameDescription.Height];
            _depthBitmap = new WriteableBitmap(_depthFrameDescription.Width, _depthFrameDescription.Height,
                96.0, 96.0, PixelFormats.Gray8, null);

            Rectangle = new Rectangle(Settings.X, Settings.Y, Settings.Width, Settings.Height);

            _kinectSensor.Open();

            DataContext = this;

            InitializeComponent();

            xText.Text = Rectangle.X.ToString();
            yText.Text = Rectangle.Y.ToString();
            zMinText.Text = Settings.ZMin.ToString();
            zMaxText.Text = Settings.ZMax.ToString();
            widthText.Text = Rectangle.Width.ToString();
            heightText.Text = Rectangle.Height.ToString();

            Sensor_IsAvailableChanged(null, null);

            //_pipeClient = new PipeClient();
            //_pipeClient.StartPipeClient();
        }

        #region handlers

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            int value;
            zCalibrated = false;
            if (int.TryParse(box.Text, out value))
            {
                switch (box.Name)
                {
                    case "xText":
                        Settings.X = value;
                        Rectangle.X = value;
                        _lowerXBound = value;
                        break;
                    case "yText":
                        Settings.Y = value;
                        Rectangle.Y = value;
                        _lowerYBound = value;
                        break;
                    case "widthText":
                        Settings.Width = value;
                        Rectangle.Width = value;
                        _upperXBound = Rectangle.X + value;
                        break;
                    case "heightText":
                        Settings.Height = value;
                        Rectangle.Height = value;
                        _upperYBound = Rectangle.Y + value;
                        break;
                    case "zMinText":
                        Settings.ZMin = value;
                        zCalibrated = true;
                        break;
                    case "zMaxText":
                        Settings.ZMax = value;
                        zCalibrated = true;
                        break;
                }
            }

            _tileWidth = Rectangle.Width / 4;
            _tileHeight = Rectangle.Height / 2;
        }

        private void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            _run = !_run;
            BtnSwitch.Content = _run ? "ON" : "OFF";
        }

        private void BtnFlip_Click(object sender, RoutedEventArgs e)
        {
            _flip = !_flip;

            var b = (Button)sender;
            b.Content = _flip ? "FLIP OFF" : "FLIP ON";
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Settings.Save();
            if (_depthFrameReader != null)
            {
                _depthFrameReader.Dispose();
                _depthFrameReader = null;
            }
            if (_kinectSensor != null)
            {
                _kinectSensor.Close();
                _kinectSensor = null;
            }
        }

        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            var bounds = ZBounds;
            var depthFrameProcessed = false;
            using (var depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    using (var depthBuffer = depthFrame.LockImageBuffer())
                    {
                        if (((_depthFrameDescription.Width * _depthFrameDescription.Height) == (depthBuffer.Size / _depthFrameDescription.BytesPerPixel)) &&
                            (_depthFrameDescription.Width == _depthBitmap.PixelWidth) && (_depthFrameDescription.Height == _depthBitmap.PixelHeight))
                        {
                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, bounds[0], bounds[1]);

                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                _depthBitmap.WritePixels(
                     new Int32Rect(0, 0, _depthBitmap.PixelWidth, _depthBitmap.PixelHeight),
                     _depthPixels,
                     _depthBitmap.PixelWidth,
                     0
                );
            }
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            SetStatusText();
        }

        #endregion

        #region methods

        public void SetStatusText()
        {
            StatusText = _kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText :
              Properties.Resources.NoSensorStatusText;
        }

        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            ushort* frameData = (ushort*)depthFrameData;
            var tileWidth = Rectangle.Width / 4;
            var tileHeight = Rectangle.Height / 2;

            try
            {
                Loop((int)depthFrameDataSize, _depthFrameDescription.BytesPerPixel, frameData, minDepth, maxDepth);
            }
            catch (Exception ex)
            {
                var result = MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Error");
            }

            FilterFlyingPixelsAndPushButtons();

            _aHandled = false;
            _qHandled = false;
            _dHandled = false;
            _eHandled = false;
            _uHandled = false;
            _jHandled = false;
            _oHandled = false;
            _lHandled = false;
        }

        private unsafe void Loop(int depthFrameDataSize, uint bytesPerPixel, ushort* frameData, ushort minDepth, ushort maxDepth)
        {
            var width = _depthFrameDescription.Width;
            var height = _depthFrameDescription.Height;

            for (int i = 0; i < (int)(depthFrameDataSize / bytesPerPixel); ++i)
            {
                ushort pixelDepth = frameData[i];

                int x = i % _depthFrameDescription.Width;
                int y = i / _depthFrameDescription.Width;

                var detected = pixelDepth > 0 && pixelDepth > minDepth && pixelDepth < maxDepth; //is depth within our depth-margin

                //if true, pixel is within bounding rectangle
                var contains = x >= Rectangle.X &&
                    x <= Rectangle.X + Rectangle.Width &&
                    y >= Rectangle.Y &&
                    y <= Rectangle.Y + Rectangle.Height;

                //determine z-coordinate of the surface (possibly unreliable)
                if (!zCalibrated && contains)
                {
                    zCalibrated = true;
                    Settings.ZMax = Convert.ToInt32(pixelDepth);
                    Settings.ZMax = Convert.ToInt32(pixelDepth - 5);
                    zMaxText.Text = Convert.ToInt32(pixelDepth).ToString();
                    zMinText.Text = Convert.ToInt32(pixelDepth - 5).ToString();
                }

                byte b;

                if (detected)
                {
                    b = (byte)(contains ? 255 : 0); //pixel within depth-margin (z) and withing rectangle (x and y) should be white
                    _mapping[x, y] = new PixelMap { ByteValue = b, Index = i };
                }
                else
                {
                    if (contains)
                        b = 100; //draw bounding rectangle
                    else
                    {
                        b = (byte)(pixelDepth / MapDepthToByte);
                        if (b == 255)
                            b = 100;
                    }
                }
                _depthPixels[i] = b;
            }
        }

        /// <summary>
        /// Loop through the pixels.
        /// 1. Check pixels with byte-value of 255. These are the 'detected' pixels
        /// 2. Check if they're inside a areamask with also 'detected' pixels (count at least similar to parameter 'minConstraint')
        /// 3. if retval == true, leave the pixel alone
        /// 4. is retval == false, make the bytevalue of the pixel 100 ('undetect' it)
        /// </summary>
        private void FilterFlyingPixelsAndPushButtons()
        {
            if (Rectangle == null)
                return;

            var recY = Rectangle.Y;
            var recX = Rectangle.X;
            var recW = Rectangle.Width;
            var recH = Rectangle.Height;

            //loop through the pixels withing the rectangle
            for (var y = recY; y < recY + recH; y++)
            {
                for (var x = recX; x < recX + recW; x++)
                {
                    var pixel = _mapping[x, y];
                    if (pixel == null)
                        continue;
                    if (pixel.ByteValue == 255 && !IsPositiveInPixelArea(x, y))
                    {
                        pixel.ByteValue = 100;
                        _depthPixels[pixel.Index] = 100;
                    }
                }
            }

            for (var y = recY; y < recY + recH; y++)
            {
                for (var x = recX; x < recX + recW; x++)
                {
                    var pixel = _mapping[x, y];
                    if (pixel != null)
                        PushButtons(x, y, pixel.ByteValue == 255);
                }
            }

            _mapping = new PixelMap[_depthFrameDescription.Width, _depthFrameDescription.Height];
        }

        private bool IsPositiveInPixelArea(int x, int y)
        {
            //  return true;
            var radius = 5;
            var limit = radius + 2;
            var count = 0;

            var upperXBound = x + radius <= _upperXBound ? x + radius : _upperXBound;
            var lowerXBound = x - radius >= _lowerXBound ? x - radius : Rectangle.X;

            var upperYBound = y + radius <= _upperYBound ? y + radius : _upperYBound;
            var lowerYBound = y - radius >= _lowerYBound ? y - radius : _lowerYBound;

            for (var y1 = lowerYBound; y1 <= upperYBound; y1++)
            {
                for (var x1 = lowerXBound; x1 <= upperXBound; x1++)
                {
                    var mapping = _mapping[x1, y1];
                    if (mapping == null)
                        continue;
                    if (mapping.ByteValue == 255)
                        count++;
                    if (count == limit)
                        break;
                }
                if (count == limit)
                    break;
            }

            if (count >= limit)
                return true;

            for (var y1 = lowerYBound; y1 <= upperYBound; y1++)
                for (var x1 = lowerXBound; x1 <= upperXBound; x1++)
                {
                    var m = _mapping[x1, y1];
                    if (m != null)
                    {
                        m.ByteValue = 0;
                        _depthPixels[m.Index] = 0;
                    }
                }

            return false;
        }

        private void PushButtons(int x, int y, bool detected)
        {
            #region determine button

            VirtualKeyCode keyCode = VirtualKeyCode.RETURN;
            if (x > Rectangle.X && x <= _tileWidth + Rectangle.X)
            {
                if (_flip)
                {
                    if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                    {
                        if (!_aHandled)
                        {
                            _aHandled = detected;
                            keyCode = VirtualKeyCode.VK_A;
                        }
                    }
                    else
                    {
                        if (!_qHandled)
                        {
                            _qHandled = detected;
                            keyCode = VirtualKeyCode.VK_Q;
                        }
                    }
                }
                else
                {
                    if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                    {
                        if (!_qHandled)
                        {
                            _qHandled = detected;
                            keyCode = VirtualKeyCode.VK_Q;
                        }
                    }
                    else
                    {
                        if (!_aHandled)
                        {
                            _aHandled = detected;
                            keyCode = VirtualKeyCode.VK_A;
                        }
                    }
                }
            }
            else if (x > _tileWidth + Rectangle.X && x < _tileWidth * 2 + Rectangle.X)
            {
                if (_flip)
                {
                    if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                    {
                        if (!_dHandled)
                        {
                            _dHandled = detected;
                            keyCode = VirtualKeyCode.VK_D;
                        }
                    }
                    else
                    {
                        if (!_eHandled)
                        {
                            _eHandled = detected;
                            keyCode = VirtualKeyCode.VK_E;
                        }
                    }
                }
                else
                {
                    if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                    {
                        if (!_eHandled)
                        {
                            _eHandled = detected;
                            keyCode = VirtualKeyCode.VK_E;
                        }
                    }
                    else
                    {
                        if (!_dHandled)
                        {
                            _dHandled = detected;
                            keyCode = VirtualKeyCode.VK_D;
                        }
                    }
                }
            }
            else if (x > _tileWidth * 2 + Rectangle.X && x < _tileWidth * 3 + Rectangle.X)
            {
                if (_flip)
                {
                    if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                    {
                        if (!_jHandled)
                        {
                            _jHandled = detected;
                            keyCode = VirtualKeyCode.VK_J;
                        }
                    }
                    else
                    {
                        if (!_uHandled)
                        {
                            _uHandled = detected;
                            keyCode = VirtualKeyCode.VK_U;
                        }
                    }
                }
                else
                {
                    if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                    {
                        if (!_uHandled)
                        {
                            _uHandled = detected;
                            keyCode = VirtualKeyCode.VK_U;
                        }
                    }
                    else
                    {
                        if (!_jHandled)
                        {
                            _jHandled = detected;
                            keyCode = VirtualKeyCode.VK_J;
                        }
                    }
                }
            }
            else if (x > _tileWidth * 3 + Rectangle.X)
            {
                if (_flip)
                {
                    if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                    {
                        if (!_lHandled)
                        {
                            _lHandled = detected;
                            keyCode = VirtualKeyCode.VK_L;
                        }
                    }
                    else
                    {
                        if (!_oHandled)
                        {
                            _oHandled = detected;
                            keyCode = VirtualKeyCode.VK_O;
                        }
                    }
                }
                else
                {
                    if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                    {
                        if (!_oHandled)
                        {
                            _oHandled = detected;
                            keyCode = VirtualKeyCode.VK_O;
                        }
                    }
                    else
                    {
                        if (!_lHandled)
                        {
                            _lHandled = detected;
                            keyCode = VirtualKeyCode.VK_L;
                        }
                    }
                }
            }

            #endregion

            if (keyCode == VirtualKeyCode.RETURN)
                return;

            if (detected)
            {
                DoKeyDown(keyCode);
            }
            else
            {
                if (keyCode == VirtualKeyCode.VK_A && !_aHandled)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_E && !_eHandled)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_Q && !_qHandled)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_D && !_dHandled)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_U && !_uHandled)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_J && !_jHandled)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_O && !_oHandled)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_L && !_lHandled)
                    DoKeyUp(keyCode);
            }
        }

        public void DoKeyDown(VirtualKeyCode key)
        {
            if (!_run)
                return;

            try
            {
                if (_downCount == int.MaxValue)
                    _downCount = 0;

                _downCount = checked(_downCount + 1);
            }
            catch (OverflowException)
            {
                _downCount = 0;
            }

            if (_downCount % _buttonTrigger != 0)
                return;

            //if (!Keys[key])
            PushButton(key, ButtonDirection.Down);
        }

        public void DoKeyUp(VirtualKeyCode key)
        {
            return;
            //if (!_run)
            //    return;

            //try
            //{
            //    _upCount = checked(_upCount + 1);
            //}
            //catch (OverflowException)
            //{
            //    _upCount = 0;
            //}

            //if (_upCount % _buttonTrigger != 0)
            //    return;

            //if (Keys[key])
            //{
            //    if (key == VirtualKeyCode.VK_A)
            //        _aHandled = false;
            //    else if (key == VirtualKeyCode.VK_E)
            //        _eHandled = false;
            //    else if (key == VirtualKeyCode.VK_Q)
            //        _qHandled = false;
            //    else if (key == VirtualKeyCode.VK_D)
            //        _dHandled = false;
            //    else if (key == VirtualKeyCode.VK_U)
            //        _uHandled = false;
            //    else if (key == VirtualKeyCode.VK_J)
            //        _jHandled = false;
            //    else if (key == VirtualKeyCode.VK_O)
            //        _oHandled = false;
            //    else if (key == VirtualKeyCode.VK_L)
            //        _lHandled = false;

            //    PushButton(key, ButtonDirection.Up);
            //}
        }

        public void PushButton(VirtualKeyCode key, ButtonDirection buttonDirection)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(buttonDirection + ": " + key.ToString());
                switch (buttonDirection)
                {
                    case ButtonDirection.Up:
                        //_upCount = 0;
                        Keys[key] = false;
                        _inputSimulator.Keyboard.KeyUp(key);
                        //_pipeClient.SendJson(key.ToString().Replace("VK_", "") + "_" + buttonDirection.ToString().ToUpper());
                        break;
                    case ButtonDirection.Down:
                        //_upCount = 0;
                        //Keys[key] = true;
                        //_pipeClient.SendJson(key.ToString().Replace("VK_", "") + "_" + buttonDirection.ToString().ToUpper());
                        _inputSimulator.Keyboard.KeyDown(key);
                        Task.Run(async () =>
                        {
                            await Task.Delay(200);
                            await Dispatcher.BeginInvoke(new Action(() => {
                                _inputSimulator.Keyboard.KeyUp(key);
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
