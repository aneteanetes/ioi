float4 TargetColor : register(c0);
float4 ReplacementColor : register(c1);

texture coloredTexture;
sampler coloredTextureSampler = sampler_state
{
    texture = <coloredTexture>;
};

float4 ColorReplacePixelShaderFunction(float2 textureCoordinate : TEXCOORD0) : COLOR0
{
    float tolerance = 0.01f;
    
    float4 color = tex2D(coloredTextureSampler, textureCoordinate);
    
    if (color.r == TargetColor.r 
        && color.g == TargetColor.g 
        && color.b == TargetColor.b)
    {
        return ReplacementColor;
    }
    else
    {
        return color;
    }
}

technique ColorReplace
{
    pass ColorReplacePass
    {
        PixelShader = compile ps_2_0 ColorReplacePixelShaderFunction();
    }
}