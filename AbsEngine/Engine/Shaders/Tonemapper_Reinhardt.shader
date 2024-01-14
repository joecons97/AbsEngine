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
float _WhitePoint = 1.5;
float _Exposure = 1.5;

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

float luminance(float3 color) {
    return dot(color, float3(0.299, 0.587, 0.114));
}

float4 frag(Fragment vertData)
{
    float3 col = SampleTex(0, vertData.Uv).rgb;

    float lin = luminance(col);

    float lout = (lin * (1.0 + lin / (_WhitePoint * _WhitePoint))) / (1.0 + lin);
    float3 cout = col / lin * lout;


    return float4(clamp(cout * _Exposure, 0.0, 1.0), 1.0);
}