using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SDL2.SDL;

using GEngine.Game;
using System.Diagnostics;

namespace GEngine.Engine
{
    public class EngineProperties
    {
        public double TargetTPS { get; set; }
        public double TargetFPS { get; set; }
        public double TPSOffset { get; set; }
        public double FPSOffset { get; set; }
        public bool EnableFramelimiter { get; set; }
        public string Title { get; set; }
        public double TargetFrametime
        {
            get
            {
                return 1000.00 / TargetFPS;
            }
        }
        public double TargetLogictime
        {
            get
            {
                return 1000.00 / TargetTPS;
            }
        }

        public EngineProperties()
        {
            TargetTPS = 64;
            TargetFPS = 60;
            TPSOffset = 0;
            FPSOffset = 0;
            EnableFramelimiter = true;
            Title = "GEngine | Re:";
        }
    }

    public class GameEngineEventArgs : EventArgs
    {
        public GameEngineEventType EventType { get; set; }
    }

    public class GameEngine
    {
        //Engine Props
        public EngineMode Mode { get; set; }
        public EngineProperties Properties { get; set; }

        //Threads
        private Thread _SyncThread, _AsyncThread_L, _AsyncThread_D;
        private bool _Started = false;
        private bool _StopThread = false, _ForcedThread = false;
        private bool _Aborted_S = false, _Aborted_AL = false, _Aborted_AG = false;

        //Sub-Modules
        private AudioEngine _audio;
        private GraphicsEngine _graphics;
        private InputManager _input;
        private SceneManager _scenes;
        private ResourceManager _resource;

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
        public ResourceManager ResourceManager
        {
            get
            {
                return _resource;
            }
        }

        //SDL Stuff
        private IntPtr _SDL_Renderer;
        private IntPtr _SDL_Window;

        //init stuff
        private bool _initLogic = false, _initGraphics = false;
        private byte test = 0;
        private bool rev = false;

        //Window Stuff
        private bool _allowClose = true, _handleClose = false;
        public Size WindowSize
        {
            get
            {
                int w, h;
                SDL_GetWindowSize(_SDL_Window, out w, out h);
                return new Size(w, h);
            }
        }
        public bool AllowClose
        {
            get
            {
                return _allowClose;
            }
            set
            {
                _allowClose = value;
            }
        }
        public bool HandleClose
        {
            get
            {
                return _handleClose;
            }
            set
            {
                _handleClose = value;
            }
        }

        //Stats
        private double _cur_frametime = 0, _cur_logictime = 0, _cur_totaltime = 0;
        private const uint _timeMargin = 3;
        private double _fps = 0, _tps = 0;
        public double CurrentFrametime
        {
            get
            {
                return _cur_frametime;
            }
        }
        public double CurrentLogictime
        {
            get
            {
                return _cur_logictime;
            }
        }
        public double TotalTime
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
        public bool Running
        {
            get
            {
                return !(!_Started || _StopThread);
            }
        }

        //Special Props
        public bool LogicPause { get; set; }
        public bool DrawPause { get; set; }

        //Events
        public delegate void GameEngineEventHandler(GameEngineEventArgs eventArgs);
        public event GameEngineEventHandler OnWindowClose;

        public GameEngine(EngineMode mode = EngineMode.Synchronous, VideoBackend backend = VideoBackend.Auto)
        {
            Properties = new EngineProperties();
            _resource = new ResourceManager(_SDL_Renderer); //I don't know if this would work.
            _audio = new AudioEngine(_resource);
            _graphics = new GraphicsEngine(backend);
            _input = new InputManager();
            _scenes = new SceneManager();

            _input.WindowEvent += InputHandler_WindowEvent;
        }

        private void InputHandler_WindowEvent(InputCallbackEventArg eventArg)
        {
            switch (eventArg.CallbackType)
            {
                case InputCallbackType.WindowClose:
                    if (_allowClose)
                        if (_handleClose)
                        {
                            ForceStop();
                        }
                        else
                        {
                            OnWindowClose?.Invoke(new GameEngineEventArgs()
                            {
                                EventType = GameEngineEventType.WindowClose
                            });
                        }
                    break;
            }
        }

        public void Start()
        {
            switch (Mode)
            {
                case EngineMode.Synchronous:
                    _Started = true;
                    _SyncThread = new Thread(new ThreadStart(Sync_Loop));
                    _SyncThread.Start();
                    break;
                case EngineMode.Asynchronous:
                    _Started = true;

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
            if (!_ForcedThread)
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
            Debug.Log("GameEngine.Stop()", "Engine stopped.");
        }
        private void ForceStop()
        {
            _StopThread = true;
            _ForcedThread = true;
            Thread.Sleep(10);
            SDL_Quit();
            Debug.Log("GameEngine.ForceStop()", "Engine forcibly stopped.");
        }
        private void InitLogic()
        {
            if (!_initLogic)
            {
                _audio.Init();
                _input.Init();
                _initLogic = true;
            }
        }
        private void InitGraphics()
        {
            if (!_initGraphics)
            {
                _graphics.Init();
                _graphics.CreateWindow(Properties.Title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 800, 600);
                _graphics.CreateRenderer();
                _SDL_Renderer = _graphics.Renderer;
                _SDL_Window = _graphics.Window;
                _graphics.RenderClearColor = new ColorRGBA(120, 180, 230);
                _initGraphics = true;
            }
        }
        private void LogicStep()
        {
            _input.PollEvent();
            if (!LogicPause)
            {
                //Do stuff here
                if (test == 0)
                {
                    rev = false;
                } else if (test == 255)
                {
                    rev = true;
                }
                if (rev)
                {
                    test--;
                } else
                {
                    test++;
                }
                //Console.WriteLine(test);

                string s = SDL_GetError();
                if (s != "" && !_StopThread)
                {
                    Stop();
                    throw new EngineException("Unexpected SDL Error occured, engine halted.", "GameEngine.LogicStep()");
                }
            }
        }
        private void DrawStep()
        {
            if (!DrawPause)
            {
                //_graphics.RenderClear();

                //SDL_RenderPresent(_SDL_Renderer);
            }
        }
        private void Sync_Loop()
        {
            //Init
            InitLogic();
            InitGraphics();
            double c_tick = 0, cl_tick = 0;
            double l_elapsed = Properties.TargetLogictime + 1;
            double d_elapsed = Properties.TargetFrametime + 1;
            double l_time = 0;
            double d_time = 0;
            double loop_elapsed = 0;
            const double delay_time = 1;
            Sampler fpsAvg = new Sampler(5000);
            Sampler tpsAvg = new Sampler(5000);
            SDL_FRect rectangle = new SDL_FRect();
            rectangle.x = 16;
            rectangle.y = 16;
            rectangle.w = 300;
            rectangle.h = 200;
            _graphics.SetRenderDrawColor(new ColorRGBA(test, test, test));
            do
            {
                loop_elapsed = GetPreciseMs();
                c_tick = GetPreciseMs();
                cl_tick = GetPreciseMs();
                if (l_elapsed >= (Properties.TargetLogictime - Properties.TPSOffset))
                {
                    l_time = l_elapsed;
                    //while (LogicPause) SDL_Delay(10);
                    l_elapsed = GetPreciseMs();
                    LogicStep();
                    l_elapsed = GetPreciseMs() - l_elapsed;
                } else
                {
                    //PreciseWait(delay_time);
                    SDL_Delay(1);
                    l_elapsed += GetPreciseMs() - c_tick;
                }
                if (d_elapsed >= (Properties.TargetFrametime - Properties.FPSOffset) || !Properties.EnableFramelimiter)
                {
                    d_time = d_elapsed;
                    d_elapsed = GetPreciseMs();
                    //DrawStep();
                    SDL_RenderClear(_SDL_Renderer);

                    _graphics.SetRenderDrawColor(new ColorRGBA(test, test, test));
                    SDL_RenderDrawRectF(_SDL_Renderer, ref rectangle);

                    SDL_RenderPresent(_SDL_Renderer);

                    d_elapsed = GetPreciseMs() - d_elapsed;
                } else
                {
                    //PreciseWait(delay_time);
                    SDL_Delay(1);
                    d_elapsed += GetPreciseMs() - cl_tick;
                }
                //Console.WriteLine("[Debug] F: {0}ms({5}ms), L: {1}ms({6}ms), T: {2}ms - FPS: {3}, TPS: {4}", Math.Round(d_time, 2), Math.Round(l_time, 2), Math.Round(loop_elapsed, 2), Math.Round(fpsAvg.GetAverage(), 2), Math.Round(tpsAvg.GetAverage(), 2), Math.Round(Properties.TargetFrametime, 2), Math.Round(Properties.TargetLogictime, 2));
                loop_elapsed = GetPreciseMs() - loop_elapsed;
                _cur_logictime = l_time;
                _cur_frametime = d_time;
                _cur_totaltime = loop_elapsed;
                fpsAvg.AddPoint(1000.00 / (double)d_time);
                tpsAvg.AddPoint(1000.00 / (double)l_time);
                _fps = fpsAvg.GetAverage();
                _tps = tpsAvg.GetAverage();
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
        private static double TicksToMs(long nano)
        {
            return (double)((decimal)nano / (decimal)10000.0000);
        }
        private static double GetPreciseMs()
        {
            return TicksToMs(DateTime.Now.Ticks);
        }
        private static void PreciseWait(double ms)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var match = Math.Round((ms/1000) * Stopwatch.Frequency);
            while (sw.ElapsedTicks < match) { }
        }
    }
}
