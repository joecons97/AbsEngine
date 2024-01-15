#define culling back

struct v2f 
{
    vec4 worldPos;
    vec2 uvs;
};

uniform float _NearClipPlane;
uniform float _FarClipPlane;

#ifdef VERT

    layout (location = 0) in vec3 vPos;
    layout (location = 1) in vec4 vColor;
    layout (location = 2) in vec2 vUvs;

    uniform mat4 _Mvp;

    out v2f vertData;

    void main() 
    {
         //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        gl_Position = _Mvp * vec4(vPos, 1.0);
        vertData.uvs = vec2(vUvs.x, vUvs.y);
    }

#endif

#ifdef FRAG

    in v2f vertData;

    out vec4 FragColor;

    uniform vec4 Colour;
    
    void main()
    {
        FragColor = Colour;
    }

#endif