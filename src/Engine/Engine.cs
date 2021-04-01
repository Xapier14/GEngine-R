using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace GEngine.Engine
{
    public class GameEngine
    {
        public GameEngine()
        {

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
        public void Stop()
        {
            SDL_Quit();
        }
        public void LogicLoop()
        {

        }
        public void DrawLoop()
        {

        }
    }
}
