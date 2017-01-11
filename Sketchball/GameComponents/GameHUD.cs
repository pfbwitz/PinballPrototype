using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Sketchball.GameComponents
{
    /// <summary>
    /// Visual element that takes care of the head-up display.
    /// </summary>
    public class GameHUD
    {
        private Game Game;

        /// <summary>
        /// Gets the width of the HUD. Setting is not currently supported.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the HUD. Setting is not currently supported.
        /// </summary>
        public int Height { get; private set; }

        public GameHUD(Game game)
        {
            Game = game;
            Width = 997;
            Height = 1385;
        }

        public void Draw(DrawingContext g)
        {
            // 2. Create text objects
            var scoreTitle1 = GetText("Player 1");
            var scoreText1 = GetText(Game.Score1.ToString());

            var scoreTitle2 = GetText("Player 2");
            var scoreText2 = GetText(Game.Score2.ToString());

         
            var scoreTitle3 = GetText("Player 3");
            var scoreText3 = GetText(Game.Score3.ToString());

            var scoreTitle4 = GetText("Player 4");
            var scoreText4 = GetText(Game.Score4.ToString());

            // 3. Draw the stuff
            g.PushTransform(new TranslateTransform(Width / 4f, Height / 3.5f));
            {
                var add = Program.IsFourPlayerMode ? 0 : Width / 3 + 50;

                g.DrawText(scoreTitle1, new Point(add + Width / 4 + scoreTitle1.Width / 2, Height / 3));
                g.DrawText(scoreText1, new Point(add + Width / 4 + scoreTitle1.Width / 2 + scoreTitle1.Width + 10, Height / 3));

                g.PushTransform(new RotateTransform(180, scoreText2.Width / 2, 32 + scoreText2.Height / 2));
                g.DrawText(scoreText2, new Point(-(add + 280), 300));
                g.Pop();
                g.PushTransform(new RotateTransform(180, scoreTitle2.Width / 2, 32 + scoreTitle2.Height / 2));
                g.DrawText(scoreTitle2, new Point(-(add + 280 + scoreText2.Width + 10), 300));
                g.Pop();

                if (Program.IsFourPlayerMode)
                {
                    g.PushTransform(new RotateTransform(180, scoreText3.Width / 2, scoreText3.Height / 2 + 32));
                    g.DrawText(scoreText3, new Point(-(Width + 32), 300));
                    g.Pop();
                    g.PushTransform(new RotateTransform(180, scoreTitle3.Width / 2, scoreTitle3.Height / 2 + 32));
                    g.DrawText(scoreTitle3, new Point(-(scoreText3.Width + 10 + Width + 32), 300));
                    g.Pop();

                    g.DrawText(scoreTitle4, new Point(Width + scoreTitle4.Width / 2, Height / 3));
                    g.DrawText(scoreText4, new Point(Width + scoreTitle4.Width / 2 + scoreTitle4.Width + 10 + 10, Height / 3));
                }
            }
            g.Pop();
        }

        private FormattedText GetText(string text)
        {
            Typeface typeface = new Typeface(FontManager.Mechanik, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal, new FontFamily("Arial"));
            return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 22, Brushes.White);
        }
    }
}
