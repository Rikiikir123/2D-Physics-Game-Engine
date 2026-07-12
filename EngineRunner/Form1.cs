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
using Engine.Physics.Shapes;
using static Engine.Physics.Shapes.RShape;

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

            // test scene: a few bodies falling under gravity with modest sideways velocity,
            // landing on a shared platform - meant to be watched at a normal, readable speed
            // instead of stress-testing with huge impulses that tunnel through colliders

            //Rigidbody (position, shape, mass, IsStatic, useGravity)
            RRigidBody circleA = new RRigidBody(
                new RVector2(300f, 50f),
                new RCircleShape(25f),
                10f,
                false,
                true);
            // no impulse, just drops straight down onto the platform

            RRigidBody circleB = new RRigidBody(
                new RVector2(500f, 60f),
                new RCircleShape(20f),
                8f,
                false,
                true);
            circleB.AddImpulse(new RVector2(-150f * circleB.Mass, 0f));  // drifts left at ~150 px/s

            //rect
            RRigidBody rectA = new RRigidBody(
                new RVector2(620f, 40f),
                new RRectangleShape(60f, 40f),
                15f,
                false,
                true);
            rectA.AddImpulse(new RVector2(-100f * rectA.Mass, 0f));  // drifts left at ~100 px/s

            //RAABB (left, right, top, bottom)
            RAABB platform = new RAABB(100f, 700f, 350f, 370f);

            world.Bodies.Add(circleA);
            world.Bodies.Add(circleB);
            world.Bodies.Add(rectA);

            world.StaticColliders.Add(platform);

            stopwatch = new Stopwatch();
            stopwatch.Start();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16; // ~60 FPS
            // run gameloop every timer tick
            timer.Tick += GameLoop;
            timer.Start();
        }









        private float lastTime = 0f;

        private void GameLoop(object? sender, EventArgs e)
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

            // repaint
            Invalidate();
        }

        // draw the world
        protected override void OnPaint(PaintEventArgs e)
        {
           base.OnPaint(e);

           foreach (RRigidBody body in world.Bodies)
            {
                if (body.Shape is RCircleShape circle)
                {
                    e.Graphics.FillEllipse(
                    Brushes.BlueViolet,
                    body.Position.X,
                    body.Position.Y,
                    circle.Radius * 2f,
                    circle.Radius * 2f
                    );
                }
                if (body.Shape is RRectangleShape rectangle)
                {
                    e.Graphics.FillRectangle(
                    Brushes.BlueViolet,
                    body.Position.X,
                    body.Position.Y,
                    rectangle.Width,
                    rectangle.Height
                    );
                }
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
