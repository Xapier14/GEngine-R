﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine.Game;

using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Utilities;

using Microsoft.Xna.Framework;
using System.Diagnostics;

using static GEngine.GEngine;

namespace GEngine.Engine
{
    public class PhysicsAttributes
    {
        public BodyType BodyType { get; set; }
        public Size PhysicsBodySize { get; set; }
        public float Density { get; set; }
        public float Radius { get; set; }
        public PhysicsBodyType PhysicsBodyType { get; set; }

        public PhysicsAttributes()
        {
            PhysicsBodyType = PhysicsBodyType.Box;
            BodyType = BodyType.Static;
            PhysicsBodySize = new Size();
            Density = 0f;
            Radius = 0f;
        }

        public PhysicsAttributes(PhysicsAttributes copy)
        {
            BodyType = copy.BodyType;
            PhysicsBodySize = copy.PhysicsBodySize;
            PhysicsBodyType = copy.PhysicsBodyType;
            Density = copy.Density;
            Radius = copy.Radius;
        }

        public PhysicsAttributes(BodyType bodyType, int width, int height, float density)
        {
            PhysicsBodyType = PhysicsBodyType.Box;
            BodyType = bodyType;
            PhysicsBodySize = new Size(width, height);
            Density = density;
        }
        public PhysicsAttributes(BodyType bodyType, float radius, float density)
        {
            PhysicsBodyType = PhysicsBodyType.Circle;
            BodyType = bodyType;
            PhysicsBodySize = new Size(0, 0);
            Radius = radius;
            Density = density;
        }
    }
    public class PhysicsVariables
    {
        public float Direction { get; set; }
        
        public PhysicsVariables()
        {
            Direction = 0f;
        }
    }
    public class PhysicsWorld
    {
        private const float UNIT_SCALE = 0.1f;
        private World _velcroWorld;
        private List<BodyDefPair> _bodyDefPairs;
        private Stopwatch _timer;

        public PhysicsWorld(Coord gravity)
        {
            _bodyDefPairs = new List<BodyDefPair>();
            _velcroWorld = new World(new Vector2(ConvertUnits.ToSimUnits(gravity.X * UNIT_SCALE), ConvertUnits.ToSimUnits(gravity.Y * UNIT_SCALE)));
            _timer = new Stopwatch();
            _timer.Start();
        }

        public void AddObject(Instance inst)
        {
            if (ContainsInstance(inst))
                throw new PhysicsException("Object already in world.", "PhysicsWorld.AddObjecct()");
            PhysicsBodyType phyType = inst.PhysicsAttributes.PhysicsBodyType;
            BodyType bodyType = inst.PhysicsAttributes.BodyType;
            Size bodySize = inst.PhysicsAttributes.PhysicsBodySize;
            float bodyDensity = inst.PhysicsAttributes.Density;
            float bodyRotation = inst.PhysicsVariables.Direction;
            float bodyRadius = inst.PhysicsAttributes.Radius;
            Vector2 bodyPosition = new Vector2(ConvertUnits.ToSimUnits(inst.Position.X * UNIT_SCALE), ConvertUnits.ToSimUnits(inst.Position.Y * UNIT_SCALE));

            Body body = null;
            switch (phyType)
            {
                case PhysicsBodyType.Box:
                    body = BodyFactory.CreateRectangle(_velcroWorld, ConvertUnits.ToSimUnits(bodySize.W), ConvertUnits.ToSimUnits(bodySize.H), bodyDensity, bodyPosition, bodyRotation, bodyType, inst);
                    break;
                case PhysicsBodyType.Circle:
                    body = BodyFactory.CreateCircle(_velcroWorld, ConvertUnits.ToSimUnits(bodyRadius), bodyDensity, bodyPosition, bodyType, inst);
                    break;
                default:
                    throw new PhysicsException("Invalid PhysicsBodyType.", "PhysicsWorld.AddObject()");
            }
            BodyDefPair pair = new BodyDefPair(inst, body);
            _bodyDefPairs.Add(pair);
            Debug.Log("PhysicsWorld.AddObject()", $"Added object of type '{inst.BaseReference.ObjectName}'");
        }
        public void UpdateCycle()
        {
            _velcroWorld.Step((float)GEngine.Game.CurrentLogictime * 0.01f);
            // update instance vars
            foreach (BodyDefPair bdp in _bodyDefPairs)
            {
                Debug.Log("PhysicsWorld.UpdateCycle()", $"Updating {bdp.Owner.BaseReference.ObjectName}... (Position: {bdp.Owner.Position.X}, {bdp.Owner.Position.Y})");
                Body b = bdp.InstanceBody;
                Instance i = bdp.Owner;
                UpdateInstance(b, ref i);
            }
        }

        internal Vector2 Coor2Vec2(Coord c, float scale = 1)
        {
            return new Vector2((float)c.X * scale, (float)c.Y * scale);
        }

        internal Coord Vec2Coord(Vector2 c, float scale = 1)
        {
            return new Coord(Convert.ToInt32(c.X * scale), Convert.ToInt32(c.Y * scale));
        }

        internal bool ContainsInstance(Instance inst)
        {
            foreach (BodyDefPair bdp in _bodyDefPairs)
            {
                if (bdp.Owner == inst)
                    return true;
            }
            return false;
        }

        internal void UpdateInstance(Body body, ref Instance instance)
        {
            Coord pos = Vec2Coord(body.Position, 1f / UNIT_SCALE);
            instance.Position.X = pos.X;
            instance.Position.Y = pos.Y;
            instance.PhysicsVariables.Direction = body.Rotation;
        }
    }
    internal struct BodyDefPair
    {
        public Body InstanceBody;
        public Instance Owner;

        public BodyDefPair(Instance inst, Body body)
        {
            InstanceBody = body;
            Owner = inst;
        }
    }
}
