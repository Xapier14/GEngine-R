using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine.Engine
{
    public class ResourceManager
    {
        private ResourceCollection _Audio, _Textures;

        public ResourceManager()
        {
            _Audio = new ResourceCollection();
            _Textures = new ResourceCollection();
        }
    }
    public class ResourceBase
    {
        public IntPtr DataPtr { get; set; }
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
