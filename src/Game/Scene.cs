using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine;
using GEngine.Game;

namespace GEngine.Game
{
    public class SceneProperties
    {
        public OriginType OriginType { get; set; }
    }
    public abstract class Scene
    {
        public SceneProperties Properties { get; set; }
        public GameObjectCollection GameObjects { get; set; }
    }
}
