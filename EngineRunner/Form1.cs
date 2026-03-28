using System;
using System.Timers;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Engine.Math;
using Engine.Physics;
using Engine.Physics.Bodies;
using Engine.Physics.World;

namespace EngineRunner
{
    public partial class Form1 : Form
    {
        private const float FixedDeltaTime = 1f / 120f; // 120 physics steps/sec
        private float accumulator = 0f;
 
        private System.Windows.Forms.Timer timer;
        private Stopwatch stopwatch;

        private float clientHeight;
        private float clientWidth;

        private PhysicsWorld world;

        public Form1()
        {
            // initializes the form
            InitializeComponent();
            // reduces flickering when redrawing
            this.DoubleBuffered = true;

            world = new PhysicsWorld();

            //Rigidbody (position, width, height, mass, IsStatic, useGravity)
            RRigidBody body = new RRigidBody(new RVector2(500f, 400f), 20f, 20f, 1000f, false, true);
            body.AddImpulse(new RVector2(-1000f, -500f));         // direction toward (x,y) pixels per second


            //RAABB (left, right, top, bottom)
            RAABB platform = new RAABB(150f, 350f, 250f, 270f);

            world.Bodies.Add(body);
            world.StaticColliders.Add(platform);

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
            float deltaTime = currentTime - lastTime;                   
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
                world.UpdateBounds(clientHeight, clientWidth);
                world.Step(FixedDeltaTime);    // runs one fixed physics step
                accumulator -= FixedDeltaTime;                           //.. then subtract one fixed physics step time from the time 
            }

            Invalidate();
        }

        // draw the world
        protected override void OnPaint(PaintEventArgs e)
        {
           base.OnPaint(e);

           foreach (RRigidBody body in world.Bodies)
            {
                e.Graphics.FillEllipse(
                    Brushes.Red,
                    body.Position.X,
                    body.Position.Y,
                    body.Width,
                    body.Height
                );
            }
            
            foreach (RAABB collider in world.StaticColliders)
            {
                e.Graphics.FillRectangle(
                    Brushes.Blue,
                    collider.Left,
                    collider.Top,
                    collider.Right - collider.Left,
                    collider.Bottom - collider.Top
                );
            }
        }
    }
}
