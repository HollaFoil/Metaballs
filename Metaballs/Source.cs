using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaballs
{
    internal class Source
    {


        public Source(float[] loc, float[] dir, float scale)
        {
            this.location = loc;
            this.direction = dir;
            this.scale = scale;
        }
        public Source()
        {

        }
        public float GetDistanceSquared(float x, float y)
        {
            return scale*((location[0] - x) * (location[0] - x) + (location[1] - y) * (location[1] - y));
        }
        public void UpdatePosition(float scale, int sizeX, int sizeY)
        {
            location[0] += direction[0]*scale*sizeX;
            location[1] += direction[1]*scale*sizeY;

            if (location[0] > 1.0f*sizeX) { 
                direction[0] = -direction[0];
                location[0] = 1.0f* sizeX;
            }
            if (location[0] < -1.0f* sizeX)
            {
                direction[0] = -direction[0];
                location[0] = -1.0f* sizeX;
            }

            if (location[1] > 1.0f* sizeY)
            {
                direction[1] = -direction[1];
                location[1] = 1.0f* sizeY;
            }
            if (location[1] < -1.0f* sizeY)
            {
                direction[1] = -direction[1];
                location[1] = -1.0f* sizeY;
            }
        }
        
        public float[] location { get; set; }
        public float[] direction { get; set; }
        public float scale { get; set; }

    }
}