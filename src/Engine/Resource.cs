using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine.Engine
{
    public static class ResourceManager
    {

    }
    public class ResourceBase
    {
        public IntPtr DataPtr { get; set; }
        public string ResourceName { get; set; }
    }
    public class AudioResource : ResourceBase
    {
        
    }
    public class TextureResource : ResourceBase
    {

    }
}
