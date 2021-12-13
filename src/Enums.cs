﻿using System;
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
        Information
    }
    public enum RenderScaleQuality
    {
        Nearest,
        Linear,
        Anisotropic
    }
    public enum InputCallbackType
    {
        WindowClose,
        RenderDeviceReset,
        FocusGained,
        FocusLost,
        WindowExposed,
        WindowShown
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
}