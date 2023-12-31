﻿#define culling back
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

    layout(std430, binding = 3) buffer ssbo1 {
        mat4 transforms[];
    };

    uniform mat4 _WorldMatrix;
    uniform mat4 _Mvp;

    out v2f vertData;

    void main() 
    {
         //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        gl_Position = _Mvp * vec4(vPos, 1.0);
        
        vertData.localPos = vec4(vPos, 1.0);
        vertData.worldPos = _WorldMatrix * vec4(vPos, 1.0);
        vertData.uvs = vec2(vUvs.x, vUvs.y);
        vertData.vertexColour = vColor;
    }

#endif

#ifdef FRAG

    in v2f vertData;
    
    uniform sampler2D _DepthMap;
    uniform vec3 _CameraPosition;
    uniform float _NearClipPlane;
    uniform float _FarClipPlane;

    uniform sampler2D uAtlas;
    uniform float uWaterDepth = 0.1;

    const vec4 waterColour = vec4(0.094, 0.605, 0.894, 1);

    out vec4 FragColor;

    float depthToLinear(float depth, float nearPlane, float farPlane)
    {
	    return 2.0 * nearPlane * farPlane / (farPlane + nearPlane - (2.0 * depth - 1.0) * (farPlane - nearPlane));
    }

    float GetDepth(float exponent)
    {
        vec2 texSize = textureSize(_DepthMap, 0);
        vec2 uvs = gl_FragCoord.xy / texSize;
        float depth = texture(_DepthMap, uvs).r;

        float waterDepth = depthToLinear(depth, _NearClipPlane, _FarClipPlane);

        float waterDist = depthToLinear(gl_FragCoord.z, _NearClipPlane, _FarClipPlane);

        float totalDepth = waterDepth - waterDist;

        return exp(-totalDepth * exponent);
    }

    void main()
    {
        vec2 uv = vertData.uvs.yx;
        vec4 col = texture(uAtlas, uv) * waterColour;

        //Maybe re-add later

        //vec3 viewDir = normalize(_CameraPosition - vertData.worldPos.xyz);
        //float fresnel = 1 - clamp(dot(vec3(0,1,0), viewDir), 0, 1);

        //fresnel *= 1.5;

        col.a = mix(col.a, 1, 1 - GetDepth(uWaterDepth));

        FragColor = col;
    }

#endif