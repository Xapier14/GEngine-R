using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SDL2.SDL;

using GEngine.Game;

namespace GEngine.Engine
{
    public class EngineProperties
    {
        public int TargetTPS { get; set; }
        public int TargetFPS { get; set; }
        public bool EnableFramelimiter { get; set; }
        public string Title { get; set; }

        public bool FloorValues { get; set; }

        public int TargetFrametime
        {
            get
            {
                if (FloorValues)
                {
                    return (int)Math.Floor((double)1000/(double)TargetFPS);
                } else
                {
                    return (int)Math.Ceiling((double)1000 / (double)TargetFPS);
                }
            }
        }
        
        public EngineProperties()
        {
            FloorValues = true;
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

        //Threads
        private Thread _SyncThread, _AsyncThread_L, _AsyncThread_D;
        private bool _StopThread = false;
        private bool _Aborted_S = false, _Aborted_AL = false, _Aborted_AG = false;

        //Sub-Modules
        private AudioEngine _audio;
        private GraphicsEngine _graphics;
        private InputManager _input;
        private SceneManager _scenes;

        public AudioEngine AudioEngine
        {
            get
            {
                return _audio;
            }
        }
        public GraphicsEngine GraphicsEngine
        {
            get
            {
                return _graphics;
            }
        }
        public InputManager InputManager
        {
            get
            {
                return _input;
            }
        }
        public SceneManager SceneManager
        {
            get
            {
                return _scenes;
            }
        }

        //SDL Stuff
        private IntPtr _SDL_Renderer;
        private IntPtr _SDL_Window;

        public GameEngine(EngineMode mode = EngineMode.Synchronous)
        {
            Properties = new EngineProperties();
            _audio = new AudioEngine();
            _graphics = new GraphicsEngine();
            _input = new InputManager();
        }
        public void Start()
        {
            switch (Mode)
            {
                case EngineMode.Synchronous:
                    _SyncThread = new Thread(new ThreadStart(Sync_Loop));
                    _SyncThread.Start();
                    break;
                case EngineMode.Asynchronous:

                    break;
                default:
                    //unknown mode
                    throw new EngineException("Unknown engine mode.", "GameEngine.Start()");
            }
        }
        public void Stop()
        {
            _StopThread = true;
            Thread.Sleep(10);
            switch (Mode)
            {
                case EngineMode.Synchronous:
                    while (!_Aborted_S) Thread.Sleep(10);
                    break;
                case EngineMode.Asynchronous:
                    while (!_Aborted_AL || !_Aborted_AG) Thread.Sleep(10);
                    break;
                default:
                    //unknown mode
                    throw new EngineException("Unknown engine mode.", "GameEngine.Stop()");
            }
            SDL_Quit();
        }
        private void InitLogic()
        {
            _audio.Init();
            _input.Init();
        }
        private void InitGraphics()
        {
            _graphics.Init();
            _graphics.CreateWindow(Properties.Title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 800, 600);
            _graphics.CreateRenderer();
            _SDL_Renderer = _graphics.Renderer;
            _SDL_Window = _graphics.Window;
            _graphics.RenderClearColor = new ColorRGBA(120, 180, 230);
        }
        private void LogicStep()
        {
            _input.PollEvent();
            SDL_RenderPresent(_SDL_Renderer);
            Thread.Sleep(10);
        }
        private void DrawStep()
        {
            _graphics.RenderClear();
        }
        private void Sync_Loop()
        {
            //Init
            InitLogic();
            InitGraphics();
            do
            {
                LogicStep();
                DrawStep();
            } while (!_StopThread);
            _Aborted_S = true;
        }
        private void Async_LogicLoop()
        {
            //Init
            InitLogic();
            do
            {
                LogicStep();
            } while (!_StopThread);
        }
        private void Async_DrawLoop()
        {
            //Init
            InitGraphics();
            do
            {
                DrawStep();
            } while (!_StopThread);
        }
    }
}
