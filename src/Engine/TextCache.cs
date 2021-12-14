using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace GEngine.Engine
{
    public class TextCache
    {
        private GraphicsEngine _graphics;
        private int _maxCached;
        private Dictionary<TextCacheInfo, IntPtr> _cache;

        public TextCache(GraphicsEngine graphics, int maxCached = 100)
        {
            _maxCached = maxCached;
            _graphics = graphics;
            _cache = new();
        }

        public bool HasCache(FontResource font, string text, ColorRGBA color)
        {
            foreach (var info in _cache.Keys)
                if (info.Font == font &&
                    info.Text == text &&
                    info.Color == color)
                    return true;
            return false;
        }
        private IntPtr GetTexture(FontResource font, string text, ColorRGBA color)
        {
            foreach (var info in _cache)
                if (info.Key.Font == font &&
                    info.Key.Text == text &&
                    info.Key.Color == color)
                    return info.Value;
            return IntPtr.Zero;
        }

        public IntPtr RetrieveTexture(FontResource font, string text, ColorRGBA color)
        {
            TextCacheInfo info = new()
            { Font = font, Text = text, Color = color };
            if (HasCache(font, text, color))
            {
                return GetTexture(font, text, color);
            } else
            {
                if (_cache.Count >= _maxCached)
                {
                    var first = _cache.FirstOrDefault();
                    SDL_DestroyTexture(first.Value);
                    _cache.Remove(first.Key);
                }

                IntPtr surface = TTF_RenderText_Solid(font.DataPtr[0], text, new SDL_Color()
                {
                    r = color.Red,
                    g = color.Green,
                    b = color.Blue,
                    a = color.Alpha
                });
                if (surface == IntPtr.Zero)
                {
                    throw new EngineException("Error creating surface.", "TextCache.RetrieveTexture()");
                }
                IntPtr texture = SDL_CreateTextureFromSurface(_graphics.Renderer, surface);
                SDL_FreeSurface(surface);
                if (texture == IntPtr.Zero)
                {
                    throw new EngineException("Error creating texture from surface.", "TextCache.RetrieveTexture()");
                }

                _cache.Add(info, texture);

                return texture;
            }
        }
    }
}
