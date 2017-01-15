using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Collections.Generic;
using DepthTracker.Hands;
using System.Linq;
using System.Runtime.InteropServices;
using Fleck;

namespace DepthTracker.UI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        #region attr

        public ImageSource ImageSource { get { return _depthBitmap; } }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                   // PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                }
            }
        }

        private List<Tile> _tiles = new List<Tile>();

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
            InitializeSockets();

            _kinectSensor = KinectSensor.GetDefault();
            _depthFrameReader = _kinectSensor.DepthFrameSource.OpenReader();
            _depthFrameReader.FrameArrived += Reader_FrameArrived;
            _depthFrameDescription = _kinectSensor.DepthFrameSource.FrameDescription;
            _depthPixels = new byte[_depthFrameDescription.Width * _depthFrameDescription.Height];
            _depthBitmap = new WriteableBitmap(_depthFrameDescription.Width, _depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

            _kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;
            _kinectSensor.Open();

            StatusText = _kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText : 
                Properties.Resources.NoSensorStatusText;

            DataContext = this;

            InitializeComponent();
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
            var depthFrameProcessed = false;

            using (var depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    using (var depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((_depthFrameDescription.Width * _depthFrameDescription.Height) == (depthBuffer.Size / _depthFrameDescription.BytesPerPixel)) &&
                            (_depthFrameDescription.Width == _depthBitmap.PixelWidth) && (_depthFrameDescription.Height == _depthBitmap.PixelHeight))
                        {
                            ushort maxDepth = 1100;// ushort.MaxValue;
                            ushort minDepth = 1000;// depthFrame.DepthMinReliableDistance

                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, minDepth, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                RenderDepthPixels();
                var tileWidth = _depthFrameDescription.Width / 4;
                var tileHeight = _depthFrameDescription.Height / 2;

                var json = _tiles.Serialize();
                foreach (var socket in _sockets)
                    socket.Send(json);

                //foreach(var t in _tiles)

                //    //var rectangle = new Int32Rect((t.Col - 1) * tileWidth, (t.Row - 1) * tileHeight, tileWidth, tileHeight);
                //    //var clickX = rectangle.X + rectangle.Width / 2;
                //    //var clickY = rectangle.Y + rectangle.Height / 2;
                //    //SetCursorPos(clickX, clickY);

                //    //MouseOperations.MouseEvent(t.Touch ? MouseOperations.MouseEventFlags.LeftDown : 
                //    //    MouseOperations.MouseEventFlags.LeftUp);
                //}
                _tiles.Clear();
            }
        }

        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / _depthFrameDescription.BytesPerPixel); ++i)
            {
                ushort pixelDepth = frameData[i];
                var detected = pixelDepth >= minDepth && pixelDepth <= maxDepth;
                _depthPixels[i] = (byte)(detected ? 255 : 0);

                var tile = Tile.GetInstanceForPixel(
                    new Point(i % _depthFrameDescription.Width, i / _depthFrameDescription.Height), 
                    new Point(_depthFrameDescription.Width, _depthFrameDescription.Height), 
                    detected
                    );
                if (!_tiles.Any(t => t.Col == tile.Col && t.Row == tile.Row))
                    _tiles.Add(tile);
            }
        }

        private void RenderDepthPixels()
        {
            _depthBitmap.WritePixels(
                new Int32Rect(0, 0, _depthBitmap.PixelWidth, _depthBitmap.PixelHeight),
                _depthPixels, _depthBitmap.PixelWidth, 0
                );
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            StatusText = _kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                : Properties.Resources.SensorNotAvailableStatusText;
        }

        #region sockets

        static List<IWebSocketConnection> _sockets;
        static bool _initialized = false;
        private static void InitializeSockets()
        {
            _sockets = new List<IWebSocketConnection>();

            var server = new WebSocketServer("ws://localhost:8181");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Connected to " + socket.ConnectionInfo.ClientIpAddress);
                    _sockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Disconnected from " + socket.ConnectionInfo.ClientIpAddress);
                    _sockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                };
            });

            _initialized = true;

            //Console.ReadLine();
        }

        #endregion
    }
}
