using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace GEngine.Engine
{
    public class AudioEngine
    {
        public AudioEngine()
        {

        }
        public void Init()
        {
            SDL_InitSubSystem(SDL_INIT_AUDIO);
        }
    }
}
