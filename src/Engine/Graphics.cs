using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

using GEngine.Game;

namespace GEngine.Engine
{
    public class GraphicsEngine
    {
        private ColorRGBA _renderColor;
        public ColorRGBA RenderClearColor
        {
            get
            {
                return _renderColor;
            }
            set
            {
                _renderColor = value;
                SDL_SetRenderDrawColor(Renderer, value.Red, value.Green, value.Blue, value.Alpha);
            }
        }
        public IntPtr Window { get; set; }
        public IntPtr Renderer { get; set; }
        public GraphicsEngine(VideoBackend backend)
        {
            switch (backend)
            {
                case VideoBackend.Direct3D:
                    SDL_SetHint(SDL_HINT_RENDER_DRIVER, "direct3d");
                    break;
                case VideoBackend.OpenGL:
                    SDL_SetHint(SDL_HINT_RENDER_DRIVER, "opengl");
                    break;
                case VideoBackend.OpenGL_ES:
                    SDL_SetHint(SDL_HINT_RENDER_DRIVER, "opengles");
                    break;
                case VideoBackend.OpenGL_ES2:
                    SDL_SetHint(SDL_HINT_RENDER_DRIVER, "opengles2");
                    break;
                case VideoBackend.Metal:
                    SDL_SetHint(SDL_HINT_RENDER_DRIVER, "metal");
                    break;
                case VideoBackend.Software:
                    SDL_SetHint(SDL_HINT_RENDER_DRIVER, "software");
                    break;
            }
            RenderClearColor = new ColorRGBA(140, 180, 200);
        }
        public void Init()
        {
            if (SDL_InitSubSystem(SDL_INIT_VIDEO) != 0)
            {
                //ERROR
                Debug.Log("Graphics.Init()", "Error initializing SDL2 Video Sub-System.");
                Debug.Log("SDL_GetError()", SDL_GetError());
                throw new EngineException("Error initializing SDL2 Video Sub-System.", "Graphics.Init()");
            } else
            {
                //OK
            }
        }
        public IntPtr CreateWindow(string windowTitle, int x, int y, int w, int h)
        {
            IntPtr win;
            //Create window
            win = SDL_CreateWindow(windowTitle, x, y, w, h, 0);
            if (win == IntPtr.Zero)
            {
                //Error
                Debug.Log("Graphics.CreateWindow(string, int, int, int, int)", "Error creating SDL window.");
                Debug.Log("SDL_GetError()", SDL_GetError());
                throw new EngineException("Error creating SDL window.", "Graphics.CreateWindow(string, int, int, int, int)");
            }
            Debug.Log("Graphics.CreateWindow(string, int, int, int, int)", "Created SDL window.");
            Window = win;
            return win;
        }
        public IntPtr CreateRenderer()
        {
            if (Window == IntPtr.Zero)
                throw new EngineException("Window is NULL.", "Graphics.CreateRenderer()");
            IntPtr ren;
            ren = SDL_CreateRenderer(Window, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);
            if (ren == IntPtr.Zero)
            {
                //Error
                Debug.Log("Graphics.CreateRenderer()", "Error creating SDL renderer.");
                Debug.Log("SDL_GetError()", SDL_GetError());
                throw new EngineException("Error creating SDL renderer.", "Graphics.CreateRenderer()");
            }
            Debug.Log("Graphics.CreateRenderer()", "Created SDL renderer.");
            Renderer = ren;
            return ren;
        }
        public void RenderClear()
        {
            if (Renderer == IntPtr.Zero)
                throw new EngineException("Renderer is NULL.", "Graphics.RenderClear()");
            try
            {
                if (SDL_RenderClear(Renderer) != 0)
                {
                    Debug.Log("Graphics.RenderClear()", "Error in SDL_RenderClear().");
                    Debug.Log("SDL_GetError()", SDL_GetError());
                    throw new EngineException("Error in SDL_RenderClear().", "Graphics.RenderClear()");
                }
            } catch
            {
                Debug.Log("Graphics.RenderClear()", "General error in SDL_RenderClear().");
            }
        }
        public void SetRenderDrawColor(ColorRGBA color)
        {
            SDL_SetRenderDrawColor(Renderer, color.Red, color.Green, color.Blue, color.Alpha);
        }
        public void DrawScene()
        {

        }
    }
}
