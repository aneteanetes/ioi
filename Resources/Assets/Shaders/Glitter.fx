texture Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;
};

texture patternTexture;
sampler patternTextureSampler = sampler_state
{
    texture = <patternTexture>;
};


float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 glitterOffset = tex2D(patternTextureSampler, uv).rg; // Assuming red/green channels store offsets
    float4 color = tex2D(TextureSampler, uv + glitterOffset); // Apply offset to main texture
    return color; // Return the modified color
}

technique P0
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
