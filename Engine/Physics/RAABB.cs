using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Physics
{
    public struct RAABB
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public RAABB(float left, float right, float top, float bottom)
        {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }

        public bool Intersects(RAABB other) // collision detection
        {
            return Left < other.Right &&    // check horizontal overlap
                   Right > other.Left &&    // 
                   Top < other.Bottom &&    // check vertical overlap
                   Bottom > other.Top;      // 
        }
    }
}
