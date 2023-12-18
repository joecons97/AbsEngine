#define vs vert;
#define ps frag;
#define culling back;

struct vertIn
{
    float3 Pos;
    float4 Col;
    float2 Uv;
};

struct v2f
{
    float4 Position;
    float4 Col;
    float2 Uvs;
};

float4x4 _Mvp;
sampler2D uAtlas;

v2f vert(vertIn i)
{
    v2f vertResult;
    vertResult.Position = mul(_Mvp, float4(i.Pos, 1.0));
    vertResult.Uvs = i.Uv;
    vertResult.Col = i.Col;
    
    return vertResult;
}

float4 frag(v2f vertResult)
{
    float4 col = tex2D(uAtlas, vertResult.Uvs.yx);

    if(col.a < 0.01)
        discard;

    return vertResult.Col * col;
}