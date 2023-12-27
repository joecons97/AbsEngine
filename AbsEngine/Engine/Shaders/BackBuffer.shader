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
float _Time;

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

float4 frag(Fragment vertData)
{
    return SampleTex(0, vertData.Uv);
}