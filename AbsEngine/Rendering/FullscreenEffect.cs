using AbsEngine.Exceptions;

namespace AbsEngine.Rendering;

public abstract class FullscreenEffect
{
    public abstract void OnRender(RenderTexture source, RenderTexture destination);

    public void Blit(RenderTexture source, RenderTexture destination, Material material)
    {
        if (Game.Instance == null)
            throw new GameInstanceException();

        destination.Bind();

        material.SetTexture("_ColorMap", source.ColorTexture);
        material.SetTexture("_DepthMap", source.DepthTexture);
        material.Bind();
        Renderer.BLIT_QUAD.Bind();

        Game.Instance.Graphics.DrawElements((uint)Renderer.BLIT_QUAD.Triangles.Length);

        destination.UnBind();
    }
}
