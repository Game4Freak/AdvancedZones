using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game4Freak.AdvancedZones
{
    public class Node
    {
        private float x;
        private float z;
        private float y;

        public Node()
        {

        }

        public Node(float nX, float nZ, float nY)
        {
            x = nX;
            z = nZ;
            y = nY;
        }

        public float getX()
        {
            return x;
        }

        public float getZ()
        {
            return z;
        }

        public float getY()
        {
            return y;
        }
    }
}
