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
        public double TPSOffset { get; set; }
        public double FPSOffset { get; set; }
        public bool EnableFramelimiter { get; set; }
        public string Title { get; set; }

        public bool FloorValues { get; set; }

        public uint TargetFrametime
        {
            get
            {
                if (FloorValues)
                {
                    return (uint)Math.Floor((double)1000/(double)TargetFPS);
                } else
                {
                    return (uint)Math.Ceiling((double)1000 / (double)TargetFPS);
                }
            }
        }

        public uint TargetLogictime
        {
            get
            {
                if (FloorValues)
                {
                    return (uint)Math.Floor((double)1000 / (double)TargetTPS);
                }
                else
                {
                    return (uint)Math.Ceiling((double)1000 / (double)TargetTPS);
                }
            }
        }

        public EngineProperties()
        {
            FloorValues = true;
            TargetTPS = 64;
            TargetFPS = 60;
            TPSOffset = 0;
            FPSOffset = 0;
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

        //Window Stuff
        public Size WindowSize
        {
            get
            {
                int w, h;
                SDL_GetWindowSize(_SDL_Window, out w, out h);
                return new Size(w, h);
            }
        }

        //Stats
        private uint _cur_frametime = 0, _cur_logictime = 0, _cur_totaltime = 0;
        private const uint _timeMargin = 3;
        private double _fps = 0, _tps = 0;
        public uint CurrentFrametime
        {
            get
            {
                return _cur_frametime;
            }
        }
        public uint CurrentLogictime
        {
            get
            {
                return _cur_logictime;
            }
        }
        public uint TotalTime
        {
            get
            {
                return _cur_totaltime;
            }
        }
        public bool PoorFramerate
        {
            get
            {
                return CurrentFrametime > (Properties.TargetFrametime + _timeMargin);
            }
        }
        public bool PoorLogicrate
        {
            get
            {
                return CurrentLogictime > (Properties.TargetLogictime + _timeMargin);
            }
        }
        public double FPS
        {
            get
            {
                return _fps;
            }
        }
        public double TPS
        {
            get
            {
                return _tps;
            }
        }

        public GameEngine(EngineMode mode = EngineMode.Synchronous)
        {
            Properties = new EngineProperties();
            _audio = new AudioEngine();
            _graphics = new GraphicsEngine();
            _input = new InputManager();
            _scenes = new SceneManager();
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
            SDL_Delay(1);
            string s = SDL_GetError();
            if (s != "")
            {
                Stop();
                throw new EngineException("Unexpected SDL Error occured, engine halted.", "GameEngine.LogicStep()");
            }
        }
        private void DrawStep()
        {
            _graphics.RenderClear();
            SDL_Delay(1);
            SDL_RenderPresent(_SDL_Renderer);
        }
        private void Sync_Loop()
        {
            //Init
            InitLogic();
            InitGraphics();
            uint c_tick = 0, cl_tick = 0;
            uint l_elapsed = Properties.TargetLogictime + 1;
            uint d_elapsed = Properties.TargetFrametime + 1;
            uint l_time = 0;
            uint d_time = 0;
            uint loop_elapsed = 0;
            const uint delay_time = 1;
            Sampler fpsAvg = new Sampler(5000);
            Sampler tpsAvg = new Sampler(5000);
            do
            {
                loop_elapsed = SDL_GetTicks();
                c_tick = SDL_GetTicks();
                if (l_elapsed > (Properties.TargetLogictime - Properties.TPSOffset - delay_time))
                {
                    l_time = l_elapsed;
                    l_elapsed = SDL_GetTicks();
                    LogicStep();
                    l_elapsed = SDL_GetTicks() - l_elapsed;
                } else
                {
                    SDL_Delay(delay_time);
                    l_elapsed += SDL_GetTicks() - c_tick;
                }
                cl_tick = SDL_GetTicks();
                if (d_elapsed > (Properties.TargetFrametime - Properties.FPSOffset - delay_time) || !Properties.EnableFramelimiter)
                {
                    d_time = d_elapsed;
                    d_elapsed = SDL_GetTicks();
                    DrawStep();
                    d_elapsed = SDL_GetTicks() - d_elapsed;
                } else
                {
                    SDL_Delay(delay_time);
                    d_elapsed += SDL_GetTicks() - cl_tick;
                }
                loop_elapsed = SDL_GetTicks() - loop_elapsed;
                _cur_logictime = l_time;
                _cur_frametime = d_time;
                _cur_totaltime = loop_elapsed;
                //Console.WriteLine("[Debug] Dt: {0}, Lt: {1}", d_elapsed, l_elapsed);
                fpsAvg.AddPoint(1000.00 / (double)d_time);
                tpsAvg.AddPoint(1000.00 / (double)l_time);
                _fps = fpsAvg.GetAverage();
                _tps = tpsAvg.GetAverage();
                Console.WriteLine("[Debug] F: {0}ms, L: {1}ms, T: {2}ms - FPS: {3}, TPS: {4}", d_time, l_time, loop_elapsed, Math.Round(fpsAvg.GetAverage(), 2), Math.Round(tpsAvg.GetAverage(), 2));
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
