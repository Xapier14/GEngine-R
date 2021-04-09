using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine.Engine;

namespace GEngine.Game
{
    public abstract class GameObject
    {
        public TextureResource DefaultSprite { get; set; }
        public PhysicsAttributes PhysicsAttributes { get; set; }
        public string ObjectName { get; set; }
        public bool IsAnimated { get; set; }
        public int DefaultImageSpeed { get; set; }
        public int DefaultImageIndex { get; set; }
        public double DefaultImageAngle { get; set; }

        public Instance CreateInstance(out Guid hash)
        {
            Instance newInstance = new Instance()
            {
                BaseReference = this,
                Sprite = DefaultSprite,
                ImageIndex = DefaultImageIndex,
                ImageSpeed = DefaultImageSpeed,
                ImageAngle = DefaultImageAngle,
                IsAnimated = IsAnimated,
                ReferenceType = Type
            };
            hash = newInstance.Hash;
            return newInstance;
        }

        public virtual void OnCreate(Instance caller, SceneInstance scene)
        {

        }

        public virtual void Step(Instance caller, SceneInstance scene)
        {

        }

        public virtual void OnDestroy(Instance caller, SceneInstance scene)
        {

        }
        public Type Type { get; set; }
    }
    public class Instance
    {
        public TextureResource Sprite { get; set; }
        public PhysicsVariables PhysicsVariables { get; set; }
        public GameObject BaseReference { get; set; }
        public dynamic Reference
        {
            get
            {
                return Convert.ChangeType(BaseReference, ReferenceType);
            }
        }
        public Coord Position;
        public int ImageIndex { get; set; }
        public int ImageSpeed { get; set; }
        public int Depth { get; set; }
        public double ImageAngle { get; set; }
        public bool IsAnimated { get; set; }
        public Guid Hash { get; private set; }
        public Type ReferenceType { get; set; }

        public Instance()
        {
            Hash = Guid.NewGuid();
            PhysicsVariables = new PhysicsVariables();
            Position = new Coord(0, 0);
            Depth = 0;
        }
    }
}
