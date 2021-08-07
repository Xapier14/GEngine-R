using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

using static SDL2.SDL;
using static SDL2.SDL_image;
using static SDL2.SDL_mixer;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;

namespace GEngine.Engine
{
    public class ResourceManager
    {
        private const bool FLAG_ALLOW_MISSING_METADATA = false; //hack, allows resources with missing metadata. requires explicit information on sprite sizes
        private ResourceCollection _Audio, _Textures;
        private IntPtr _SDL_Renderer;
        public bool EngineInit = false;

        public ResourceManager()
        {
        }
        public void Init(IntPtr gameRenderer)
        {
            _Audio = new ResourceCollection();
            _Textures = new ResourceCollection();
            IMG_Init(IMG_InitFlags.IMG_INIT_JPG | IMG_InitFlags.IMG_INIT_PNG | IMG_InitFlags.IMG_INIT_TIF);
            Mix_Init(MIX_InitFlags.MIX_INIT_OGG | MIX_InitFlags.MIX_INIT_MP3 | MIX_InitFlags.MIX_INIT_OPUS | MIX_InitFlags.MIX_INIT_MID | MIX_InitFlags.MIX_INIT_FLAC);
            _SDL_Renderer = gameRenderer;
#pragma warning disable CS0162 // Unreachable code detected
            if (FLAG_ALLOW_MISSING_METADATA) Debug.Log("ResourceManager", "Warning! Resource manager has 'IGNORE_MISSING_METADATA' set to true.");
#pragma warning restore CS0162 // Unreachable code detected
        }
        public void Quit()
        {
            foreach(AudioResource au in _Audio)
            {
                au.Destroy();
            }
            _Audio.Clear();
            foreach(TextureResource te in _Textures)
            {
                te.Destroy();
            }
            _Textures.Clear();
        }
        public void SetRenderer(IntPtr sdlRenderer)
        {
            _SDL_Renderer = sdlRenderer;
        }
        public bool HasTexture(string resourceName)
        {
            foreach (TextureResource res in _Textures)
            {
                if (res.ResourceName == resourceName) return true;
            }
            return false;
        }
        public void RebuildTextures()
        {
            foreach(TextureResource tex in _Textures)
            {
                tex.Rebuild(_SDL_Renderer);
            }
        }

        public void SetTextureSpriteSize(string resourceName, Size size)
        {
            if (!HasTexture(resourceName))
            {
                Debug.Log("ResourceManager.SetTextureSpriteSize()", $"Target texture resource '{resourceName}' not found.");
                return;
            }
            foreach (TextureResource text in _Textures)
            {
                if (text.ResourceName == resourceName)
                {
                    text.SpriteSize = new Size(size.W, size.H);
                    return;
                }
            }
        }

        public void SetTextureSpriteSize(string resourceName, int w, int h)
        {
            if (!HasTexture(resourceName))
            {
                Debug.Log("ResourceManager.SetTextureSpriteSize()", $"Target texture resource '{resourceName}' not found.");
                return;
            }
            foreach (TextureResource text in _Textures)
            {
                if (text.ResourceName == resourceName)
                {
                    text.SpriteSize = new Size(w, h);
                    return;
                }
            }
        }

        unsafe public void LoadAsTexture(string fileLocation, string resourceName)
        {
            while (!EngineInit) SDL_Delay(100);
            if (_SDL_Renderer == IntPtr.Zero)
            {
                Debug.Log("ResourceManager.LoadAsTexture()", "SDL Renderer not yet initialized, waiting for init.");
                Debug.Log("ResourceManager.LoadAsTexture()", "Warning! Call 'GameEngine.Start()' first before loading game resources.");
                Debug.Log("ResourceManager.LoadAsTexture()", "Otherwise, this will be an endless loop.");
                while (_SDL_Renderer == IntPtr.Zero) SDL_Delay(100);
                Debug.Log("ResourceManager.LoadAsTexture()", "SDL Renderer initialized.");
                //throw new ResourceException($"SDL Renderer not yet initialized, try calling GameEngine.InitGraphics() first", fileLocation);
            }
            if (_Textures.Contains(resourceName)) throw new ResourceException($"A resource with the same name '{resourceName}' already exists.", fileLocation);
            if (!File.Exists(fileLocation)) throw new ResourceException($"Error loading resource '{resourceName}'. File not found.", fileLocation);
            ZipArchive archive;
            try
            {
                archive = ZipFile.OpenRead(fileLocation);
            }
            catch
            {
                throw new ResourceException($"Error loading resource '{resourceName}'.", fileLocation);
            }
            // sort by name
            var entries = archive.Entries.OrderBy(zip => zip.Name);
            bool loadedMetadata = false;
            List<string> files = new List<string>();
            Size spriteSize = new Size(), sheetSize = new Size();
            bool isSheet = false;
            foreach (ZipArchiveEntry entry in entries)
            {
                if (entry.Name == "metadata")
                {
                    loadedMetadata = true;
                    using (Stream ms = entry.Open())
                    {
                        try
                        {
                            StreamReader reader = new StreamReader(ms);
                            string json = reader.ReadToEnd();
                            reader.Close();
                            JsonDocument doc = JsonDocument.Parse(json);
                            var meta = doc.RootElement;
                            isSheet = meta.GetProperty("IsSheet").GetBoolean();
                            spriteSize.W = meta.GetProperty("SpriteSize").GetProperty("Width").GetInt32();
                            spriteSize.H = meta.GetProperty("SpriteSize").GetProperty("Height").GetInt32();
                            sheetSize.W = meta.GetProperty("SheetSize").GetProperty("Width").GetInt32();
                            sheetSize.H = meta.GetProperty("SheetSize").GetProperty("Height").GetInt32();
                            doc.Dispose();
                        } catch
                        {
                            throw new ResourceException($"Error parsing metadata for '{resourceName}'.", fileLocation);
                        }
                    }
                } else
                {
                    files.Add(entry.Name);
                }
            }
            if (!loadedMetadata && !FLAG_ALLOW_MISSING_METADATA)
            {
                archive.Dispose();
                throw new ResourceException($"Error loading resource '{resourceName}'. Metadata not found.", fileLocation);
            }
            List<IntPtr> surfaces = new List<IntPtr>();
            List<IntPtr> textures = new List<IntPtr>();
            /* Load resource here */
            SDL_GetError();
            foreach (string file in files)
            {
                try
                {
                    ZipArchiveEntry data = archive.GetEntry(file);
                    Stream s = data.Open();
                    byte[] buffer = new byte[data.Length];
                    s.Read(buffer, 0, buffer.Length);
                    string fn = "temp" + data.Name;
                    File.WriteAllBytes(fn, buffer);
                    //Marshal.Copy(buffer, 0, ptrArray, buffer.Length);
                    //GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    //IntPtr ptrArray = handle.AddrOfPinnedObject();
                    //IntPtr ptrArray = Marshal.AllocHGlobal(buffer.Length);
                    //s.Read(buffer, 0, buffer.Length);
                    //Marshal.Copy(buffer, 0, ptrArray, buffer.Length);
                    //Debug.Log("ResourceManager.LoadAsTexture()", $"Read texture data from {file}#{resourceName} @ {buffer.Length} byte(s).");
                    //IntPtr rwops = SDL_RWFromMem(ptrArray, buffer.Length);
                    IntPtr surface = IMG_Load(fn);
                    //IntPtr surface = IMG_Load_RW(rwops, 0);
                    SDL_Surface sd = *(SDL_Surface*)surface;
                    File.Delete(fn);
                    string sE1 = SDL_GetError();
                    if (surface == IntPtr.Zero)// || rwops == IntPtr.Zero)
                    {
                        Debug.Log("ResourceManager.LoadAsTexture()", $"Error reading texture data {file}#{resourceName}. {sE1}");
                        continue;
                    }
                    //Debug.Log("ResourceManager.LoadAsTexture()", $"Loaded texture data as surface from {file}#{resourceName}.");
                    IntPtr texture;
                    while (_SDL_Renderer == IntPtr.Zero) SDL_Delay(100);
                    try
                    {
                        texture = SDL_CreateTextureFromSurface(_SDL_Renderer, surface);
                    } catch (Exception e)
                    {
                        Debug.Log("ResourceManager.LoadAsTexture()", $"Error creating texture data {file}#{resourceName}. {e.Message}");
                        continue;
                    }
                    string sE2 = SDL_GetError();
                    if (texture == IntPtr.Zero)
                    {
                        Debug.Log("ResourceManager.LoadAsTexture()", $"Error creating texture data {file}#{resourceName}. {sE2}");
                        continue;
                    }
                    surfaces.Add(surface);
                    textures.Add(texture);
                    //Debug.Log("ResourceManager.LoadAsTexture()", $"Created texture from surface {file}#{resourceName}.");
                    //SDL_FreeSurface(surface);
                    //Marshal.FreeHGlobal(ptrArray);
                    //handle.Free();
                    //SDL_RWclose(rwops);
                } catch
                {
                    Debug.Log("ResourceManager.LoadAsTexture()", $"Error loading texture {file}#{resourceName}. Skipped file.");
                }
            }
            archive.Dispose();
            TextureResource res = new TextureResource()
            {
                IsSpriteSheet = isSheet,
                SheetSize = sheetSize,
                SpriteSize = spriteSize,
                DataPtr = surfaces.ToArray(),
                Textures = textures.ToArray(),
                ResourceName = resourceName
            };
            _Textures.Add(res);
            Debug.Log("ResourceManager.LoadAsTexture()", $"Successfully added texture {resourceName} to resources.");
        }

        public void LoadAsAudio(string fileLocation, string resourceName, AudioType audioType)
        {
            while (!EngineInit) SDL_Delay(100);
            if (_Audio.Contains(resourceName)) throw new ResourceException($"A resource with the same name '{resourceName}' already exists.", fileLocation);
            if (!File.Exists(fileLocation)) throw new ResourceException($"Error loading resource '{resourceName}'. File not found.", fileLocation);
            try
            {
                switch (audioType)
                {
                    default:
                        throw new ResourceException("Audio type is invalid.", "ResourceManager.LoadAsAudio()");
                    case AudioType.Music:
                        IntPtr mus = Mix_LoadMUS(fileLocation);
                        if (SDL_GetError() != "" || mus == IntPtr.Zero) throw new ResourceException($"Error loading '{fileLocation}'.","ResourceManager.LoadAsAudio()");
                        AudioResource a1 = new AudioResource();
                        a1.DataPtr = new IntPtr[1] { mus };
                        a1.ResourceName = resourceName;
                        a1.AudioType = AudioType.Music;
                        _Audio.Add(a1);
                        return;
                    case AudioType.Effect:
                        IntPtr eff = Mix_LoadWAV(fileLocation);
                        if (SDL_GetError() != "" || eff == IntPtr.Zero) throw new ResourceException($"Error loading '{fileLocation}'.", "ResourceManager.LoadAsAudio()");
                        AudioResource a2 = new AudioResource();
                        a2.DataPtr = new IntPtr[1] { eff };
                        a2.ResourceName = resourceName;
                        a2.AudioType = AudioType.Effect;
                        _Audio.Add(a2);
                        return;
                }
            } catch
            {
                throw new ResourceException("Error in loading audio.", "ResourceManager.LoadAsAudio()");
            }
        }

        public AudioResource GetAudioResource(string resourceName)
        {
            return (AudioResource)_Audio.Get(resourceName);
        }
        public TextureResource GetTextureResource(string resourceName)
        {
            try
            {
                return (TextureResource)_Textures.Get(resourceName);
            } catch (EngineException ex)
            {
                Debug.Log("ResourceManager.GetTextureResource()", ex.Message);
                throw;
            }
        }
    }
    public class ResourceBase
    {
        public IntPtr[] DataPtr { get; set; }
        public string ResourceName { get; set; }

        public virtual void Destroy()
        {

        }
    }
    public class AudioResource : ResourceBase
    {
        public const ResourceType Type = ResourceType.Audio;
        public AudioType AudioType { get; set; }
    }
    public class TextureResource : ResourceBase
    {
        public const ResourceType Type = ResourceType.Texture;
        public IntPtr[] Textures { get; set; }
        public bool IsSpriteSheet { get; set; }
        public Size SpriteSize { get; set; }
        public Size SheetSize { get; set; }
        public int Count { get => Textures.Length; }

        public override void Destroy()
        {
            foreach (IntPtr tex in Textures)
            {
                SDL_DestroyTexture(tex);
            }
            foreach (IntPtr sur in DataPtr)
            {
                SDL_FreeSurface(sur);
            }
        }

        public void Rebuild(IntPtr renderer)
        {
            foreach (IntPtr tex in Textures)
            {
                SDL_DestroyTexture(tex);
            }
            int i = 0;
            foreach (IntPtr sur in DataPtr)
            {
                List<IntPtr> textures = new List<IntPtr>();
                IntPtr tex = SDL_CreateTextureFromSurface(renderer, sur);
                if (tex == IntPtr.Zero)
                {
                    Debug.Log("TextureResource.Rebuild()", $"Could not rebuild texture {ResourceName}#{i}: {SDL_GetError()}");
                }
                i++;
            }
        }
    }
}
