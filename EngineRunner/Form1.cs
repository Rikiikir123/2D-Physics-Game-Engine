using System;
using System.Timers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Engine.Math;
using Engine.Physics;

namespace EngineRunner
{
    public partial class Form1 : Form
    {
        private const float BallRadius = 6f;
        private const float PegRadius = 7f;
        private const float SpawnZoneHeight = 70f;
        private const float SidePadding = 25f;
        private const float PegHorizontalSpacing = 55f;
        private const float PegVerticalSpacing = 45f;
        private const float Restitution = 0.6f;

        private readonly List<Rigidbody2D> balls = new List<Rigidbody2D>();
        private readonly List<Vector2> pegs = new List<Vector2>();

        private readonly Stopwatch stopwatch;
        private readonly System.Windows.Forms.Timer timer;

        private float lastTime;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            Text = "Pachinko Prototype";

         
            BuildPegField();
            Resize += (_, __) => BuildPegField();
            MouseDown += OnMouseDown;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            timer = new System.Windows.Forms.Timer();
  
            timer.Interval = 16;
            timer.Tick += GameLoop;
            timer.Start();
        }

 
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.Y > SpawnZoneHeight)
            {
                return;
            }

            SpawnBall(e.X);
        }

        private void SpawnBall(float x)
        {
            float clampedX = Math.Clamp(x, BallRadius, ClientSize.Width - BallRadius);
            var body = new Rigidbody2D(new Vector2(clampedX, BallRadius + 2f));
            body.Velocity = new Vector2(0f, 0f);
            balls.Add(body);
        }

        private void BuildPegField()
        {
            pegs.Clear();

            float top = SpawnZoneHeight + 20f;
            float bottom = ClientSize.Height - 60f;
            int row = 0;

            for (float y = top; y < bottom; y += PegVerticalSpacing)
            {
                bool offset = row % 2 == 1;
                float startX = SidePadding + (offset ? PegHorizontalSpacing * 0.5f : 0f);

                for (float x = startX; x <= ClientSize.Width - SidePadding; x += PegHorizontalSpacing)
                {
                    pegs.Add(new Vector2(x, y));
                }

                row++;
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            float currentTime = stopwatch.ElapsedMilliseconds / 1000f;
            float deltaTime = currentTime - lastTime;
            lastTime = currentTime;

         
            if (deltaTime <= 0f)
            {
                return;
            }

           
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                var ball = balls[i];
                ball.Update(deltaTime);

                ResolveWallCollision(ball);
                ResolvePegCollisions(ball);

                if (ball.Position.Y - BallRadius > ClientSize.Height)
                {
                    balls.RemoveAt(i);
                }
            }

            Invalidate();
        }

        private void ResolveWallCollision(Rigidbody2D ball)
        {
            if (ball.Position.X - BallRadius < 0f)
            {
                ball.Position.X = BallRadius;
                if (ball.Velocity.X < 0f)
                {
                    ball.Velocity.X *= -Restitution;
                }
            }

            float right = ClientSize.Width - BallRadius;
            if (ball.Position.X > right)
            {
                ball.Position.X = right;
                if (ball.Velocity.X > 0f)
                {
                    ball.Velocity.X *= -Restitution;
                }
            }
        }

        private void ResolvePegCollisions(Rigidbody2D ball)
        {
            float minDistance = BallRadius + PegRadius;

            for (int i = 0; i < pegs.Count; i++)
            {
                Vector2 peg = pegs[i];
                Vector2 delta = ball.Position - peg;
                float distance = delta.Magnitude;

                if (distance <= 0.0001f || distance >= minDistance)
                {
                    continue;
                }

                Vector2 normal = delta / distance;
                float penetration = minDistance - distance;
                ball.Position += normal * penetration;

                float velocityIntoPeg = Vector2.Dot(ball.Velocity, normal);
                if (velocityIntoPeg < 0f)
                {
                    ball.Velocity -= normal * ((1f + Restitution) * velocityIntoPeg);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

          
            e.Graphics.Clear(Color.WhiteSmoke);

            using var spawnPen = new Pen(Color.SteelBlue, 2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            e.Graphics.DrawLine(spawnPen, 0, SpawnZoneHeight, ClientSize.Width, SpawnZoneHeight);
            e.Graphics.DrawString("Click above this line to drop balls", Font, Brushes.SteelBlue, 8f, 8f);

            foreach (Vector2 peg in pegs)
            {
                e.Graphics.FillEllipse(
                    Brushes.DimGray,
                    peg.X - PegRadius,
                    peg.Y - PegRadius,
                    PegRadius * 2f,
                    PegRadius * 2f);
            }

            foreach (Rigidbody2D ball in balls)
            {
                e.Graphics.FillEllipse(
                    Brushes.Firebrick,
                    ball.Position.X - BallRadius,
                    ball.Position.Y - BallRadius,
                    BallRadius * 2f,
                    BallRadius * 2f);
            }

            e.Graphics.DrawString($"Balls: {balls.Count}", Font, Brushes.Black, 8f, ClientSize.Height - 28f);
        }
    }
}