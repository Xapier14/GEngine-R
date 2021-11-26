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
        public PhysicsAttributes DefaultPhysicsAttributes { get; set; }
        public string ObjectName { get; set; }
        public bool IsActivated { get; set; }
        public bool IsOptimized { get; set; }
        public bool IsAnimated { get; set; }
        public int DefaultImageSpeed { get; set; }
        public int DefaultImageIndex { get; set; }
        public double DefaultImageAngle { get; set; }
        public double DefaultScaleX { get; set; }
        public double DefaultScaleY { get; set; }
        public bool DefaultFlipX { get; set; }
        public bool DefaultFlipY { get; set; }
        public Coord DefaultOffset { get; set; }
        public Dictionary<string, object> DefaultInstanceVariables { get; set; }

        public Instance CreateInstance(out Guid hash)
        {
            Instance newInstance = new Instance()
            {
                BaseReference = this,
                Sprite = DefaultSprite,
                ImageIndex = DefaultImageIndex,
                ImageSpeed = DefaultImageSpeed,
                ImageAngle = DefaultImageAngle,
                IsActivated = IsActivated,
                IsOptimized = IsOptimized,
                IsAnimated = IsAnimated,
                ReferenceType = Type,
                ScaleX = DefaultScaleX,
                ScaleY = DefaultScaleY,
                FlipX = DefaultFlipX,
                FlipY = DefaultFlipY,
                Offset = new Coord(DefaultOffset.X, DefaultOffset.Y),
                PhysicsAttributes = new PhysicsAttributes(DefaultPhysicsAttributes)
            };
            foreach (var pair in DefaultInstanceVariables)
            {
                newInstance.InstanceVariables.Add(pair.Key, pair.Value);
            }
            hash = newInstance.Hash;
            return newInstance;
        }

        public GameObject()
        {
            DefaultScaleX = 1;
            DefaultScaleY = 1;
            DefaultImageAngle = 0;
            DefaultImageIndex = 0;
            DefaultImageSpeed = 0;
            DefaultFlipX = false;
            DefaultFlipY = false;
            IsAnimated = true;
            IsActivated = true;
            DefaultPhysicsAttributes = new PhysicsAttributes();
            DefaultInstanceVariables = new();
        }

        public virtual void OnCreate(Instance caller, SceneInstance scene)
        {

        }

        public virtual void Step(Instance caller, SceneInstance scene)
        {
            //constraints
            if (caller.ImageAngle >= 360) caller.ImageAngle -= 360;
            if (caller.ImageAngle < 0) caller.ImageAngle += 360;
            var viewPos = scene.ViewPosition - scene.ViewOrigin;
            var viewSize = scene.BaseReference.SceneSize;
            if (caller.IsOptimized)
            {
                int x = caller.Position.X;
                int y = caller.Position.Y;
                int sW = caller.Sprite != null ? caller.Sprite.SpriteSize.W : 0;
                int sH = caller.Sprite != null ? caller.Sprite.SpriteSize.H : 0;

                int left = x - sW;
                int right = x + sW;
                int top = y - sH;
                int bottom = y + sH;

                if (left >= viewPos.X && right <= viewPos.X + viewSize.W &&
                    top >= viewPos.Y && bottom <= viewPos.Y + viewSize.H)
                {
                    caller.IsActivated = true;
                } else
                {
                    caller.IsActivated = false;
                }
            }
        }

        public virtual void OnDestroy(Instance caller, SceneInstance scene)
        {

        }

        public virtual void OnDraw(Instance caller, SceneInstance scene, GraphicsEngine graphics)
        {

            graphics.DrawSprite(caller.Sprite, caller.Position, caller.ImageAngle, caller.ScaleX, caller.ScaleY, caller.ImageIndex, caller.Offset.X, caller.Offset.Y, scene.ViewPosition.X - scene.ViewOrigin.X, scene.ViewPosition.Y - scene.ViewOrigin.Y, caller.FlipX, caller.FlipY);
        }
        public Type Type { get; set; }
    }
    public class Instance
    {
        public TextureResource Sprite { get; set; }
        public PhysicsVariables PhysicsVariables { get; set; }
        public PhysicsAttributes PhysicsAttributes { get; set; }
        public GameObject BaseReference { get; set; }
        public dynamic Reference
        {
            get
            {
                return Convert.ChangeType(BaseReference, ReferenceType);
            }
        }
        public Coord Position;
        private int _imageIndex;
        public int ImageIndex { get
            {
                return _imageIndex;
            }
            set
            {
                if (value < 0) throw new EngineException("ImageIndex cannot be lesser than 0.");
                if (value != 0 && Sprite != null)
                {
                    if (value >= Sprite.Count) throw new EngineException("ImageIndex is out-of-range.");
                }
                _imageIndex = value;
            }
        }
        public int ImageSpeed { get; set; }
        public int Depth { get; set; }
        public double ImageAngle { get; set; }
        public bool IsActivated { get; set; }
        public bool IsOptimized { get; set; }
        public bool IsAnimated { get; set; }
        public Guid Hash { get; private set; }
        public Type ReferenceType { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public Coord Offset { get; set; }
        private int _currentImageSpeed;
        public Dictionary<string, object> InstanceVariables { get; set; }

        public Instance()
        {
            Hash = Guid.NewGuid();
            PhysicsVariables = new PhysicsVariables();
            Position = new Coord(0, 0);
            Depth = 0;
            _imageIndex = 0;
            _currentImageSpeed = 0;
            InstanceVariables = new();
        }

        public void AnimationStep()
        {
            if (_currentImageSpeed >= ImageSpeed)
            {
                _currentImageSpeed = 0;
                if (Sprite == null) return;
                if (ImageIndex >= Sprite.Count - 1)
                {
                    ImageIndex = 0;
                } else
                {
                    ImageIndex++;
                }
            } else
            {
                _currentImageSpeed++;
            }
        }
        
        public object this[string variableName]
        {
            get
            {
                if (InstanceVariables.TryGetValue(variableName, out object ret)){
                    return ret;
                } else {
                    return null;
                }
            }
            set
            {
                if (InstanceVariables.ContainsKey(variableName))
                    InstanceVariables.Remove(variableName);
                InstanceVariables.Add(variableName, value);
            }
        }

        public T Get<T>(string variableName)
        {
            if (InstanceVariables.TryGetValue(variableName, out object ret))
            {
                return (T)ret;
            }
            else
            {
                return default;
            }
        }
        public object Get(string variableName)
        {
            if (InstanceVariables.TryGetValue(variableName, out object ret))
            {
                return ret;
            }
            else
            {
                return null;
            }
        }

        public void Set<T>(string variableName, T value)
        {
            if (InstanceVariables.ContainsKey(variableName))
                InstanceVariables.Remove(variableName);
            InstanceVariables.Add(variableName, value);
        }
        public void Set(string variableName, object value)
        {
            if (InstanceVariables.ContainsKey(variableName))
                InstanceVariables.Remove(variableName);
            InstanceVariables.Add(variableName, value);
        }

        public override string ToString()
        {
            return BaseReference.ObjectName + ": " + Hash;
        }
    }
}
