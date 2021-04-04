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
using System.Runtime.InteropServices;

namespace GEngine.Engine
{
    public class ResourceManager
    {
        private ResourceCollection _Audio, _Textures;
        private IntPtr _SDL_Renderer;

        public ResourceManager(IntPtr gameRenderer)
        {
            _Audio = new ResourceCollection();
            _Textures = new ResourceCollection();
            IMG_Init(IMG_InitFlags.IMG_INIT_JPG | IMG_InitFlags.IMG_INIT_PNG | IMG_InitFlags.IMG_INIT_TIF);
            _SDL_Renderer = gameRenderer;
        }
        public void LoadAsTexture(string fileLocation, string resourceName)
        {
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
            bool loadedMetadata = false;
            List<string> files = new List<string>();
            Size spriteSize = new Size(), sheetSize = new Size();
            bool isSheet = false;
            foreach (ZipArchiveEntry entry in archive.Entries)
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
                            spriteSize.W = meta.GetProperty("SpriteSize.W").GetInt32();
                            spriteSize.H = meta.GetProperty("SpriteSize.H").GetInt32();
                            sheetSize.W = meta.GetProperty("SheetSize.W").GetInt32();
                            sheetSize.H = meta.GetProperty("SheetSize.H").GetInt32();
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
            if (!loadedMetadata)
            {
                archive.Dispose();
                if (!File.Exists(fileLocation)) throw new ResourceException($"Error loading resource '{resourceName}'. Metadata not found.", fileLocation);
            }
            List<IntPtr> surfaces = new List<IntPtr>();
            List<IntPtr> textures = new List<IntPtr>();
            /* Load resource here */
            foreach (string file in files)
            {
                try
                {
                    ZipArchiveEntry data = archive.GetEntry(file);
                    Stream s = data.Open();
                    byte[] byteData = new byte[s.Length];
                    GCHandle handle = GCHandle.Alloc(byteData, GCHandleType.Pinned);
                    s.Read(byteData, 0, byteData.Length);
                    Debug.Log("ResourceManager.LoadAsTexture()", $"Read texture data from {file}#{resourceName} @ {byteData.Length} byte(s).");
                    IntPtr rwops = SDL_RWFromMem(handle.AddrOfPinnedObject(), byteData.Length);
                    IntPtr surface = IMG_Load_RW(rwops, 1);
                    Debug.Log("ResourceManager.LoadAsTexture()", $"Loaded texture data as surface from {file}#{resourceName}.");
                    IntPtr texture = SDL_CreateTextureFromSurface(_SDL_Renderer, surface);
                    surfaces.Add(surface);
                    textures.Add(texture);
                    Debug.Log("ResourceManager.LoadAsTexture()", $"Created texture from surface {file}#{resourceName}.");
                    //SDL_FreeSurface(surface);
                    handle.Free();
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
    }
    public class ResourceBase
    {
        public IntPtr[] DataPtr { get; set; }
        public string ResourceName { get; set; }
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
    }
}
