program-shared
{
    #version 420 core

    struct v2f 
    {
        vec4 worldPos;
        vec4 worldNormal;
        vec2 uvs;
    };
}

program-vert
{
    layout (location = 0) in vec3 vPos;
    layout (location = 1) in vec4 vColor;
    layout (location = 2) in vec2 vUvs;
    layout (location = 3) in vec3 vNormal;

    uniform mat4 uWorldMatrix;
    uniform mat4 uMvp;

    out v2f vertData;

    void main() 
    {
	     //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
        gl_Position = uMvp * vec4(vPos, 1.0);

        vertData.worldPos = uWorldMatrix * vec4(vPos, 1.0);
        vertData.worldNormal = normalize(uWorldMatrix * vec4(vNormal, 0));
        vertData.uvs = vec2(vUvs.x, vUvs.y);
    }
}

program-frag
{
    in v2f vertData;

    out vec4 FragColor;

    uniform sampler2D uTexture;
    uniform sampler2D uTexture1;

    void main()
    {
        vec4 texCol = texture(uTexture, vertData.uvs);
        vec4 texCol1 = texture(uTexture1, vertData.uvs);
        FragColor = vec4(mix(texCol1, texCol, vertData.uvs.x));
    }
}