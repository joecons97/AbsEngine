using AbsEngine.IO;
using Silk.NET.Maths;

namespace AbsEngine.Rendering;

public class Material
{
    public Shader Shader { get; }

    Dictionary<string, Texture> _textures = new();

    public Material(string shaderName)
    {
        Shader = ShaderLoader.GetShaderByName(shaderName)
            ?? throw new ArgumentException(nameof(shaderName), $"Shader with name not found {shaderName}");
    }

    public void Bind()
    {
        Shader.Bind();

        foreach (var texture in _textures)
        {
            Shader.SetTexture(texture.Key, texture.Value);
        }
    }

    public void SetInt(string name, int value)
        => Shader?.SetInt(name, value);
    public void SetUint(string name, uint value)
        => Shader?.SetUint(name, value);
    public void SetFloat(string name, float value)
        => Shader?.SetFloat(name, value);
    public void SetVector(string name, Vector4D<float> value)
        => Shader?.SetVector(name, value);
    public void SetColor(string name, System.Drawing.Color value)
        => Shader?.SetVector(name, new Vector4D<float>(value.R/255f, value.G / 255f, value.B / 255f, value.A / 255f));
    public void SetMatrix(string name, Matrix4X4<float> value)
        => Shader?.SetMatrix(name, value);
    public void SetTexture(string name, Texture texture)
    {
        if (_textures.ContainsKey(name))
            _textures[name] = texture;
        else
            _textures.Add(name, texture);
    }
}
