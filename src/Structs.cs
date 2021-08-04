using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    public struct ColorRGBA
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public byte Alpha { get; set; }
        public ColorRGBA(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = 255;
        }
        public ColorRGBA(byte r, byte g, byte b, byte a)
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;
        }
    }
    public struct Coord
    {
        public static Coord Zero = new Coord(0, 0);
        public int X { get; set; }
        public int Y { get; set; }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Coord operator +(Coord a, Coord b) => new(a.X + b.X, a.Y + b.Y);
        public static Coord operator -(Coord a, Coord b) => new(a.X - b.X, a.Y - b.Y);
    }
    public struct Size
    {
        public int W { get; set; }
        public int H { get; set; }

        public Size(int w, int h)
        {
            W = w;
            H = h;
        }
    }
}
