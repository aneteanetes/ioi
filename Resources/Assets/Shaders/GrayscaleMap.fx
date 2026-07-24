float grayIntensive;

texture coloredTexture;
sampler coloredTextureSampler = sampler_state
{
    texture = <coloredTexture>;
};

texture patternTexture;
sampler patternTextureSampler = sampler_state
{
    texture = <patternTexture>;
};

float4 GrayscaleMapPixelShaderFunction(float2 textureCoordinate : TEXCOORD0) : COLOR0
{
    //float2 coord = mul(textureCoordinate, projection);
    
    float4 color = tex2D(coloredTextureSampler, textureCoordinate);
    float4 pattern = tex2D(patternTextureSampler, textureCoordinate);
    
    if (pattern.r == 1 
        && pattern.g == 1 
        && pattern.b == 1)
    {
        return color;
    }
    else
    {
        float3 colrgb = color.rgb;
        float greycolor = dot(colrgb, float3(0.3, 0.59, 0.11));

        colrgb.rgb = lerp(dot(greycolor, float3(0.3, 0.59, 0.11)), colrgb, grayIntensive);

        return float4(colrgb.rgb, color.a);
    }
}

technique GrayscaleMap
{
    pass GrayscaleMapPass
    {
        PixelShader = compile ps_2_0 GrayscaleMapPixelShaderFunction();
    }
}