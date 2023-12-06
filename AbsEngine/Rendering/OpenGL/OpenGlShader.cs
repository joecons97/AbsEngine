using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace AbsEngine.Rendering.OpenGL;

internal class OpenGLShader : IBackendShader
{
    private readonly uint _handle;
    private readonly GL _gl;
    private List<string> _samplers = new();
    private List<string> _uniforms = new();
    private TriangleFace? _culling = TriangleFace.Back;
    private bool _isTransparent = false;
    private int _renderQueuePos = 0;
    private BlendingFactor _blendSrcFactor = BlendingFactor.SrcColor;
    private BlendingFactor _blendDstFactor = BlendingFactor.OneMinusSrcColor;

    public OpenGLShader()
    {
        _gl = ((OpenGLGraphics)Game.Instance!.Graphics).Gl;

        _handle = _gl.CreateProgram();
    }

    public void Bind()
    {
        //_gl.DepthFunc(DepthFunction);
        if (_culling.HasValue)
        {
            _gl.Enable(EnableCap.CullFace);
            _gl.CullFace(_culling.Value);
        }
        else
        {
            _gl.Disable(EnableCap.CullFace);
        }

        if (_isTransparent)
        {
            _gl.Enable(EnableCap.Blend);
            _gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
            _gl.BlendFunc(_blendSrcFactor, _blendDstFactor);
        }
        else
        {
            _gl.Disable(EnableCap.Blend);
        }

        foreach (var globalVariable in Shader._globalVariables)
        {
            if (_uniforms.Contains(globalVariable.Key))
            {
                int location = _gl.GetUniformLocation(_handle, globalVariable.Key);
                if (location == -1)
                    continue;

                switch (globalVariable.Value.Type)
                {
                    case GlobalShaderVariableType.Int:
                        _gl.Uniform1(location, (int)globalVariable.Value.Value);
                        break;
                    case GlobalShaderVariableType.Uint:
                        _gl.Uniform1(location, (uint)globalVariable.Value.Value);
                        break;
                    case GlobalShaderVariableType.Float:
                        _gl.Uniform1(location, (float)globalVariable.Value.Value);
                        break;
                    case GlobalShaderVariableType.Vector4:
                        unsafe
                        {
                            Vector4D<float> value = (Vector4D<float>)globalVariable.Value.Value;
                            _gl.Uniform4(location, 1, (float*)&value);
                        }
                        break;
                    case GlobalShaderVariableType.Vector3:
                        unsafe
                        {
                            Vector3D<float> value = (Vector3D<float>)globalVariable.Value.Value;
                            _gl.Uniform3(location, 1, (float*)&value);
                        }
                        break;
                    case GlobalShaderVariableType.Matrix:
                        unsafe
                        {
                            Matrix4X4<float> value = (Matrix4X4<float>)globalVariable.Value.Value;
                            _gl.UniformMatrix4(location, 1, false, (float*)&value);
                        }
                        break;
                }
            }
        }

        _gl.UseProgram(_handle);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }

    public void LoadFromString(string str)
    {
        GetUniforms(str);
        GetDirectives(ref str);

        var vertProgram = "#version 420 core\n#define VERT\n" + str;
        var fragProgram = "#version 420 core\n#define FRAG\n" + str;

        //Load the individual shaders.
        uint vertex = CompileShader(ShaderType.VertexShader, vertProgram);
        uint fragment = CompileShader(ShaderType.FragmentShader, fragProgram);

        //Attach the individual shaders.
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);
        //Check for linking errors.
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
        }

        //Detach and delete the shaders
        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public void SetFloat(string name, float value)
    {
        //Setting a uniform on a shader using a name.
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
        {
            return;
        }
        _gl.ProgramUniform1(_handle, location, value);
    }

    public void SetInt(string name, int value)
    {
        //Setting a uniform on a shader using a name.
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
        {
            return;
        }
        _gl.ProgramUniform1(_handle, location, value);
    }

    public void SetMatrix(string name, Matrix4X4<float> value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            return;
        }

        unsafe
        {
            _gl.ProgramUniformMatrix4(_handle, location, 1, false, (float*)&value);
        }
    }

    public void SetTexture(string name, IBackendTexture texture)
    {
        if (texture is OpenGLTexture tex)
        {
            var unit = _samplers.IndexOf(name);
            if (unit == -1)
                return;

            _gl.ActiveTexture(TextureUnit.Texture0 + unit);
            tex.Bind();
            SetInt(name, unit);
        }
        else
            throw new ArgumentException($"Texture of type {texture} is not supported in OpenGLShader", nameof(texture));
    }

    public void SetUint(string name, uint value)
    {
        //Setting a uniform on a shader using a name.
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
        {
            return;
        }
        _gl.ProgramUniform1(_handle, location, value);
    }

    public void SetVector(string name, Vector4D<float> value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            return;
        }

        unsafe
        {
            _gl.ProgramUniform4(_handle, location, 1, (float*)&value);
        }
    }

    private void GetUniforms(string shader)
    {
        var allLines = shader.Replace("\r", "").Split("\n").ToList();
        var samplers = allLines.Where(x => x.Contains("uniform"));
        foreach (var item in samplers)
        {
            var line = item.Trim();
            var split = line.Split(' ');
            var type = split[1];
            var name = split[2].Replace(";", "");

            if (type.StartsWith("sampler") && !_samplers.Contains(name))
                _samplers.Add(name);
            else if (!_uniforms.Contains(name))
                _uniforms.Add(name);
        }
    }

    private void GetDirectives(ref string shader)
    {
        var allLines = shader.Replace("\r", "").Split("\n").ToList();
        var directives = allLines.Where(x => x.Contains("#define"));
        foreach (var item in directives)
        {
            var directive = item.Trim().ToLower();
            var split = directive.Split(" ");
            var type = split[1];
            var param = split[2];

            switch (type)
            {
                case "include":
                    var file = File.ReadAllText(param.Trim('\"'));
                    shader = $"{file}\n{shader}".Replace(item, "");
                    break;
                case "culling":
                    _culling = param.ToLower() switch
                    {
                        "back" => TriangleFace.Back,
                        "front" => TriangleFace.Front,
                        "both" => TriangleFace.FrontAndBack,
                        "none" => null,
                        _ => TriangleFace.Back
                    };
                    break;
                case "blending":
                    var srcStr = param.Split(":")[0].ToLower();
                    var dstStr = param.Split(":")[1].ToLower();

                    BlendingFactor srcBlendingFactor = default;
                    BlendingFactor dstBlendingFactor = default;

                    if (!Enum.TryParse(srcStr, true, out srcBlendingFactor))
                        throw new InvalidDataException($"Invalid blending type: {srcStr}");

                    if (!Enum.TryParse(dstStr, true, out dstBlendingFactor))
                        throw new InvalidDataException($"Invalid blending type: {dstStr}");

                    _blendSrcFactor = srcBlendingFactor;
                    _blendDstFactor = dstBlendingFactor;

                    break;
                case "queue":

                    if (param.ToLower() == "transparent")
                    {
                        _isTransparent = true;
                        _renderQueuePos = Renderer.TRANSPARENT_QUEUE_POSITION;
                    }
                    else
                    {
                        if (!int.TryParse(param, out _renderQueuePos))
                            throw new InvalidDataException($"Invalid render queue position: {param}");
                    }

                    break;
            }
        }
    }

    private uint CompileShader(ShaderType type, string program)
    {
        uint handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, program);
        _gl.CompileShader(handle);
        string infoLog = _gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }

    public int GetRenderQueuePosition()
    {
        return _renderQueuePos;
    }
}
