using AbsEngine.Rendering;
using StbImageSharp;

namespace AbsEngine.IO;

public static class TextureLoader
{
    public static Texture? LoadTexture(string fileLocation)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(fileLocation), ColorComponents.RedGreenBlueAlpha);

        var texture = new Texture();
        var backend = texture.GetBackendTexture();
        backend.LoadFromResult(result);
        texture.ApplyBackendTexture(backend);

        return texture;
    }

    public static ImageResult LoadImageResult(string fileLocation)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        return ImageResult.FromMemory(File.ReadAllBytes(fileLocation), ColorComponents.RedGreenBlueAlpha);
    }
}
