using Sketchball.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Sketchball.Elements
{
    /// <summary>
    /// Represents the default implementation of IMachineLayout.
    /// </summary>
    [DataContract]
    public class DefaultLayout : IMachineLayout
    {

        private StartingRamp _ramp;
        private List<PinballElement> _elements;


        private class Frame : PinballElement
        {
            private static readonly Size size = new Size(2 * 997, 1385);

            internal Frame() : base(0, 0) { }

            protected override void Init()
            {
                var totalWidth = (int)this.Width;
                int totalHeight = (int)this.Height;
                Scale = 1 / 2f;
            }

            protected override Size BaseSize
            {
                get { return size; }
            }

            protected override void InitResources()
            {
                Image = Booster.LoadImage("TableSlim.png");
            }
        }

        public DefaultLayout()
        {
            Init();
        }


        private void Init()
        {
            _elements = new List<PinballElement>();
            _elements.Add(new Frame());

            // Add ramp
            _ramp = new StartingRamp();
            //_elements.Add(_ramp);

            _ramp.X = Width - _ramp.Width - 4;
            _ramp.Y = Height - _ramp.Height;


            var baseLeftX = 134;
            var baseRightX = 253;
            var baseTopY = 19;
            var baseBottomtY = 105;
            float power = 2;
            var flipperscale = 1.5;
            //Add flippers
            #region left
            Flipper lflipper = new LeftFlipper() { X = baseLeftX, Y = Height - baseBottomtY };
            lflipper.Name = "1";
            lflipper.BounceFactor = power;
            lflipper.Scale = flipperscale;
            _elements.Add(lflipper);

            Flipper rflipper = new RightFlipper() { X = baseRightX, Y = Height - baseBottomtY };
            rflipper.Name ="1";
            rflipper.Scale = flipperscale;
            rflipper.BounceFactor = power;
            _elements.Add(rflipper);

            Flipper lflipperSec = new LeftFlipper() { X = baseLeftX + - 60, Y = Height - baseBottomtY * 2 + 20 };
            lflipperSec.Name = "1";
            lflipperSec.BounceFactor = power - 0.5f;
            lflipperSec.Scale = flipperscale * 0.6;
            _elements.Add(lflipperSec);

            Flipper rflipperSec = new RightFlipper() { X = baseRightX + 115, Y = Height - baseBottomtY * 2 + 20 };
            rflipperSec.Name = "1";
            rflipperSec.Scale = flipperscale * 0.6;
            rflipperSec.BounceFactor = power - 0.5f;
            _elements.Add(rflipperSec);

            Flipper lflipperTop = new LeftFlipper() { X = baseRightX, Y = -baseTopY };
            lflipperTop.Name = "2";
            lflipperTop.Scale = flipperscale;
            lflipperTop.BounceFactor = power;
            lflipperTop.BaseRotation = 180;
            lflipperTop.Trigger = System.Windows.Forms.Keys.E;
            _elements.Add(lflipperTop);

            Flipper rflipperTop = new RightFlipper() { X = baseLeftX, Y = -baseTopY };
            rflipperTop.Scale = flipperscale;
            rflipperTop.Name = "2";
            rflipperTop.BounceFactor = power;
            rflipperTop.BaseRotation = 180;
            rflipperTop.Trigger = System.Windows.Forms.Keys.Q;
            _elements.Add(rflipperTop);

            Flipper lflipperTopSec = new LeftFlipper() { X = baseRightX + 115, Y = -baseTopY * -2.5 + 70 };
            lflipperTopSec.Name = "2";
            lflipperTopSec.Scale = flipperscale * 0.6;
            lflipperTopSec.BounceFactor = power - 0.5f;
            lflipperTopSec.BaseRotation = 180;
            lflipperTopSec.Trigger = System.Windows.Forms.Keys.E;
            _elements.Add(lflipperTopSec);

            Flipper rflipperTopSec = new RightFlipper() { X = baseLeftX - 60, Y = -baseTopY * -2.5 + 70 };
            rflipperTopSec.Scale = flipperscale * 0.6;
            rflipperTopSec.Name = "2";
            rflipperTopSec.BounceFactor = power - 0.5f;
            rflipperTopSec.BaseRotation = 180;
            rflipperTopSec.Trigger = System.Windows.Forms.Keys.Q;
            _elements.Add(rflipperTopSec);
            #endregion

            if (Program.IsFourPlayerMode)
            {
                #region right
                Flipper lflipper2 = new LeftFlipper() { X = baseLeftX + 997 / 2, Y = Height - baseBottomtY };
                lflipper2.Scale = flipperscale;
                lflipper2.Name = "4";
                lflipper2.BounceFactor = power;
                lflipper2.Trigger = System.Windows.Forms.Keys.J;
                _elements.Add(lflipper2);

                Flipper rflipper2 = new RightFlipper() { X = baseRightX + 997 / 2, Y = Height - baseBottomtY };
                rflipper2.Scale = flipperscale;
                rflipper2.Name = "4";
                rflipper2.BounceFactor = power;
                rflipper2.Trigger = System.Windows.Forms.Keys.L;
                _elements.Add(rflipper2);

                Flipper lflipper2Sec = new LeftFlipper() { X = baseLeftX + 997 / 2 - 60, Y = Height - baseBottomtY * 2 };
                lflipper2Sec.Scale = flipperscale * 0.6;
                lflipper2Sec.Name = "4";
                lflipper2Sec.BounceFactor = power * 0.5f;
                lflipper2Sec.Trigger = System.Windows.Forms.Keys.J;
                _elements.Add(lflipper2Sec);

                Flipper rflipper2Sec = new RightFlipper() { X = baseRightX + 997 / 2 + 115, Y = Height - baseBottomtY * 2 };
                rflipper2Sec.Scale = flipperscale * 0.6;
                rflipper2Sec.Name = "4";
                rflipper2Sec.BounceFactor = power * 0.5f;
                rflipper2Sec.Trigger = System.Windows.Forms.Keys.L;
                _elements.Add(rflipper2Sec);

                Flipper lflipperTop2 = new LeftFlipper() { X = baseRightX + 997 / 2, Y = -baseTopY };
                lflipperTop2.Scale = flipperscale;
                lflipperTop2.Name = "3";
                lflipperTop2.BounceFactor = power;
                lflipperTop2.BaseRotation = 180;
                lflipperTop2.Trigger = System.Windows.Forms.Keys.O;
                _elements.Add(lflipperTop2);

                Flipper rflipperTop2 = new RightFlipper() { X = baseLeftX + 997 / 2, Y = -baseTopY };
                rflipperTop2.Scale = flipperscale;
                rflipperTop2.Name = "3";
                rflipperTop2.BounceFactor = power;
                rflipperTop2.BaseRotation = 180;
                rflipperTop2.Trigger = System.Windows.Forms.Keys.U;
                _elements.Add(rflipperTop2);

                Flipper lflipperTop2Sec = new LeftFlipper() { X = baseRightX + 997 / 2 + 115, Y = -baseTopY * -2.5 + 70 };
                lflipperTop2Sec.Scale = flipperscale * 0.6;
                lflipperTop2Sec.Name = "3";
                lflipperTop2Sec.BounceFactor = power * 0.5f;
                lflipperTop2Sec.BaseRotation = 180;
                lflipperTop2Sec.Trigger = System.Windows.Forms.Keys.O;
                _elements.Add(lflipperTop2Sec);

                Flipper rflipperTop2Sec = new RightFlipper() { X = baseLeftX + 997 / 2 - 60, Y = -baseTopY * -2.5 + 70 };
                rflipperTop2Sec.Scale = flipperscale * 0.6;
                rflipperTop2Sec.Name = "3";
                rflipperTop2Sec.BounceFactor = power * 0.5f;
                rflipperTop2Sec.BaseRotation = 180;
                rflipperTop2Sec.Trigger = System.Windows.Forms.Keys.U;
                _elements.Add(rflipperTop2Sec);
                #endregion
            }
        }

        public int Width
        {
            get
            {
                var factor = Program.IsFourPlayerMode ? 2 : 1;
                return factor * 997 / 2;
            }
        }

        public int Height
        {
            get { return 1385 / 2; }
        }

        public StartingRamp Ramp
        {
            get { return _ramp; }
        }
        /// <summary>
        /// Initializes the machine with a layout.
        /// !!! Only use once on a machine !!!
        /// </summary>
        /// <param name="machine"></param>
        public void Apply(PinballMachine machine)
        {
            machine.StaticElements.Clear();
            foreach (PinballElement el in _elements) machine.StaticElements.Add(el);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Init();
        }

        public object Clone()
        {
            return new DefaultLayout();
        }
    }
}
