using Engine.Math;

namespace Engine.Physics.Collision
{
    // result of a narrow-phase check, carries what resolution needs to know
    // normal points from the first shape toward the second
    public struct RCollisionManifold
    {
        public RVector2 Normal;
        public float Penetration;

        public RCollisionManifold(RVector2 normal, float penetration)
        {
            Normal = normal;
            Penetration = penetration;
        }
    }
}
