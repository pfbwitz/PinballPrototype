using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Threading;
using DepthTracker.EmuCV;
using System.Windows.Input;
using System.Windows.Forms;

namespace DepthTracker.UI
{
    public partial class CalibrationWindow : Window, INotifyPropertyChanged
    {
        public static int DimensionsWidth;
        public static int DimensionsHeight;

        public CalibrationWindow()
        {
            _kinectSensor = KinectSensor.GetDefault();

            _multiFrameSourceReader = _kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex);

            _multiFrameSourceReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

            var colorFrameDescription = _kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            DimensionsWidth = colorFrameDescription.Width;
            DimensionsHeight = colorFrameDescription.Height;
            _colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            _bitmapBackBufferSize = (uint)((_colorBitmap.BackBufferStride * (_colorBitmap.PixelHeight - 1)) + (_colorBitmap.PixelWidth * _bytesPerPixel));

            _kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

            _kinectSensor.Open();

            DataContext = this;

            InitializeComponent();

            StatusText = _kinectSensor.IsAvailable ? "Running" : "Not connected";
        }

        #region properties

        private readonly int _bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        private uint _bitmapBackBufferSize;

        private KinectSensor _kinectSensor = null;

        private MultiSourceFrameReader _multiFrameSourceReader = null;

        private WriteableBitmap _colorBitmap = null;

        private string _statusText = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource ImageSource
        {
            get { return _colorBitmap; }
        }

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                }
            }
        }

        #endregion

        #region handlers

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_multiFrameSourceReader != null)
            {
                _multiFrameSourceReader.Dispose();
                _multiFrameSourceReader = null;
            }

            //if (_kinectSensor != null)
            //{
            //    _kinectSensor.Close();
            //    _kinectSensor = null;
            //}
        }

        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            var calib = new CalibrationTemplateWindow();
            //if (Screen.AllScreens.Length > 1)
            //{
            //    var s2 = Screen.AllScreens[1];
            //    Rectangle r2 = s2.WorkingArea;
            //    calib.Top = r2.Top;
            //    calib.Left = r2.Left;
            //    calib.Show();
            //}
            //else
                calib.Show();

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                var dt = DateTime.Now;
                await Dispatcher.BeginInvoke(new Action(() => {
                    if (_colorBitmap != null)
                    {
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                        calib.Close();
                        Focus();

                        Rectangle? r;
                       
                        while(DateTime.Now <= dt.AddSeconds(5))
                        {
                            var result = ShapeHelper.GetRectangle(_colorBitmap, out r);
                            if(result && r.HasValue)
                            {
                                new MainWindow(r.Value).Show();
                                Close();
                                break;
                            }
                        }

                        //while (!ShapeHelper.GetRectangle(_colorBitmap, out r))
                        //{
                        //    if (r.HasValue)
                        //    {
                        //        new MainWindow(r.Value).Show();
                        //        Close();
                        //        break;
                        //    }
                        //    //else if (DateTime.Now >= dt.AddSeconds(5))
                        //    //    break;
                        //}
                    }
                }));
            });

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(_colorBitmap));

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "KinectScreenshot-Color-" + DateTime.Now.ToString("hh'-'mm'-'ss") + ".png");

            var s = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                    encoder.Save(fs);
            }
            catch (IOException)
            {
                s = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
            }
            finally
            {
                StatusText = s;
            }
        }

        private unsafe void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            int depthWidth = 0;
            int depthHeight = 0;

            DepthFrame depthFrame = null;

            ColorFrame colorFrame = null;
       
            bool isBitmapLocked = false;

            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            if (multiSourceFrame == null)
                return;

            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();

                if ((depthFrame == null) || (colorFrame == null))
                    return;
                
                depthWidth = depthFrame.FrameDescription.Width;
                depthHeight = depthFrame.FrameDescription.Height;

                //using (var depthFrameData = depthFrame.LockImageBuffer())
                //{
                //    if (depthFrame != null)
                //    {
                //        using (var depthBuffer = depthFrame.LockImageBuffer())
                //        {
                //            if (((depthWidth * depthHeight) == (depthBuffer.Size / depthFrame.FrameDescription.BytesPerPixel)))
                //            {
                //                ushort* frameData = (ushort*)depthBuffer.UnderlyingBuffer;
                //                var centerPixel = new System.Drawing.Point(depthWidth / 2, depthHeight / 2);
                //                for (int i = 0; i < (int)(depthBuffer.Size / _bytesPerPixel); ++i)
                //                {
                //                    var pixelPoint = new System.Drawing.Point(i % depthWidth, i / depthHeight);

                //                    if (centerPixel.X + 10 > pixelPoint.X && centerPixel.X - 10 < centerPixel.Y &&
                //                        centerPixel.Y + 10 > pixelPoint.Y && centerPixel.Y - 10 < pixelPoint.Y)
                //                    {
                //                        CenterZ = frameData[i];
                //                        break;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                depthFrame.Dispose();
                depthFrame = null;

                _colorBitmap.Lock();
                isBitmapLocked = true;

                colorFrame.CopyConvertedFrameDataToIntPtr(_colorBitmap.BackBuffer, _bitmapBackBufferSize, ColorImageFormat.Bgra);

                if (colorFrame != null)
                {
                    var colorFrameDescription = colorFrame.FrameDescription;

                    using (var colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        _colorBitmap.Lock();

                        if ((colorFrameDescription.Width == _colorBitmap.PixelWidth) && (colorFrameDescription.Height == _colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                _colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            _colorBitmap.AddDirtyRect(new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight));
                        }

                        _colorBitmap.Unlock();
                    }
                }

                colorFrame.Dispose();
                colorFrame = null;

            }
            finally
            {
                if (isBitmapLocked)
                    _colorBitmap.Unlock();
                
                if (colorFrame != null)
                    colorFrame.Dispose();
            }
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            StatusText = _kinectSensor.IsAvailable ? "Running" : "Not connected";
        }

        #endregion
    }
}


//using System;
//using System.ComponentModel;
//using System.IO;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using Microsoft.Kinect;
//using System.Drawing;
//using System.Threading.Tasks;
//using System.Windows.Threading;
//using DepthTracker.EmuCV;
//using System.Windows.Input;

//namespace DepthTracker.UI
//{
//    public partial class CalibrationWindow : Window, INotifyPropertyChanged
//    {
//        public static int DimensionsWidth;
//        public static int DimensionsHeight;

//        public CalibrationWindow()
//        {
//            _kinectSensor = KinectSensor.GetDefault();

//            _colorFrameReader = _kinectSensor.ColorFrameSource.OpenReader();

//            _colorFrameReader.FrameArrived += Reader_ColorFrameArrived;

//            var colorFrameDescription = _kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
//            DimensionsWidth = colorFrameDescription.Width;
//            DimensionsHeight = colorFrameDescription.Height;
//            _colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

//            _kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

//            _kinectSensor.Open();

//            DataContext = this;

//            InitializeComponent();

//            StatusText = _kinectSensor.IsAvailable ? "Running" : "Not connected";
//        }

//        #region properties

//        private KinectSensor _kinectSensor = null;

//        private ColorFrameReader _colorFrameReader = null;

//        private WriteableBitmap _colorBitmap = null;

//        private string _statusText = null;

//        public event PropertyChangedEventHandler PropertyChanged;

//        public ImageSource ImageSource
//        {
//            get { return _colorBitmap; }
//        }

//        public string StatusText
//        {
//            get
//            {
//                return _statusText;
//            }
//            set
//            {
//                if (_statusText != value)
//                {
//                    _statusText = value;
//                    PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
//                }
//            }
//        }

//        #endregion

//        #region handlers

//        private void MainWindow_Closing(object sender, CancelEventArgs e)
//        {
//            if (_colorFrameReader != null)
//            {
//                _colorFrameReader.Dispose();
//                _colorFrameReader = null;
//            }

//            if (_kinectSensor != null)
//            {
//                _kinectSensor.Close();
//                _kinectSensor = null;
//            }
//        }

//        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
//        {
//            var calib = new CalibrationTemplateWindow();
//            calib.Show();

//            Task.Run(async () =>
//            {
//                await Task.Delay(1000);

//                await Dispatcher.BeginInvoke(new Action(() => {
//                    if (_colorBitmap != null)
//                    {
//                        Mouse.OverrideCursor = Cursors.Arrow;
//                        calib.Close();
//                        Focus();

//                        Rectangle? r;
//                        while (!ShapeHelper.GetRectangle(_colorBitmap, out r))
//                        {
//                            if (r.HasValue)
//                            {
//                                var w = new MainWindow(r.Value);
//                                w.Show();
//                                Close();
//                                break;
//                            }
//                        }
//                    }
//                }));
//            });

//            var encoder = new PngBitmapEncoder();
//            encoder.Frames.Add(BitmapFrame.Create(_colorBitmap));

//            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
//                "KinectScreenshot-Color-" + DateTime.Now.ToString("hh'-'mm'-'ss") + ".png");

//            var s = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
//            try
//            {
//                using (var fs = new FileStream(path, FileMode.Create))
//                    encoder.Save(fs);
//            }
//            catch (IOException)
//            {
//                s = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
//            }
//            finally
//            {
//                StatusText = s;
//            }
//        }

//        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
//        {
//            using (var colorFrame = e.FrameReference.AcquireFrame())
//            {
//                if (colorFrame != null)
//                {
//                    var colorFrameDescription = colorFrame.FrameDescription;

//                    using (var colorBuffer = colorFrame.LockRawImageBuffer())
//                    {
//                        _colorBitmap.Lock();

//                        // verify data and write the new color frame data to the display bitmap
//                        if ((colorFrameDescription.Width == _colorBitmap.PixelWidth) && (colorFrameDescription.Height == _colorBitmap.PixelHeight))
//                        {
//                            colorFrame.CopyConvertedFrameDataToIntPtr(
//                                _colorBitmap.BackBuffer,
//                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
//                                ColorImageFormat.Bgra);

//                            _colorBitmap.AddDirtyRect(new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight));
//                        }

//                        _colorBitmap.Unlock();
//                    }
//                }
//            }
//        }

//        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
//        {
//            StatusText = _kinectSensor.IsAvailable ? "Running" : "Not connected";
//        }

//        #endregion
//    }
//}
