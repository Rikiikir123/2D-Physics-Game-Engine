using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Physics.Shapes
{
    public abstract class RShape
    {
        public class RRectangleShape : RShape
        {
            public float Width;
            public float Height;
            public RRectangleShape (float width, float height)
            {
                Width = width;
                Height = height;
            }
        }
        public class RCircleShape : RShape
        {
            public float Radius;
            public RCircleShape (float radius)
            {
                Radius = radius;
            }
        }
    }
}
