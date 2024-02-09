#include "Content/Shaders/Includes/VoxelShared.glsl"

#include "Content/Shaders/Includes/VoxelVert.glsl"

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

        col = vertData.vertexColour * col * vertData.lightValue;

        col = mix(col, uFogColour, vertData.fogAmount);

        FragColor = col;
    }

#endif