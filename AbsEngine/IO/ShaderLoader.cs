using AbsEngine.Rendering;

namespace AbsEngine.IO;

public static class ShaderLoader
{
    static Dictionary<string, string> _loadedShaders = new();

    internal static void ScanShaders()
    {
        var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".glsl") || s.EndsWith(".hlsl") || s.EndsWith(".shader")).ToList();

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            _loadedShaders.Add(fileName, file);
        }
    }

    public static Shader? GetShaderByName(string name)
    {
        if (!_loadedShaders.ContainsKey(name))
            throw new ArgumentException(nameof(name), $"Shader with name {name} not found.");

        var file = _loadedShaders[name];
        var shader = new Shader();
        var backend = shader.GetBackendShader();
        var contents = "";

        if (file.EndsWith(".shader"))
            contents = Game.Instance?.Graphics.ShaderTranspiler.TranspileFromFile(file) ?? File.ReadAllText(file);
        else
            contents = File.ReadAllText(file);

        if (string.IsNullOrEmpty(contents) == false)
            backend.LoadFromString(contents);
        else
            throw new FileNotFoundException("Shader file not found!", file);

        shader.ApplyBackendShader(backend);

        return shader;
    }
}
