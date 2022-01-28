using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static SDL2.SDL;

using GEngine.Game;
using System.Diagnostics;
using System.Security;
using Genbox.VelcroPhysics.Utilities;

namespace GEngine.Engine
{
    public class EngineProperties
    {
        public Size WindowResolution { get; set; }
        public Size InternalResolution { get; set; }
        public double TargetTPS { get; set; }
        public double TargetFPS { get; set; }
        public double TPSOffset { get; set; }
        public double FPSOffset { get; set; }
        public bool EnableFramelimiter { get; set; }
        public bool HideConsoleWindow { get; set; }
        public bool AllowResize { get; set; }
        public string Title { get; set; }
        public double TargetFrametime
        {
            get
            {
                return (1000.00 / TargetFPS) + FPSOffset;
            }
        }
        public double TargetLogictime
        {
            get
            {
                return (1000.00 / TargetTPS) + TPSOffset;
            }
        }
        public RenderScaleQuality RenderScaleQuality { get; set; }
        public bool TPSAnimations { get; set; }
        public bool EnableDebug { get; set; }
        public bool AutoOffset { get; set; }

        public EngineProperties()
        {
            WindowResolution = new(800, 600);
            InternalResolution = WindowResolution;
            TPSAnimations = false;
            TargetTPS = 64;
            TargetFPS = 60;
            TPSOffset = -0.15; //Play around with this to time the game speed.
            FPSOffset = -0.02; //Play around with this to time the fps. (Try: -0.02 & -0.15)
            EnableFramelimiter = true;
            HideConsoleWindow = true;
            Title = "GEngine | Re:";
            RenderScaleQuality = RenderScaleQuality.Nearest;
            EnableDebug = false;
            AllowResize = false;
            AutoOffset = true;
        }
    }

    public class GameEngineEventArgs : EventArgs
    {
        public GameEngineEventType EventType { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    //[SuppressUnmanagedCodeSecurity]
    public class GameEngine
    {
        //Engine Props
        public EngineMode Mode { get; set; }
        public EngineProperties Properties { get; set; }
        private VideoBackend _vBackend { get; set; }
        public bool ResourcesLoaded { get; set; }

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
        private AutoOffset _offsets;

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
        //private byte test = 0;
        //private bool rev = false;

        //Window Stuff
        private bool _allowClose = true, _handleClose = false, _handleResize = false;
        private bool _showDebug = false;
        public Size WindowSize
        {
            get
            {
                SDL_GetWindowSize(_SDL_Window, out int w, out int h);
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

        public bool HandleResize
        {
            get
            {
                return _handleResize;
            }
            set
            {
                _handleResize = value;
            }
        }

        //OpenGL fix
        private bool _rebuilt = false;
        private double _rebuildOnCall
        {
            get
            {
                return Math.Ceiling(Properties.TargetFPS / 10); //Rebuild after 1/32 fps
            }
        }
        private int _rebuildCurrentCall = 0;

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
        public event GameEngineEventHandler OnWindowResize;
        public event GameEngineEventHandler OnInfoMsg;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public GameEngine(EngineMode mode = EngineMode.Synchronous, VideoBackend backend = VideoBackend.Auto, float baseUnit = 8f)
        {
            SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
            SDL_SetHint(SDL_HINT_RENDER_BATCHING, "1");
            ConvertUnits.SetDisplayUnitToSimUnitRatio(baseUnit);
            ResourcesLoaded = false;
            Properties = new EngineProperties();
            _resource = new ResourceManager();
            _audio = new AudioEngine(_resource);

            if (!IsRenderDriverAvailable(backend))
            {
                Debug.Log("GameEngine.GameEngine()", $"Render Driver '{BackendToString(backend)}' is not available. Switched to software fallback.");
                backend = VideoBackend.Software;
            }

            _graphics = new GraphicsEngine(backend);
            _input = new InputManager();
            _scenes = new SceneManager();

            _vBackend = backend;

            _input.WindowEvent += InputHandler_WindowEvent;
            _input.EngineEvent += InputHandler_EngineEvent;
        }

        private void InformMessage(string msg)
        {
            OnInfoMsg?.Invoke(new GameEngineEventArgs()
            {
                EventType = GameEngineEventType.Information,
                Message = msg
            });
        }

        public string ShowMessageBox(string title, string message, params string[] buttons)
        {
            SDL_MessageBoxData msgBox = new();
            msgBox.message = message;
            msgBox.title = title;
            msgBox.numbuttons = buttons.Length;
            msgBox.window = _SDL_Window;
            if (buttons.Length < 1)
            {
                msgBox.numbuttons = 1;
            }
            SDL_MessageBoxButtonData[] bData = new SDL_MessageBoxButtonData[msgBox.numbuttons];
            if (buttons.Length < 1)
            {
                bData[0] = new SDL_MessageBoxButtonData()
                {
                    buttonid = 1,
                    flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
                    text = "Ok"
                };
            } else
            {
                for (int i = 0; i < buttons.Length; ++i)
                {
                    bData[i] = new SDL_MessageBoxButtonData()
                    {
                        buttonid = i,
                        text = buttons[i]
                    };
                }
            }
            msgBox.buttons = bData;

            if (SDL_ShowMessageBox(ref msgBox, out int buttonId) != 0)
            {
                Debug.Log("Could not show message box!");
                return "";
            } else
            {
                if (buttonId >= 0 && buttonId < buttons.Length)
                {
                    return buttons[buttonId];
                } else
                {
                    return "";
                }
            }

        }

        public string BackendToString(VideoBackend backend)
        {
            return backend switch
            {
                VideoBackend.Direct3D => "direct3d",
                VideoBackend.OpenGL => "opengl",
                VideoBackend.OpenGL_ES => "opengles",
                VideoBackend.OpenGL_ES2 => "opengles2",
                VideoBackend.Metal => "metal",
                VideoBackend.Software => "software",
                VideoBackend.Auto => "auto",
                _ => "n/a",
            };
        }

        private string ParseRenderScale(RenderScaleQuality quality)
        {
            return quality switch
            {
                RenderScaleQuality.Nearest => "0",
                RenderScaleQuality.Linear => "1",
                RenderScaleQuality.Anisotropic => "2",
                _ => "0",
            };
        }

        public VideoBackend StringToBackend(string backend)
        {
            return backend.ToLower() switch
            {
                "direct3d" => VideoBackend.Direct3D,
                "opengl" => VideoBackend.OpenGL,
                "opengles" => VideoBackend.OpenGL_ES,
                "opengles2" => VideoBackend.OpenGL_ES2,
                "metal" => VideoBackend.Metal,
                "software" => VideoBackend.Software,
                "auto" => VideoBackend.Auto,
                _ => VideoBackend.Auto,
            };
        }

        public bool IsRenderDriverAvailable(VideoBackend backend)
        {
            if (backend == VideoBackend.Auto) return true;
            string[] drivers = GetAvailableRenderDrivers();
            for (int i = 0; i < drivers.Length; ++i)
                if (drivers[i] == BackendToString(backend)) return true;
            return false;
        }

        public string[] GetAvailableRenderDrivers()
        {
            string[] ret = new string[SDL_GetNumRenderDrivers()];
            for (int i = 0; i < ret.Length; ++i)
            {
                SDL_GetRenderDriverInfo(i, out SDL_RendererInfo info);
                ret[i] = UTF8_ToManaged(info.name);
            }
            return ret;
        }

        private void InputHandler_EngineEvent(InputCallbackEventArg eventArg)
        {
            switch (eventArg.CallbackType)
            {
                case InputCallbackType.RenderDeviceReset:
                    _resource.RebuildTextures();
                    break;
            }
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
                case InputCallbackType.FocusGained:
                    if (_vBackend == VideoBackend.OpenGL || _vBackend == VideoBackend.OpenGL_ES || _vBackend == VideoBackend.OpenGL_ES2)
                    {
                        //_resource.RebuildTextures();
                    }
                    break;
                case InputCallbackType.WindowSizeChanged:
                    if (_handleResize)
                    {
                        Size newSize = (Size)eventArg.Data;
                        _graphics.SetInternalResolution(newSize.W, newSize.H);
                    }
                    else
                    {
                        OnWindowResize?.Invoke(new GameEngineEventArgs()
                        {
                            EventType = GameEngineEventType.WindowResize,
                            Data = eventArg.Data
                        });
                    }
                    break;
            }
        }

        public void LoadConfig(string configFileLocation)
        {
            if (_Started) throw new EngineException("Cannot load config when engine has already started.", "GameEngine.LoadConfig()");
            if (Loader.TryParseIni(configFileLocation, out Dictionary<string, string> config))
            {
                foreach (KeyValuePair<string, string> pair in config)
                {

                    try
                    {
                        switch (pair.Key.ToLower())
                        {
                            default:
                                Debug.Log("GameEngine.LoadConfig()", $"Key '{pair.Key}' not valid.");
                                break;
                            case "target_fps":
                                Properties.TargetFPS = double.Parse(pair.Value);
                                break;
                            case "framelimiter":
                                Properties.EnableFramelimiter = bool.Parse(pair.Value);
                                break;
                            case "video_backend":
                                _graphics.SetVideoBackend(StringToBackend(pair.Value));
                                break;
                                    //add config values here.
                        }

                    }
                    catch
                    {
                        Debug.Log("GameEngine.LoadConfig()", $"Invalid value '{pair.Value}' for key '{pair.Key}'");
                        continue;
                    }
                }
            } else
            {
                Debug.Log("GameEngine.LoadConfig()", "No configuration file found.");
            }
        }

        public void Start()
        {
            if (Properties.HideConsoleWindow)
            {
                ShowWindow(GetConsoleWindow(), 0);
            } else
            {
                ShowWindow(GetConsoleWindow(), 5);
            }
            _StopThread = false;
            ResourcesLoaded = false;
            _SDL_Renderer = IntPtr.Zero;
            _SDL_Window = IntPtr.Zero;
            switch (Mode)
            {
                case EngineMode.Synchronous:
                    _Started = true;
                    _SyncThread = new Thread(new ThreadStart(Sync_Loop));
                    _SyncThread.Name = "Synchronous Game Thread " + _SyncThread.GetHashCode().ToString();
                    _SyncThread.Start();
                    break;
                case EngineMode.Asynchronous:
                    _Started = true;

                    break;
                default:
                    //unknown mode
                    throw new EngineException("Unknown engine mode.", "GameEngine.Start()");
            }
            while (_SDL_Renderer == IntPtr.Zero || _SDL_Window == IntPtr.Zero) Thread.Sleep(10);
            _resource.SetRenderer(_graphics.Renderer);
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
            FreeResources();
            SDL_Quit();
            Debug.Log("GameEngine.Stop()", "Engine stopped.");
        }
        private void FreeResources()
        {
            _scenes.Clear();
            SDL_DestroyRenderer(_SDL_Renderer);
            SDL_DestroyWindow(_SDL_Window);
            _resource.Quit();
        }
        public void ForceStop()
        {
            _StopThread = true;
            _ForcedThread = true;
            Thread.Sleep(10);
            FreeResources();
            SDL_Quit();
            _scenes.Clear();
            Debug.Log("GameEngine.ForceStop()", "Engine forcibly stopped.");
        }
        private void InitLogic()
        {
            if (!_initLogic)
            {
                //_resource.Init(IntPtr.Zero);
                _audio.Init();
                _input.Init();
                _initLogic = true;
                _offsets = new(this);
            }
        }
        private void InitGraphics()
        {
            if (!_initGraphics)
            {
                _graphics.Init();
                _graphics.SetResize(Properties.AllowResize);
                _graphics.CreateWindowAndRenderer(Properties.Title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, Properties.WindowResolution.W, Properties.WindowResolution.H, out _SDL_Window, out _SDL_Renderer);
                //_graphics.CreateWindow(Properties.Title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 800, 600);
                //_graphics.CreateRenderer();
                //SDL_Delay(2000);
                //_SDL_Renderer = _graphics.Renderer;
                //_SDL_Window = _graphics.Window;
                SDL_RenderSetLogicalSize(_SDL_Renderer, Properties.InternalResolution.W, Properties.InternalResolution.H);
                _graphics.RenderClearColor = new ColorRGBA(120, 180, 230);
                _resource.Init(_SDL_Renderer);
                _resource.EngineInit = false;
                _initGraphics = true;
                //_resource.RebuildTextures();
            }
        }
        private void LogicStep()
        {
            double start = GetPreciseMs();
            _input.PollEvent();
            if (!LogicPause)
            {
                //Do stuff here
                /*
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
                */

                if (_input.IsPressed(SDL_Keycode.SDLK_F8) && Properties.EnableDebug)
                    _showDebug = !_showDebug;

                try
                {
                    _scenes.SceneStep();
                    if (Properties.TPSAnimations)
                        _scenes.AnimationStep();
                } catch (EngineException ex)
                {
                    Debug.Log($"[ErrorHandler] Engine error occurred!\n" +
                              $"           [*] Reason: {ex.Message}");
                    string button = ShowMessageBox("Game error caught",
                                                  $"Logic Step:\n" +
                                                  $"{(ex.SourceFile != "" ? $"{ex.SourceFile}\n" : "")}{ex.Message}\n" +
                                                  $"{ex.StackTrace}\n\n" +
                                                  $"Code: {ex.HResult}",
                                                  "Abort", "Continue");
                    if (button != "Continue")
                        Environment.Exit(ex.HResult);
                } catch (Exception ex)
                {
                    Debug.Log($"[ErrorHandler] Critical engine error occurred!\n" +
                              $"           [*] Reason: {ex.Message}");
                    ShowMessageBox("Engine error caught",
                                  $"Logic Step:\n" +
                                  $"{ex.Message}\n" +
                                  $"{ex.StackTrace}\n\n" +
                                  $"Code: {ex.HResult}",
                                  "Abort");
                    Environment.Exit(ex.HResult);
                }

                string s = SDL_GetError();
                if (s != "" && !_StopThread && false)
                {
                    Console.WriteLine("[UE] ! - " + s);
                    ForceStop();
                    throw new EngineException("Unexpected SDL Error occured, engine halted.", "GameEngine.LogicStep()");
                }
                if (Properties.AutoOffset)
                    _offsets.AdjustOffsets();
            }
            double end = GetPreciseMs();
            if (start - end > Properties.TargetLogictime)
            {
                Debug.Log("GameEngine.LogicStep()", $"Logic step took {start-end-Properties.TargetLogictime}ms longer than target logic time.");
            }
        }
        public void DrawStep()
        {
            double start = GetPreciseMs();
            if (!DrawPause)
            {
                // Clear
                _graphics.RenderClear();
                // rebuild if texture rebuild is raised
                //_graphics.RebuildTexturesOnCall(_resource);
                // If engine is set to rebuild textures
                if (!_rebuilt)
                {
                    // And that it is due for rebuild
                    if (_rebuildCurrentCall > _rebuildOnCall)
                    {
                        // And that the backend is some form of OpenGL
                        if (_vBackend == VideoBackend.OpenGL || _vBackend == VideoBackend.OpenGL_ES || _vBackend == VideoBackend.OpenGL_ES2)
                        {
                            // Rebuild textures
                            _resource.RebuildTextures();
                            InformMessage("Textures rebuilt.");
                        }
                        _rebuilt = true;
                    } else
                    {
                        _rebuildCurrentCall++;
                    }
                }
                if (_scenes.CurrentScene != "")
                try
                {
                    SceneInstance si = _scenes.GetInstance(_scenes.CurrentScene);
                    if (si != null) _graphics.DrawScene(si);
                } catch (EngineException ex)
                {
                    Debug.Log("GameEngine.DrawStep()", $"Could not draw current scene ('{_scenes.CurrentScene}'). Reason: {ex.Message}");
                    //Debug.Log("GameEngine.DrawStep()", $"Reason: {ex.Message}");
                    //Debug.Log("SDL_ERROR-GE_DS - " + SDL_GetError());
                }

                // show debug
                try
                {
                    if (_showDebug && Properties.EnableDebug)
                    {
                        Size internalRes = Properties.InternalResolution;//_graphics.GetInternalResolution();

                        int spacing = internalRes.H / 30;
                        if (!_resource.HasFont("font_debugOverlay"))
                        {
                            _resource.LoadAsFont("Arial.ttf", "font_debugOverlay", spacing);
                        }
                        var drawColor = _graphics.GetRendererDrawColor();
                        var textColor = new ColorRGBA(255, 0, 255, 200); //20, 200, 60, 180);
                        var backColor = new ColorRGBA(50, 50, 50, 80); //20, 200, 60, 180);
                        int boxPadding = spacing/2;
                        var font = _resource.GetFontResource("font_debugOverlay");

                        int leftMargin = internalRes.W / 50;
                        int topMargin = internalRes.H / 40;
                        SDL_GetVersion(out SDL_version ver);

                        string[] lines = { $"{Info.VersionString}",
                                           $"{Properties.Title}",
                                           $"FPS: {Math.Round(FPS, 2)}/{Math.Round(Properties.TargetFPS, 2)} ({Math.Round(CurrentFrametime, 2)}ms){(FPS < Properties.TargetFPS ? " [!]" : string.Empty)}",
                                           $"TPS: {Math.Round(TPS, 2)}/{Math.Round(Properties.TargetTPS, 2)} ({Math.Round(CurrentLogictime, 2)}ms){(TPS < Properties.TargetTPS ? " [!]" : string.Empty)}",
                                           $"Offsets: D={Math.Round(Properties.FPSOffset, 3)}ms ({_offsets.FpsLevel}), L={Math.Round(Properties.TPSOffset, 3)}ms ({_offsets.TpsLevel})",
                                           $"Video Backend: {(_vBackend == VideoBackend.Auto ? "Auto" : BackendToString(_vBackend))}",
                                           $"SDL Version: {ver.major}.{ver.minor}.{ver.patch}",
                                           $"Current Scene: {(_scenes.CurrentScene != string.Empty ? _scenes.CurrentScene : "none")}",
                                           $"View Position: {(_scenes.HasInstance(_scenes.CurrentScene) ? _scenes.GetInstance(_scenes.CurrentScene).ViewPosition : "none")}",
                                           $"Text Cache: {_graphics.TextCache.Cached}/{_graphics.TextCache.MaxCached}",
                                           $"Internal Resolution: {_graphics.GetInternalResolution()}"
                        };
                        int maxWidth = 0;
                        int textHeight = _graphics.MeasureText(font, "|").H;
                        for (int i = 0; i < lines.Length; ++i)
                        {
                            var textSize = _graphics.MeasureText(font, lines[i]);
                            if (maxWidth < textSize.W)
                                maxWidth = textSize.W;
                        }
                        _graphics.SetRenderDrawColor(backColor);
                        _graphics.DrawRectangleFilled(leftMargin - boxPadding, topMargin - boxPadding, leftMargin + maxWidth + boxPadding, topMargin + (textHeight * (lines.Length - 1)) + boxPadding);

                        _graphics.SetRenderDrawColor(textColor);
                        for (int i = 0; i < lines.Length; ++i)
                            _graphics.DrawText(font, lines[i], leftMargin, topMargin + spacing * i);
                        _graphics.SetRenderDrawColor(drawColor);
                    }
                } catch { }


                // Render to Screen
                SDL_RenderPresent(_SDL_Renderer);
                // Advance animations for current scene
                if (!Properties.TPSAnimations)_scenes.AnimationStep();
            }
            double end = GetPreciseMs();
            if (start - end > Properties.TargetLogictime)
            {
                Debug.Log("GameEngine.DrawStep()", $"Draw step took {start - end - Properties.TargetFrametime}ms longer than target frame time.");
            }
        }
        private void Sync_Loop()
        {
            //Init
            InitLogic();
            InitGraphics();
            Stopwatch logicTimer = new Stopwatch();
            Stopwatch drawTimer = new Stopwatch();
            Sampler fpsAvg = new Sampler(100);
            Sampler tpsAvg = new Sampler(100);

            //Initial Stuff
            GEngine.LoadStatics(this);
            SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, ParseRenderScale(Properties.RenderScaleQuality));

            _resource.EngineInit = true;
            while (!ResourcesLoaded)
            {
                SDL_Delay(1000);
            }

            logicTimer.Start();
            drawTimer.Start();
            double total;
            bool flip = false;
            double old_frametime = 0;
            do
            {
                total = GetPreciseMs();
                if (ETtoMS(logicTimer.ElapsedTicks) >= Properties.TargetLogictime)
                {
                    _cur_logictime = ETtoMS(logicTimer.ElapsedTicks);
                    tpsAvg.AddPoint(1000.00/_cur_logictime);
                    logicTimer.Restart();
                    LogicStep();
                }
                if (ETtoMS(drawTimer.ElapsedTicks) >= Properties.TargetFrametime || !Properties.EnableFramelimiter)
                {
                    if (!flip)
                    {
                        _cur_frametime = ETtoMS(drawTimer.ElapsedTicks);
                        fpsAvg.AddPoint(1000.00 / (_cur_frametime + old_frametime));
                        drawTimer.Restart();
                        DrawStep();
                    }
                    if (!Properties.EnableFramelimiter) flip = !flip;
                }
                total = GetPreciseMs() - total;
                _cur_totaltime = total;
                _fps = fpsAvg.GetAverage();
                _tps = tpsAvg.GetAverage();
            } while (!_StopThread);
            _Aborted_S = true;
            _SyncThread = null;
            _initLogic = false;
            _initGraphics = false;
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
        private static double ETtoMS(long timerTick)
        {
            return ((double)timerTick / (double)Stopwatch.Frequency) * 1000.00;
        }
    }
}
