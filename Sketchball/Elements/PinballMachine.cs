﻿using Sketchball.GameComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace Sketchball.Elements
{

    /// <summary>
    /// Represents a pinball machine that can be customized.
    /// </summary>
    [DataContract(IsReference=true)]
    public class PinballMachine : ICloneable, IDisposable
    {

        // 400px = 1m
        public const float PIXELS_TO_METERS_RATIO = 500f / 1;
        
        /// <summary>
        /// Gets the elements that were added by the user.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public ElementCollection DynamicElements { get; private set; }

        /// <summary>
        /// Gets the elements that are static and can't be changed.
        /// (Usually added through the layout)
        /// </summary>
        [Browsable(false)]
        public ElementCollection StaticElements { get; private set; }

        /// <summary>
        /// Gets the balls currently added.
        /// </summary>
        [Browsable(false)]
        public ElementCollection Balls { get; private set; }

        /// <summary>
        /// Gets _all_ elements added to this pinball machine.
        /// </summary>
        [Browsable(false)]
        public IEnumerable<PinballElement> Elements
        {
            get
            {
                // Locks are needed to prevent changes on the lists while they're being enumerated.
                lock (StaticElements)
                {
                    foreach (PinballElement el in StaticElements)
                        yield return el;
                }
                lock(DynamicElements) {
                    foreach (PinballElement el in DynamicElements)
                        yield return el;
                }
                lock(Balls) {
                    foreach (PinballElement el in Balls)
                        yield return el;
                }
            }
        }

        /// <summary>
        /// Gets the layout used to define this pinball machine.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public IMachineLayout Layout { get; private set; }

        // -------  DERIVED PROPERTIES            
        protected StartingRamp Ramp { get { return Layout.Ramp; } }
        /// <summary>
        /// Gets the width of this machine.
        /// </summary>
        [Browsable(false)]
        public int Width { get { return Layout.Width; } }

        /// <summary>
        /// Gets the height of this machine.
        /// </summary>
        [Browsable(false)]
        public int Height { get { return Layout.Height; } }


        private const float DEFAULT_GRAVITY = 9.81f;
        [DataMember]
        [Browsable(true), Description("The gravity used in this pinball machine.")]
        public float Gravity { get; set; }


        private const double DEFAULT_ANGLE = (3.14 / 180 * 7);

        /// <summary>
        /// Tilt of the pinball machine in radians.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public double Angle { get; set; }

        
        /// <summary>
        /// Don't use outside the editor!
        /// </summary>
        [Browsable(true), DisplayName("Angle"), Description("The tilt angle of the machine in degrees.")]
        public int AngleProperty { 
            get {
                return (int)Math.Round(Angle / Math.PI * 180);
            } 
            set {
                Angle = value / 180.0 * Math.PI;    
            }
        }

        /// <summary>
        /// Gets the calculated acceleration based on the gravity and the angle.
        /// </summary>
        [Browsable(false)]
        public Vector Acceleration
        {
            get
            {
                return new Vector(0, Math.Sin(Angle) * Gravity * PIXELS_TO_METERS_RATIO);
            }
        }

        public Vector GetAcceleration(Ball ball)
        {
            var g = Gravity;
            if (ball.X < Width - 100 && ball.Y < Height / 2)
                g = -g;

            return new Vector(0, Math.Sin(Angle) * g * PIXELS_TO_METERS_RATIO);
        }

        /// <summary>
        /// Gets the highscore list.
        /// </summary>
        //[DataMember]
        //[Browsable(false)]
        //public HighscoreList Highscores { get; private set; }
       

        /// <summary>
        /// Creates a new pinball machine with the default layout.
        /// </summary>
        public PinballMachine() : this(new DefaultLayout()) {}


        /// <summary>
        /// Creates a new pinball machine with a given layout.
        /// </summary>
        /// <param name="layout"></param>
        public PinballMachine(IMachineLayout layout)
        {
            Layout = layout;
            Gravity = DEFAULT_GRAVITY;
            Angle = DEFAULT_ANGLE;
            //Highscores = new HighscoreList();

            Init();
        }

        private void Init()
        {
            StaticElements = new ElementCollection(this);
            Balls = new ElementCollection(this);

            if (DynamicElements == null) DynamicElements = new ElementCollection(this);

            Layout.Apply(this);
        }

        public virtual void Draw(DrawingContext g)
        {
            g.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));

            var b = new SolidColorBrush { Color = Color.FromArgb(80, 0, 0, 0) };

            g.DrawRectangle(b, null, new Rect(0, 0, Width, Height));

            // Draw contours
            var b2 = new SolidColorBrush { Color = Color.FromArgb(80, 255, 255, 255) };
            Pen pen = new Pen(b2, 1);
            //for (int y = 0; y <= Height; y += 10)
            //{
            //    g.DrawLine(pen, new Point(0, y), new Point(Width, y));
            //}

            //for (int x = 0; x <= Width; x += 10)
            //{
            //    g.DrawLine(pen, new Point(x, 0), new  Point(x, Height));
            //}

            foreach (PinballElement element in Elements)
            {
                g.PushTransform(new TranslateTransform(element.X, element.Y));
                element.Draw(g);
                g.Pop();
            }

            g.Pop();
        }


        public void Add(PinballElement element)
        {
            DynamicElements.Add(element);
        }

        public bool Remove(PinballElement element)
        {
            return DynamicElements.Remove(element);
        }


        public object Clone()
        {
            PinballMachine machine = new PinballMachine(Layout);

            machine.Angle = Angle;
            machine.Gravity = Gravity;

            // Clone elements
            // (clone static ones just to be sure their settings are OK)
            machine.StaticElements = new ElementCollection(machine);
            foreach (PinballElement element in StaticElements)
            {
                machine.StaticElements.Add((PinballElement)element.Clone());
            }

            machine.DynamicElements = new ElementCollection(machine);
            foreach (PinballElement element in DynamicElements)
            {
                machine.DynamicElements.Add((PinballElement)element.Clone());
            }

            machine.Layout = Layout.Clone() as IMachineLayout;
            //machine.Highscores = new HighscoreList();
            //foreach (var entry in Highscores)
            //{
            //    machine.Highscores.Add(new HighscoreEntry(entry.Player, entry.Score, entry.Date));
            //}
            return machine;
        }


        /// <summary>
        /// Disposes the pinball machine and frees all resources used by it.
        /// </summary>
        public void Dispose()
        {
            StaticElements.Clear();
            DynamicElements.Clear();
            Balls.Clear();
        }


#region Serialization
        /// <summary>
        /// Saves the pinball machine to a file.
        /// </summary>
        /// <param name="path">Path of the file.</param>
        public void Save(string path)
        {
            using (var stream = File.Open(path, FileMode.Create))
            {
                // Truncate
                stream.SetLength(0);
                Save(stream);
            }
        }

        /// <summary>
        /// Saves the machine to an output stream.
        /// </summary>
        /// <param name="output"></param>
        public void Save(Stream output)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            serializer.WriteObject(output, this);
        }


        /// <summary>
        /// Loads a pinball machine from a file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static PinballMachine FromFile(string path)
        {
            PinballMachine pbm;
            using (var stream = File.OpenRead(path))
            {
                pbm = FromStream(stream);
            }
            return pbm;
        }

        /// <summary>
        /// Loads a pinball machine from an input stream.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static PinballMachine FromStream(Stream input)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            return (PinballMachine)serializer.ReadObject(input);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Init();
        }

    
        /// <summary>
        /// Gets whether or not this is a valid pinball machine. You can query the problem found with LastProblem.
        /// </summary>
        /// <returns></returns>
        public bool IsValid() {
            foreach (var p in Elements)
            {
                // Make sure bounding containers are up-to-date.
                p.RegenerateBounds();
            }

            // Validity check. 
            foreach (var p1 in DynamicElements)
            {
                foreach (var p2 in Elements)
                {
                    if (p2 != p1)
                    {
                        if (p2.BoundingContainer.Intersects(p1.BoundingContainer))
                        {
                            LastProblem = new ValidationProblem("Two elements are overlapping.", new PinballElement[] { p1, p2 });
                            return false;
                        }
                    }
                }
            }

            // Check for wormhole validity (will probably be changed in the future to work with IDs)
            foreach(var entry in this.Elements.OfType<WormholeEntry>())
            {
                if (entry.WormholeExit == null)
                {
                    LastProblem = new ValidationProblem("There is a worm hole pointing into nirvana. Add an exit.", entry);
                    return false;
                }
            }

            // We're fine.
            LastProblem = null;
            return true;
        }

        /// <summary>
        /// Last problem that was detected. ALWAYS use in conjunction with IsValid().
        /// </summary>
        [Browsable(false)]
        public ValidationProblem LastProblem { get; private set; }
    

#endregion

        /// <summary>
        /// Brings an element to front for drawing.
        /// </summary>
        /// <param name="element"></param>
        public void BringToFront(PinballElement element)
        {
            DynamicElements.MoveToTail(element);   
        }
    }

}
