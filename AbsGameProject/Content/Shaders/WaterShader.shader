#define vs vert;
#define ps frag;
#define culling back;
#define queue transparent;
#define blending SrcAlpha:OneMinusSrcAlpha;

struct vertIn
{
    float3 Pos;
    float4 Col;
    float2 Uv;
};

struct v2f
{
    float4 Position;
    float2 uvs;
};

float4x4 _Mvp;
float2 _Resolution;

sampler2D uAtlas;
sampler2D _DepthMap;
float3 _CameraPosition;
float _NearClipPlane;
float _FarClipPlane;
float _WaterDepth;

v2f vert(vertIn i)
{
    v2f vertResult;
    vertResult.Position = mul(_Mvp, float4(i.Pos, 1.0));
    vertResult.uvs = i.Uv;
    
    return vertResult;
}

float depthToLinear(float depth, float nearPlane, float farPlane)
{
    return 2.0 * nearPlane * farPlane / (farPlane + nearPlane - (2.0 * depth - 1.0) * (farPlane - nearPlane));
}

float GetDepth(float exponent, float4 fragCoord)
{
    float2 uvs = fragCoord.xy / _Resolution;
    float depth = tex2D(_DepthMap, uvs).r;

    float waterDepth = depthToLinear(depth, _NearClipPlane, _FarClipPlane);

    float waterDist = depthToLinear(fragCoord.z, _NearClipPlane, _FarClipPlane);

    float totalDepth = waterDepth - waterDist;

    return exp(-totalDepth * exponent);
}

float4 frag(v2f vertResult)
{
    float2 uv = vertResult.uvs.yx;
    float4 col = tex2D(uAtlas, uv);

    //Maybe re-add later

    //vec3 viewDir = normalize(_CameraPosition - vertData.worldPos.xyz);
    //float fresnel = 1 - clamp(dot(vec3(0,1,0), viewDir), 0, 1);

    //fresnel *= 1.5;
    
    col.a = lerp(col.a, 1, 1 - GetDepth(0.1, gl_FragCoord));
    
    return col;
}