using System;
using System.Timers;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Engine.Math;
using Engine.Physics;

namespace EngineRunner
{
    public partial class Form1 : Form
    {
        private RRigidBody body;

        private System.Windows.Forms.Timer timer;
        private Stopwatch stopwatch;

        private float clientHeight;
        private float clientWidth;

        private RAABB platform;

        public Form1()
        {
            // initializes the form
            InitializeComponent();

            // reduces flickering when redrawing
            this.DoubleBuffered = true;

            //Rigidbody (position, width, height, mass, useGravity)
            body = new RRigidBody(new RVector2(200, 200), 20f, 20f, 1f, true);
            body.Velocity = new RVector2(20000, 200000);          // direction toward (x,y) pixels per second
            
            //RAABB (left, right, top, bottom)
            platform = new RAABB(150f, 350f, 250f, 270f);    

            stopwatch = new Stopwatch();
            stopwatch.Start();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16; // ~60 FPS
            timer.Tick += GameLoop;
            timer.Start();
        }

        private float lastTime = 0f;

        private void GameLoop(object sender, EventArgs e)
        {
            float currentTime = stopwatch.ElapsedMilliseconds / 1000f;
            float deltaTime = currentTime - lastTime;
            lastTime = currentTime;

            clientHeight = this.ClientSize.Height;
            clientWidth = this.ClientSize.Width;

            body.Update(deltaTime, clientHeight, clientWidth);

            // check if ball collides with platform
            if (body.Bounds.Intersects(platform) && body.Velocity.Y > 0)
            {
                body.Position.Y = platform.Top - body.Height;
                body.Velocity.Y *= -0.5f;
                //CURRENTLY JUST EXPECTS THE BALL TO FALL FROM THE TOP
                //TODO: MAKE THE ENGINE REMEMBER WHERE THE OBJECT CAME FROM 
                //BEFORE THE COLLISION HAPPENED
            }

            Invalidate(); // triggers redraw
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

           

            e.Graphics.FillEllipse(
                Brushes.Red,
                body.Position.X,
                body.Position.Y,
                body.Width,
                body.Height
            );

            e.Graphics.FillRectangle(
                Brushes.Blue,
                platform.Left,
                platform.Top,
                platform.Right - platform.Left,
                platform.Bottom - platform.Top
            );


        }
    }
}
