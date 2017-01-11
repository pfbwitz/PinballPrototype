using Sketchball.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace Sketchball.GameComponents
{
    /// <summary>
    /// Represents the "world"/"scene" in which the pinball machine is placed. Purely of graphical relevance.
    /// </summary>
    public class GameWorld
    {
        private const int MARGIN = 0;//60;
        private const int PADDING = 0;//40;

        private ImageSource BG = Booster.LoadImage("bg_desk.jpg");

        private Game Game;

        public GameWorld(Game game)
        {
            Game = game;
        }

        /// <summary>
        /// Draws the entire world with the PinballMachine at (0,0). Refer to Width and Height to get the notebook-aware dimensions.
        /// </summary>
        /// <param name="g"></param>
        public void Draw(DrawingContext g)
        {
            double height = Height + MARGIN * 6; // we're adding margins _again_ just to be sure.
            double width  = height / BG.Height * BG.Width;
           
            g.DrawImage(BG, new Rect((Width - width) / 2, 0, width, height));
            DrawMachine(g);
        }

        private void DrawMachine(DrawingContext g)
        {
            // InnerWidth + Padding
            int width = Game.Machine.Width;
            int height = Game.Machine.Height;

            // Draw book
            g.PushTransform(new TranslateTransform(MARGIN, MARGIN));

            // Draw machine
            g.PushTransform(new TranslateTransform(PADDING, PADDING));
            Game.Machine.Draw(g);
            g.Pop();
            g.Pop();

        }

        /// <summary>
        /// Gets the notebook-aware width (width + paddings and margins)
        /// </summary>
        public int Width
        {
            get
            {
                return Game.Machine.Width + PADDING * 2 + MARGIN * 2;
            }
        }


        /// <summary>
        /// Gets the notebook-aware height (height + paddings + margins)
        /// </summary>
        public int Height
        {
            get
            {
                return Game.Machine.Height + PADDING * 2 + MARGIN * 2;
            }
        }

        /// <summary>
        /// Gets the offset from (0, 0) to the pinball machine, i.e. the position where it is drawn at PinballMachine.Size without all the margins and paddings.
        /// </summary>
        public Point Offset
        {
            get
            {
                return new Point(MARGIN + PADDING, MARGIN + PADDING);
            }
        }
    }
}
