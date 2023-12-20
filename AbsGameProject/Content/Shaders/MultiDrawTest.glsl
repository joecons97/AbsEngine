#define culling back

struct v2f 
{
    vec4 localPos;
    vec2 uvs;
    vec4 color;
};

struct multidrawBuffer
{
    mat4 transform;
    vec4 color;
};

#ifdef VERT

    layout (location = 0) in vec3 vPos;
    layout (location = 1) in vec4 vColor;
    layout (location = 2) in vec2 vUvs;

    layout(std430, binding = 3) buffer multiDrawBuff 
    {
        multidrawBuffer buffers[];
    };

    uniform mat4 _Vp;

    out v2f vertData;

    void main() 
    {
        //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        mat4 mvp = _Vp * buffers[gl_DrawID].transform;
        gl_Position = mvp * vec4(vPos, 1.0);
        
        vertData.localPos = vec4(vPos, 1.0);
        vertData.uvs = vec2(vUvs.x, vUvs.y);
        vertData.color = buffers[gl_DrawID].color;
    }

#endif

#ifdef FRAG

    in v2f vertData;
    
    uniform sampler2D uAtlas;

    out vec4 FragColor;

    void main()
    {
        vec2 uv = vertData.uvs.yx;
        vec4 col = vertData.color;

        FragColor = col;
    }

#endif