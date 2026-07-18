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
        private const int StressBodyCount = 80;
        private float accumulator = 0f;

        private System.Windows.Forms.Timer timer;
        private Stopwatch stopwatch;

        private float clientHeight;
        private float clientWidth;

        private RPhysicsWorld world;
        private RRigidBody? player;
        private RPlayerController? playerController;

        // toggle with F1 during runtime to hide debug info for clean presentation
        private bool showDebug = true;
        // F2 switches between the playable platformer and a crowded evaluation scene
        private bool stressScene = false;

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
            LoadPlatformerScene();

            stopwatch = new Stopwatch();
            stopwatch.Start();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16; // ~60 FPS
            // run gameloop every timer tick
            timer.Tick += GameLoop;
            timer.Start();
        }

        private void LoadPlatformerScene()
        {
            world.Bodies.Clear();
            world.StaticColliders.Clear();
            stressScene = false;

            // playable level: solid ground + platforms, plus one-way platforms you can jump up through
            world.StaticColliders.Add(new RStaticCollider(new RAABB(0f, 800f, 400f, 450f)));           // ground
            world.StaticColliders.Add(new RStaticCollider(new RAABB(80f, 280f, 320f, 340f)));          // solid low
            world.StaticColliders.Add(new RStaticCollider(new RAABB(320f, 520f, 240f, 260f)));         // solid mid
            world.StaticColliders.Add(new RStaticCollider(new RAABB(560f, 760f, 160f, 180f)));         // solid high

            // one-way: jump up through from below, then stand on top
            world.StaticColliders.Add(new RStaticCollider(new RAABB(200f, 360f, 280f, 292f), true));
            world.StaticColliders.Add(new RStaticCollider(new RAABB(440f, 620f, 200f, 212f), true));

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
        }

        // crowded scene for measuring broad-phase candidate counts vs brute force
        private void LoadStressScene()
        {
            world.Bodies.Clear();
            world.StaticColliders.Clear();
            stressScene = true;
            player = null;
            playerController = null;

            world.StaticColliders.Add(new RStaticCollider(new RAABB(0f, 800f, 420f, 480f)));

            Random rng = new Random(42);
            for (int i = 0; i < StressBodyCount; i++)
            {
                float x = 40f + (float)rng.NextDouble() * 700f;
                float y = 20f + (float)rng.NextDouble() * 300f;
                bool useCircle = i % 2 == 0;

                RShape shape = useCircle
                    ? new RCircleShape(10f + (float)rng.NextDouble() * 10f)
                    : new RRectangleShape(16f + (float)rng.NextDouble() * 14f, 16f + (float)rng.NextDouble() * 14f);

                RRigidBody body = new RRigidBody(
                    new RVector2(x, y),
                    shape,
                    3f + (float)rng.NextDouble() * 5f,
                    false,
                    true);
                body.Restitution = 0.3f;
                world.Bodies.Add(body);
            }
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

                if (playerController != null)
                {
                    bool moveLeft = heldKeys.Contains(Keys.A) || heldKeys.Contains(Keys.Left);
                    bool moveRight = heldKeys.Contains(Keys.D) || heldKeys.Contains(Keys.Right);
                    bool jumpHeld = heldKeys.Contains(Keys.Space) || heldKeys.Contains(Keys.W) || heldKeys.Contains(Keys.Up);
                    playerController.ApplyInput(moveLeft, moveRight, jumpPressed, jumpHeld, FixedDeltaTime);
                }
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

            // F2 swaps platformer demo <-> evaluation stress scene
            if (e.KeyCode == Keys.F2)
            {
                if (stressScene)
                {
                    LoadPlatformerScene();
                }
                else
                {
                    LoadStressScene();
                }
                return;
            }

            // F3 toggles spatial-hash broad-phase for A/B comparison
            if (e.KeyCode == Keys.F3)
            {
                world.UseBroadPhase = !world.UseBroadPhase;
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

            // draw static colliders - one-way platforms use a lighter fill so they're obvious in demos
            foreach (RStaticCollider collider in world.StaticColliders)
            {
                RAABB bounds = collider.Bounds;
                Brush fill = collider.IsOneWay ? Brushes.SkyBlue : Brushes.SteelBlue;
                g.FillRectangle(
                    fill,
                    bounds.Left,
                    bounds.Top,
                    bounds.Right - bounds.Left,
                    bounds.Bottom - bounds.Top
                );

                if (showDebug && collider.IsOneWay)
                {
                    // dashed-style top edge to mark the solid landing surface
                    using (Pen dashPen = new Pen(Color.DodgerBlue, 2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    {
                        g.DrawLine(dashPen, bounds.Left, bounds.Top, bounds.Right, bounds.Top);
                    }
                }
            }

            // draw each body
            foreach (RRigidBody body in world.Bodies)
            {
                RVector2 center;
                bool isPlayer = player != null && body == player;

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

                if (showDebug && !stressScene)
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
                // FPS + evaluation metrics - bottom right so it stays readable on a light background
                float fps = lastDeltaTime > 0f ? 1f / lastDeltaTime : 0f;
                int sleepingCount = 0;
                foreach (var body in world.Bodies)
                {
                    if (body.IsSleeping) sleepingCount++;
                }

                string broadPhaseLabel = world.UseBroadPhase ? "hash" : "brute";
                string sceneLabel = stressScene ? "stress" : "platformer";
                string hudText =
                    $"FPS: {fps:F0}  |  Bodies: {world.Bodies.Count}  |  Sleeping: {sleepingCount}  |  Pairs: {world.LastCandidatePairCount}  |  BP: {broadPhaseLabel}  |  {sceneLabel}  |  [F2] scene  [F3] broad-phase  [F1] debug";
                SizeF textSize = g.MeasureString(hudText, SystemFonts.DefaultFont);
                float hudX = System.Math.Max(6f, this.ClientSize.Width - textSize.Width - 6f);
                float hudY = this.ClientSize.Height - textSize.Height - 6f;
                g.DrawString(hudText, SystemFonts.DefaultFont, Brushes.Black, hudX, hudY);
            }
        }
    }
}
