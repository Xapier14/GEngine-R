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
        public bool DrawBorders { get; set; }
        public ColorRGBA BorderColor { get; set; }
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
        private bool _init = false;

        private VideoBackend _backend;

        public GraphicsEngine(VideoBackend backend)
        {
            SetVideoBackend(backend);
            RenderClearColor = new ColorRGBA(140, 180, 200);
            BorderColor = new ColorRGBA(0, 40, 200);
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
                _init = true;
            }
        }
        public void SetVideoBackend(VideoBackend backend)
        {
            if (_init) throw new EngineException("Graphics module already initialized, cannot change video backend.", "GraphicsEngine.SetVideoBackend()");
            switch (backend)
            {
                case VideoBackend.Direct3D:
                    SDL_SetHint(SDL_HINT_RENDER_DRIVER, "direct3d");
                    break;
                case VideoBackend.OpenGL:
                    SDL_SetHint(SDL_HINT_RENDER_DRIVER, "opengl");
                    SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_DEPTH_SIZE, 32);
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
            _backend = backend;
        }
        public void CreateWindowAndRenderer(string windowTitle, int x, int y, int w, int h, out IntPtr window, out IntPtr renderer)
        {
            IntPtr wi, re;
            int flag = 0;
            if (_backend != VideoBackend.Direct3D && _backend != VideoBackend.Metal && _backend != VideoBackend.Software) flag = (int)SDL_WindowFlags.SDL_WINDOW_OPENGL;
            int res = SDL_CreateWindowAndRenderer(w, h, (SDL_WindowFlags)flag, out wi, out re);
            if (res != 0)
            {
                Debug.Log("Graphics.CreateWindowAndRenderer()", "Error creating SDL window & renderer.");
                Debug.Log("SDL_GetError()", SDL_GetError());
                throw new EngineException("Error creating SDL window & renderer.", "Graphics.CreateWindowAndRenderer()");
            }
            SDL_SetWindowTitle(wi, windowTitle);
            if (wi == IntPtr.Zero)
            {
                //Error
                Debug.Log("Graphics.CreateWindowAndRenderer()", "Error creating SDL window.");
                throw new EngineException("Error creating SDL window.", "Graphics.CreateWindowAndRenderer()");
            }
            if (re == IntPtr.Zero)
            {
                //Error
                Debug.Log("Graphics.CreateWindowAndRenderer()", "Error creating SDL renderer.");
                throw new EngineException("Error creating SDL renderer.", "Graphics.CreateWindowAndRenderer()");
            }
            window = wi;
            renderer = re;
            Window = wi;
            Renderer = re;
        }
        public IntPtr CreateWindow(string windowTitle, int x, int y, int w, int h)
        {
            IntPtr win;
            //Create window
            win = SDL_CreateWindow(windowTitle, x, y, w, h, 0);
            if (win == IntPtr.Zero)
            {
                //Error
                Debug.Log("Graphics.CreateWindow()", "Error creating SDL window.");
                Debug.Log("SDL_GetError()", SDL_GetError());
                throw new EngineException("Error creating SDL window.", "Graphics.CreateWindow()");
            }
            Debug.Log("Graphics.CreateWindow()", "Created SDL window.");
            Window = win;
            return win;
        }
        public IntPtr CreateRenderer()
        {
            if (Window == IntPtr.Zero)
                throw new EngineException("Window is NULL.", "Graphics.CreateRenderer()");
            IntPtr ren;
            ren = SDL_CreateRenderer(IntPtr.Zero, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);
            if (ren == IntPtr.Zero)
            {
                //Error
                Debug.Log("Graphics.CreateRenderer()", "Error creating SDL renderer.");
                Debug.Log("SDL_GetError()", SDL_GetError());
                throw new EngineException("Error creating SDL renderer.", "Graphics.CreateRenderer()");
            }
            Debug.Log("Graphics.CreateRenderer()", "Created SDL renderer.");
            Renderer = ren;

            SDL_GetRendererInfo(ren, out SDL_RendererInfo info);

            Console.WriteLine("[Renderer Info] Name: {0}", UTF8_ToManaged(info.name));
            Console.WriteLine("[Renderer Info] Flags: {0}", info.flags);
            Console.WriteLine("[Renderer Info] Max Texture Size: {0}x{1}", info.max_texture_width, info.max_texture_height);

            return ren;
        }
        public SDL_RendererInfo GetRendererInfo(IntPtr renderer)
        {
            SDL_GetRendererInfo(renderer, out SDL_RendererInfo info);
            return info;
        }
        public void RenderClear()
        {
            if (Renderer == IntPtr.Zero) return;
                //throw new EngineException("Renderer is NULL.", "Graphics.RenderClear()");
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
        public void DrawScene(SceneInstance scene)
        {
            InstanceCollection instances = scene.Instances;
            //instances.SortByDepth();

            //draw sprites
            foreach(Instance inst in instances)
            {
                DrawSprite(inst.Sprite, inst.Position, inst.ImageAngle, inst.ScaleX, inst.ScaleY, inst.ImageIndex, inst.Offset.X, inst.Offset.Y);
            }
        }
        public void DrawSprite(TextureResource texture, Coord position, double angle, double scaleX, double scaleY, int textureIndex, int offsetX = 0, int offsetY = 0)
        {
            SDL_Rect dst = new SDL_Rect();
            dst.x = position.X - offsetX;
            dst.y = position.Y - offsetY;
            dst.w = Convert.ToInt32(Math.Floor(texture.SpriteSize.W * scaleX));
            dst.h = Convert.ToInt32(Math.Floor(texture.SpriteSize.H * scaleY));

            //Console.WriteLine("[Graphics.DrawSprite] Drawing texture '{0}'[{1}] onto ({2}, {3}) with size {4}x{5}.", texture.ResourceName, textureIndex, position.X, position.Y, texture.SpriteSize.W, texture.SpriteSize.H);

            //SetRenderDrawColor(new ColorRGBA(255, 255, 255, 255));

            int res = SDL_RenderCopyEx(Renderer, texture.Textures[textureIndex], IntPtr.Zero, ref dst, angle, IntPtr.Zero, SDL_RendererFlip.SDL_FLIP_NONE);
            if (res != 0)
            {
                //error
                throw new EngineException("Error rendering texture '" + texture.ResourceName + "'.", "GraphicsEngine.DrawSprite()");
            }
            if (DrawBorders)
            {
                int rdc = SDL_GetRenderDrawColor(Renderer, out byte r, out byte g, out byte b, out byte a);
                if (rdc != 0)
                {
                    //error
                    throw new EngineException("Error getting current render color.", "GraphicsEngine.DrawSprite()");
                }
                int srdc1 = SDL_SetRenderDrawColor(Renderer, BorderColor.Red, BorderColor.Green, BorderColor.Blue, BorderColor.Alpha);
                if (srdc1 != 0)
                {
                    //error
                    throw new EngineException("Error setting render draw color to debug border color.", "GraphicsEngine.DrawSprite()");
                }
                int bres = SDL_RenderDrawRect(Renderer, ref dst);
                if (bres != 0)
                {
                    //error
                    throw new EngineException("Error drawing sprite border for '" + texture.ResourceName + "'.", "GraphicsEngine.DrawSprite()");
                }
                int srdcp = SDL_SetRenderDrawColor(Renderer, 200, 240, 20, 255);
                if (srdcp != 0)
                {
                    //error
                    throw new EngineException("Error setting render draw color to debug point.", "GraphicsEngine.DrawSprite()");
                }
                int pres = SDL_RenderDrawPoint(Renderer, position.X, position.Y);
                if (pres != 0)
                {
                    //error
                    throw new EngineException("Error drawing sprite border for '" + texture.ResourceName + "'.", "GraphicsEngine.DrawSprite()");
                }
                int srdc2 = SDL_SetRenderDrawColor(Renderer, r, g, b, a);
                if (srdc2 != 0)
                {
                    //error
                    throw new EngineException("Error restoring render draw color.", "GraphicsEngine.DrawSprite()");
                }
            }
        }
    }
}
