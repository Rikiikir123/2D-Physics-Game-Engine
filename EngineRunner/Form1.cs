using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using Engine.Math;
using Engine.Physics;
using Engine.Physics.Bodies;
using Engine.Physics.Controllers;
using Engine.Physics.Collision;
using Engine.Physics.World;
using Engine.Physics.Shapes;
using static Engine.Physics.Shapes.RShape;

namespace EngineRunner
{
    public partial class Form1 : Form
    {
        private const float FixedDeltaTime = 1f / 120f; // 120 physics steps/sec
        private const int StressBodyCount = 100;
        private float accumulator = 0f;

        private System.Windows.Forms.Timer timer;
        private Stopwatch stopwatch;

        private float clientHeight;
        private float clientWidth;

        private RPhysicsWorld world;
        private RRigidBody? player;
        private RPlayerController? playerController;
        private RVector2 playerSpawn = new RVector2(50f, 340f);

        private int score = 0;

        // toggle with F1 during runtime to hide debug info for clean presentation
        private bool showDebug = true;
        // F2 switches between the playable platformer and a crowded evaluation scene
        private bool stressScene = false;
        // P pauses physics; while paused, . or N advances exactly one fixed step
        private bool physicsPaused = false;
        private bool stepOnce = false;

        // held movement keys are tracked continuously, jump is consumed once per press
        private readonly HashSet<Keys> heldKeys = new();
        private bool jumpPressed = false;

        // tracked separately from stopwatch so the FPS counter works frame-to-frame
        private float lastTime = 0f;
        private float lastDeltaTime = 0.016f;

        // raw per-frame fps swings too wildly to read, so the HUD shows an average
        // refreshed a couple times a second instead of every single frame
        private float fpsAccumTime = 0f;
        private int fpsAccumFrames = 0;
        private float displayedFps = 0f;

        // FPS mixes physics cost with GDI+ rendering cost, which at high body counts drowns out
        // the broad-phase vs brute-force difference we actually care about for evaluation - so we
        // separately time just world.Step() and report an average per-step cost, unaffected by paint.
        private readonly Stopwatch physicsStopwatch = new Stopwatch();
        private float physicsMsWindowTime = 0f;
        private double physicsMsAccum = 0.0;
        private int physicsStepsAccum = 0;
        private float displayedPhysicsStepMs = 0f;

        // semi-transparent brushes for trigger volumes
        private readonly Brush coinBrush = new SolidBrush(Color.FromArgb(160, 255, 215, 0));
        private readonly Brush hazardBrush = new SolidBrush(Color.FromArgb(140, 220, 60, 60));

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
            world.OnStaticContact += OnStaticContact;
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
            world.ClearTriggerContactHistory();
            stressScene = false;
            score = 0;

            // playable level: solid ground + platforms, plus one-way platforms you can jump up through
            world.StaticColliders.Add(new RStaticCollider(new RAABB(0f, 800f, 400f, 450f)));           // ground
            world.StaticColliders.Add(new RStaticCollider(new RAABB(80f, 280f, 320f, 340f)));          // solid low
            world.StaticColliders.Add(new RStaticCollider(new RAABB(320f, 520f, 240f, 260f)));         // solid mid
            world.StaticColliders.Add(new RStaticCollider(new RAABB(560f, 760f, 160f, 180f)));         // solid high

            // one-way: jump up through from below, then stand on top
            world.StaticColliders.Add(new RStaticCollider(new RAABB(200f, 360f, 280f, 292f), true));
            world.StaticColliders.Add(new RStaticCollider(new RAABB(440f, 620f, 200f, 212f), true));

            // horizontal mover - rides left/right between PathMin and PathMax
            RStaticCollider horizontalMover = new RStaticCollider(new RAABB(120f, 240f, 300f, 316f));
            horizontalMover.Velocity = new RVector2(80f, 0f);
            horizontalMover.PathMin = 80f;
            horizontalMover.PathMax = 360f;
            world.StaticColliders.Add(horizontalMover);

            // vertical elevator - rides up/down
            RStaticCollider verticalMover = new RStaticCollider(new RAABB(480f, 580f, 300f, 316f));
            verticalMover.Velocity = new RVector2(0f, -60f);
            verticalMover.PathMin = 180f;
            verticalMover.PathMax = 360f;
            world.StaticColliders.Add(verticalMover);

            // collectible trigger - walk through, score once on Enter
            RStaticCollider coin = new RStaticCollider(new RAABB(350f, 382f, 200f, 232f));
            coin.IsTrigger = true;
            coin.Tag = "coin";
            world.StaticColliders.Add(coin);

            RStaticCollider coin2 = new RStaticCollider(new RAABB(620f, 652f, 120f, 152f));
            coin2.IsTrigger = true;
            coin2.Tag = "coin";
            world.StaticColliders.Add(coin2);

            // hazard trigger - walk in and respawn at start
            RStaticCollider hazard = new RStaticCollider(new RAABB(280f, 380f, 380f, 400f));
            hazard.IsTrigger = true;
            hazard.Tag = "hazard";
            world.StaticColliders.Add(hazard);

            // the player: a rectangle body that never sleeps, so it always responds to input
            playerSpawn = new RVector2(50f, 340f);
            player = new RRigidBody(
                playerSpawn,
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
            world.ClearTriggerContactHistory();
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

        // gameplay reactions for trigger enter/stay/exit - engine only reports the contact
        private void OnStaticContact(RContactEvent e)
        {
            if (e.Phase != RContactPhase.Enter)
            {
                return;
            }

            if (e.Collider.Tag == "coin")
            {
                e.Collider.Enabled = false;
                score++;
            }
            else if (e.Collider.Tag == "hazard" && player != null && e.Body == player)
            {
                player.Position = playerSpawn;
                player.Velocity = RVector2.Zero;
                player.PlatformVelocity = RVector2.Zero;
            }
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            float currentTime = stopwatch.ElapsedMilliseconds / 1000f;  // seconds
            float rawDeltaTime = currentTime - lastTime;   // true wall-clock time since last call, unclamped
            lastTime = currentTime;

            // fps must reflect the real wall-clock rate, not the clamped value below - otherwise a
            // frame that actually took 450ms (~2fps) gets silently counted as if it took 50ms (~20fps)
            fpsAccumTime += rawDeltaTime;
            fpsAccumFrames++;
            if (fpsAccumTime >= 0.5f)
            {
                displayedFps = fpsAccumFrames / fpsAccumTime;
                fpsAccumTime = 0f;
                fpsAccumFrames = 0;
            }

            float deltaTime = rawDeltaTime;

            // clamp huge spikes so the physics accumulator doesn't try to catch up in one big burst
            // (this must stay separate from the fps calculation above)
            if (deltaTime > 0.05f)
            {
                deltaTime = 0.05f;
            }

            lastDeltaTime = deltaTime;
            accumulator += deltaTime;

            clientHeight = this.ClientSize.Height;
            clientWidth = this.ClientSize.Width;

            if (physicsPaused)
            {
                // discard real-time accumulation so unpausing doesn't catch up in a burst
                accumulator = 0f;

                if (stepOnce)
                {
                    stepOnce = false;
                    TimedPhysicsStep();
                }
            }
            else
            {
                while (accumulator >= FixedDeltaTime)
                {
                    TimedPhysicsStep();
                    accumulator -= FixedDeltaTime;
                }
            }

            // average physics-only cost over its own half-second window, independent of the fps
            // window above, so it stays readable and immune to however expensive paint happens to be.
            // uses rawDeltaTime (not the clamped value) so the window still spans real wall-clock time
            // even when individual frames run far slower than the 50ms clamp.
            physicsMsWindowTime += rawDeltaTime;
            if (physicsMsWindowTime >= 0.5f && physicsStepsAccum > 0)
            {
                displayedPhysicsStepMs = (float)(physicsMsAccum / physicsStepsAccum);
                physicsMsWindowTime = 0f;
                physicsMsAccum = 0.0;
                physicsStepsAccum = 0;
            }

            // repaint
            Invalidate();
        }

        // wraps a single fixed physics step with timing, isolated from rendering cost
        private void TimedPhysicsStep()
        {
            physicsStopwatch.Restart();
            RunPhysicsStep();
            physicsStopwatch.Stop();

            physicsMsAccum += physicsStopwatch.Elapsed.TotalMilliseconds;
            physicsStepsAccum++;
        }

        // one fixed physics frame: bounds, player input, then world step
        private void RunPhysicsStep()
        {
            world.UpdateBounds(clientHeight, clientWidth);
            UpdateMovingPlatformPaths();

            if (playerController != null)
            {
                bool moveLeft = heldKeys.Contains(Keys.A) || heldKeys.Contains(Keys.Left);
                bool moveRight = heldKeys.Contains(Keys.D) || heldKeys.Contains(Keys.Right);
                bool jumpHeld = heldKeys.Contains(Keys.Space) || heldKeys.Contains(Keys.W) || heldKeys.Contains(Keys.Up);
                playerController.ApplyInput(moveLeft, moveRight, jumpPressed, jumpHeld, FixedDeltaTime);
            }
            jumpPressed = false;

            world.Step(FixedDeltaTime);
        }

        // reverse movers when they hit their path endpoints (engine stays dumb about paths)
        private void UpdateMovingPlatformPaths()
        {
            foreach (RStaticCollider collider in world.StaticColliders)
            {
                if (!collider.IsMoving || collider.PathMin == collider.PathMax)
                {
                    continue;
                }

                if (collider.Velocity.X != 0f)
                {
                    if (collider.Bounds.Left < collider.PathMin || collider.Bounds.Right > collider.PathMax)
                    {
                        collider.Velocity = new RVector2(-collider.Velocity.X, collider.Velocity.Y);
                    }
                }
                else if (collider.Velocity.Y != 0f)
                {
                    if (collider.Bounds.Top < collider.PathMin || collider.Bounds.Bottom > collider.PathMax)
                    {
                        collider.Velocity = new RVector2(collider.Velocity.X, -collider.Velocity.Y);
                    }
                }
            }
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

            // P pauses / unpauses the simulation
            if (e.KeyCode == Keys.P)
            {
                physicsPaused = !physicsPaused;
                stepOnce = false;
                return;
            }

            // . or N while paused advances exactly one physics frame
            if (physicsPaused && (e.KeyCode == Keys.OemPeriod || e.KeyCode == Keys.N))
            {
                stepOnce = true;
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

            // draw static colliders - triggers, movers, one-way, solid each get a distinct look
            foreach (RStaticCollider collider in world.StaticColliders)
            {
                if (!collider.Enabled)
                {
                    continue;
                }

                RAABB bounds = collider.Bounds;
                Brush fill;
                if (collider.IsTrigger)
                {
                    fill = collider.Tag == "hazard" ? hazardBrush : coinBrush;
                }
                else if (collider.IsMoving)
                {
                    fill = Brushes.Teal;
                }
                else if (collider.IsOneWay)
                {
                    fill = Brushes.SkyBlue;
                }
                else
                {
                    fill = Brushes.SteelBlue;
                }

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

            // score is always visible in the platformer scene
            if (!stressScene)
            {
                g.DrawString($"Score: {score}", SystemFonts.DefaultFont, Brushes.Black, 8f, 8f);
            }

            if (showDebug)
            {
                // FPS + evaluation metrics, bottom of the screen. Drawn on its own
                // opaque background bar (rather than plain text over whatever is
                // underneath, e.g. the steel-blue ground) so it stays readable
                // regardless of scene contents, and split into two shorter lines
                // so it isn't cut off by narrower window widths.
                float fps = displayedFps;
                int sleepingCount = 0;
                foreach (var body in world.Bodies)
                {
                    if (body.IsSleeping) sleepingCount++;
                }

                string broadPhaseLabel = world.UseBroadPhase ? "hash" : "brute";
                string sceneLabel = stressScene ? "stress" : "platformer";
                string pauseLabel = physicsPaused ? "PAUSED" : "running";

                string hudLine1 =
                    $"FPS: {fps:F0}  |  StepMs: {displayedPhysicsStepMs:F2}  |  Bodies: {world.Bodies.Count}  |  Sleeping: {sleepingCount}  |  Pairs: {world.LastCandidatePairCount}  |  BP: {broadPhaseLabel}  |  {sceneLabel}  |  {pauseLabel}";
                string hudLine2 =
                    "yellow=coin  red=hazard  teal=mover  |  [P] pause  [.] step  [F2] scene  [F3] BP  [F1] debug";

                using (Font hudFont = new Font(SystemFonts.DefaultFont.FontFamily, 10f, FontStyle.Bold))
                {
                    SizeF size1 = g.MeasureString(hudLine1, hudFont);
                    SizeF size2 = g.MeasureString(hudLine2, hudFont);
                    float boxWidth = System.Math.Max(size1.Width, size2.Width) + 12f;
                    float boxHeight = size1.Height + size2.Height + 8f;

                    float boxX = System.Math.Max(6f, this.ClientSize.Width - boxWidth - 6f);
                    float boxY = this.ClientSize.Height - boxHeight - 6f;

                    using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, 15, 15, 15)))
                    {
                        g.FillRectangle(bgBrush, boxX, boxY, boxWidth, boxHeight);
                    }

                    g.DrawString(hudLine1, hudFont, Brushes.White, boxX + 6f, boxY + 4f);
                    g.DrawString(hudLine2, hudFont, Brushes.White, boxX + 6f, boxY + 4f + size1.Height);
                }
            }
        }
    }
}
