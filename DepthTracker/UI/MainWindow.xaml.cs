using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using WindowsInput;
using WindowsInput.Native;
using System.Drawing;
using System.Windows.Controls;
using System.Linq;

namespace DepthTracker.UI
{
    public class PixelMap
    {
        public byte ByteValue;
        public int X;
        public int Y;
        public int Index;
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public Rectangle Rectangle;

        private int _lowerXBound;
        private int _upperXBound;
        private int _lowerYBound;
        private int _upperYBound;

        #region attr

        private PixelMap[,] _mapping;

        //private PipeClient _pipeClient;

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

        //private List<Tile> _tiles;

        private const int MapDepthToByte = 8000 / 256; // Map depth range to byte range

        private KinectSensor _kinectSensor = null;

        private DepthFrameReader _depthFrameReader = null;

        private FrameDescription _depthFrameDescription = null;

        private WriteableBitmap _depthBitmap = null;

        private byte[] _depthPixels = null;

        private string _statusText = null;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public MainWindow()
        {
            InitSensor();
            InitReader();
            InitDepthAndBitmap();

            Rectangle = new Rectangle(168, 118, 200, 140);

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

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            //_tiles.Clear();
            //if (_pipeClient != null)
            //{
            //    _pipeClient.Dispose();
            //    _pipeClient = null;
            //}
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
                         
                            //ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, minDepth: bounds[0], maxDepth: bounds[1]);
                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, bounds[0], bounds[1]);
                            try
                            {
                                FilterFlyingPixels();
                            }
                            catch(Exception ex)
                            {
                                var result = MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Flying pixel error");
                            }
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                RenderDepthPixels();
            }
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            SetStatusText();
        }

        #endregion

        #region methods

        private void InitSensor()
        {
            _kinectSensor = KinectSensor.GetDefault();
            _kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;
        }

        private void InitReader()
        {
            _depthFrameReader = _kinectSensor.DepthFrameSource.OpenReader();
            _depthFrameReader.FrameArrived += Reader_FrameArrived;
            _depthFrameDescription = _kinectSensor.DepthFrameSource.FrameDescription;

            _mapping = new PixelMap[_depthFrameDescription.Width, _depthFrameDescription.Height];

            _inputSimulator = new InputSimulator();
            _tileWidth = Rectangle.Width / 4;
            _tileHeight = Rectangle.Height / 2;

        }

        private int _tileWidth;
        private int _tileHeight;
        InputSimulator _inputSimulator;

        private void InitDepthAndBitmap()
        {
            _depthPixels = new byte[_depthFrameDescription.Width * _depthFrameDescription.Height];
            _depthBitmap = new WriteableBitmap(_depthFrameDescription.Width, _depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);
        }

        public void SetStatusText()
        {
            StatusText = _kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText :
              Properties.Resources.NoSensorStatusText;
        }

        Dictionary<System.Drawing.Point, byte> pixels = new Dictionary<System.Drawing.Point, byte>();

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

            _aPressed = false;
            _qPressed = false;
            _dPressed = false;
            _ePressed = false;
            _uPressed = false;
            _jPressed = false;
            _oPressed = false;
            _lPressed = false;
        }

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

        private bool _qPressed = false;
        private bool _aPressed = false;
        private bool _ePressed = false;
        private bool _dPressed = false;
        private bool _jPressed = false;
        private bool _oPressed = false;
        private bool _uPressed = false;
        private bool _lPressed = false;

        bool zCalibrated = false;
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

        private void PushButtons(int x, int y, bool detected)
        {
            VirtualKeyCode keyCode = VirtualKeyCode.RETURN;
            if (x > Rectangle.X && x <= _tileWidth + Rectangle.X)
            {
                if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                {
                    if (!_qPressed)
                    {
                        _qPressed = detected;
                        keyCode = VirtualKeyCode.VK_Q;
                    }
                }
                else
                {
                    if (!_aPressed)
                    {
                        _aPressed = detected;
                        keyCode = VirtualKeyCode.VK_A;
                    }
                }
            }
            else if (x > _tileWidth + Rectangle.X && x < _tileWidth * 2 + Rectangle.X)
            {
                if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                {
                    if (!_ePressed)
                    {
                        _ePressed = detected;
                        keyCode = VirtualKeyCode.VK_E;
                    }
                }
                else
                {
                    if (!_dPressed)
                    {
                        _dPressed = detected;
                        keyCode = VirtualKeyCode.VK_D;
                    }
                }
            }
            else if (x > _tileWidth * 2 + Rectangle.X && x < _tileWidth * 3 + Rectangle.X)
            {
                if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                {
                    if (!_uPressed)
                    {
                        _uPressed = detected;
                        keyCode = VirtualKeyCode.VK_U;
                    }
                }
                else
                {
                    if (!_jPressed)
                    {
                        _jPressed = detected;
                        keyCode = VirtualKeyCode.VK_J;
                    }
                }
            }
            else if (x > _tileWidth * 3 + Rectangle.X)
            {
                if (y >= Rectangle.Y && y <= _tileHeight + Rectangle.Y)
                {
                    if (!_oPressed)
                    {
                        _oPressed = detected;
                        keyCode = VirtualKeyCode.VK_O;
                    }
                }
                else
                {
                    if (!_lPressed)
                    {
                        _lPressed = detected;
                        keyCode = VirtualKeyCode.VK_L;
                    }
                }
            }

            if (keyCode == VirtualKeyCode.RETURN)
                return;

            if (detected)
                DoKeyDown(keyCode);
            else
            {
                if (keyCode == VirtualKeyCode.VK_A && !_aPressed)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_E && !_ePressed)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_Q && !_qPressed)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_D && !_dPressed)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_U && !_uPressed)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_J && !_jPressed)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_O && !_oPressed)
                    DoKeyUp(keyCode);
                else if (keyCode == VirtualKeyCode.VK_L && !_lPressed)
                    DoKeyUp(keyCode);
            }
        }

        /// <summary>
        /// Loop through the pixels.
        /// 1. Check pixels with byte-value of 255. These are the 'detected' pixels
        /// 2. Check if they're inside a areamask with also 'detected' pixels (count at least similar to parameter 'minConstraint')
        /// 3. if retval == true, leave the pixel alone
        /// 4. is retval == false, make the bytevalue of the pixel 100 ('undetect' it)
        /// </summary>
        private void FilterFlyingPixels()
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
            var limit = 7;
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
                    try
                    {
                        var m = _mapping[x1, y1];
                        if(m != null)
                        {
                            m.ByteValue = 0;
                            _depthPixels[m.Index] = 0;
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                }

            return false;
        }

        public void DoKeyDown(VirtualKeyCode key)
        {
            if (!_run)
                return;

            if (!Keys[key])
            {
                Keys[key] = true;

                try
                {
                    _inputSimulator.Keyboard.KeyDown(key);
                }
                catch (Exception ex)
                {

                }

                
            }
        }

        public void DoKeyUp(VirtualKeyCode key)
        {
            if (!_run)
                return;

            if (Keys[key])
            {
                if (key == VirtualKeyCode.VK_A)
                    _aPressed = false;
                else if (key == VirtualKeyCode.VK_E)
                    _ePressed = false;
                else if (key == VirtualKeyCode.VK_Q)
                    _qPressed = false;
                else if (key == VirtualKeyCode.VK_D)
                    _dPressed = false;
                else if (key == VirtualKeyCode.VK_U)
                    _uPressed = false;
                else if (key == VirtualKeyCode.VK_J)
                    _jPressed = false;
                else if (key == VirtualKeyCode.VK_O)
                    _oPressed = false;
                else if (key == VirtualKeyCode.VK_L)
                    _lPressed = false;

                Keys[key] = false;
                try
                {
                    _inputSimulator.Keyboard.KeyUp(key);
                }
                catch(Exception ex)
                {

                }
            }
        }

        private void RenderDepthPixels()
        {
            _depthBitmap.WritePixels(
                new Int32Rect(0, 0, _depthBitmap.PixelWidth, _depthBitmap.PixelHeight),
                _depthPixels,
                _depthBitmap.PixelWidth,
                0
                );
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        #endregion

        public int Z = 0;

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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

        bool _run;
        private void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            _run = !_run;
            BtnSwitch.Content = _run ? "ON" : "OFF";
        }
    }
}
