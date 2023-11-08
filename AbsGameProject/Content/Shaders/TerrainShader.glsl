﻿#define culling back

struct v2f 
{
    vec4 localPos;
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

    uniform mat4 uWorldMatrix;
    uniform mat4 uMvp;

    out v2f vertData;

    void main() 
    {
         //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        gl_Position = uMvp * vec4(vPos, 1.0);

        vertData.localPos = vec4(vPos / 16.0, 1.0);
        vertData.worldPos = uWorldMatrix * vec4(vPos, 1.0);
        vertData.worldNormal = normalize(uWorldMatrix * vec4(vNormal, 0));
        vertData.worldTangent = normalize(uWorldMatrix * vec4(vTangent, 0));
        vertData.uvs = vec2(vUvs.x, vUvs.y);
    }

#endif

#ifdef FRAG

    in v2f vertData;

    out vec4 FragColor;

    void main()
    {
        float ndl = clamp(dot(vertData.worldNormal, vec4(0.25, -0.5, 0.75, 1)), 0.0, 1.0) + 0.25;
        FragColor = vec4(vertData.localPos.x,0,0, 0);//vec4(ndl,ndl,ndl,1);
    }

#endif