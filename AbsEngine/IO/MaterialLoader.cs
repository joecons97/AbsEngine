using AbsEngine.Rendering;
using Silk.NET.Maths;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AbsEngine.IO;

public static class MaterialLoader
{
    public static Material LoadMaterial(string fileName)
    {
        if (File.Exists(fileName) == false)
            throw new FileNotFoundException(fileName);

        var jsonNodes = JsonNode.Parse(File.ReadAllText(fileName)) ?? throw new InvalidDataException();
        var jsonObject = jsonNodes.AsObject();

        var shaderName = jsonObject["shader"]?.GetValue<string>() ?? throw new InvalidDataException();
        var mat = new Material(shaderName);

        var uniforms = jsonObject["uniforms"]?.AsArray() ?? throw new InvalidDataException();

        foreach (var key in uniforms)
        {
            if (key == null) continue;

            var name = key["name"]?.GetValue<string>() ?? throw new InvalidDataException();
            var type = key["type"]?.GetValue<string>() ?? throw new InvalidDataException();
            JsonNode value = key["value"] ?? throw new InvalidDataException();

            switch(type)
            {
                case "texture":
                    var tex = TextureLoader.LoadTexture(value.GetValue<string>()) ?? throw new InvalidDataException();
                    mat.SetTexture(name, tex); 
                    break;
                case "int":
                    mat.SetInt(name, value.GetValue<int>());
                    break;
                case "float":
                    mat.SetFloat(name, value.GetValue<float>());
                    break;
                case "uint":
                    mat.SetUint(name, value.GetValue<uint>());
                    break;
                case "vector":
                    mat.SetVector(name, value.Deserialize<Vector4D<float>>());
                    break;
                case "matrix":
                    mat.SetMatrix(name, value.Deserialize<Matrix4X4<float>>());
                    break;
                default:
                    throw new InvalidDataException();
            }
        }

        return mat;
    }
}
