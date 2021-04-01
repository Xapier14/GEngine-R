using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace GEngine.Engine
{
    public class InputManager
    {
        private Dictionary<SDL_Keycode, bool> _keys;
        public InputManager()
        {
            _keys = new Dictionary<SDL_Keycode, bool>();
        }
        public void Init()
        {
            SDL_InitSubSystem(SDL_INIT_EVENTS);
        }
        public void PollEvent()
        {
            while (SDL_PollEvent(out SDL_Event e) != 0)
            {
                switch (e.type)
                {
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
                }
            }
        }
    }
}
