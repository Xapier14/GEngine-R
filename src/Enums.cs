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
    enum EngineMode
    {
        Synchronous,
        Asynchronous
    }
    enum VideoBackend
    {
        Direct3D,
        OpenGL,
        OpenGL_ES,
        OpenGL_ES2,
        Metal,
        Software
    }
    enum AudioType
    {
        Music,
        Effect
    }

}
namespace GEngine.Game
{
    enum OriginType
    {
        CenterOrigin,
        ManualOrigin
    }
}