using DepthTracker.Common;
using DepthTracker.Settings;
using Microsoft.Kinect;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsInput;

namespace DepthTracker.UI
{
    internal class TrackerProperties<T> where T : ISettings, new()
    {
        #region attr

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

        public bool Housamfix = false;

        public bool ZCalibrated = false;

        public Rectangle Rectangle;

        public int LowerXBound;

        public int UpperXBound;

        public int LowerYBound;

        public int UpperYBound;

        public bool Run
        {
            get { return Settings.Run; }
            set { Settings.Run = value; }
        }

        public bool Flip
        {
            get { return Settings.Flip; }
            set {
                Settings.Flip = value;
            }
        }

        public PixelMap[,] Mapping;

        public int Z = 0;

        public const int MapDepthToByte = 8000 / 256; // Map depth range to byte range

        public KinectSensor KinectSensor = null;

        public DepthFrameReader DepthFrameReader = null;

        public FrameDescription DepthFrameDescription = null;

        public WriteableBitmap DepthBitmap = null;

        public byte[] DepthPixels = null;

        public int TileWidth;

        public int TileHeight;

        public InputSimulator InputSimulator;

        #endregion

        public TrackerProperties(ITracker window) 
        {
            Window = window;
            Settings = SettingsHelper<T>.GetInstance();
        }


        public void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            var bounds = ZBounds;
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
                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, bounds[0], bounds[1]);

                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                DepthBitmap.WritePixels(
                     new Int32Rect(0, 0, DepthBitmap.PixelWidth, DepthBitmap.PixelHeight),
                     DepthPixels,
                     DepthBitmap.PixelWidth,
                     0
                );
            }
        }

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
                    Window.SetTitle(Convert.ToInt32(pixelDepth).ToString());
                }

                byte b;

                if (detected)
                {
                    b = (byte)(contains ? 255 : 0); //pixel within depth-margin (z) and withing rectangle (x and y) should be white
                    Mapping[x, y] = new PixelMap { ByteValue = b, Index = i };
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

        public bool IsPositiveInPixelArea(int x, int y)
        {
            //  return true;
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

        public void Setup()
        {
            KinectSensor = KinectSensor.GetDefault();
            KinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

            DepthFrameReader = KinectSensor.DepthFrameSource.OpenReader();
            DepthFrameReader.FrameArrived += Reader_FrameArrived;
            DepthFrameDescription = KinectSensor.DepthFrameSource.FrameDescription;

            Mapping = new PixelMap[DepthFrameDescription.Width, DepthFrameDescription.Height];

            InputSimulator = new InputSimulator();
            TileWidth = Rectangle.Width / 4;
            TileHeight = Rectangle.Height / 2;

            DepthPixels = new byte[DepthFrameDescription.Width * DepthFrameDescription.Height];
            DepthBitmap = new WriteableBitmap(DepthFrameDescription.Width, DepthFrameDescription.Height,
                96.0, 96.0, PixelFormats.Gray8, null);

            Rectangle = new Rectangle(Settings.X, Settings.Y, Settings.Width, Settings.Height);

            KinectSensor.Open();
        }

        public void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            SetStatusText();
        }

        public void SetStatusText()
        {
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
                        LowerYBound = value;
                        break;
                    case "yText":
                        Settings.Y = value;
                        Rectangle.Y = value;
                        LowerYBound = value;
                        break;
                    case "widthText":
                        Settings.Width = value;
                        Rectangle.Width = value;
                        UpperYBound = Rectangle.X + value;
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
            TileHeight = Rectangle.Height / 2;
        }

        public void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            Run = !Run;
            ((Button)sender).Content = Run ? "ON" : "OFF";
        }

        public void BtnFlip_Click(object sender, RoutedEventArgs e)
        {
            Flip = !Flip;
            ((Button)sender).Content = Flip ? "FLIP OFF" : "FLIP ON";
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

            for (var y = recY; y < recY + recH; y++)
            {
                for (var x = recX; x < recX + recW; x++)
                {
                    var pixel = Mapping[x, y];
                    if (pixel != null)
                        Window.PushButtons(x, y, pixel.ByteValue == 255);
                }
            }

            Mapping = new PixelMap[DepthFrameDescription.Width, DepthFrameDescription.Height];
        }
    }
}
