using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using Engine.Math;
using Engine.Physics;
using Engine.Physics.Bodies;
using Engine.Physics.Controllers;
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

        private RPhysicsWorld world;
        private RRigidBody player;
        private RPlayerController playerController;

        // toggle with F1 during runtime to hide debug info for clean presentation
        private bool showDebug = true;

        // held movement keys are tracked continuously, jump is consumed once per press
        private readonly HashSet<Keys> heldKeys = new();
        private bool jumpPressed = false;

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
            // form must see key events before any child control does
            this.KeyPreview = true;

            world = new RPhysicsWorld();

            // playable level: a ground floor plus a few platforms at increasing height so
            // reaching the top row requires jumping between them.
            RAABB ground = new RAABB(0f, 800f, 400f, 450f);
            RAABB platformLow = new RAABB(80f, 280f, 320f, 340f);
            RAABB platformMid = new RAABB(320f, 520f, 240f, 260f);
            RAABB platformHigh = new RAABB(560f, 760f, 160f, 180f);

            world.StaticColliders.Add(ground);
            world.StaticColliders.Add(platformLow);
            world.StaticColliders.Add(platformMid);
            world.StaticColliders.Add(platformHigh);

            // the player: a rectangle body that never sleeps, so it always responds to input
            player = new RRigidBody(
                new RVector2(50f, 340f),
                new RRectangleShape(30f, 50f),
                10f,
                false,
                true);
            player.CanSleep = false;
            player.Restitution = 0f; // player shouldn't bounce off the ground/platforms like a regular object
            playerController = new RPlayerController(player);

            world.Bodies.Add(player);

            // a small pushable prop to demonstrate dynamic collision while playing
            RRigidBody prop = new RRigidBody(
                new RVector2(400f, 370f),
                new RCircleShape(18f),
                4f,
                false,
                true);
            world.Bodies.Add(prop);

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

                bool moveLeft = heldKeys.Contains(Keys.A) || heldKeys.Contains(Keys.Left);
                bool moveRight = heldKeys.Contains(Keys.D) || heldKeys.Contains(Keys.Right);
                playerController.ApplyInput(moveLeft, moveRight, jumpPressed);
                jumpPressed = false;

                world.Step(FixedDeltaTime);    // runs one fixed physics step
                accumulator -= FixedDeltaTime;
            }

            // repaint
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // F1 toggles debug info for clean presentation, separate from movement keys
            if (e.KeyCode == Keys.F1)
            {
                showDebug = !showDebug;
                return;
            }

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                jumpPressed = true;
            }

            heldKeys.Add(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            heldKeys.Remove(e.KeyCode);
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
                bool isPlayer = body == player;

                if (body.Shape is RCircleShape circle)
                {
                    Brush fill = body.IsSleeping ? Brushes.Gray : (body.IsGrounded ? Brushes.MediumPurple : Brushes.BlueViolet);
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

                        // sleeping outline
                        if (body.IsSleeping)
                        {
                            g.DrawEllipse(Pens.DarkGray,
                                body.Position.X - 2f, body.Position.Y - 2f,
                                circle.Radius * 2f + 4f, circle.Radius * 2f + 4f);
                        }
                    }
                }
                else if (body.Shape is RRectangleShape rect)
                {
                    Brush fill = isPlayer
                        ? Brushes.OrangeRed
                        : (body.IsSleeping ? Brushes.Gray : (body.IsGrounded ? Brushes.MediumPurple : Brushes.BlueViolet));
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

                        // sleeping outline
                        if (body.IsSleeping)
                        {
                            g.DrawRectangle(Pens.DarkGray,
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
                int sleepingCount = 0;
                foreach (var body in world.Bodies)
                {
                    if (body.IsSleeping) sleepingCount++;
                }

                string hudText = $"FPS: {fps:F0}  |  Bodies: {world.Bodies.Count}  |  Sleeping: {sleepingCount}  |  A/D move  Space jump  [F1] toggle debug";
                SizeF textSize = g.MeasureString(hudText, SystemFonts.DefaultFont);
                float hudX = this.ClientSize.Width - textSize.Width - 6f;
                float hudY = this.ClientSize.Height - textSize.Height - 6f;
                g.DrawString(hudText, SystemFonts.DefaultFont, Brushes.Black, hudX, hudY);
            }
        }
    }
}
