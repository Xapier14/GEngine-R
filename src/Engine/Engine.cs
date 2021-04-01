using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace GEngine.Engine
{
    public class EngineProperties
    {
        public int TargetTPS { get; set; }
        public int TargetFPS { get; set; }
        public bool EnableFramelimiter { get; set; }
        public string Title { get; set; }
        
        public EngineProperties()
        {
            TargetTPS = 64;
            TargetFPS = 60;
            EnableFramelimiter = true;
            Title = "GEngine | Re:";
        }
    }
    public class GameEngine
    {
        //Engine Props
        public EngineMode Mode { get; set; }
        public EngineProperties Properties { get; set; }

        //Sub-Modules
        private Scene CurrentScene { get; set; }
        private AudioEngine _audio;
        public AudioEngine AudioEngine
        {
            get
            {
                return _audio;
            }
        }

        //SDL Stuff
        private IntPtr _SDL_Renderer;
        private IntPtr _SDL_Window;

        public GameEngine(EngineMode mode = EngineMode.Synchronous)
        {
            Properties = new EngineProperties();
        }
        public void Init()
        {
            if (SDL_Init(SDL_INIT_EVERYTHING) != 0)
            {
                //Init Error
                Debug.Log("Init()", "Cannot initialize SDL.");
                Debug.Log("Init()", $"Error: {SDL_GetError()}");
                throw new EngineException("Cannot initialize SDL.", "GameEngine.Init()");
            } else
            {
                //Init Success
                Debug.Log("Init()", "Initialized SDL.");
            }
        }
        public void Start()
        {

        }
        public void Stop()
        {
            SDL_Quit();
        }
        private void LogicStep()
        {

        }
        private void DrawStep()
        {

        }
        private void Sync_Loop()
        {

        }
        private void Async_LogicLoop()
        {

        }
        private void Async_DrawLoop()
        {

        }
    }
}
