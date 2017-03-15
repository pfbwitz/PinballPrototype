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

namespace DepthTrackerClicks.UI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region attr

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private const int _buttonTrigger = 2;

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

        private bool _clickHandled = false;

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

        public MainWindow()
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

            Rectangle = new Rectangle(200, 140, 200, 140);

            _kinectSensor.Open();

            DataContext = this;

            InitializeComponent();

            xText.Text = Rectangle.X.ToString();
            yText.Text = Rectangle.Y.ToString();
            zMinText.Text = 1000.ToString();
            zMaxText.Text = 1100.ToString();
            widthText.Text = Rectangle.Width.ToString();
            heightText.Text = Rectangle.Height.ToString();

            Sensor_IsAvailableChanged(null, null);
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
                        Rectangle.X = value;
                        _lowerXBound = value;
                        break;
                    case "yText":
                        Rectangle.Y = value;
                        _lowerYBound = value;
                        break;
                    case "widthText":
                        Rectangle.Width = value;
                        _upperXBound = Rectangle.X + value;
                        break;
                    case "heightText":
                        Rectangle.Height = value;
                        _upperYBound = Rectangle.Y + value;
                        break;
                    case "zMinText":
                    case "zMaxText":
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
            StatusText = _kinectSensor.IsAvailable ? "Running" :
              "Not connected";
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
            catch(Exception ex)
            {
                var result = MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Error");
            }

            FilterFlyingPixelsAndPushButtons();

            _clickHandled = false;
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
           for(var y = recY; y < recY + recH; y++)
            {
                for(var x = recX;x < recX + recW;x++)
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
                for(var x = recX; x < recX + recW; x++)
                {
                    var pixel = _mapping[x, y];
                    if(pixel != null)
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

            for(var y1 = lowerYBound;y1 <= upperYBound; y1++)
            {
                for(var x1 = lowerXBound;x1<=upperXBound;x1++)
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

        private int _x;
        private int _y;

        private void PushButtons(int x, int y, bool detected)
        {
            var posXPercentage = (x - Rectangle.X) / Rectangle.Width * 100;
            var posYPercentage = (y - Rectangle.Y) / Rectangle.Height * 100;
            var c = false;
            if (_flip)
            {
                posXPercentage = 100 - posXPercentage;
                posYPercentage = 100 - posYPercentage;
            }

            _x = Convert.ToInt32(posXPercentage / 100 * SystemParameters.PrimaryScreenWidth);
            _y = Convert.ToInt32(posYPercentage / 100 * SystemParameters.PrimaryScreenHeight);

            if (!_clickHandled)
            {
                _clickHandled = detected;
                c = true;
            }

            if (!c)
                return;

            if (detected)
            {
                DoKeyDown();
            }
            else
            {
                DoKeyUp();
            }

           

        }

        public void DoKeyDown()
        {
            if (!_run)
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
            if (!_run)
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
                        _mouseDown = false;
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
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
