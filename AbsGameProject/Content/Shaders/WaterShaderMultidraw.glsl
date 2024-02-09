#define queue transparent
#define blending SrcAlpha:OneMinusSrcAlpha

#include "Content/Shaders/Includes/VoxelShared.glsl"

#include "Content/Shaders/Includes/VoxelVert.glsl"

#ifdef FRAG

    flat in int chunkScale;
    in v2f vertData;
    
    uniform float _NearClipPlane;
    uniform float _FarClipPlane;
    uniform sampler2D _DepthMap;
    uniform vec3 _CameraPosition;

    uniform sampler2D uAtlas;
    uniform float uWaterDepth = 0.1;

    uniform vec4 uFogColour;

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

        vec4 col = vec4(0);
        if (chunkScale == 1) {
            col = texture(uAtlas, uv);
        }
        else {
            col = textureLod(uAtlas, uv, 10);
        }

        col *= vertData.lightValue;
        col *= waterColour;
        //Maybe re-add later

        //vec3 viewDir = normalize(_CameraPosition - vertData.worldPos.xyz);
        //float fresnel = 1 - clamp(dot(vec3(0,1,0), viewDir), 0, 1);

        //fresnel *= 1.5;

        col.a = mix(col.a, 1, 1 - GetDepth(uWaterDepth));
        
        col = mix(col, uFogColour, vertData.fogAmount);

        FragColor = col;
    }

#endif