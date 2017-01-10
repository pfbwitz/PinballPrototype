using Sketchball.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Sketchball.Elements
{
    /// <summary>
    /// Represents a left slingshot.
    /// </summary>
    [DataContract]
    public class Wall : PinballElement
    {
        private static readonly Size size = new Size(50, 200);
        private static readonly SoundPlayer player = new SoundPlayer(Properties.Resources.SSlingshot);


        protected override Size BaseSize
        {
            get { return size; }
        }

        public Wall()
        {
            Value = 10;
        }

        protected override void Init()
        {
            Vector p1 = new Vector(0, 0);
            Vector p2 = new Vector(50, 0);
            Vector p3 = new Vector(50, 200);
            Vector p4 = new Vector(0, 200);

            p1 /= 4;
            p2 /= 4;
            p3 /= 4;
            p4 /= 4;

            BoundingLine bL1 = new BoundingLine(p1, p2);
            BoundingLine bL2 = new BoundingLine(p2, p3);
            BoundingLine bL3 = new BoundingLine(p3, p4);
            BoundingLine bL4 = new BoundingLine(p4, p1);

            var f = 1;
            bL1.BounceFactor = f;
            bL2.BounceFactor = f;
            bL3.BounceFactor = f;
            bL4.BounceFactor = f;
            this.BoundingContainer.AddBoundingBox(bL1);
            this.BoundingContainer.AddBoundingBox(bL2);
            this.BoundingContainer.AddBoundingBox(bL3);
            this.BoundingContainer.AddBoundingBox(bL4);

        }

        protected override void InitResources()
        {
            Image = Booster.LoadImage("Wall.png");
        }

        public override void OnIntersection(Ball b)
        {
            GameWorld.Sfx.Play(player);
        }

    }
}
