using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine.Game;

namespace GEngine.Engine
{
    public  class SceneManager
    {
        private Dictionary<string, Scene> _Scenes;

        public SceneManager()
        {
            _Scenes = new Dictionary<string, Scene>();
        }

        public void AddScene(string name, Scene scene)
        {
            if (!_Scenes.ContainsKey(name))
            {
                _Scenes.Add(name, scene);
            } else
            {
                throw new EngineException("A scene with the same name already exists in the SceneManager.",
                                          "SceneManager.AddScene(string, Scene)");
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
                                          "SceneManager.GetScene(string)");
            }
        }
        public void RemoveScene(string name)
        {
            if (_Scenes.ContainsKey(name))
            {
                _Scenes.Remove(name);
            } else
            {
                throw new EngineException("A scene with the name specified does not exist in the SceneManager.",
                                          "SceneManager.RemoveScene(string)");
            }
        }
        public bool HasScene(string name)
        {
            return _Scenes.ContainsKey(name);
        }
    }
}
