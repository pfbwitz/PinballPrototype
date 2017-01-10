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

        private double smilieWidth;
        private double smilieHeight;

        private double lineWidth;
        private double lineHeight;

        // Resources
        private ImageSource BG = Booster.LoadImage("postit.png");
        private ImageSource Line = Booster.LoadImage("line.png");
        private ImageSource LineThrough = Booster.LoadImage("line_strikethrough.png");
        private ImageSource GoodSmilie = Booster.LoadImage("smilie_happy.png");
        private ImageSource MediumSmilie = Booster.LoadImage("smilie_nervous.png");
        private ImageSource BadSmilie = Booster.LoadImage("smilie_lamenting.png");



        public GameHUD(Game game)
        {
            Game = game;
            Width = 997;
            Height = 1385;//(int)(Width / BG.Width * BG.Height);

            lineWidth = 10;
            lineHeight = lineWidth / Line.Width * Line.Height;

            smilieWidth = 100;
            smilieHeight = smilieWidth / GoodSmilie.Width * GoodSmilie.Height;

        }

        public void Draw(DrawingContext g)
        {
            // 1. Draw BG
            //g.DrawImage(BG, new Rect(0, 0, Width, Height));


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
                g.DrawText(scoreTitle1, new Point(Width / 4 + scoreTitle1.Width / 2, Height / 3));
                g.DrawText(scoreText1, new Point(Width / 4 + scoreTitle1.Width / 2 + scoreTitle1.Width + 10, Height / 3));

                g.PushTransform(new RotateTransform(180, scoreText2.Width / 2, 32 + scoreText2.Height / 2));
                g.DrawText(scoreText2, new Point(-280, 300));
                g.Pop();
                g.PushTransform(new RotateTransform(180, scoreTitle2.Width / 2, 32 + scoreTitle2.Height / 2));
                g.DrawText(scoreTitle2, new Point(-(280 + scoreText2.Width + 10), 300));
                g.Pop();

                g.PushTransform(new RotateTransform(180, scoreText3.Width / 2, scoreText3.Height / 2 + 32));
                g.DrawText(scoreText3, new Point(-(Width + 32), 300));
                g.Pop();
                g.PushTransform(new RotateTransform(180, scoreTitle3.Width / 2, scoreTitle3.Height / 2 + 32));
                g.DrawText(scoreTitle3, new Point(-(scoreText3.Width + 10 + Width + 32), 300));
                g.Pop();

                g.DrawText(scoreTitle4, new Point(Width + scoreTitle4.Width / 2, Height / 3));
                g.DrawText(scoreText4, new Point(Width + scoreTitle4.Width / 2 + scoreTitle4.Width + 10 + 10, Height / 3));
                //g.DrawText(livesTitle, new Point(0, scoreTitle.Height + 5));


                // --- Lives ---
                //int i = 0;
                //for (; i < Game.Lives; i++)
                //{
                //    g.DrawImage(Line, new Rect(livesTitle.Width + (i + 1) * lineWidth, scoreText.Height + 5, lineWidth, lineHeight));
                //}
                //for (; i < Game.TOTAL_LIVES; i++)
                //{
                //    g.DrawImage(LineThrough, new Rect(livesTitle.Width + (i + 1) * lineWidth, scoreText.Height + 5, lineWidth, lineHeight));
                //}

                //g.PushTransform(new TranslateTransform(Width / 6, livesTitle.Height * 2));
                //{
                //    // --- Smilie ---
                //    //if (Game.Lives < Game.TOTAL_LIVES / 3)
                //    //{
                //    //    g.DrawImage(BadSmilie, new Rect(0, 0, smilieWidth, smilieHeight));
                //    //}
                //    //else if (Game.Lives < Game.TOTAL_LIVES / 3 * 2)
                //    //{
                //    //    g.DrawImage(MediumSmilie, new Rect(0, 0, smilieWidth, smilieHeight));
                //    //}
                //    //else
                //    //{
                //    //    g.DrawImage(GoodSmilie, new Rect(0, 0, smilieWidth, smilieHeight));
                //    //}
                //}
                //g.Pop();
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
