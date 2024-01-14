#define vs vert;
#define ps frag;
#define culling back;
#define blending SrcColor:OneMinusSrcColor;

struct Vertex
{
    float3 Pos;
    float4 Col;
    float2 Uv;
};

struct Fragment
{
    float4 Position;
    float2 Uv;
};

sampler2D _ColorMap;
float _Exposure = 3;

float4 SampleTex(float offset, float2 uv)
{
    uv = float2(uv.x, uv.y);
    return tex2D(_ColorMap, uv);
}

Fragment vert(Vertex i)
{
    Fragment vertData;
    vertData.Position = float4(i.Pos, 1);
    vertData.Uv = i.Uv;
    
    return vertData;
}

float _A = 0.15;
float _B = 0.50;
float _C = 0.10;
float _D = 0.20;
float _E = 0.02;
float _F = 0.30;
float _W = 11.2;

float3 Uncharted2Tonemap(float3 x)
{
    return ((x*(_A*x+_C*_B)+_D*_E)/(x*(_A*x+_B)+_D*_F))-_E/_F;
}

float4 frag(Fragment vertData)
{
    float3 col = SampleTex(0, vertData.Uv).rgb;

    float3 curr = _Exposure * Uncharted2Tonemap(col);

    float3 whiteScale = 1.0 / Uncharted2Tonemap(float3(_W));
    float3 color = curr * whiteScale;

    return float4(clamp(color, 0.0, 1.0), 1.0);
}