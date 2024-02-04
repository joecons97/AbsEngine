#define culling back
#define GOOD_FOG

struct v2f 
{
    vec4 worldPos;
    vec2 uvs;
    vec4 vertexColour;
    float fogAmount;
};

struct ChunkBuffer
{
    int scale;

    float p1;
    float p2;
    float p3;
    mat4 worldMat;
};

#ifdef VERT

    layout (location = 0) in vec3 vPos;
    layout (location = 1) in vec4 vColor;
    layout (location = 2) in vec2 vUvs;
    
    layout(std430, binding = 3) buffer multiDrawBuff
    {
        ChunkBuffer bufferData[];
    };

    uniform mat4 _Vp;
    
    uniform float FogMaxDistance;
    uniform float FogMinDistance;
    uniform vec3 _CameraPosition;

    out v2f vertData;
    flat out int chunkScale;
    
    float getLinearFogStrength(float fogMin, float fogMax, float dist)
    {
        if (dist >= fogMax) return 1;
        if (dist <= fogMin) return 0;

        return 1 - (fogMax - dist) / (fogMax - fogMin);
    }

    void main() 
    {
        ChunkBuffer chunkData = bufferData[gl_DrawID];
        mat4 worldMat = chunkData.worldMat;
        mat4 mvp = _Vp * worldMat;
        gl_Position = mvp * vec4(vPos, 1.0);

        vertData.uvs = vUvs.xy;
        vertData.vertexColour = vColor;

        chunkScale = chunkData.scale;
        
        #ifdef GOOD_FOG
            vertData.worldPos = worldMat * vec4(vPos, 1.0);
            float fogDistance = length(_CameraPosition - vertData.worldPos.xyz);
        #else
            float fogDistance = length(gl_Position.xyz);
        #endif

        vertData.fogAmount = clamp(getLinearFogStrength(FogMinDistance, FogMaxDistance, fogDistance), 0.0, 1.0);
    }

#endif

#ifdef FRAG

    flat in int chunkScale;
    in v2f vertData;

    uniform sampler2D uAtlas;

    uniform vec4 uFogColour;
    
    out vec4 FragColor;
    
    void main()
    {
        vec4 col = vec4(0);

        if (chunkScale == 1) {
            col = texture(uAtlas, vertData.uvs.yx);
            if (col.a < 0.05)
                discard;
        }
        else {
            col = textureLod(uAtlas, vertData.uvs.yx, chunkScale + 1);
        }

        col = vertData.vertexColour * col;

        col = mix(col, uFogColour, vertData.fogAmount);

        FragColor = col;
    }

#endif