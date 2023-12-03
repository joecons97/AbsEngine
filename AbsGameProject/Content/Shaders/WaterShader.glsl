#define culling back
#define queue transparent
#define blending SrcAlpha:OneMinusSrcAlpha

struct v2f 
{
    vec4 localPos;
    vec4 worldPos;
    vec4 worldNormal;
    vec2 uvs;
    vec4 worldTangent;
    vec4 vertexColour;
};

#ifdef VERT

    layout (location = 0) in vec3 vPos;
    layout (location = 1) in vec4 vColor;
    layout (location = 2) in vec2 vUvs;

    uniform mat4 uWorldMatrix;
    uniform mat4 uMvp;

    out v2f vertData;

    void main() 
    {
         //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        gl_Position = uMvp * vec4(vPos, 1.0);

        vertData.localPos = vec4(vPos, 1.0);
        vertData.worldPos = uWorldMatrix * vec4(vPos, 1.0);
        vertData.uvs = vec2(vUvs.x, vUvs.y);
        vertData.vertexColour = vColor;
    }

#endif

#ifdef FRAG

    in v2f vertData;

    uniform sampler2D uAtlas;

    uniform vec3 _CameraPosition;

    out vec4 FragColor;

    void main()
    {
        vec2 uv = vertData.uvs.yx;
        vec4 col = texture(uAtlas, uv);

        vec3 viewDir = normalize(_CameraPosition - vertData.worldPos.xyz);
        float fresnel = 1 - clamp(dot(vec3(0,1,0), viewDir), 0, 1);

        fresnel *= 1.5;

        col.a = mix(col.a, 1, fresnel);

        FragColor = col;
    }

#endif