using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaballs
{
    internal class Grid
    {
        public Source[] sources { get; set; }
        public float threshold { get; set; }
        public float cellSize { get; set; }
        public int count { get; set; }
        public Grid(Source[] sources, float threshold, float cellSize)
        {
            this.sources = sources;
            this.threshold = threshold;
            this.cellSize = cellSize;
        }
        public void UpdateSources(float scale, int sizeX, int sizeY)
        {
            foreach (Source source in sources)
            {
                source.UpdatePosition(scale, sizeX, sizeY);
                //Console.WriteLine("Location: " + source.location[0].ToString() + " " + source.location[1].ToString() + "\n");
                //Console.WriteLine("Direction: " + source.direction[0].ToString() + " " + source.direction[1].ToString() + "\n");
                //Console.WriteLine("Scale: " + source.scale);
                //Console.WriteLine();
            }
            //Console.WriteLine();
        }
        public List<float[]> GetVerticesMarchingSquares(float sizeX, float sizeY)
        {
            //Console.WriteLine("Threshold: " + threshold.ToString());
            float step = cellSize;
            List<float[]> vertices = new List<float[]>();
            for (float i = -1.0f* sizeX; i <= 1.0f* sizeX; i += step* sizeX)
            {
                for (float j = -1.0f* sizeY; j <= 1.0f* sizeY; j += step* sizeY)
                {
                    float ax = i + step* sizeX, ay = j;
                    float bx = i + step* sizeX, by = j + step* sizeY;
                    float cx = i, cy = j + step* sizeY;
                    float dx = i, dy = j;

                    float aval = GetDistancesToPoint(ax, ay);
                    int a = IsWithinThreshold(aval);
                    float bval = GetDistancesToPoint(bx, by);
                    int b = IsWithinThreshold(bval);
                    float cval = GetDistancesToPoint(cx, cy);
                    int c = IsWithinThreshold(cval);
                    float dval = GetDistancesToPoint(dx, dy);
                    int d = IsWithinThreshold(dval);
                    
                    

                    int index = a * 8 + b * 4 + c * 2 + d*1;
                    //Console.WriteLine(aval);
                    if (index != 0 && index != 15)
                    {
                        aval += 0.000000000000001f;
                    }
                    /*
                     interpolations:
                        left:    vertices.Add(new float[] { Lerp(dx, ax, dval, aval), ay });
                        bottom:  vertices.Add(new float[] { dx, Lerp(dy, cy, dval, cval) });
                        right:   vertices.Add(new float[] { Lerp(cx, bx, cval, bval), by});
                        top:     vertices.Add(new float[] { ax, Lerp(ay, by, aval, bval)});
                    */
                    if (index == 0 || index == 15) continue;
                    else if (index == 1 || index == 14)
                    {
                        vertices.Add(new float[] { Lerp(dx, ax, dval, aval), ay });
                        vertices.Add(new float[] { dx, Lerp(dy, cy, dval, cval) });
                    }
                    else if (index == 2 || index == 13)
                    {
                        vertices.Add(new float[] { dx, Lerp(dy, cy, dval, cval) });
                        vertices.Add(new float[] { Lerp(cx, bx, cval, bval), by });
                    }
                    else if (index == 3 || index == 12)
                    {
                        vertices.Add(new float[] { Lerp(dx, ax, dval, aval), ay });
                        vertices.Add(new float[] { Lerp(cx, bx, cval, bval), by });
                    }
                    else if (index == 4 || index == 11)
                    {
                        vertices.Add(new float[] { ax, Lerp(ay, by, aval, bval) });
                        vertices.Add(new float[] { Lerp(cx, bx, cval, bval), by });

                    }
                    else if (index == 5)
                    {
                        vertices.Add(new float[] { Lerp(dx, ax, dval, aval), ay });
                        vertices.Add(new float[] { ax, Lerp(ay, by, aval, bval) });
                        vertices.Add(new float[] { dx, Lerp(dy, cy, dval, cval) });
                        vertices.Add(new float[] { Lerp(cx, bx, cval, bval), by });
                    }
                    else if (index == 6 || index == 9)
                    {
                        vertices.Add(new float[] { dx, Lerp(dy, cy, dval, cval) });
                        vertices.Add(new float[] { ax, Lerp(ay, by, aval, bval) });
                    }
                    else if (index == 7 || index == 8)
                    {
                        vertices.Add(new float[] { Lerp(dx, ax, dval, aval), ay });
                        vertices.Add(new float[] { ax, Lerp(ay, by, aval, bval) });
                    }
                    else if (index == 10)
                    {
                        vertices.Add(new float[] { Lerp(dx, ax, dval, aval), ay });
                        vertices.Add(new float[] { dx, Lerp(dy, cy, dval, cval) });
                        vertices.Add(new float[] { ax, Lerp(ay, by, aval, bval) });
                        vertices.Add(new float[] { Lerp(cx, bx, cval, bval), by });
                    }
                    
                }
            }
            
            //Console.WriteLine(vertices.Count);
            count = vertices.Count;
            //Console.WriteLine();
            //Console.WriteLine(vertices.Count);
            //Console.WriteLine((count - 1).ToString() + ": " + vertices[count - 1][0].ToString() + " " + vertices[count - 1][1].ToString());
            //Console.WriteLine((count - 2).ToString() + ": " + vertices[count - 2][0].ToString() + " " + vertices[count - 2][1].ToString());
            return vertices;
        }
        public void BringBackSources(int newX, int newY, int oldX, int oldY)
        {
            foreach (var source in sources)
            {
                source.location[0] = source.location[0] / ((float)oldX) * newX;
                source.location[1] = source.location[1] / ((float)oldY) * newY;
            }
        }
        public void CalculateThreshold(int sizeX, int sizeY)
        {
            float step = cellSize*10;
            List<float> vertices = new List<float>();
            for (float i = -1.0f * sizeX; i <= 1.0f * sizeX - step * sizeX; i += step * sizeX)
            {
                for (float j = -1.0f * sizeY; j <= 1.0f * sizeY - step * sizeY; j += step * sizeY)
                {
                    float ax = i, ay = j;
                    float aval = GetDistancesToPoint(ax, ay);
                    vertices.Add(aval);

                    //Console.WriteLine(aval);

                }
            }
            vertices.Sort();
            threshold = vertices[(int)Math.Ceiling(vertices.Count * 0.7f)];
        }
        private float GetDistancesToPoint(float x, float y)
        {
            float sum = 0f;
            foreach (var source in sources) sum += 1 / source.GetDistanceSquared(x, y);
            return sum;
        }
        private int IsWithinThreshold(float sum)
        {
            return (sum >= threshold) ? 1 : 0;
        }
        float Lerp(float ax, float bx, float aval, float bval)
        {
            //if (Math.Abs(aval - bval) < 0.1) return (ax + bx) / 2;
            return ax + (bx - ax)*((threshold-aval)/(bval-aval));
        }
    }
}
