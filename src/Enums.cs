using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Genbox.VelcroPhysics.Dynamics;

namespace GEngine
{

}
namespace GEngine.Engine
{
    public enum VideoBackend
    {
        Direct3D,
        OpenGL,
        OpenGL_ES,
        OpenGL_ES2,
        Metal,
        Software,
        Auto
    }
    public enum AudioType
    {
        Music,
        Effect
    }
    public enum ResourceType
    {
        Audio,
        Texture,
        Font
    }
    public enum GameEngineEventType
    {
        WindowClose,
        WindowResize,
        Information
    }
    public enum RenderScaleQuality
    {
        Nearest,
        Linear,
        Anisotropic
    }

    public enum FontQuality
    {
        Fast,
        Quality
    }
    public enum InputCallbackType
    {
        WindowClose,
        RenderDeviceReset,
        FocusGained,
        FocusLost,
        WindowExposed,
        WindowShown,
        WindowSizeChanged
    }
    public enum MouseRelation
    {
        Monitor,
        Scene,
        Viewport
    }

    public enum TextHorizontalAlign
    { 
        Left,
        Center,
        Right
    }
    public enum TextVerticalAlign
    {
        Top,
        Middle,
        Bottom
    }

    public enum LoaderType
    { 
        Ignore,
        Automatic
    }

    public enum ClassConstructorType
    { 
        Automatic,
        Default,
        Type1,
        Type2,
        Type3
    }
}
namespace GEngine.Game
{
    public enum OriginType
    {
        CenterOrigin,
        ManualOrigin
    }
    public enum PhysicsBodyType
    {
        Box,
        Circle
    }
    public enum BodyType
    {
        Static,
        Kinematic,
        Dynamic
    }
}