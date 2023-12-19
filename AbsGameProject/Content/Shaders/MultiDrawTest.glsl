#define culling back

struct v2f 
{
    vec4 localPos;
    vec2 uvs;
    vec4 vertexColour;
};

#ifdef VERT

    layout (location = 0) in vec3 vPos;
    layout (location = 1) in vec4 vColor;
    layout (location = 2) in vec2 vUvs;

    layout(std430, binding = 3) buffer multiDrawBuff 
    {
        mat4 transforms[];
    };

    uniform mat4 _Vp;

    out v2f vertData;

    void main() 
    {
        //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        mat4 mvp = _Vp * transforms[gl_DrawID];
        gl_Position = mvp * vec4(vPos, 1.0);
        
        vertData.localPos = vec4(vPos, 1.0);
        vertData.uvs = vec2(vUvs.x, vUvs.y);
        vertData.vertexColour = vColor;
    }

#endif

#ifdef FRAG

    in v2f vertData;
    
    uniform sampler2D uAtlas;

    out vec4 FragColor;

    void main()
    {
        vec2 uv = vertData.uvs.yx;
        vec4 col = vec4(1,0,0,1);

        FragColor = col;
    }

#endif