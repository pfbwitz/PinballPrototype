using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Collections.Generic;
using DepthTracker.Tiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.IO;
using DepthTracker.Connection;
using System.Threading;
using System.Text;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using System.Threading.Tasks;

namespace DepthTracker.UI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        #region attr

        private PipeClient _pipeClient;

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

        private List<Tile> _tiles;

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

            _kinectSensor.Open();

            DataContext = this;

            InitializeComponent();

            Sensor_IsAvailableChanged(null, null);
        }

        #region handlers

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _tiles.Clear();
            if (_pipeClient != null)
            {
                _pipeClient.Dispose();
                _pipeClient = null;
            }
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
                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, minDepth: 1000, maxDepth: 1100);
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

            _inputSimulator = new InputSimulator();
            _tileWidth = _depthFrameDescription.Width / 4;
            _tileHeight = _depthFrameDescription.Height / 2;

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
            var tileWidth = _depthFrameDescription.Width / 4;
            var tileHeight = _depthFrameDescription.Height / 2;

            Loop((int)depthFrameDataSize, _depthFrameDescription.BytesPerPixel, frameData, minDepth, maxDepth);

            _aPressed = false;
            _qPressed = false;
            _dPressed = false;
            _ePressed = false;
            _uPressed = false;
            _jPressed = false;
            _oPressed = false;
            _lPressed = false;
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

        private unsafe void Loop(int depthFrameDataSize, uint bytesPerPixel, ushort* frameData, ushort minDepth, ushort maxDepth)
        {
            for (int i = 0; i < (int)(depthFrameDataSize / bytesPerPixel); ++i)
            {
                ushort pixelDepth = frameData[i];
                var detected = pixelDepth >= minDepth && pixelDepth <= maxDepth;
                _depthPixels[i] = (byte)(detected ? 255 : 0);

                var pixelPoint = new System.Drawing.Point(i % _depthFrameDescription.Width, i / _depthFrameDescription.Height);
                VirtualKeyCode keyCode = VirtualKeyCode.RETURN;
                if (pixelPoint.X > 0 && pixelPoint.X <= _tileWidth)
                {
                    if(pixelPoint.Y >= 0 && pixelPoint.Y <= _tileHeight)
                    {
                        if(!_qPressed)
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
                else if (pixelPoint.X > _tileWidth && pixelPoint.X < _tileWidth * 2)
                {
                    if(pixelPoint.Y >= 0 && pixelPoint.Y <= _tileHeight)
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
                else if (pixelPoint.X > _tileWidth * 2 && pixelPoint.X < _tileWidth * 3)
                {
                    if(pixelPoint.Y >= 0 && pixelPoint.Y <= _tileHeight)
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
                else if (pixelPoint.X > _tileWidth * 3)
                {
                    if(pixelPoint.Y >= 0 && pixelPoint.Y <= _tileHeight)
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
                    continue;

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
        }

        public void DoKeyDown(VirtualKeyCode key)
        {
            if (!Keys[key])
            {
                Keys[key] = true;
                _inputSimulator.Keyboard.KeyDown(key);
            }
        }
         
        public void DoKeyUp(VirtualKeyCode key)
        {
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
                _inputSimulator.Keyboard.KeyUp(key);
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
    }
}
