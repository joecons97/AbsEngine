#define culling back

struct v2f 
{
    vec2 uvs;
};

#ifdef VERT

    layout (location = 0) in vec3 vPos;
    layout (location = 2) in vec2 vUvs;

    out v2f vertData;

    void main() 
    {
        gl_Position = vec4(vPos, 1.0);
        vertData.uvs = vUvs.xy;
    }

#endif

#ifdef FRAG

    in v2f vertData;

    uniform sampler2D uBackBuffer;

    out vec4 FragColor;

    void main()
    {
        FragColor = texture(uBackBuffer, vertData.uvs);
    }

#endif