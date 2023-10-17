using AbsEngine.Rendering;
using StbImageSharp;

namespace AbsEngine.IO;

public static class TextureLoader
{
    public static Texture? LoadTexture(string fileLocation)
    {
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(fileLocation), ColorComponents.RedGreenBlueAlpha);

        var texture = new Texture();
        var backend = texture.GetBackendTexture();
        backend.LoadFromResult(result);
        texture.ApplyBackendTexture(backend);

        return texture;
    }
}
