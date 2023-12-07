using AbsEngine.Rendering;

namespace AbsGameProject.FullscreenEffects;

public class Grayscale : FullscreenEffect
{
    Material invertMateral = new Material("Grayscale");

    public override void OnRender(RenderTexture source, RenderTexture destination)
    {
        Blit(source, destination, invertMateral);
    }
}
