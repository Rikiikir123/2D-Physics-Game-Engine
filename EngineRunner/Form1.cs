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

        private float floorY;
        private float clientWidth;

        public Form1()
        {
            // initializes the form
            InitializeComponent();

            // reduces flickering when redrawing
            this.DoubleBuffered = true;

            //Rigidbody (position, width, height, mass, useGravity)
            body = new RRigidBody(new RVector2(200, 200), 20f, 20f, 1f, true);
            body.Velocity = new RVector2(0, -1000); // (x,y) pixels per second


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

            floorY = this.ClientSize.Height;
            clientWidth = this.ClientSize.Width;

            body.Update(deltaTime, floorY, clientWidth);

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

            
        }
    }
}
