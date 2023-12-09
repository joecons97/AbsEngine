using AbsEngine.Rendering.ShaderTranspiler;
using System.Text.RegularExpressions;

namespace AbsEngine.Rendering.OpenGL;

internal class OpenGLShaderTranspiler : IShaderTranspiler
{
    Dictionary<string, string> _typeConversions = new Dictionary<string, string>()
    {
        {"float4x4",    "mat4" },
        {"float3x3",    "mat3" },
        {"float2x2",    "mat2" },
        {"float4",      "vec4" },
        {"float3",      "vec3" },
        {"float2",      "vec2" },
        {"bool4",       "bvec4" },
        {"bool3",       "bvec3" },
        {"bool2",       "bvec2" },
        {"int4",        "ivec4" },
        {"int3",        "ivec3" },
        {"int2",        "ivec2" },
        {"tex2D",       "texture" },
    };

    public string TranspileFromFile(string fileName)
    {
        var shader = ShaderTree.GetParseFile(fileName);
        if (shader == null)
            throw new Exception($"Failed to parse shader tree information from file {fileName}");

        return GenerateFullShader(shader);
    }

    private string GenerateFullShader(ShaderTree shader)
    {
        var sharedShader = GenerateSharedSection(shader!);

        var vertShader = $"#ifdef VERT\n\n{GenerateVertexShader(shader!)}\n\n#endif";
        var fragShader = $"#ifdef FRAG\n\n{GenerateFragmentShader(shader!)}\n\n#endif";

        var fullShader = sharedShader + "\n" + vertShader + "\n" + fragShader;

        var conversions = _typeConversions.OrderByDescending(x => x.Key.Length);
        foreach (var conversion in conversions)
        {
            fullShader = fullShader.Replace(conversion.Key, conversion.Value);
        }

        return fullShader;
    }

    private string GenerateSharedSection(ShaderTree shader)
    {
        string sharedShader = "#define include Engine/Shaders/Includes/GLSLInc.glsl\n";

        var vsDirective = shader.Directives.FirstOrDefault(x => x.Name == "vs");
        if (vsDirective == null)
            throw new Exception("Shader must define a vertex shader main function via the #define vs directive");

        var vsFunc = shader.Functions.FirstOrDefault(x => x.Name == vsDirective.Params.FirstOrDefault());
        if (vsFunc == null)
            throw new Exception($"Could not find vertex shader main function {vsDirective.Params.FirstOrDefault()}");

        var psDirective = shader.Directives.FirstOrDefault(x => x.Name == "ps");
        if (psDirective == null)
            throw new Exception("Shader must define a pixel shader main function via the #define vs directive");

        var psFunc = shader.Functions.FirstOrDefault(x => x.Name == psDirective.Params.FirstOrDefault());
        if (psFunc == null)
            throw new Exception($"Could not find pixel shader main function {psDirective.Params.FirstOrDefault()}");


        sharedShader += $"{string.Join('\n', shader.Directives.Select(x => x.ToString()))}";
        sharedShader += "\n\n";
        sharedShader += $"{string.Join('\n', shader.Types.Select(x => x.ToString() + "\n"))}";
        sharedShader += "\n\n";
        sharedShader += $"{string.Join('\n', shader.Members.Select((x, i) => $"uniform " + x.ToString()))}";
        sharedShader += "\n\n";
        sharedShader += $"{string.Join('\n', shader.Functions.Where(x => x.Name != vsFunc.Name && x.Name != psFunc.Name).Select(x => x.ToString() + "\n"))}";

        return sharedShader;
    }

    private string GenerateVertexShader(ShaderTree shader)
    {
        string vertexShader = "";

        var vsDirective = shader.Directives.FirstOrDefault(x => x.Name == "vs");
        if (vsDirective == null)
            throw new Exception("Shader must define a vertex shader main function via the #define vs directive");

        var vsFunc = shader.Functions.FirstOrDefault(x => x.Name == vsDirective.Params.FirstOrDefault());
        if (vsFunc == null)
            throw new Exception($"Could not find vertex shader main function {vsDirective.Params.FirstOrDefault()}");

        if (vsFunc.Params.Count != 1)
            throw new Exception($"Incorrect number of parameters passed into vertex shader function. Expected 1, found {vsFunc.Params.Count}");

        var vsType = shader.Types.FirstOrDefault(x => x.Name == vsFunc.Type);
        if (vsType == null)
            throw new Exception($"Could not find return type {vsFunc.Type} for vertex shader main function");

        if (vsType.Members.Any(x => x.Name == "Position" && x.Type.ToLower() == "float4") == false)
            throw new Exception($"Vertex shader return type must contain a \"Position\" member of type float4");

        var vsInType = shader.Types.FirstOrDefault(x => x.Name == vsFunc.Params.First().Type);
        if (vsInType == null)
            throw new Exception($"Could not find input type {vsFunc.Params.First().Type} for vertex shader main function");

        int i = 0;
        foreach (var item in vsInType.Members)
        {
            vertexShader += $"layout (location = {i}) in {item.Type} {item.Name};\n";
            i++;
        }

        string vertexMainFunction = "";

        vertexMainFunction += "\nvoid main() {";

        var outVarName = "";
        var outVarDefRegex = new Regex($@"({vsFunc.Type}) (\w+);");
        Regex? outPositionRegex = null;

        foreach (var item in vsFunc.CodeLines)
        {
            string line = item;
            if (line.ToLower().StartsWith("return"))
                break;

            var match = outVarDefRegex.Match(line);
            if (match.Captures.Any())
            {
                var varNameGroup = match.Groups.Values.FirstOrDefault(x => x.Value.Contains(vsFunc.Type) == false);
                if (varNameGroup == null)
                    throw new Exception("Unable to determine the name of the vertex shader output variable");

                outVarName = varNameGroup.Value;
                continue;
            }

            var inputName = vsFunc.Params.First().Name;

            if (string.IsNullOrEmpty(outVarName) == false)
            {
                if (outPositionRegex == null)
                    outPositionRegex = new Regex($@"({outVarName})\.(Position)\s*=");

                var glPosMatch = outPositionRegex.Match(line);
                if (glPosMatch.Captures.Any())
                {
                    line = line + line.Replace(glPosMatch.Value, "\ngl_Position =");
                }
            }

            if (string.IsNullOrEmpty(inputName) == false)
            {
                var regex = new Regex($@"(?<!\w){inputName}\.");
                var removedInput = regex.Replace(line, "");
                vertexMainFunction += "\n" + removedInput;
            }
        }

        vertexMainFunction += "\n}";

        var outVar = $"out {vsFunc.Type} {outVarName};\n";
        vertexShader += outVar;
        vertexShader += vertexMainFunction;

        return vertexShader;
    }

    private string GenerateFragmentShader(ShaderTree shader)
    {
        string pixelShader = "";

        var psDirective = shader.Directives.FirstOrDefault(x => x.Name == "ps");
        if (psDirective == null)
            throw new Exception("Shader must define a pixel shader main function via the #define vs directive");

        var psFunc = shader.Functions.FirstOrDefault(x => x.Name == psDirective.Params.FirstOrDefault());
        if (psFunc == null)
            throw new Exception($"Could not find pixel shader main function {psDirective.Params.FirstOrDefault()}");

        if (psFunc.Params.Count != 1)
            throw new Exception($"Incorrect number of parameters passed into pixel shader function. Expected 1, found {psFunc.Params.Count}");
        ;
        if (psFunc.Type != "float4")
            throw new Exception($"The pixel shader function must be return a float4");

        var psInType = shader.Types.FirstOrDefault(x => x.Name == psFunc.Params.First().Type);
        if (psInType == null)
            throw new Exception($"Could not find input type {psFunc.Params.First().Type} for pixel shader main function");

        pixelShader += $"in {psFunc.Params.First().Type} {psFunc.Params.First().Name};\n";
        pixelShader += $"out float4 FragColor;\n";

        string vertexMainFunction = "";

        vertexMainFunction += "\nvoid main() {\n";

        foreach (var item in psFunc.CodeLines)
        {
            string line = item;
            if (line.ToLower().StartsWith("return"))
            {
                vertexMainFunction += line.Replace("return", "FragColor =");
                break;
            }

            vertexMainFunction += line + "\n";
        }

        vertexMainFunction += "\n}";

        pixelShader += vertexMainFunction;

        return pixelShader;
    }
}
