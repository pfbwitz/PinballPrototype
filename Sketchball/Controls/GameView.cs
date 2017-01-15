using DepthTracker.Hands;
using Fleck;
using Sketchball.Elements;
using Sketchball.GameComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Sketchball.Controls
{
    /// <summary>
    /// Control that houses a game of pinball and provides a view on it.
    /// </summary>
    class GameView : PinballControl
    {

        /// <summary>
        /// Arbitrarily chosen FPS number that controls the update interval. We only have lose control over the visual update process
        /// i.e. we don't know when the update is done. If we use an update worker, we may be too fast and thus block user input. Furthermore,
        /// we would have to use a Dispatcher, which would make the application more prone to errors.
        /// By using a common timer with a certain tick rate, we may lose some accuracy, but in exchange Windows can take care of it all.
        /// </summary>
        private const int MAX_FPS = 40;

        /// <summary>
        /// The camera being used to look at the scene.
        /// </summary>
        public Camera Camera { get; private set; }

        // The HUD
        private GameHUD HUD;

        // The game scene.
        private GameWorld gameWorld;

        public Game Game;
        private System.Windows.Forms.Timer timer;

        /// <summary>
        /// Creates a new PinballGameControl based on a machine template.
        /// </summary>
        /// <param name="machine">Template for the game machine.</param>
        public GameView(Game game)
            : base()
        {
            InitializeSockets();

            foreach(var s in _sockets)
                s.OnMessage += HandleWebSocketInput;
            
            Game = game;
            gameWorld = new GameWorld(Game);
            HUD = new GameHUD(Game);

            Camera = new GameFieldCamera(gameWorld, HUD);

            // Little hack needed so that we get all input events.
            Focusable = true;
            Loaded += (s, e) => { Focus(); };

            // Init camera
            Camera.Size = new Size(Width, Height);

            // Set up the update timer
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000 / MAX_FPS;
            timer.Tick += OnTick;
            timer.Start();

            // Wire up a few event listeners

            MouseDown += GameView_MouseDown;
            MouseUp += GameView_MouseUp;
            PreviewKeyDown += HandleKeyDown;
            SizeChanged += ResizeCamera;
            Game.StatusChanged += delegate { Dispatcher.Invoke(delegate { InvalidateVisual(); }, System.Windows.Threading.DispatcherPriority.Render); };

            // Let's draw with high quality
            SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
        }

        private void GameView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GetFlipper(PointToScreen(Mouse.GetPosition(this))).OnKeyUp();
        }

        private void GameView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GetFlipper(PointToScreen(Mouse.GetPosition(this))).OnKeyDown();
        }

        private bool GetMouseXQuarter(Point pos, int position, out int squarePos)
        {
            var xParam = Program.IsFourPlayerMode ? 4 : 2;
            var result = (pos.X > Width / xParam * position && pos.X < Width / xParam * (position + 1));
            squarePos = result ? position + 1 : 0;
            return result;
        }

        private bool GetMouseYQuarter(Point pos, int position, out int squarePos)
        {
            var result = (pos.Y > Height / 2 * position && pos.Y < Height / 2 * (position + 1));
            squarePos = result ? position + 1 : 0;
            return result;
        }

        private Flipper GetFlipper(Point pos)
        {
            var xQuarter = 0;
            var yQuarter = 0;

            var xParam = Program.IsFourPlayerMode ? 4 : 2;
            for (var i = 0; i < xParam; i++)
                if (GetMouseXQuarter(pos, i, out xQuarter))
                    break;
            
            for (var i = 0; i < 2; i++)
                if (GetMouseYQuarter(pos, i, out yQuarter))
                    break;

            if(yQuarter == 1)
            {
                if (xQuarter == 1 || xQuarter == 3)
                    return Game.Machine.StaticElements.OfType<RightFlipper>().ElementAt(xQuarter == 1 || xQuarter == 2 ? 1 : 3);
                return Game.Machine.StaticElements.OfType<LeftFlipper>().ElementAt(xQuarter == 1 || xQuarter == 2 ? 1 : 3);
            }
            else
            {
                if (xQuarter == 1 || xQuarter == 3)
                    return Game.Machine.StaticElements.OfType<LeftFlipper>().ElementAt(xQuarter == 1 || xQuarter == 2 ? 0 : 2);
                return Game.Machine.StaticElements.OfType<RightFlipper>().ElementAt(xQuarter == 1 || xQuarter == 2 ? 0 : 2);
            }
        }

        #region sockets

        static List<IWebSocketConnection> _sockets;
        static bool _initialized = false;
        private void InitializeSockets()
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
                socket.OnMessage += HandleWebSocketInput;
            });

            _initialized = true;

            //Console.ReadLine();
        }

        private void HandleWebSocketInput(string input)
        {
            var tiles = TileSerializer.Deserialize(input);
            var tileWidth = Width / (Program.IsFourPlayerMode ? 4 : 2);
            var tileHeight = Height / 2;
            foreach (var t in tiles)
            {
                //the middle of a tile
                var point = new Point((t.Col - 1) * tileWidth + (tileWidth / 2), (t.Row - 1) * tileHeight + (tileHeight / 2));
                var flipper = GetFlipper(point);

                flipper.OnKey(t.Touch);
            }
        }

        #endregion

        private void ResizeCamera(object sender, SizeChangedEventArgs e)
        {
            Camera.Size = new Size((int)Width, (int)Height);
        }


        /// <summary>
        /// Handles key presses (used to initiate a new game)
        /// </summary>
        private void HandleKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    if ((!Game.IsRunning) || Game.Status == GameStatus.GameOver)
                        Game.Start();
                    break;

                case Key.Enter:
                    if (Game.Status == GameStatus.Playing)
                        Game.Pause();
                    else if (Game.Status == GameStatus.Paused)
                        Game.Resume();
                    break;
            }

        }

        // Draw loop
        private void OnTick(object sender, EventArgs e)
        {
            if (isCancelled)
                timer.Dispose();
            else if(Game.Status == GameStatus.Playing) // Only draw if it's needed
                InvalidateVisual();
        }

        protected override void OnDispose()
        {
            // ElementHost sometimes fails to properly remove all references to a WPF control,
            // which is why we set all our own references to NULL so that at least those aren't kept alive forever.
            timer.Dispose();
            Game = null;
            gameWorld = null;
            HUD = null;
            Camera = null;
        }

        // Draw scene
        protected override void Draw(DrawingContext g)
        {

            if (Game.Status == GameStatus.GameOver || Game.Status == GameStatus.Paused)
            {
                // We'll make a first render pass with a blur shader, and after that we'll draw an overlay on it
                var firstPass = new DrawingVisual();
                firstPass.Effect = new BlurEffect();

                using (var g2 = firstPass.RenderOpen())
                {
                    Camera.Draw(g2);
                }

                g.DrawImage(GetImage(firstPass, (int)Width, (int)Height), new Rect(0, 0, Width, Height));


                if (Game.Status == GameStatus.GameOver)
                {
                    DrawOverlay(g, Colors.DarkRed, "GAME OVER", "Press [SPACE] to try again.");
                }
                else if (Game.Status == GameStatus.Paused)
                {
                    DrawOverlay(g, Colors.DarkBlue, "PAUSED", "Press [ENTER] to resume.");
                }

            }
            else
            {
                // We're playing, so just let the camera draw itself
                Camera.Draw(g);
            }
       }

        
        private void DrawOverlay(DrawingContext g, Color color, string title, string msg)
        {
            var col = Color.FromArgb(40, color.R, color.G, color.B);

            Brush brush = new SolidColorBrush(col);
            Brush solidBrush = new SolidColorBrush(color);

            var caption = Booster.GetText(title, new FontFamily("Impact"), 40, solidBrush);
            var text = Booster.GetText(msg, new FontFamily("Arial"), 13, solidBrush);
            double x = (Width - caption.Width) / 2;


            g.DrawRectangle(brush, null, new Rect(0, 0, Width, Height));
            g.DrawText(caption, new Point(x, (Height - caption.Height) / 2));
            g.DrawText(text, new Point(x, (Height + caption.Height) / 2));
        }

        // Turns a DrawingVisual into an ImageSource. Somewhat bothersome procedure, hence outsourced into its own function.
        private ImageSource GetImage(DrawingVisual visual, int width, int height)
        {
            visual.Clip = new RectangleGeometry(new Rect(0, 0, width, height));
            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);
            return bmp;
        }
    }
}
