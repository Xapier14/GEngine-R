using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace GEngine.Engine
{
    public class InputCallbackEventArg : EventArgs
    {
        public InputCallbackType CallbackType { get; set; }
    }
    public class InputManager
    {
        private Dictionary<SDL_Keycode, bool> _keys, _statePressed, _stateReleased;
        private bool _leftMB = false, _rightMB = false, _middleMB = false;

        public delegate void InputManagerEventHandler(InputCallbackEventArg eventArg);
        public event InputManagerEventHandler WindowEvent;
        public event InputManagerEventHandler EngineEvent;

        public bool MouseLeftButtonDown
        {
            get
            {
                return _leftMB;
            }
        }

        public bool MouseRightButtonDown
        {
            get
            {
                return _rightMB;
            }
        }

        public bool MouseMiddleButtonDown
        {
            get
            {
                return _middleMB;
            }
        }
        public InputManager()
        {
            _keys = new Dictionary<SDL_Keycode, bool>();
            _statePressed = new Dictionary<SDL_Keycode, bool>();
            _stateReleased = new Dictionary<SDL_Keycode, bool>();
        }
        public void Init()
        {
            SDL_InitSubSystem(SDL_INIT_EVENTS);
        }
        public bool IsDown(SDL_Keycode key)
        {
            if (_keys.TryGetValue(key, out bool result))
            {
                return result;
            } else
            {
                return false;
            }
        }
        public bool IsUp(SDL_Keycode key)
        {
            return !IsDown(key);
        }
        public bool IsPressed(SDL_Keycode key)
        {
            throw new NotImplementedException();
        }
        public bool IsReleased(SDL_Keycode key)
        {
            throw new NotImplementedException();
        }
        public void PollEvent()
        {
            while (SDL_PollEvent(out SDL_Event e) != 0)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_WINDOWEVENT:
                        switch (e.window.windowEvent)
                        {
                            case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                                WindowEvent?.Invoke(new InputCallbackEventArg()
                                {
                                    CallbackType = InputCallbackType.WindowClose
                                });
                                break;
                            case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                                WindowEvent?.Invoke(new InputCallbackEventArg()
                                {
                                    CallbackType = InputCallbackType.FocusGained
                                });
                                break;
                            case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                                WindowEvent?.Invoke(new InputCallbackEventArg()
                                {
                                    CallbackType = InputCallbackType.FocusLost
                                });
                                break;
                            case SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED:
                                WindowEvent?.Invoke(new InputCallbackEventArg()
                                {
                                    CallbackType = InputCallbackType.WindowExposed
                                });
                                break;
                            case SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
                                WindowEvent?.Invoke(new InputCallbackEventArg()
                                {
                                    CallbackType = InputCallbackType.WindowShown
                                });
                                break;
                        }
                        break;
                    case SDL_EventType.SDL_KEYDOWN:
                        if (_keys.ContainsKey(e.key.keysym.sym))
                        {
                            _keys[e.key.keysym.sym] = true;
                        } else
                        {
                            _keys.Add(e.key.keysym.sym, true);
                        }
                        break;
                    case SDL_EventType.SDL_KEYUP:
                        if (_keys.ContainsKey(e.key.keysym.sym))
                        {
                            _keys[e.key.keysym.sym] = false;
                        }
                        else
                        {
                            _keys.Add(e.key.keysym.sym, false);
                        }
                        break;
                    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        switch (e.button.button){
                            case (byte)SDL_BUTTON_LEFT:
                                _leftMB = true;
                                break;
                            case (byte)SDL_BUTTON_RIGHT:
                                _rightMB = true;
                                break;
                            case (byte)SDL_BUTTON_MIDDLE:
                                _middleMB = true;
                                break;
                        }
                        break;
                    case SDL_EventType.SDL_MOUSEBUTTONUP:
                        switch (e.button.button)
                        {
                            case (byte)SDL_BUTTON_LEFT:
                                _leftMB = false;
                                break;
                            case (byte)SDL_BUTTON_RIGHT:
                                _rightMB = false;
                                break;
                            case (byte)SDL_BUTTON_MIDDLE:
                                _middleMB = false;
                                break;
                        }
                        break;
                    case SDL_EventType.SDL_RENDER_DEVICE_RESET:
                        EngineEvent?.Invoke(new InputCallbackEventArg()
                        {
                            CallbackType = InputCallbackType.RenderDeviceReset
                        });
                        break;
                }
            }
        }
    }
}
