#define culling back

struct v2f 
{
    vec4 worldPos;
    vec4 worldNormal;
    vec2 uvs;
    vec4 worldTangent;
};

#ifdef VERT

    layout (location = 0) in vec3 vPos;
    layout (location = 1) in vec4 vColor;
    layout (location = 2) in vec2 vUvs;
    layout (location = 3) in vec3 vNormal;
    layout (location = 4) in vec3 vTangent;

    uniform mat4 _WorldMatrix;
    uniform mat4 _Mvp;

    out v2f vertData;

    void main() 
    {
         //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        gl_Position = _Mvp * vec4(vPos, 1.0);

        vertData.worldPos = _WorldMatrix * vec4(vPos, 1.0);
        vertData.worldNormal = normalize(_WorldMatrix * vec4(vNormal, 0));
        vertData.worldTangent = normalize(_WorldMatrix * vec4(vTangent, 0));
        vertData.uvs = vec2(vUvs.x, vUvs.y);
    }

#endif

#ifdef FRAG

    in v2f vertData;

    out vec4 FragColor;

    uniform vec4 Colour;

    void Unity_Rectangle_float(vec2 UV, float Width, float Height, out float Out)
    {
        vec2 d = abs(UV * 2 - 1) - vec2(Width, Height);
        d = 1 - d / fwidth(d);
        Out = 1 - min(max(min(d.x, d.y), 0), 1);
    }

    void main()
    {
        vec2 st = vertData.uvs.xy;
        float square = 0;

        Unity_Rectangle_float(vertData.uvs.xy, 0.9, 0.9, square);

        if(square <= 0.5)
            discard;

        FragColor = vec4(vec3(1 - square), 1.0);
    }

#endif