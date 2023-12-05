using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;

namespace AbsEngine.Rendering.OpenGL;

internal class OpenGLGraphics : IGraphics
{
    internal GL Gl { get; private set; }

    public GraphicsAPIs GraphicsAPIs { get; }

    public OpenGLGraphics(IWindow window, GraphicsAPIs graphicsAPIs)
    {
        Gl = window.CreateOpenGL();

        GraphicsAPIs = graphicsAPIs;
    }

    public void ClearScreen(Color colour)
    {
        Gl.ClearColor(colour);
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public unsafe void DrawElements(uint length)
    {
        Gl.DrawElements(PrimitiveType.Triangles, length, DrawElementsType.UnsignedInt, null);
    }

    public unsafe void DrawArrays(uint length)
    {
        Gl.DrawArrays(PrimitiveType.Triangles, 0, length);
    }

    public void UpdateViewport(Vector2D<int> viewport)
    {
        Gl.Viewport(viewport);
    }

    public void SetActiveDepthTest(bool enabled)
    {
        if(enabled)
            Gl.Enable(GLEnum.DepthTest); 
        else
            Gl.Disable(GLEnum.DepthTest);
    }
}
