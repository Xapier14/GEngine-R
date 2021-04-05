using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine;
using GEngine.Engine;

namespace GEngine.Game
{
    public class View
    {
        public Coord Position { get; set; }
        public Size Size { get; set; }
        public OriginType OriginType { get; set; }

        public View()
        {
            Position = new Coord() { X = 0, Y = 0 };
            Size = new Size() { W = 0, H = 0 };
            OriginType = OriginType.ManualOrigin;
        }

        public View(int w, int h)
        {
            Position = new Coord() { X = 0, Y = 0 };
            Size = new Size() { W = w, H = h };
            OriginType = OriginType.ManualOrigin;
        }

        public View(Size size)
        {
            Position = new Coord() { X = 0, Y = 0 };
            Size = new Size() { W = size.W, H = size.H };
            OriginType = OriginType.ManualOrigin;
        }
    }
    public class SceneProperties
    {
        public View View { get; set; }
    }
    public abstract class Scene
    {
        public SceneProperties Properties { get; set; }
        public Size SceneSize { get; set; }
        public GameObjectCollection GameObjects { get; set; }

        public Scene(Size sceneSize, Size viewSize)
        {
            SceneSize = sceneSize;
            Properties = new SceneProperties();
            Properties.View = new View(viewSize);
            Properties.View.OriginType = OriginType.CenterOrigin;
        }

        public static SceneInstance CreateInstance()
        {
            throw new NotImplementedException();
        }
    }
    public class SceneInstance
    {
        public Scene Reference { get; set; }
    }
}
