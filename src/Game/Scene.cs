using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine;
using GEngine.Engine;

namespace GEngine.Game
{
    public class SceneProperties
    {
        public bool AnimateSprites { get; set; }
        public SceneProperties(Size viewSize)
        {
            AnimateSprites = true;
        }
    }
    public abstract class Scene
    {
        public string Name { get; set; }
        public SceneProperties Properties { get; set; }
        public Size SceneSize { get; set; }
        public GameObjectInfoCollection GameObjects { get; set; }
        public Type ReferenceType { get; set; }

        public Scene(Size sceneSize, Size viewSize)
        {
            SceneSize = sceneSize;
            Properties = new SceneProperties(viewSize);
            GameObjects = new GameObjectInfoCollection();
        }

        public SceneInstance CreateInstance()
        {
            SceneInstance instance = new SceneInstance();
            instance.BaseReference = this;
            instance.Instances = new InstanceCollection();
            instance.ReferenceType = ReferenceType;
            instance.ViewPosition = new Coord(0, 0);
            instance.ViewOrigin = new Coord(0, 0);

            foreach(var obj in GameObjects)
            {
                Instance inst = obj.GameObject.CreateInstance(out Guid guid);
                inst.Position = new Coord(obj.Position.X, obj.Position.Y);
                instance.Instances.Add(inst);
                inst.BaseReference.OnCreate(inst, instance);
            }

            return instance;
        }

        public virtual void OnCreate(SceneInstance caller)
        {

        }

        public virtual void Step(SceneInstance caller)
        {
            foreach (Instance inst in caller.Instances)
            {
                inst.BaseReference.Step(inst, caller);
            }
        }

        public virtual void OnDestroy(SceneInstance caller)
        {
            foreach (Instance inst in caller.Instances)
            {
                inst.BaseReference.OnDestroy(inst, caller);
            }
        }
    }
    public class SceneInstance
    {
        public Scene BaseReference { get; set; }
        public InstanceCollection Instances { get; set; }
        public Type ReferenceType { get; set; }
        public Coord ViewPosition = new Coord(0,0);
        public Coord ViewOrigin = new Coord(0, 0);
        public dynamic Reference
        {
            get
            {
                return Convert.ChangeType(BaseReference, ReferenceType);
            }
        }
        public void Reinstance()
        {
            BaseReference.OnDestroy(this);
            BaseReference.OnCreate(this);
        }
    }
}
