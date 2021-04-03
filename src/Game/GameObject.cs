using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine.Engine;

namespace GEngine.Game
{
    public class GameObject
    {
        public TextureResource Sprite { get; set; }
        public PhysicsAttributes PhysicsAttributes { get; set; }
        public string ObjectName { get; set; }
    }
    public class Instance
    {
        public PhysicsVariables PhysicsVariables { get; set; }
        public GameObject Reference { get; set; }
        public Coord Position { get; set; }
    }
}
