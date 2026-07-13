using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
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

        // toggle with D key during runtime to hide debug info for clean presentation
        private bool showDebug = true;

        // tracked separately from stopwatch so the FPS counter works frame-to-frame
        private float lastTime = 0f;
        private float lastDeltaTime = 0.016f;

        public Form1()
        {
            // initializes the form
            InitializeComponent();
            this.Text = "Physics Engine";
            // reduces flickering when redrawing
            this.DoubleBuffered = true;

            world = new PhysicsWorld();

            // test scene: bodies with varied shapes and gentle velocities landing on two platforms.
            // circleA drops straight down onto the main platform.
            // circleB drifts left and collides with circleA on the platform (circle-circle contact).
            // rectA drifts left and falls to a lower ledge (rect-static contact).
            // circleC arcs upward, falls back down onto rectA (circle-rect dynamic contact).

            RRigidBody circleA = new RRigidBody(
                new RVector2(320f, 50f),
                new RCircleShape(25f),
                10f,
                false,
                true);
            // no impulse, drops straight down

            RRigidBody circleB = new RRigidBody(
                new RVector2(530f, 80f),
                new RCircleShape(20f),
                8f,
                false,
                true);
            circleB.AddImpulse(new RVector2(-180f * circleB.Mass, 0f));  // drifts left at ~180 px/s

            RRigidBody rectA = new RRigidBody(
                new RVector2(560f, 60f),
                new RRectangleShape(60f, 40f),
                15f,
                false,
                true);
            rectA.AddImpulse(new RVector2(-80f * rectA.Mass, 0f));  // drifts left at ~80 px/s toward the ledge

            RRigidBody circleC = new RRigidBody(
                new RVector2(180f, 250f),
                new RCircleShape(18f),
                6f,
                false,
                true);
            // small arc: rightward and slightly up so it lands on rectA as it settles on the ledge
            circleC.AddImpulse(new RVector2(120f * circleC.Mass, -180f * circleC.Mass));

            // main platform: wide, center of window
            RAABB mainPlatform = new RAABB(80f, 680f, 320f, 340f);

            // ledge: narrower, lower and to the right
            RAABB ledge = new RAABB(460f, 700f, 410f, 430f);

            world.Bodies.Add(circleA);
            world.Bodies.Add(circleB);
            world.Bodies.Add(rectA);
            world.Bodies.Add(circleC);

            world.StaticColliders.Add(mainPlatform);
            world.StaticColliders.Add(ledge);

            stopwatch = new Stopwatch();
            stopwatch.Start();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16; // ~60 FPS
            // run gameloop every timer tick
            timer.Tick += GameLoop;
            timer.Start();
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            float currentTime = stopwatch.ElapsedMilliseconds / 1000f;  // seconds
            float deltaTime = currentTime - lastTime;
            lastTime = currentTime;

            // clamp huge spikes (if a frame took too long act like it didn't)
            if (deltaTime > 0.05f)
            {
                deltaTime = 0.05f;
            }

            lastDeltaTime = deltaTime;
            accumulator += deltaTime;

            clientHeight = this.ClientSize.Height;
            clientWidth = this.ClientSize.Width;

            while (accumulator >= FixedDeltaTime)
            {
                world.UpdateBounds(clientHeight, clientWidth);
                world.Step(FixedDeltaTime);    // runs one fixed physics step
                accumulator -= FixedDeltaTime;
            }

            // repaint
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            // press D to toggle debug overlay on/off
            if (e.KeyCode == Keys.D)
            {
                showDebug = !showDebug;
            }
        }

        // draw the world
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            // draw static colliders
            foreach (RAABB collider in world.StaticColliders)
            {
                g.FillRectangle(
                    Brushes.SteelBlue,
                    collider.Left,
                    collider.Top,
                    collider.Right - collider.Left,
                    collider.Bottom - collider.Top
                );
            }

            // draw each body
            foreach (RRigidBody body in world.Bodies)
            {
                RVector2 center;

                if (body.Shape is RCircleShape circle)
                {
                    Brush fill = body.IsGrounded ? Brushes.MediumPurple : Brushes.BlueViolet;
                    g.FillEllipse(fill, body.Position.X, body.Position.Y, circle.Radius * 2f, circle.Radius * 2f);
                    center = new RVector2(body.Position.X + circle.Radius, body.Position.Y + circle.Radius);

                    if (showDebug)
                    {
                        // AABB outline
                        RAABB bounds = body.Bounds;
                        g.DrawRectangle(Pens.Gray,
                            bounds.Left, bounds.Top,
                            bounds.Right - bounds.Left,
                            bounds.Bottom - bounds.Top);

                        // grounded ring
                        if (body.IsGrounded)
                        {
                            g.DrawEllipse(Pens.LimeGreen,
                                body.Position.X - 2f, body.Position.Y - 2f,
                                circle.Radius * 2f + 4f, circle.Radius * 2f + 4f);
                        }
                    }
                }
                else if (body.Shape is RRectangleShape rect)
                {
                    Brush fill = body.IsGrounded ? Brushes.MediumPurple : Brushes.BlueViolet;
                    g.FillRectangle(fill, body.Position.X, body.Position.Y, rect.Width, rect.Height);
                    center = new RVector2(body.Position.X + rect.Width / 2f, body.Position.Y + rect.Height / 2f);

                    if (showDebug)
                    {
                        // AABB outline (same as shape for rectangles, but shows the bounds are correct)
                        RAABB bounds = body.Bounds;
                        g.DrawRectangle(Pens.Gray,
                            bounds.Left, bounds.Top,
                            bounds.Right - bounds.Left,
                            bounds.Bottom - bounds.Top);

                        // grounded outline
                        if (body.IsGrounded)
                        {
                            g.DrawRectangle(Pens.LimeGreen,
                                body.Position.X - 2f, body.Position.Y - 2f,
                                rect.Width + 4f, rect.Height + 4f);
                        }
                    }
                }
                else
                {
                    continue;
                }

                if (showDebug)
                {
                    // velocity vector from center, scaled down to be readable
                    const float velocityScale = 0.08f;
                    const float maxLength = 80f;

                    RVector2 vel = body.Velocity * velocityScale;
                    float velLen = vel.Length;
                    if (velLen > maxLength)
                    {
                        vel = vel / velLen * maxLength;
                    }

                    if (velLen > 0.5f)
                    {
                        g.DrawLine(Pens.LimeGreen,
                            center.X, center.Y,
                            center.X + vel.X, center.Y + vel.Y);

                        // small arrowhead dot at the tip
                        g.FillEllipse(Brushes.LimeGreen,
                            center.X + vel.X - 2f, center.Y + vel.Y - 2f, 4f, 4f);
                    }
                }
            }

            if (showDebug)
            {
                // FPS counter - bottom right so it stays readable on a light background
                float fps = lastDeltaTime > 0f ? 1f / lastDeltaTime : 0f;
                string hudText = $"FPS: {fps:F0}  |  Bodies: {world.Bodies.Count}  |  [D] toggle debug";
                SizeF textSize = g.MeasureString(hudText, SystemFonts.DefaultFont);
                float hudX = this.ClientSize.Width - textSize.Width - 6f;
                float hudY = this.ClientSize.Height - textSize.Height - 6f;
                g.DrawString(hudText, SystemFonts.DefaultFont, Brushes.Black, hudX, hudY);
            }
        }
    }
}
