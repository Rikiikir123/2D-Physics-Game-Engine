using Engine.Math;

namespace Engine.Physics.Collision
{
    // narrow-phase: figures out if two shapes overlap and, if so, how (normal + penetration)
    public static class CollisionDetector
    {
        public static bool TryDetectAABBvsAABB(RAABB a, RAABB b, out RCollisionManifold manifold)
        {
            manifold = default;

            float overlapLeft = a.Right - b.Left;
            float overlapRight = b.Right - a.Left;
            float overlapTop = a.Bottom - b.Top;
            float overlapBottom = b.Bottom - a.Top;

            // no overlap on at least one axis, no collision
            if (overlapLeft <= 0f || overlapRight <= 0f || overlapTop <= 0f || overlapBottom <= 0f)
            {
                return false;
            }

            float minOverlapX = System.Math.Min(overlapLeft, overlapRight);
            float minOverlapY = System.Math.Min(overlapTop, overlapBottom);

            RVector2 normal;
            float penetration;

            // push out along whichever axis has the smaller overlap
            if (minOverlapX < minOverlapY)
            {
                penetration = minOverlapX;
                normal = overlapLeft < overlapRight ? new RVector2(1f, 0f) : new RVector2(-1f, 0f);
            }
            else
            {
                penetration = minOverlapY;
                normal = overlapTop < overlapBottom ? new RVector2(0f, 1f) : new RVector2(0f, -1f);
            }

            manifold = new RCollisionManifold(normal, penetration);
            return true;
        }

        public static bool TryDetectCircleVsCircle(RVector2 centerA, float radiusA, RVector2 centerB, float radiusB, out RCollisionManifold manifold)
        {
            manifold = default;

            RVector2 delta = centerB - centerA;
            float distanceSquared = delta.X * delta.X + delta.Y * delta.Y;
            float radiusSum = radiusA + radiusB;

            if (distanceSquared >= radiusSum * radiusSum)
            {
                return false;
            }

            float distance = (float)System.Math.Sqrt(distanceSquared);

            RVector2 normal;
            float penetration;

            // centers exactly overlap, pick an arbitrary direction
            if (distance == 0f)
            {
                normal = new RVector2(1f, 0f);
                penetration = radiusSum;
            }
            else
            {
                normal = delta / distance;
                penetration = radiusSum - distance;
            }

            manifold = new RCollisionManifold(normal, penetration);
            return true;
        }

        // works for both a dynamic circle vs a rectangle and a circle vs a static platform,
        // since both just need the rectangle's bounds
        public static bool TryDetectCircleVsAABB(RVector2 center, float radius, RAABB box, out RCollisionManifold manifold)
        {
            manifold = default;

            // prefer flat face contacts over corner normals when the circle is mostly outside
            // on one side - stops the slow diagonal slide when a circle sits near a rectangle edge
            float distToTop = box.Top - center.Y;
            if (distToTop > 0f && distToTop < radius && center.X + radius > box.Left && center.X - radius < box.Right)
            {
                manifold = new RCollisionManifold(new RVector2(0f, 1f), radius - distToTop);
                return true;
            }

            float distToBottom = center.Y - box.Bottom;
            if (distToBottom > 0f && distToBottom < radius && center.X + radius > box.Left && center.X - radius < box.Right)
            {
                manifold = new RCollisionManifold(new RVector2(0f, -1f), radius - distToBottom);
                return true;
            }

            float distToLeft = box.Left - center.X;
            if (distToLeft > 0f && distToLeft < radius && center.Y + radius > box.Top && center.Y - radius < box.Bottom)
            {
                manifold = new RCollisionManifold(new RVector2(1f, 0f), radius - distToLeft);
                return true;
            }

            float distToRight = center.X - box.Right;
            if (distToRight > 0f && distToRight < radius && center.Y + radius > box.Top && center.Y - radius < box.Bottom)
            {
                manifold = new RCollisionManifold(new RVector2(-1f, 0f), radius - distToRight);
                return true;
            }

            bool centerInsideX = center.X >= box.Left && center.X <= box.Right;
            bool centerInsideY = center.Y >= box.Top && center.Y <= box.Bottom;

            RVector2 normal;
            float penetration;

            // circle center is horizontally inside the box span, treat as top/bottom collision only
            if (centerInsideX && !centerInsideY)
            {
                if (center.Y < box.Top)
                {
                    float distanceToTop = box.Top - center.Y;
                    penetration = radius - distanceToTop;

                    if (penetration <= 0f)
                        return false;

                    normal = new RVector2(0f, 1f);
                }
                else
                {
                    float distanceToBottom = center.Y - box.Bottom;
                    penetration = radius - distanceToBottom;

                    if (penetration <= 0f)
                        return false;

                    normal = new RVector2(0f, -1f);
                }
            }
            // circle center is vertically inside the box span, treat as left/right collision only
            else if (!centerInsideX && centerInsideY)
            {
                if (center.X < box.Left)
                {
                    float distanceToLeft = box.Left - center.X;
                    penetration = radius - distanceToLeft;

                    if (penetration <= 0f)
                        return false;

                    normal = new RVector2(1f, 0f);
                }
                else
                {
                    float distanceToRight = center.X - box.Right;
                    penetration = radius - distanceToRight;

                    if (penetration <= 0f)
                        return false;

                    normal = new RVector2(-1f, 0f);
                }
            }
            // corner case, compare against the closest point on the box
            else
            {
                float closestX = Clamp(center.X, box.Left, box.Right);
                float closestY = Clamp(center.Y, box.Top, box.Bottom);

                RVector2 closestPoint = new RVector2(closestX, closestY);
                RVector2 delta = center - closestPoint;

                float distanceSquared = delta.X * delta.X + delta.Y * delta.Y;

                if (distanceSquared >= radius * radius)
                    return false;

                float distance = (float)System.Math.Sqrt(distanceSquared);

                if (distance == 0f)
                    return false;

                normal = (closestPoint - center) / distance;
                penetration = radius - distance;

                if (penetration <= 0f)
                    return false;
            }

            manifold = new RCollisionManifold(normal, penetration);
            return true;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
