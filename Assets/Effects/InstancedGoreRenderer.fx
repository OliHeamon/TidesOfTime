texture goreTexture;

float2 textureSize;

float3 cutColor;

float edgeIntensity;

float threshold;

sampler2D GoreSampler = sampler_state
{
    Texture = (goreTexture);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

matrix transformMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 VertexTexCoord : TEXCOORD0;
    row_major float4x4 InstanceTransform : NORMAL0;
    float4 InstanceTexCoord : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 VertexTexCoord : TEXCOORD0;
    float4 InstanceTexCoord : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, mul(input.InstanceTransform, transformMatrix));
    output.VertexTexCoord = input.VertexTexCoord;
    output.InstanceTexCoord = input.InstanceTexCoord;

    return output;
}

float rand(float2 uv) 
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float tri(float amp, float period, float x) 
{
    return (((4 * amp) / period) * abs(((x - (period / 4)) % period) - (period / 2))) - amp;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // The InstanceTexCoord parameter is a Vector4. The first 2 elements are the top-left of the desired region while the last two are the bottom-right.
    // Using this and the VertexTexCoord argument (which is just a standard 0..1 UV) we can sample the desired region.
    float sampleX = lerp(input.InstanceTexCoord.x, input.InstanceTexCoord.z, input.VertexTexCoord.x);
    float sampleY = lerp(input.InstanceTexCoord.y, input.InstanceTexCoord.w, input.VertexTexCoord.y);

    // This UV refers to the texture coordinates with respect to the original NPC texture.
    float2 uv = float2(sampleX, sampleY);

    float4 color = tex2D(GoreSampler, uv);

    // Dimensions of the NPC texture.
    float imageWidth = (input.InstanceTexCoord.z - input.InstanceTexCoord.x) * textureSize.x;
    float imageHeight = (input.InstanceTexCoord.w - input.InstanceTexCoord.y) * textureSize.y;

    // These are used as an edge threshold. The first number indicates how many pixels from the edge the cutoff is.
    float2 edgeBoundary = 2 / float2(imageWidth, imageHeight);
    float2 redBoundary = (8 + (4 * rand(uv))) / float2(imageWidth, imageHeight);

    // "Normalised" distances for both UVs (where 1 is the distance from the middle to the texture's inscribed circle).
    float pixelDistance = distance(float2(0.5f, 0.5f), input.VertexTexCoord) * 2;
    float uvDistance = distance(float2(0.5f, 0.5f), uv) * 2;

    // Square both for a steeper cutoff.
    pixelDistance *= pixelDistance;
    uvDistance *= uvDistance;

    // Checks to see if the edge thresholds are met.
    bool edgeOfFrame = input.VertexTexCoord.x >= (1 - edgeBoundary.x) || input.VertexTexCoord.x <= edgeBoundary.x || input.VertexTexCoord.y >= (1 - edgeBoundary.y) || input.VertexTexCoord.y <= edgeBoundary.y;
    bool redFrame = input.VertexTexCoord.x >= (1 - redBoundary.x) || input.VertexTexCoord.x <= redBoundary.x || input.VertexTexCoord.y >= (1 - redBoundary.y) || input.VertexTexCoord.y <= redBoundary.y;

    // If the gore color is transparent, do not add blood.
    bool shouldBeCutColor = any(cutColor) && redFrame;

    float4 actualCutColor = (shouldBeCutColor * float4(cutColor, 1)) + (!shouldBeCutColor * color);

    // On non-transparent pixels, add blood. The blood's colour interpolates using the distance from the middle.
    float4 redColor = (any(color) * lerp(color, actualCutColor, saturate(pixelDistance / 1.1f))) + (!any(color) * color);

    // Add edge noise.
    float4 finalColor = !(edgeOfFrame && rand(uv) > 0.5f) * redColor;

    // Cut off sharp corners on the outer edge of the sprite with respect to the original texture.
    bool isOutsideRange = uvDistance > 1.2f;

    return !isOutsideRange * finalColor;
}

technique Technique1
{
    pass InstancedGoreRendererPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};