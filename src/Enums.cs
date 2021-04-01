using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{

}
namespace GEngine.Engine
{
    public enum EngineMode
    {
        Synchronous,
        Asynchronous
    }
    public enum VideoBackend
    {
        Direct3D,
        OpenGL,
        OpenGL_ES,
        OpenGL_ES2,
        Metal,
        Software
    }
    public enum AudioType
    {
        Music,
        Effect
    }

}
namespace GEngine.Game
{
    public enum OriginType
    {
        CenterOrigin,
        ManualOrigin
    }
}