using GEngine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    public static class GEngine
    {
        public static GameEngine Game { get; set; }
        public static InputManager Input { get; set; }
        public static AudioEngine Audio { get; set; }
        public static SceneManager Scenes { get; set; }
        public static ResourceManager Resources { get; set; }
        public static GraphicsEngine Graphics { get; set; }

        public static void LoadStatics(GameEngine game)
        {
            Game = game;
            Input = game.InputManager;
            Audio = game.AudioEngine;
            Scenes = game.SceneManager;
            Resources = game.ResourceManager;
            Graphics = game.GraphicsEngine;
        }
    }
}
