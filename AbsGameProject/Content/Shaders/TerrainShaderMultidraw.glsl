#define culling back

struct v2f 
{
    vec4 localPos;
    vec4 worldPos;
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
        mat4 worldMat = transforms[gl_DrawID];
        mat4 mvp = _Vp * worldMat;
        gl_Position = mvp * vec4(vPos, 1.0);

        vertData.localPos = vec4(vPos, 1.0);
        vertData.worldPos = worldMat * vec4(vPos, 1.0);
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
        vec4 col = texture(uAtlas, vertData.uvs.yx);

        if(col.a < 0.05)
            discard;

        FragColor = vertData.vertexColour * col;
    }

#endif