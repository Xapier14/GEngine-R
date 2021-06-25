# GEngine | Re
A 2D Game Engine built on SDL2.
A Rewrite of an old project of mine.

*This is a __work-in-progress__ project, expect no support.*

## Important Notes:
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FXapier14%2FGEngine-R.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FXapier14%2FGEngine-R?ref=badge_shield)

OpenGL backend is buggy, use direct3d or software mode instead.

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
- [ ] CLI Dev Kit

## Known Issues
- OpenGL & Textures with transparency.
- Possible Memory Corruption on LoadAsTexture().
- Certain graphics drivers are incompatible with VideoBackends.
- Objects that are out of screen are still drawn.

## Dependencies
* SDL2
* SDL2-CS.Core
* Genbox.VelcroPhysics
* .Net 5.0 ([x86](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-5.0.6-windows-x86-installer))

## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FXapier14%2FGEngine-R.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FXapier14%2FGEngine-R?ref=badge_large)
