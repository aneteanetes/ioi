float colorOffset; // Amount to offset each color channel
float2 center;

texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
};

float4 PixelShaderFunction(float2 textureCoords : TEXCOORD) : COLOR
{
    float2 toCenter = textureCoords - center;
    float dist = length(toCenter);

    float shiftedOffset = colorOffset * dist;
    
    float4 color = tex2D(TextureSampler, textureCoords);

    //Red channel offset
    float4 red = tex2D(TextureSampler, textureCoords + float2(-shiftedOffset, 0));

    // Green channel offset
    float4 green = tex2D(TextureSampler, textureCoords + float2(0, -shiftedOffset));

    // Blue channel offset
    float4 blue = tex2D(TextureSampler, textureCoords + float2(shiftedOffset, 0));
    
    float modifier = shiftedOffset / colorOffset;
    
    //Combine the color
    color.r = red.r;
    color.g = green.g;
    color.b = blue.b;
    
    return color/modifier;
}

technique ChromaticAberration
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}