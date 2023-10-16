using Silk.NET.Maths;
using Silk.NET.OpenGL;
using static System.Net.Mime.MediaTypeNames;

namespace AbsEngine.Rendering.OpenGL;

internal class OpenGlShader : IBackendShader
{
    private uint _handle;
    private GL _gl;

    private Dictionary<string, int> _uniforms = new Dictionary<string, int>();

    public OpenGlShader()
    {
        _gl = ((OpenGLGraphics)Game.Instance!.Graphics).Gl;

        _handle = _gl.CreateProgram();
    }

    public void Bind()
    {
        //_gl.DepthFunc(DepthFunction);
        //if (EnableCulling)
        //{
        //    _gl.Enable(EnableCap.CullFace);
        //    _gl.CullFace(CullFaceMode);
        //}
        //else
        //{
        //    _gl.Disable(EnableCap.CullFace);
        //}
        _gl.UseProgram(_handle);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }

    public void LoadFromString(string str)
    {
        string sharedProgram = GetProgram("shared", str);
        string vertProgram = sharedProgram + GetProgram("vert", str);
        string fragProgram = sharedProgram + GetProgram("frag", str);

        //Load the individual shaders.
        uint vertex = LoadShader(ShaderType.VertexShader, vertProgram);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragProgram);

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

    private string GetProgram(string program, string shaderContents)
    {
        var allLines = shaderContents.Replace("\r", "").Split("\n").ToList();

        int programNameIndex = allLines.IndexOf("program-" + program);
        if (programNameIndex == -1)
            throw new Exception($"Program \"{program}\" not found in shader ");

        allLines.RemoveRange(0, programNameIndex + 1);

        int programStartIndex = allLines.IndexOf("{");
        if (programStartIndex == -1)
            throw new Exception($"Expected \"{{\"");

        allLines.RemoveRange(0, programStartIndex + 1);

        int programEndIndex = allLines.IndexOf("}");
        if (programEndIndex == -1)
            throw new Exception($"Expected \"}}\"");

        allLines.RemoveRange(programEndIndex, allLines.Count - programEndIndex);

        var finalProgram = string.Join('\n', allLines);

        var includes = allLines.Where(x => x.Contains("#include"));
        foreach (var item in includes)
        {
            throw new NotImplementedException();
            //var file = item.Replace("#include", "").Trim().Trim('"');
            //var includedFile = SceneLoader.ReadFileContents(file, out _);
            //finalProgram = finalProgram.Replace(item, includedFile);
        }

        return finalProgram + "\n";
    }

    private uint LoadShader(ShaderType type, string program)
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
}
