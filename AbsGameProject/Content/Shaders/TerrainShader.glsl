#define culling back

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
    layout (location = 3) in vec3 vNormal;
    layout (location = 4) in vec3 vTangent;

    uniform mat4 uWorldMatrix;
    uniform mat4 uMvp;

    out v2f vertData;

    void main() 
    {
         //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        gl_Position = uMvp * vec4(vPos, 1.0);

        vertData.localPos = vec4(vPos, 1.0);
        vertData.worldPos = uWorldMatrix * vec4(vPos, 1.0);
        vertData.worldNormal = normalize(uWorldMatrix * vec4(vNormal, 0));
        vertData.worldTangent = normalize(uWorldMatrix * vec4(vTangent, 0));
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
        vec4 col = texture2D(uAtlas, vertData.uvs.yx);

        if(col.a < 0.5)
            discard;

        float ndl = clamp(dot(vertData.worldNormal, vec4(0.25, -0.5, 0.75, 1)), 0.0, 1.0) + 0.25;
        FragColor = col * vertData.vertexColour;//vec4(col.x, col.y,0, 0);//vec4(ndl,ndl,ndl,1);
    }

#endif