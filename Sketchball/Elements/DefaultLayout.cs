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
            var baseRightX = 242;
            var baseTopY = 19;
            var baseBottomtY = 105;
            var power = 2;
            //Add flippers
            #region left
            Flipper lflipper = new LeftFlipper() { X = baseLeftX, Y = Height - baseBottomtY };
            lflipper.Name = "1";
            lflipper.BounceFactor = power;
            lflipper.Scale = 1.7;
            _elements.Add(lflipper);

            Flipper rflipper = new RightFlipper() { X = baseRightX, Y = Height - baseBottomtY };
            rflipper.Name ="1";
            rflipper.Scale = 1.7;
            rflipper.BounceFactor = power;
            _elements.Add(rflipper);

            Flipper lflipperTop = new LeftFlipper() { X = baseRightX, Y = -baseTopY };
            lflipperTop.Name = "2";
            lflipperTop.Scale = 1.7;
            lflipperTop.BounceFactor = power;
            lflipperTop.BaseRotation = 180;
            lflipperTop.Trigger = System.Windows.Forms.Keys.E;
            _elements.Add(lflipperTop);

            Flipper rflipperTop = new RightFlipper() { X = baseLeftX, Y = -baseTopY };
            rflipperTop.Scale = 1.7;
            rflipperTop.Name = "2";
            rflipperTop.BounceFactor = power;
            rflipperTop.BaseRotation = 180;
            rflipperTop.Trigger = System.Windows.Forms.Keys.Q;
            _elements.Add(rflipperTop);
            #endregion

            if (Program.IsFourPlayerMode)
            {
                #region right
                Flipper lflipper2 = new LeftFlipper() { X = baseLeftX + 997 / 2, Y = Height - baseBottomtY };
                lflipper2.Scale = 1.7;
                lflipper2.Name = "4";
                lflipper2.BounceFactor = power;
                lflipper2.Trigger = System.Windows.Forms.Keys.J;
                _elements.Add(lflipper2);

                Flipper rflipper2 = new RightFlipper() { X = baseRightX + 997 / 2, Y = Height - baseBottomtY };
                rflipper2.Scale = 1.7;
                rflipper2.Name = "4";
                rflipper2.BounceFactor = power;
                rflipper2.Trigger = System.Windows.Forms.Keys.L;
                _elements.Add(rflipper2);

                Flipper lflipperTop2 = new LeftFlipper() { X = baseRightX + 997 / 2, Y = -baseTopY };
                lflipperTop2.Scale = 1.7;
                lflipperTop2.Name = "3";
                lflipperTop2.BounceFactor = power;
                lflipperTop2.BaseRotation = 180;
                lflipperTop2.Trigger = System.Windows.Forms.Keys.O;
                _elements.Add(lflipperTop2);

                Flipper rflipperTop2 = new RightFlipper() { X = baseLeftX + 997 / 2, Y = -baseTopY };
                rflipperTop2.Scale = 1.7;
                rflipperTop2.Name = "3";
                rflipperTop2.BounceFactor = power;
                rflipperTop2.BaseRotation = 180;
                rflipperTop2.Trigger = System.Windows.Forms.Keys.U;
                _elements.Add(rflipperTop2);
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
