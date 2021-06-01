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

namespace GEngine.Engine
{
    public class PhysicsEngine
    {
        public bool UsesSimplePhysics { private get; set; }

        public PhysicsEngine(bool useSimple)
        {
            UsesSimplePhysics = useSimple;
        }
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
        private World _velcroWorld;

        public PhysicsWorld(Size worldSize, Coord gravity)
        {
            _velcroWorld = new World(new Vector2(gravity.X, gravity.Y));
        }

        public void AddObject(Instance inst)
        {
        }

        public void UpdateCycle()
        {
        }
    }
    internal struct BodyDefPair
    {
    }
}
