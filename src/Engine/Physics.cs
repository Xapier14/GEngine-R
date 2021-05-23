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
        private World _b2World;
        private Dictionary<Instance, BodyDef> _instBodyDef;

        public PhysicsWorld(Size worldSize, Coord gravity)
        {
            _instBodyDef = new Dictionary<Instance, BodyDef>();
            Vec2 g = new Vec2(gravity.X, gravity.Y);
            _b2World = new World(new AABB(), g, true);
        }

        public void AddObject
    }
}
