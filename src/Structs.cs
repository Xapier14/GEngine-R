﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    public struct Gamepad
    {
        public int Index { get; set; }
        public bool A { get; set; }
        public bool B { get; set; }
        public bool X { get; set; }
        public bool Y { get; set; }
        public bool LeftBumper { get; set; }
        public bool RightBumper { get; set; }
        public bool Start { get; set; }
        public bool Back { get; set; }
        public AnalogStick LeftStick;
        public AnalogStick RightStick;
        public Hat DPad;
        public float LeftTrigger { get; set; }
        public float RightTrigger { get; set; }
        public bool Guide { get; set; }

        public Gamepad(int index)
        {
            Index = index;
            A = false;
            B = false;
            X = false;
            Y = false;
            LeftBumper = false;
            RightBumper = false;
            LeftTrigger = 0f;
            RightTrigger = 0f;
            Start = false;
            Back = false;
            LeftStick = new();
            RightStick = new();
            DPad = new();
            Guide = false;
        }
    }
    public struct AnalogStick
    {
        public float X;
        public float Y;
        public bool Pressed;
    }
    public struct Hat
    {
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
    }
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

        public Coord(Coord copy)
        {
            X = copy.X;
            Y = copy.Y;
        }

        public static Coord operator +(Coord a, Coord b) => new(a.X + b.X, a.Y + b.Y);
        public static Coord operator -(Coord a, Coord b) => new(a.X - b.X, a.Y - b.Y);
        public static Coord operator *(Coord a, Coord b) => new(a.X * b.X, a.Y * b.Y);
        public static Coord operator /(Coord a, Coord b) => new(a.X / b.X, a.Y / b.Y);

        public static Coord operator +(Coord a, int b) => new(a.X + b, a.Y + b);
        public static Coord operator -(Coord a, int b) => new(a.X - b, a.Y - b);
        public static Coord operator *(Coord a, int b) => new(a.X * b, a.Y * b);
        public static Coord operator /(Coord a, int b) => new(a.X / b, a.Y / b);
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
