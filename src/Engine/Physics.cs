using System;
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
using Genbox.VelcroPhysics.Shared;

namespace GEngine.Engine
{
    public class PhysicsAttributes
    {
        public BodyType BodyType { get; set; }
        public Size PhysicsBodySize { get; set; }
        public float Density { get; set; }
        public float Radius { get; set; }
        public float LinearDampening { get; set; }
        public float Friction { get; set; }
        public PhysicsBodyType PhysicsBodyType { get; set; }

        public PhysicsAttributes()
        {
            PhysicsBodyType = PhysicsBodyType.Box;
            BodyType = BodyType.Static;
            PhysicsBodySize = new Size();
            Density = 0f;
            Radius = 0f;
            LinearDampening = 0f;
            Friction = 0f;
        }

        public PhysicsAttributes(PhysicsAttributes copy)
        {
            BodyType = copy.BodyType;
            PhysicsBodySize = copy.PhysicsBodySize;
            PhysicsBodyType = copy.PhysicsBodyType;
            Density = copy.Density;
            Radius = copy.Radius;
            LinearDampening = copy.LinearDampening;
            Friction = copy.Friction;
        }

        public PhysicsAttributes(BodyType bodyType, int width, int height, float density)
        {
            PhysicsBodyType = PhysicsBodyType.Box;
            BodyType = bodyType;
            PhysicsBodySize = new Size(width, height);
            Density = density;
            LinearDampening = 0f;
            Friction = 0f;
        }
        public PhysicsAttributes(BodyType bodyType, float radius, float density)
        {
            PhysicsBodyType = PhysicsBodyType.Circle;
            BodyType = bodyType;
            PhysicsBodySize = new Size(0, 0);
            Radius = radius;
            Density = density;
            LinearDampening = 0f;
            Friction = 0f;
        }
    }
    public class PhysicsVariables
    {
        public float Direction;
        public Coord Velocity;
        
        public PhysicsVariables()
        {
            Direction = 0f;
            Velocity = new Coord(0, 0);
        }
    }
    public class PhysicsWorld
    {
        private const float UNIT_SCALE = 1f;
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
                throw new PhysicsException("Object already in world.", "PhysicsWorld.AddObject()");
            PhysicsBodyType phyType = inst.PhysicsAttributes.PhysicsBodyType;
            BodyType bodyType = inst.PhysicsAttributes.BodyType;
            Size bodySize = inst.PhysicsAttributes.PhysicsBodySize;
            float bodyDensity = inst.PhysicsAttributes.Density;
            float bodyRotation = inst.PhysicsVariables.Direction;
            float bodyRadius = inst.PhysicsAttributes.Radius;
            Vector2 bodyPosition = ConvertUnits.ToSimUnits(new Vector2(inst.Position.X, inst.Position.Y));

            Body body = null;
            body = phyType switch
            {
                PhysicsBodyType.Box => BodyFactory.CreateRectangle(_velcroWorld, ConvertUnits.ToSimUnits(bodySize.W), ConvertUnits.ToSimUnits(bodySize.H), bodyDensity, bodyPosition, bodyRotation, bodyType, inst),
                PhysicsBodyType.Circle => BodyFactory.CreateCircle(_velcroWorld, ConvertUnits.ToSimUnits(bodyRadius), bodyDensity, bodyPosition, bodyType, inst),
                _ => throw new PhysicsException("Invalid PhysicsBodyType.", "PhysicsWorld.AddObject()"),
            };

            foreach (Fixture fixture in body.FixtureList)
            {
                fixture.UserData = inst;
            }

            body.LinearDamping = inst.PhysicsAttributes.LinearDampening;
            body.Friction = inst.PhysicsAttributes.Friction;
            BodyDefPair pair = new BodyDefPair(inst, body);
            _bodyDefPairs.Add(pair);
            //Debug.Log("PhysicsWorld.AddObject()", $"Added object '{inst}' @ ({body.Position.X}, {body.Position.Y})[GamePos: {ConvertUnits.ToDisplayUnits(body.Position.X)}, {ConvertUnits.ToDisplayUnits(body.Position.Y)}]");
        }

        public bool CheckCollision(Instance src, int offsetX, int offsetY, out List<Instance> collision)
        {
            collision = new();
            Body body = GetBody(src);
            if (body == null)
            {
                collision = null;
                return false;
            }
            AABB aabb = new();
            aabb.UpperBound = new(float.MaxValue, float.MaxValue);
            aabb.LowerBound = new(-float.MinValue, -float.MinValue);
            body.GetTransform(out Transform transform);
            foreach (Fixture fixture in body.FixtureList)
            {
                fixture.Shape.ComputeAABB(ref transform, 0, out AABB child);
                if (body.FixtureList.Count > 1)
                {
                    aabb.Combine(ref child);
                } else
                {
                    aabb = child;
                }
            }
            List<Fixture> fixtures = _velcroWorld.QueryAABB(ref aabb);
            foreach(Fixture fixture in fixtures)
            {
                if (fixture.UserData != null)
                {
                    if (!collision.Contains(fixture.UserData))
                    {
                        collision.Add((Instance)fixture.UserData);
                    }
                }
            }
            return collision.Count > 0;
        }

        public void UpdateCycle()
        {
            foreach (BodyDefPair bdp in _bodyDefPairs)
            {
                Body b = bdp.InstanceBody;
                Instance i = bdp.Owner;
                ApplyBodyUpdate(ref b, i);
            }
            //_velcroWorld.ProcessChanges();
            _velcroWorld.Step((float)GEngine.Game.CurrentLogictime * 0.01f);
            // update instance vars
            foreach (BodyDefPair bdp in _bodyDefPairs)
            {
                Body b = bdp.InstanceBody;
                Instance i = bdp.Owner;
                UpdateInstance(b, ref i);
            }
        }
        public void SetObjectPosition(Instance inst)
        {
            if (!ContainsInstance(inst))
                throw new PhysicsException("Instance is not in PhysicsWorld.", "PhysicsWorld.SetObjectPosition()");
            Vector2 position = ConvertUnits.ToSimUnits(Coor2Vec2(inst.Position));
            Body body = GetBody(inst);
            
            body.Position = position;
        }

        internal Body GetBody(Instance inst)
        {
            foreach (BodyDefPair bdp in _bodyDefPairs)
            {
                if (bdp.Owner == inst)
                    return bdp.InstanceBody;
            }
            return null;
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

        internal void ApplyBodyUpdate(ref Body body, Instance instance)
        {
            Vector2 velocity = ConvertUnits.ToSimUnits(Coor2Vec2(instance.PhysicsVariables.Velocity));
            Vector2 position = ConvertUnits.ToSimUnits(Coor2Vec2(instance.Position));
            body.Position = position;
            body.LinearVelocity = velocity;
            body.Rotation = instance.PhysicsVariables.Direction;
            body.LinearDamping = instance.PhysicsAttributes.LinearDampening;
            body.Friction = instance.PhysicsAttributes.Friction;
        }

        internal void UpdateInstance(Body body, ref Instance instance)
        {
            Coord pos = Vec2Coord(new Vector2(ConvertUnits.ToDisplayUnits(body.Position.X), ConvertUnits.ToDisplayUnits(body.Position.Y)));
            Coord vel = Vec2Coord(new Vector2(ConvertUnits.ToDisplayUnits(body.LinearVelocity.X), ConvertUnits.ToDisplayUnits(body.LinearVelocity.Y)));
            //Debug.Log("PhysicsWorld.UpdateInstance()", $"Updating instance of type '{instance}'. Position: {pos.X}, {pos.Y}");
            instance.Position.X = pos.X;
            instance.Position.Y = pos.Y;
            instance.PhysicsVariables.Direction = body.Rotation;
            instance.PhysicsVariables.Velocity = vel;
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
