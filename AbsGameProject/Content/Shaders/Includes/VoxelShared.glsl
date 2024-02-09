#define culling back
#define GOOD_FOG

struct v2f
{
    vec4 worldPos;
    vec2 uvs;
    vec4 vertexColour;
    float fogAmount;
    float lightValue;
};