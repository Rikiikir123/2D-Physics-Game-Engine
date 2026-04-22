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

            //Rigidbody (position, shape, mass, IsStatic, useGravity)
            RRigidBody body = new RRigidBody(
                new RVector2(500f, 400f), 
                new RCircleShape(25f), 
                100f, 
                false, 
                true);
            body.AddImpulse(new RVector2(-10010f, -50000f));         // direction toward (x,y) pixels per second

            RRigidBody body2 = new RRigidBody(
                new RVector2(100f, 100f),
                new RCircleShape(40f),
                10f,
                false,
                true);
            body2.AddImpulse(new RVector2(-10000f, -50100f));

            //rect
            RRigidBody body3 = new RRigidBody(
                new RVector2(120f, 120f),
                new RRectangleShape(50f, 30f),
                10f,
                false,
                true);
            body3.AddImpulse(new RVector2(-10000f, -5000f));

            //RAABB (left, right, top, bottom)
            RAABB platform = new RAABB(150f, 350f, 250f, 270f);

            world.Bodies.Add(body);
            world.Bodies.Add(body2);
            world.Bodies.Add(body3);

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
