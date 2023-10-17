using AbsEngine.Rendering;

namespace AbsEngine.IO;

public static class ShaderLoader
{
    public static Shader? LoadShader(string fileLocation)
    {
        var shader = new Shader();
        var backend = shader.GetBackendShader();

        var contents = File.ReadAllText(fileLocation);
        if (string.IsNullOrEmpty(contents) == false)
            backend.LoadFromString(contents);
        else
            throw new FileNotFoundException("Shader file not found!", fileLocation);

        shader.ApplyBackendShader(backend);

        return shader;
    }
}
