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

        public Form1()
        {
            // initializes the form
            InitializeComponent();

            // reduces flickering when redrawing
            this.DoubleBuffered = true;

            body = new RRigidBody(new RVector2(100, 100),true);
            body.Velocity = new RVector2(500, 0); // 500 pixels per second

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

            body.Update(deltaTime);

            Invalidate(); // triggers redraw
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            

            e.Graphics.FillEllipse(
                Brushes.Red,
                body.Position.X,
                body.Position.Y,
                20,
                20
            );
        }
    }
}
