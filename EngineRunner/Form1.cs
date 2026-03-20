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
        private const float FixedDeltaTime = 1f / 120f; // 120 physics steps/sec
        private float accumulator = 0f;                 

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
            body = new RRigidBody(new RVector2(400, 400), 20f, 20f, 1f, true);
            body.Velocity = new RVector2(-75000f, -7500f);          // direction toward (x,y) pixels per second
            
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
            float currentTime = stopwatch.ElapsedMilliseconds / 1000f;  //sec
            float deltaTime = currentTime - lastTime;                   //
            lastTime = currentTime;

            // clamp huge spikes (if a frame took too long act like it didn't)
            if (deltaTime > 0.05f)
            {
                deltaTime = 0.05f;
            }

            accumulator += deltaTime;


            clientHeight = this.ClientSize.Height;
            clientWidth = this.ClientSize.Width;

            while (accumulator >= FixedDeltaTime)                          
            {
                body.Update(FixedDeltaTime, clientHeight, clientWidth);    // runs one fixed physics step

                if (body.Bounds.Intersects(platform))
                {
                    ResolveCollision(body, platform);
                }

                accumulator -= FixedDeltaTime;      //.. then subtract one fixed physics step time from the time accuum
            }

            Invalidate();
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

        private void ResolveCollision(RRigidBody body, RAABB platform)
        {
            RAABB b = body.Bounds;

            float overlapLeft = b.Right - platform.Left;
            float overlapRight = platform.Right - b.Left;
            float overlapTop = b.Bottom - platform.Top;
            float overlapBottom = platform.Bottom - b.Top;

            float minOverlapX = System.Math.Min(overlapLeft, overlapRight);
            float minOverlapY = System.Math.Min(overlapTop, overlapBottom);

            if (minOverlapX < minOverlapY)
            {
                // resolve horizontally
                if (overlapLeft < overlapRight)
                {
                    body.Position.X -= overlapLeft;
                }
                else
                {
                    body.Position.X += overlapRight;
                }

                body.Velocity.X *= -0.5f;
            }
            else
            {
                // resolve vertically
                if (overlapTop < overlapBottom)
                {
                    body.Position.Y -= overlapTop;
                }
                else
                {
                    body.Position.Y += overlapBottom;
                }

                body.Velocity.Y *= -0.5f;
            }
        }
    }
}
