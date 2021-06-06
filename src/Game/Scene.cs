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
        public bool UsesPhysics { get; set; }
        public Coord WorldGravity { get; set; }

        public Scene(Size sceneSize, Size viewSize)
        {
            SceneSize = sceneSize;
            Properties = new SceneProperties(viewSize);
            GameObjects = new GameObjectInfoCollection();
            UsesPhysics = false;
            WorldGravity = new Coord(0, 0);
        }

        public SceneInstance CreateInstance()
        {
            SceneInstance instance = new SceneInstance();
            instance.BaseReference = this;
            instance.ReferenceType = ReferenceType;
            instance.UsesPhysics = UsesPhysics;
            instance._initPhysics();
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
            foreach (Instance inst in caller.Instances)
            {
                if (inst.IsActivated) inst.BaseReference.OnCreate(inst, caller);
            }
        }

        public virtual void Step(SceneInstance caller)
        {
            if (caller.PhysicsWorld != null && caller.UsesPhysics)
            {
                caller.PhysicsWorld.UpdateCycle();
            }
            foreach (Instance inst in caller.Instances)
            {
                if (inst.IsActivated) inst.BaseReference.Step(inst, caller);
            }
        }

        public virtual void OnDestroy(SceneInstance caller)
        {
            foreach (Instance inst in caller.Instances)
            {
                if (inst.IsActivated) inst.BaseReference.OnDestroy(inst, caller);
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
        public bool UsesPhysics { get; set; }
        public PhysicsWorld PhysicsWorld { get; set; }
        private bool _initializedPhysics = false;

        public SceneInstance()
        {
            ViewPosition = new Coord(0, 0);
            ViewOrigin = new Coord(0, 0);
            Instances = new InstanceCollection();
        }
        internal void _initPhysics()
        {
            if (_initializedPhysics) return;
            _initializedPhysics = true;
            PhysicsWorld = new PhysicsWorld(BaseReference.SceneSize, BaseReference.WorldGravity);
            Instances.OnGeneralizedEvent += Instances_OnGeneralizedEvent;
        }

        private void Instances_OnGeneralizedEvent(InstanceCollectionEventArgs eventArgs)
        {
            switch (eventArgs.Type)
            {
                case InstanceCollectionEventType.AddObject:
                    PhysicsWorld.AddObject(eventArgs.Reference);
                    break;
            }
        }

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

        public void AnimationStep()
        {
            foreach (Instance inst in Instances)
            {
                if (inst.IsAnimated) inst.AnimationStep();
            }
        }
    }
}
