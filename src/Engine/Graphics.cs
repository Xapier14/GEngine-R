using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

using GEngine.Game;

namespace GEngine.Engine
{
    public class GraphicsEngine
    {
        const bool FLAG_WARNNULLTEXTURE = false;
        public bool DrawCollisionBounds = true;
        private ColorRGBA _renderColor;
        private bool _rebuildTextures = false;
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
            if (TTF_Init() != 0)
            {
                Debug.Log("Graphics.Init()", "Error initializing TTF.");
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
            /*
            int res = SDL_CreateWindowAndRenderer(w, h, (SDL_WindowFlags)flag, out wi, out re);
            if (res != 0)
            {
                Debug.Log("Graphics.CreateWindowAndRenderer()", "Error creating SDL window & renderer.");
                Debug.Log("SDL_GetError()", SDL_GetError());
                throw new EngineException("Error creating SDL window & renderer.", "Graphics.CreateWindowAndRenderer()");
            }
            */
            SDL_SetHint(SDL_HINT_FRAMEBUFFER_ACCELERATION, "0");
            wi = SDL_CreateWindow(windowTitle, x, y, w, h, (SDL_WindowFlags)flag);
            re = SDL_CreateRenderer(wi, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
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
        public ColorRGBA GetRendererDrawColor()
        {
            SDL_GetRenderDrawColor(Renderer, out byte r, out byte g, out byte b, out byte a);
            ColorRGBA ret = new ColorRGBA(r, g, b, a);
            return ret;
        }
        public void DrawScene(SceneInstance scene)
        {
            var instances = scene.Instances.ToList();
            //instances.SortByDepth();

            //draw sprites
            foreach(Instance inst in instances)
            {
                inst.BaseReference.OnDraw(inst, scene, this);
                // moved this line into GameObject.cs
                //DrawSprite(inst.Sprite, inst.Position, inst.ImageAngle, inst.ScaleX, inst.ScaleY, inst.ImageIndex, inst.Offset.X, inst.Offset.Y, scene.ViewPosition.X - scene.ViewOrigin.X, scene.ViewPosition.Y - scene.ViewOrigin.Y, inst.FlipX, inst.FlipY);
                if (scene.UsesPhysics && DrawCollisionBounds) DrawCollision(inst, scene.ViewPosition.X - scene.ViewOrigin.X, scene.ViewPosition.Y - scene.ViewOrigin.Y);
            }
        }
        public void DrawRectangle(int x1, int y1, int x2, int y2)
        {
            SDL_Rect rect = new();
            rect.x = x1;
            rect.y = y1;
            rect.w = Math.Abs(x2 - x1);
            rect.h = Math.Abs(y2 - y1);

            if (SDL_RenderDrawRect(Renderer, ref rect) != 0)
            {
                Debug.Log("GraphicsEngine.DrawRectangle", $"Could not draw rectangle. ({x1}, {y1}, {x2}, {y2})");
            }
        }
        public void DrawCircle(int x, int y, int radius)
        {
            if (SDL_RenderDrawCircle(Renderer, x, y ,radius) != 0)
            {
                Debug.Log("GraphicsEngine.DrawCircle", $"Could not draw circle. ({x}, {y} | Radius: {radius})");
            }
        }
        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            if (SDL_RenderDrawLine(Renderer, x1, y1, x2, y2) != 0)
            {
                Debug.Log("GraphicsEngine.DrawLine", $"Could not draw line. ({x1}, {y1}, {x2}, {y2})");
            }
        }
        public void DrawPoint(int x, int y)
        {
            if (SDL_RenderDrawPoint(Renderer, x, y) != 0)
            {
                Debug.Log("GraphicsEngine.DrawPoint", $"Could not draw point. ({x}, {y})");
            }
        }
        public void DrawText(FontResource font, string text, int x, int y, Size? fontSize = null)
        {
            ColorRGBA color = GetRendererDrawColor();
            IntPtr surface = TTF_RenderText_Solid(font.DataPtr[0], text, new SDL_Color()
            {
                r = color.Red,
                g = color.Green,
                b = color.Blue,
                a = color.Alpha
            });
            if (surface == IntPtr.Zero)
            {
                throw new EngineException("Error creating surface.", "GraphicsEngine.DrawText()");
            }
            IntPtr texture = SDL_CreateTextureFromSurface(Renderer, surface);
            if (texture == IntPtr.Zero)
            {
                throw new EngineException("Error creating texture from surface.", "GraphicsEngine.DrawText()");
            }
            if (TTF_SizeText(font.DataPtr[0], text, out int fontW, out int fontH) != 0)
            {
                throw new EngineException("Error getting text size.", "GraphicsEngine.DrawText()");
            }
            if (fontSize.HasValue)
            {
                if (fontSize.Value.W > 0 && fontSize.Value.H > 0)
                {
                    fontW = fontSize.Value.W;
                    fontH = fontSize.Value.H;
                }
            }
            SDL_FreeSurface(surface);

            // draw texture
            SDL_Rect box = new()
            {
                x = x,
                y = y,
                w = fontW,
                h = fontH
            };

            if (SDL_RenderCopy(Renderer, texture, IntPtr.Zero, ref box) != 0)
            {
                Debug.Log("GraphicsEngine.DrawText()", $"Error drawing text texture.\n{SDL_GetError()}");
            }
            SDL_DestroyTexture(texture);
        }
        public void DrawCollision(Instance inst, int sceneX, int sceneY)
        {
            PhysicsBodyType phyBody = inst.PhysicsAttributes.PhysicsBodyType;
            Coord origin = inst.Position;
            Coord scene = new Coord(sceneX, sceneY);

            Coord position = new Coord(origin.X - scene.X, origin.Y - scene.Y);
            //ColorRGBA def = GetRendererDrawColor();
            //SetRenderDrawColor(new ColorRGBA(255, 0, 0, 255));
            switch (phyBody)
            {
                case PhysicsBodyType.Box:
                    int w = inst.PhysicsAttributes.PhysicsBodySize.W / 2;
                    int h = inst.PhysicsAttributes.PhysicsBodySize.H / 2;
                    SDL_Rect rect = new SDL_Rect();
                    rect.x = position.X - w;
                    rect.y = position.Y - h;
                    rect.w = w * 2;
                    rect.h = h * 2;
                    if (SDL_RenderDrawRect(Renderer, ref rect) != 0)
                    {
                        Debug.Log("GraphicsEngine.DrawCollision", $"Could not draw collision rect for instance: {inst.Hash}");
                    }
                    //Debug.Log($"Drawing coll-box on: ({rect.x}, {rect.y})[{rect.w}x{rect.h}]");
                    break;
                case PhysicsBodyType.Circle:
                    if (SDL_RenderDrawCircle(Renderer, position.X, position.Y, Convert.ToInt32(inst.PhysicsAttributes.Radius)) != 0)
                    {
                        Debug.Log("GraphicsEngine.DrawCollision", $"Could not draw collision circle for instance: {inst.Hash}");
                    }
                    break;
            }
            //SetRenderDrawColor(def);
        }

        public void RaiseTextureRebuild()
        {
            if (!_rebuildTextures)
                _rebuildTextures = true;
        }

        public void RebuildTexturesOnCall(ResourceManager resources)
        {
            if (_rebuildTextures)
            {
                resources.RebuildTextures();
                _rebuildTextures = false;
            }
        }

        public static int SDL_RenderDrawCircle(IntPtr sdlRenderer, int x, int y, int radius)
        {
            int ret = 0;
            int xc = 0, yc = radius;
            int d = 3 - 2 * radius;
            ret += CircleOctantDraw(sdlRenderer, x, y, xc, yc);
            while (yc >= xc)
            {
                xc++;
                if (d > 0)
                {
                    yc--;
                    d = d + 4 * (xc - yc) + 10;
                } else
                {
                    d = d + 4 * xc + 6;
                }
                ret += CircleOctantDraw(sdlRenderer, x, y, xc, yc);
            }
            return ret;
        }

        private static int CircleOctantDraw(IntPtr renderer, int x, int y, int o_x, int o_y)
        {
            int ret = 0;
            ret += SDL_RenderDrawPoint(renderer, x + o_x, y + o_y);
            ret += SDL_RenderDrawPoint(renderer, x - o_x, y + o_y);
            ret += SDL_RenderDrawPoint(renderer, x + o_x, y - o_y);
            ret += SDL_RenderDrawPoint(renderer, x - o_x, y - o_y);
            ret += SDL_RenderDrawPoint(renderer, x + o_y, y + o_x);
            ret += SDL_RenderDrawPoint(renderer, x - o_y, y + o_x);
            ret += SDL_RenderDrawPoint(renderer, x + o_y, y - o_x);
            ret += SDL_RenderDrawPoint(renderer, x - o_y, y - o_x);

            return ret;
        }

        public void DrawSprite(TextureResource texture, Coord position, double angle, double scaleX, double scaleY, int textureIndex, int offsetX = 0, int offsetY = 0, int sceneX = 0, int sceneY = 0, bool flipX = false, bool flipY = false)
        {
            if (texture == null)
            {
                if (FLAG_WARNNULLTEXTURE) Debug.Log("GraphicsEngine.DrawSprite", "Sprite is missing, cannot draw instance.");
                return;
            }
            SDL_Rect dst = new SDL_Rect();
            dst.x = position.X - offsetX - sceneX;
            dst.y = position.Y - offsetY - sceneY;
            dst.w = Convert.ToInt32(Math.Floor(texture.SpriteSize.W * scaleX));
            dst.h = Convert.ToInt32(Math.Floor(texture.SpriteSize.H * scaleY));

            //Console.WriteLine("[Graphics.DrawSprite] Drawing texture '{0}'[{1}] onto ({2}, {3}) with size {4}x{5}.", texture.ResourceName, textureIndex, position.X, position.Y, texture.SpriteSize.W, texture.SpriteSize.H);

            //SetRenderDrawColor(new ColorRGBA(255, 255, 255, 255));

            SDL_RendererFlip flip = SDL_RendererFlip.SDL_FLIP_NONE;

            if (flipX)
                flip |= SDL_RendererFlip.SDL_FLIP_HORIZONTAL;
            if (flipY)
                flip |= SDL_RendererFlip.SDL_FLIP_VERTICAL;


            int index = textureIndex;
            if (index >= texture.Count || index < 0)
                index = 0;

            int res = SDL_RenderCopyEx(Renderer, texture.Textures[index], IntPtr.Zero, ref dst, angle, IntPtr.Zero, flip);
            if (res != 0)
            {
                //error
                throw new EngineException($"Error rendering texture '{texture.ResourceName}'. ({SDL_GetError()})", "GraphicsEngine.DrawSprite()");
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
