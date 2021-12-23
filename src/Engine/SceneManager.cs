using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine.Game;

namespace GEngine.Engine
{
    public class SceneManager
    {
        private Dictionary<string, Scene> _Scenes;
        private Dictionary<string, SceneInstance> _ActiveScenes;
        private SceneInstance _CurrentScene;

        public string CurrentScene
        {
            get
            {
                if (_CurrentScene == null) return "";
                return _CurrentScene.BaseReference.Name;
            }
        }

        public SceneManager()
        {
            _Scenes = new Dictionary<string, Scene>();
            _ActiveScenes = new Dictionary<string, SceneInstance>();
        }

        public void Clear()
        {
            _Scenes.Clear();
            foreach (var kp in _ActiveScenes)
            {
                foreach (var inst in kp.Value.Instances)
                {
                    inst.BaseReference.OnDestroy(inst, kp.Value);
                }
                kp.Value.Destroyed = true;
                kp.Value.BaseReference.OnDestroy(kp.Value);
            }
            _ActiveScenes.Clear();
        }

        public void AddScene(Scene scene)
        {
            if (!_Scenes.ContainsKey(scene.Name))
            {
                _Scenes.Add(scene.Name, scene);
            } else
            {
                throw new EngineException("A scene with the same name already exists in the SceneManager.",
                                          "SceneManager.AddScene()");
            }
        }
        public Scene GetScene(string name)
        {
            if (_Scenes.ContainsKey(name))
            {
                return _Scenes.GetValueOrDefault(name);
            } else
            {
                throw new EngineException("A scene with the name specified does not exist in the SceneManager.",
                                          "SceneManager.GetScene()");
            }
        }
        public SceneInstance GetInstance(string name)
        {
            if (!HasInstance(name)) throw new EngineException($"Scene '{name}' does not have an instance.");
            if (name == "") return null;
            return _ActiveScenes[name];
        }
        public void RemoveScene(string name)
        {
            if (_Scenes.ContainsKey(name))
            {
                _Scenes.Remove(name);
            } else
            {
                throw new EngineException("A scene with the name specified does not exist in the SceneManager.",
                                          "SceneManager.RemoveScene()");
            }
        }
        public bool HasScene(string name)
        {
            return _Scenes.ContainsKey(name);
        }
        public void SceneStep()
        {
            if (_CurrentScene != null && !_CurrentScene.Destroyed)
            {
                _CurrentScene.BaseReference.Step(_CurrentScene);
                _CurrentScene.Instances.SortByDepth(true);
                if (_CurrentScene.UsesPhysics)
                {
                    _CurrentScene.PhysicsWorld.UpdateCycle();
                }
            }
        }
        public void SwitchToScene(string name, bool reinstance = true)
        {
            if (!HasScene(name)) throw new EngineException($"Scene '{name}' not found.", "SceneManager.SwitchToScene()");
            if (!_ActiveScenes.ContainsKey(name))
            {
                //create new instance
                SceneInstance s = _Scenes[name].CreateInstance();
                _ActiveScenes.Add(name, s);
                _CurrentScene = s;
            } else
            {
                //has active scene
                if (reinstance)
                {
                    _ActiveScenes[name].Reinstance();
                }
                _CurrentScene = _ActiveScenes[name];
            }
        }

        public void InstantiateScene(string name)
        {
            if (!_ActiveScenes.ContainsKey(name))
            {
                //create new instance
                _ActiveScenes.Add(name, _Scenes[name].CreateInstance());
            } else
            {
                //recycle
                _ActiveScenes[name].Reinstance();
            }
            _ActiveScenes[name].BaseReference.OnCreate(_ActiveScenes[name]);
        }

        public bool HasInstance(string name)
        {
            return _ActiveScenes.ContainsKey(name);
        }

        public void AnimationStep()
        {
            if (_CurrentScene == null)
            {
                //Debug.Log("SceneManager.AnimationStep()", "Cannot advance animation on a null scene instance.");
                return;
            }
            _CurrentScene.AnimationStep();
        }
    }
}
