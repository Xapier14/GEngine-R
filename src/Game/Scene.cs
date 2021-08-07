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
            if (GameObjects.Count > 10000)
            {
                Debug.Log($"Warning! Scene '{Name}' has too many GameObjects! (Exceeds 10,000. Count: {GameObjects.Count})");
            }
            foreach(var obj in GameObjects)
            {
                Instance inst = obj.GameObject.CreateInstance(out Guid guid);
                inst.Position = new Coord(obj.Position.X, obj.Position.Y);
                instance.Instances.Add(inst);
                inst.BaseReference.OnCreate(inst, instance);
            }
            instance.BaseReference.OnCreate(instance);
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
        public bool Destroyed { get; set; }

        public SceneInstance()
        {
            Destroyed = false;
            ViewPosition = new Coord(0, 0);
            ViewOrigin = new Coord(0, 0);
            Instances = new InstanceCollection();
        }
        public void AddArray(GameObject obj, Coord start, Coord offset, int repetitions)
        {
            if (repetitions < 1) return;
            Coord pos = new(start.X, start.Y);
            for (int i = 0; i < repetitions; i++)
            {
                Instance inst = obj.CreateInstance(out Guid hash);
                inst.Position.X = pos.X;
                inst.Position.Y = pos.Y;
                Instances.Add(inst);
                pos += offset;
            }
        }
        internal void _initPhysics()
        {
            if (_initializedPhysics) return;
            _initializedPhysics = true;
            PhysicsWorld = new PhysicsWorld(BaseReference.WorldGravity);
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

        public void SetInstancePosition(Instance instance, Coord position)
        {
            if (!Instances.Contains(instance))
                throw new EngineException("Instance not exists in SceneInstance.", "SceneInstance.SetInstancePosition()");
            instance.Position = position;
            PhysicsWorld?.SetObjectPosition(instance);
        }
    }
}
