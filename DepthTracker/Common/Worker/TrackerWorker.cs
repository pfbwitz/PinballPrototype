using DepthTracker.Common.Interface;
using DepthTracker.Settings;
using DepthTracker.UI;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsInput;

namespace DepthTracker.Common.Worker
{
    internal class TrackerWorker<T> where T : ISettings, new()
    {
        #region properties

        private List<System.Drawing.Point> WhitePoints = new List<System.Drawing.Point>();

        private int _tileHeightFactor = 2;

        public bool ClickHandled { get; set; }

        public ISettings Settings;

        public ITracker Window;

        public int ButtonTrigger = 3;

        public int UpCount;

        public int DownCount;

        private ushort[] _zBounds;
        public ushort[] ZBounds
        {
            get
            {
                try
                {
                    _zBounds = new ushort[] { Convert.ToUInt16(Window.ZMinText.Text), Convert.ToUInt16(Window.ZMaxText.Text) };
                }
                catch { }
                return _zBounds;
            }
        }

        public bool QHandled = false;

        public bool AHandled = false;

        public bool EHandled = false;

        public bool DHandled = false;

        public bool JHandled = false;

        public bool OHandled = false;

        public bool UHandled = false;

        public bool LHandled = false;

        public bool RHandled = false;

        public bool THandled = false;

        public bool FHandled = false;

        public bool GHandled = false;

        public bool ZHandled = false;

        public bool XHandled = false;

        public bool CHandled = false;

        public bool VHandled = false;

        public bool ReturnHandled = false;

        public bool ZCalibrated = false;

        public Rectangle Rectangle;

        public int LowerXBound { get; set; }

        public int UpperXBound { get; set; }

        public int LowerYBound { get; set; }

        public int UpperYBound { get; set; }

        public bool Run
        {
            get { return Settings.Run; }
            set { Settings.Run = value; }
        }

        public bool FlipY
        {
            get { return Settings.FlipY; }
            set {
                Settings.FlipY = value;
            }
        }

        public bool FlipX
        {
            get { return Settings.FlipX; }
            set
            {
                Settings.FlipX = value;
            }
        }

        public PixelMap[,] Mapping;

        public int Z = 0;

        public const int MapDepthToByte = 8000 / 256; // Map depth range to byte range

        public KinectSensor KinectSensor = null;

        public DepthFrameReader DepthFrameReader = null;

        public FrameDescription DepthFrameDescription = null;

        public byte[] DepthPixels = null;

        public int TileWidth;

        public int TileHeight;

        public InputSimulator InputSimulator;

        #endregion

        public static TrackerWorker<T> GetInstance (ITracker window, int? tileHeightFactor = null)
        {
            var t = new TrackerWorker<T>(window, tileHeightFactor);
            return t.Setup();
        }

        private TrackerWorker(ITracker window, int? tileHeightFactor = null) 
        {
            if (tileHeightFactor != null && tileHeightFactor.HasValue)
                _tileHeightFactor = tileHeightFactor.Value;

            Window = window;
            Init();
        }

        private void Init()
        {
            Settings = SettingsHelper<T>.GetInstance();

            KinectSensor = KinectSensor.GetDefault();
            KinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

            DepthFrameReader = KinectSensor.DepthFrameSource.OpenReader();
            DepthFrameReader.FrameArrived += Reader_FrameArrived;
            DepthFrameDescription = KinectSensor.DepthFrameSource.FrameDescription;

            Mapping = new PixelMap[DepthFrameDescription.Width, DepthFrameDescription.Height];

            InputSimulator = new InputSimulator();
            TileWidth = Rectangle.Width / 4;
            TileHeight = Rectangle.Height / _tileHeightFactor;

            DepthPixels = new byte[DepthFrameDescription.Width * DepthFrameDescription.Height];
            Window.DepthBitmap = new WriteableBitmap(DepthFrameDescription.Width, DepthFrameDescription.Height,
                96.0, 96.0, PixelFormats.Gray8, null);

            Rectangle = new Rectangle(Settings.X, Settings.Y, Settings.Width, Settings.Height);

            KinectSensor.Open();

            if (Window.Instance is CarTracker)
                ((CarTracker)Window.Instance).InitializeComponent();
            else if (Window.Instance is PinballTracker)
                ((PinballTracker)Window.Instance).InitializeComponent();
            else if (Window.Instance is ClicksTracker)
                ((ClicksTracker)Window.Instance).InitializeComponent();
        }

        public TrackerWorker<T> Setup()
        {
            Window.Instance.DataContext = Window.Instance;

            Window.XText.Text = Rectangle.X.ToString();
            Window.YText.Text = Rectangle.Y.ToString();
            Window.ZMinText.Text = Settings.ZMin.ToString();
            Window.ZMaxText.Text = Settings.ZMax.ToString();
            Window.WidthText.Text = Settings.Width.ToString();
            Window.HeightText.Text = Rectangle.Height.ToString();

            Sensor_IsAvailableChanged(null, null);

            Window.Instance.Closing += Window_Closing;
            Window.FlipButtonY.Click += BtnFlipY_Click;
            Window.FlipButtonX.Click += BtnFlipX_Click;
            Window.SwitchButton.Click += BtnSwitch_Click;

            Window.XText.TextChanged += TextBox_TextChanged;
            Window.YText.TextChanged += TextBox_TextChanged;
            Window.WidthText.TextChanged += TextBox_TextChanged;
            Window.HeightText.TextChanged += TextBox_TextChanged;
            Window.ZMinText.TextChanged += TextBox_TextChanged;
            Window.ZMaxText.TextChanged += TextBox_TextChanged;

            Window.FlipButtonY.Content = FlipY ? "FLIP OFF Y" : "FLIP ON Y";
            Window.FlipButtonX.Content = FlipX ? "FLIP OFF X" : "FLIP ON X";
            Window.SwitchButton.Content = Run ? "ON" : "OFF";

            LowerXBound = Settings.X;
            LowerYBound = Settings.Y;
            UpperXBound = Settings.X + Settings.Width;
            UpperYBound = Settings.Y + Settings.Height;

            return this;
        }

        #region handlers

        public void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            var bounds = ZBounds;
            using (var depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    using (var depthBuffer = depthFrame.LockImageBuffer())
                    {
                        if (((DepthFrameDescription.Width * DepthFrameDescription.Height) == (depthBuffer.Size / DepthFrameDescription.BytesPerPixel)) &&
                            (DepthFrameDescription.Width == Window.DepthBitmap.PixelWidth) && (DepthFrameDescription.Height == Window.DepthBitmap.PixelHeight))
                        {
                            
                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, bounds[0], bounds[1]);
                            Window.DepthBitmap.WritePixels(
                                 new Int32Rect(0, 0, Window.DepthBitmap.PixelWidth, Window.DepthBitmap.PixelHeight),
                                 DepthPixels,
                                 Window.DepthBitmap.PixelWidth,
                                 0
                            );
                        }
                    }
                }
            }
        }

        public void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (KinectSensor == null)
                Init();
            Window.StatusText = KinectSensor.IsAvailable ? "Running" :
             "Not connected";
        }

        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            int value;
            ZCalibrated = false;
            if (int.TryParse(box.Text, out value))
            {
                switch (box.Name)
                {
                    case "xText":
                        Settings.X = value;
                        Rectangle.X = value;
                        LowerXBound = value;
                        break;
                    case "yText":
                        Settings.Y = value;
                        Rectangle.Y = value;
                        LowerYBound = value;
                        break;
                    case "widthText":
                        Settings.Width = value;
                        Rectangle.Width = value;
                        UpperXBound = Rectangle.X + value;
                        break;
                    case "heightText":
                        Settings.Height = value;
                        Rectangle.Height = value;
                        UpperYBound = Rectangle.Y + value;
                        break;
                    case "zMinText":
                        Settings.ZMin = value;
                        ZCalibrated = true;
                        break;
                    case "zMaxText":
                        Settings.ZMax = value;
                        ZCalibrated = true;
                        break;
                }
            }

            TileWidth = Rectangle.Width / 4;
            TileHeight = Rectangle.Height / _tileHeightFactor;
        }

        public void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            Run = !Run;
            ((Button)sender).Content = Run ? "ON" : "OFF";
        }

        public void BtnFlipY_Click(object sender, RoutedEventArgs e)
        {
            FlipY = !FlipY;
            ((Button)sender).Content = FlipY ? "FLIP OFF Y" : "FLIP ON Y";
        }

        public void BtnFlipX_Click(object sender, RoutedEventArgs e)
        {
            FlipX = !FlipX;
            ((Button)sender).Content = FlipX ? "FLIP OFF X" : "FLIP ON X";
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DepthFrameReader != null)
            {
                DepthFrameReader.Dispose();
                DepthFrameReader = null;
            }
            if (KinectSensor != null)
            {
                KinectSensor.Close();
                KinectSensor = null;
            }
        }

        #endregion

        #region frameData

        public unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            ushort* frameData = (ushort*)depthFrameData;
            var tileWidth = Rectangle.Width / 4;
            var tileHeight = Rectangle.Height / 2;

            try
            {
                Loop((int)depthFrameDataSize, DepthFrameDescription.BytesPerPixel, frameData, minDepth, maxDepth);
            }
            catch (Exception ex)
            {
                var result = Window.ShowMessage(ex.Message + Environment.NewLine + ex.StackTrace, "Error");
            }

            FilterFlyingPixelsAndPushButtons();

            AHandled = false;
            QHandled = false;
            DHandled = false;
            EHandled = false;
            UHandled = false;
            JHandled = false;
            OHandled = false;
            LHandled = false;

            ClickHandled = false;
        }


        public unsafe void Loop(int depthFrameDataSize, uint bytesPerPixel, ushort* frameData, ushort minDepth, ushort maxDepth)
        {
            var width = DepthFrameDescription.Width;
            var height = DepthFrameDescription.Height;

            for (int i = 0; i < (int)(depthFrameDataSize / bytesPerPixel); ++i)
            {
                ushort pixelDepth = frameData[i];

                int x = i % DepthFrameDescription.Width;
                int y = i / DepthFrameDescription.Width;

                var detected = pixelDepth > 0 && pixelDepth > minDepth && pixelDepth < maxDepth; //is depth within our depth-margin

                //if true, pixel is within bounding rectangle
                var contains = x >= Rectangle.X &&
                    x <= Rectangle.X + Rectangle.Width &&
                    y >= Rectangle.Y &&
                    y <= Rectangle.Y + Rectangle.Height;

                //determine z-coordinate of the surface (possibly unreliable)
                if (!ZCalibrated && contains)
                {
                    ZCalibrated = true;
                    Window.Instance.Title = Convert.ToInt32(pixelDepth).ToString();
                }

                byte b;
                if (detected)
                { 
                    b = (byte)(contains ? 255 : 0); //pixel within depth-margin (z) and withing rectangle (x and y) should be white
                    Mapping[x, y] = new PixelMap { ByteValue = b, Index = i };

                    if (b == 255)
                        WhitePoints.Add(new System.Drawing.Point(x, y));
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
                DepthPixels[i] = b;
            }
        }

        #endregion

        #region pixel

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

            var closestPoint = GetClosestPoint(); // white pixel closest to the center of the screen (tip of hand)

            var recY = Rectangle.Y;
            var recX = Rectangle.X;
            var recW = Rectangle.Width;
            var recH = Rectangle.Height;

            //loop through the pixels withing the rectangle
            for (var y = recY; y < recY + recH; y++)
            {
                for (var x = recX; x < recX + recW; x++)
                {
                    var pixel = Mapping[x, y];

                    if (pixel == null)
                        continue;
                    if (pixel.ByteValue == 255 && !IsPositiveInPixelArea(x, y))
                    {
                        pixel.ByteValue = 100;
                        DepthPixels[pixel.Index] = 100;
                    }
                }
            }

            var @break = false;

            for (var y = recY; y < recY + recH; y++)
            {
                if (@break)
                    break;
                for (var x = recX; x < recX + recW; x++)
                {
                    if (@break)
                        break;
                    var pixel = Mapping[x, y];

                    if (pixel != null)
                    {
                        if(x == closestPoint.X && y == closestPoint.Y)
                        {
                            Window.PushButtons(x, y, pixel.ByteValue == 255);
                            if (pixel.ByteValue == 255)
                                @break = true;
                        }
                    }
                }
            }
            Mapping = new PixelMap[DepthFrameDescription.Width, DepthFrameDescription.Height];
            WhitePoints.Clear();
        }

        public System.Drawing.Point GetClosestPoint()
        {
            var center = new System.Drawing.Point(Rectangle.X + Rectangle.Width / 2, Rectangle.Y + Rectangle.Height / 2);

            var closestPoints = WhitePoints
                .Where(p => p != center)
                .OrderBy(p => NotReallyDistanceButShouldDo(center, p));
            return closestPoints.FirstOrDefault();
        }

        private double NotReallyDistanceButShouldDo(System.Drawing.Point source, System.Drawing.Point target)
        {
            return Math.Pow(target.X - source.X, 2) + Math.Pow(target.Y - source.Y, 2);
        }

        public bool IsPositiveInPixelArea(int x, int y)
        {
            var radius = 5;
            var limit = radius + 2;
            var count = 0;

            var upperXBound = x + radius <= UpperXBound ? x + radius : UpperXBound;
            var lowerXBound = x - radius >= LowerXBound ? x - radius : Rectangle.X;

            var upperYBound = y + radius <= UpperYBound ? y + radius : UpperYBound;
            var lowerYBound = y - radius >= LowerYBound ? y - radius : LowerYBound;

            for (var y1 = lowerYBound; y1 <= upperYBound; y1++)
            {
                for (var x1 = lowerXBound; x1 <= upperXBound; x1++)
                {
                    var mapping = Mapping[x1, y1];
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
                    var m = Mapping[x1, y1];
                    if (m != null)
                    {
                        m.ByteValue = 0;
                        DepthPixels[m.Index] = 0;
                    }
                }

            return false;
        }

        #endregion
    }
}
