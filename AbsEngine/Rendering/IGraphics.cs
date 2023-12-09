using AbsEngine.Rendering.DirectX11;
using AbsEngine.Rendering.OpenGL;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;

namespace AbsEngine.Rendering;

public interface IGraphics
{
    public static IGraphics Create(IWindow window, GraphicsAPIs gfxAPi)
    {

        switch (gfxAPi)
        {
            case GraphicsAPIs.OpenGL: return new OpenGLGraphics(window, gfxAPi);
            case GraphicsAPIs.D3D11: return new DirectX11Graphics(window, gfxAPi);
            default:
                throw new ApplicationException("No graphics API has been set!");
        }
    }

    public GraphicsAPIs GraphicsAPIs { get; }
    public IShaderTranspiler ShaderTranspiler { get; }

    public void ClearScreen(Color colour);

    public void UpdateViewport(Vector2D<int> viewport);

    public void SetActiveDepthTest(bool enabled);

    public void DrawElements(uint length);

    public void DrawArrays(uint length);
}
