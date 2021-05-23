using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Box2DX.Dynamics.Controllers;
using GEngine.Game;

namespace GEngine.Engine
{
    public class PhysicsEngine
    {
        
    }
    public class PhysicsAttributes
    {
        
    }
    public class PhysicsVariables
    {

    }
    public class PhysicsWorld
    {
        private const float UNIT_SCALE = 0.1f;
        private World _b2World;
        private Dictionary<Instance, BodyDefPair> _instBodyDef;

        public PhysicsWorld(Size worldSize, Coord gravity)
        {
            _instBodyDef = new Dictionary<Instance, BodyDefPair>();
            Vec2 g = new Vec2(gravity.X * UNIT_SCALE, gravity.Y * UNIT_SCALE);
            AABB worldBounds = new AABB();
            worldBounds.LowerBound.Set(0, 0);
            worldBounds.UpperBound.Set(UNIT_SCALE * worldSize.W, UNIT_SCALE * worldSize.H);
            _b2World = new World(worldBounds, g, true);
        }

        public void AddObject(Instance inst)
        {
            BodyDef iDef = new BodyDef();
            iDef.Position.Set(inst.Position.X * UNIT_SCALE, inst.Position.Y * UNIT_SCALE);
            Body iBody = _b2World.CreateBody(iDef);

        }

        public void UpdateCycle()
        {

        }
    }
    internal struct BodyDefPair
    {

    }
}
