#define culling back
#define queue transparent
#define blending SrcAlpha:OneMinusSrcAlpha

struct v2f 
{
    vec4 localPos;
    vec4 worldPos;
    vec2 uvs;
    vec4 vertexColour;
    float fogAmount;
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
    
    uniform float FogMaxDistance;
    uniform float FogMinDistance;

    out v2f vertData;
    
    float getLinearFogStrength(float fogMin, float fogMax, float dist)
    {
        if (dist >= fogMax) return 1;
        if (dist <= fogMin) return 0;

        return 1 - (fogMax - dist) / (fogMax - fogMin);
    }

    void main() 
    {
        mat4 worldMat = transforms[gl_DrawID];
        mat4 mvp = _Vp * worldMat;
        gl_Position = mvp * vec4(vPos, 1.0);

        vertData.localPos = vec4(vPos, 1.0);
        vertData.worldPos = worldMat * vec4(vPos, 1.0);
        vertData.uvs = vec2(vUvs.x, vUvs.y);
        vertData.vertexColour = vColor;

        float fogDistance = length(gl_Position.xyz);
        vertData.fogAmount = clamp(getLinearFogStrength(FogMinDistance, FogMaxDistance, fogDistance), 0.0, 1.0);
    }

#endif

#ifdef FRAG

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
        vec4 col = texture(uAtlas, uv) * waterColour;

        //Maybe re-add later

        //vec3 viewDir = normalize(_CameraPosition - vertData.worldPos.xyz);
        //float fresnel = 1 - clamp(dot(vec3(0,1,0), viewDir), 0, 1);

        //fresnel *= 1.5;

        col.a = mix(col.a, 1, 1 - GetDepth(uWaterDepth));
        
        col = mix(col, uFogColour, vertData.fogAmount);

        FragColor = col;
    }

#endif