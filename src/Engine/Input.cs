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
        private Dictionary<int, IntPtr> _gameControllers;
        private Dictionary<int, int> _joysticks;
        private Dictionary<int, Gamepad> _gamepads;

        public delegate void InputManagerEventHandler(InputCallbackEventArg eventArg);
        public event InputManagerEventHandler WindowEvent;
        public event InputManagerEventHandler EngineEvent;

        private Coord _windowMouse, _screenMouse;

        public Coord WindowMouse
        {
            get => _windowMouse;
        }
        public Coord ScreenMouse
        {
            get => _screenMouse;
        }

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

        public double WindowScaleModifier { get; set; }

        public InputManager()
        {
            _keys = new Dictionary<SDL_Keycode, bool>();
            _statePressed = new Dictionary<SDL_Keycode, bool>();
            _stateReleased = new Dictionary<SDL_Keycode, bool>();
            _gameControllers = new();
            _gamepads = new();
            _joysticks = new();
            WindowScaleModifier = 1.0;
        }
        public void Init()
        {
            SDL_InitSubSystem(SDL_INIT_EVENTS);
            SDL_InitSubSystem(SDL_INIT_GAMECONTROLLER);
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
            if (_statePressed.TryGetValue(key, out bool result))
            {
                return result;
            }
            return false;
        }
        public bool IsReleased(SDL_Keycode key)
        {
            if (_stateReleased.TryGetValue(key, out bool result))
            {
                return result;
            }
            return false;
        }
        public bool GetGamepad(out Gamepad? gamepad, int index = 0)
        {
            if ((index < 0 && index >= _gameControllers.Count) || _gameControllers.Count == 0)
            {
                gamepad = null;
                return false;
            }
            gamepad = _gamepads[index];
            return true;
        }
        private void HandleEvents()
        {
            while (SDL_PollEvent(out SDL_Event e) != 0)
            {
                int con = -1;
                switch (e.type)
                {
                    case SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                        con = e.cdevice.which;
                        if (!_gameControllers.ContainsKey(con))
                        {
                            _gameControllers.Add(con, SDL_GameControllerOpen(con));
                            _gamepads.Add(con, new());
                            _joysticks.Add(con, SDL_JoystickInstanceID(SDL_GameControllerGetJoystick(_gameControllers[con])));
                            Debug.Log($"Connected controller (id: {con}).");
                        }
                        break;
                    case SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:

                        Debug.Log("CON_AXISUPDATE");
                        con = e.cdevice.which;
                        if (_gameControllers.ContainsKey(con))
                        {
                            SDL_GameControllerClose(_gameControllers[con]);
                            _gamepads.Remove(con);
                            _gameControllers.Remove(con);
                            _joysticks.Remove(con);
                            Debug.Log($"Disconnected controller (id: {con}).");
                        }
                        break;
                    case SDL_EventType.SDL_CONTROLLERBUTTONDOWN or SDL_EventType.SDL_CONTROLLERBUTTONUP:
                        Debug.Log("CON_UPDATE");
                        con = e.cdevice.which;
                        SDL_GameControllerButton button = (SDL_GameControllerButton)e.cbutton.button;
                        bool pressed = e.cbutton.state == SDL_PRESSED;
                        if (_gamepads.ContainsKey(con))
                        {
                            Gamepad gamepad = _gamepads[con];
                            switch (button)
                            {
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A:
                                    gamepad.A = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B:
                                    gamepad.B = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X:
                                    gamepad.X = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y:
                                    gamepad.Y = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE:
                                    gamepad.Guide = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER:
                                    gamepad.LeftBumper = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER:
                                    gamepad.RightBumper = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK:
                                    gamepad.LeftStick.Pressed = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK:
                                    gamepad.RightStick.Pressed = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP:
                                    gamepad.DPad.Up = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN:
                                    gamepad.DPad.Down = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT:
                                    gamepad.DPad.Left = pressed;
                                    break;
                                case SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT:
                                    gamepad.DPad.Right = pressed;
                                    break;
                            }
                            _gamepads.Remove(con);
                            _gamepads.Add(con, gamepad);
                        }
                        break;
                    case SDL_EventType.SDL_CONTROLLERAXISMOTION:
                        int joystickId = e.caxis.which;
                        Gamepad? gpraw = null;
                        foreach (var kp in _joysticks)
                        {
                            if (kp.Value == joystickId)
                            {
                                gpraw = _gamepads[kp.Key];
                            }
                        }
                        if (gpraw.HasValue)
                        {
                            Gamepad pad = gpraw.Value;
                            SDL_GameControllerAxis axis = (SDL_GameControllerAxis)e.caxis.axis;
                            float value = (float)e.caxis.axisValue / 32767f;
                            switch (axis)
                            {
                                case SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX:
                                    pad.LeftStick.X = value;
                                    break;
                                case SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY:
                                    pad.LeftStick.Y = value;
                                    break;
                                case SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX:
                                    pad.RightStick.X = value;
                                    break;
                                case SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY:
                                    pad.RightStick.Y = value;
                                    break;
                                case SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT:
                                    pad.LeftTrigger = value;
                                    break;
                                case SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT:
                                    pad.RightTrigger = value;
                                    break;
                            }
                        }
                        break;
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
                            if (!_keys[e.key.keysym.sym])
                                _keys[e.key.keysym.sym] = true;
                        }
                        else
                        {
                            _keys.Add(e.key.keysym.sym, true);
                        }
                        break;
                    case SDL_EventType.SDL_KEYUP:
                        if (_keys.ContainsKey(e.key.keysym.sym))
                        {
                            if (_keys[e.key.keysym.sym])
                                _keys[e.key.keysym.sym] = false;
                        }
                        else
                        {
                            _keys.Add(e.key.keysym.sym, false);
                        }
                        break;
                    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        switch (e.button.button)
                        {
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
        private void EdgeDetectKeys()
        {
            foreach(var kp in _keys)
            {
                var key = kp.Key;
                var state = kp.Value;

                // if the key is down
                if (state)
                {
                    /* KEY RELEASED */
                    // if the key is up
                    // and it is registered
                    if (_stateReleased.ContainsKey(key))
                    {
                        // remove it
                        _stateReleased.Remove(key);
                    }

                    /* KEY PRESSED */
                    // if the key has not yet been registered
                    if (!_statePressed.ContainsKey(key))
                    {
                        // register it
                        _statePressed.Add(key, true);
                    } else
                    {
                        // if the key is already registered, and is true
                        if (_statePressed[key])
                        {
                            // set it to false
                            _statePressed[key] = false;
                        }
                    }

                } else
                {
                    /* KEY PRESSED */
                    // if the key is up
                    // and it is registered
                    if (_statePressed.ContainsKey(key))
                    {
                        // remove it
                        _statePressed.Remove(key);
                    }

                    /* KEY RELEASED */
                    // if the key has not yet been registered
                    if (!_stateReleased.ContainsKey(key))
                    {
                        // register it
                        _stateReleased.Add(key, true);
                    }
                    else
                    {
                        // if the key is already registered, and is true
                        if (_stateReleased[key])
                        {
                            // set it to false
                            _stateReleased[key] = false;
                        }
                    }
                }
            }
        }
        public void PollEvent()
        {
            // get window relative mouse location
            SDL_GetMouseState(out int x, out int y);
            _windowMouse.X = (int)(x / WindowScaleModifier);
            _windowMouse.Y = (int)(y / WindowScaleModifier);

            HandleEvents();
            EdgeDetectKeys();
        }
    }
}
