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

    uniform sampler2D _ColorMap;

    out vec4 FragColor;

    void main()
    {
        vec3 col = texture(_ColorMap, vertData.uvs).xyz;
        float average = 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;
        FragColor = vec4(vec3(average), 1);
    }

#endif