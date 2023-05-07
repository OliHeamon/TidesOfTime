matrix transformMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    output.Position = mul(input.Position, transformMatrix);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    if (input.TexCoords.y > 0.425 && input.TexCoords.y < 0.575) 
    {
        return input.Color;
    }

    float uv = saturate(abs((input.TexCoords.y - 0.5) * 2) + 0.3);

    float4 purple = float4(58 / 255.0f, 32 / 255.0f, 129 / 255.0f, 1);

    return smoothstep(input.Color, purple, uv);
}

technique Technique1
{
    pass TeslaCoilLightningPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};