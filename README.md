# GEngine | Re
A 2D Game Engine built on SDL2.
A Rewrite of an old project of mine.

*This is a __work-in-progress__ project, expect no support.*

## Important Notes:
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FXapier14%2FGEngine-R.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FXapier14%2FGEngine-R?ref=badge_shield)

Use Direct3D or Software mode only as OpenGL will crash the engine when loading textures.
Using Metal is still untested.
For games with no sprite loading, OpenGL *may or may not* work.

## Getting Started
1. Clone the project and initialize the submodules
    ```
    git clone --recurse-submodules https://github.com/Xapier14/GEngine-R.git
    ```
1. Run the script `build.bat` to build the project.
1. Create a new **.NET 5+** console project.
1. Add the GEngine-R project to your solution file.
1. Create a project reference with your game project to the GEngine-R project.
1. Copy and paste the program boilerplate to your `program.cs`.
    ```c#
    using System;
    using System.Threading;

    using GEngine;
    using GEngine.Engine;

    namespace <your_namespace>
    {
        public class Program
        {
            private static GameEngine _game;
            public static void Main(string[] args)
            {
                // create the engine
                _game = new();
                
                // change engine properties here
                _game.Properties.Title = "GAME_TITLE";
                _game.Properties.EnableFramelimiter = true;
                _game.Properties.TargetFPS = 60;
                _game.Properties.TargetTPS = 128;
                _game.Properties.WindowResolution = new(800, 600);
                // _game.Properties.InternalResolution = new(400, 300);
                _game.Properties.HideConsoleWindow = false;
                _game.Properties.EnableDebug = true;
                _game.Properties.RenderScaleQuality = RenderScaleQuality.Linear;

                // handle window close
                _game.AllowClose = true;
                _game.HandleClose = false;
                _game.OnWindowClose += (eventArgs) =>
                {
                    Environment.Exit(0);
                };

                // start game
                _game.Start();

                // load resources
                _game.ResourceLoaded = true;
                
                // keep alive
                while (_game.Running)
                    Thread.Sleep(500);
            }
        }
    }
    ```
1. Create & set your configuration to either x86 or x64 and use the appropriate native libraries.
## To-Do:
- [x] Base Engine Loop
- [x] Frame/Logic Timing
- [x] Input Handler
- [x] Object/Scene based rendering
- [x] Object/Instances
- [x] Audio Module
- [x] Resources/Texture loading
- [x] Resources/Audio loading
- [x] Resource Manager
- [x] GameObject Event System
- [x] Scene Instancing
- [x] Physics System (Incomplete)
- [x] Animations
- [x] Object Texture Offsets
- [x] TCP Server/Client (Untested)
- [ ] CLI Dev Kit
- [ ] Visual Studio Item Templates (GameObject & Scene)
- [ ] Visual Studio Project Template

## Known Issues
- OpenGL & Textures with transparency.
- Memory leak on Scene reinstancing.
- ~~Inaccurate FPS & TPS Counter.~~
- Missing checks on adding GameObjects to scenes.
- Initial RenderClearColor not being set on OpenGL backend.
- Only WAV files are supported for audio.
- Missing dependency check on launch

## Dependencies
* SDL2
* SDL_Mixer
* SDL_Image
* SDL_TTF
* SDL2-CS.Core
* Genbox.VelcroPhysics
* .Net 5.0 ([x86](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-5.0.6-windows-x86-installer))

## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FXapier14%2FGEngine-R.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FXapier14%2FGEngine-R?ref=badge_large)
