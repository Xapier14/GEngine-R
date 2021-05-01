# GEngine | Re
A 2D Game Engine built on SDL2.
A Rewrite of an old project of mine.

*This is a __work-in-progress__ project, expect no support.*

## Important Notes:
Nothing worth noting yet.

## To-Do:
- [x] Base Engine Loop
- [x] Frame/Logic Timing
- [x] Input Handler
- [x] Object/Scene based rendering (incomplete)
- [x] Object/Instances
- [x] Audio Module
- [x] Resources/Texture loading
- [x] Resources/Audio loading
- [x] Resource Manager
- [x] GameObject Event System
- [x] Scene Instancing
- [ ] Physics System

## Known Issues
- OpenGL & Textures with transparency.
- Possible Memory Corruption on LoadAsTexture().
- Certain graphics drivers are incompatible with certain VideoBackends.
- Scene ViewPorts are not yet implemented.
- Instance depth sorting not yet implemented.

## How-To:
1. Grab a copy of GEngine.dll. You can find one in "bin\x86\Debug\net5.0\" (Alternatively, you can just clone the repo).
1. Grab a copy of the SDL2 library.
1. Create a new .NET project, import the SDL2 files and set them to always copy on build.
1. Reference the GEngine.dll in your project (If you cloned the repo, just reference the GEngine project).
1. Add "using GEngine;", "using GEngine.Game;", "using GEngine.Engine;", and "using static GEngine.GEngine;".
1. In your main(), create an instance of the GameEngine Class.
1. Set the properties by modifying the GameEngine.Properties member. (Alternatively, use GameEngine.LoadConfig() instead)
1. Call GameEngine.Start() to start GEngine and initialize the sub-modules.
1. Load your resources then set GameEngine.ResourcesLoaded to true. (The engine will be stalled until the resources have been loaded.)

## Creating objects:
1. Create a new class and inherit GameObject.
1. In its constructor, set the default values for sprite, image speed, image index, and image angle.
1. Override the OnCreate, Step, and OnDestroy functions. These are called when the respective events are raised.

## Creating scenes:
1. Create a new class and inherit Scene.
1. In its constructor, set the default values as well as specify the scene size for the base constructor.
1. Override the OnCreate, Step, and OnDestroy functions. These are called when the respective events are raised.

## Adding/Registering scenes:
1. After loading the resources and creating the classes, create a new object of your newly defined class.
1. Call Scenes.Add() to register your scene.

## Adding/Registering GameObjects to Scenes:
1. Create a new object of your chosen GameObject class.
1. Call Scene.GameObjects.Add()

## Switching/Loading Scenes:
1. After creating and registering scenes, call Scenes.SwitchToScene(). (Note: This will create an instance of the scene as well as game object instances.)