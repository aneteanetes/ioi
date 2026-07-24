float ColourAmount;
texture coloredTexture;

sampler coloredTextureSampler = sampler_state
{
    texture = <coloredTexture>;
};

float4 WhitePixelShaderFunction(float2 textureCoordinate : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(coloredTextureSampler, textureCoordinate);

    return float4(1.0,1.0,1.0, color.a);
}

technique Whiting
{
    pass WhitingPass
    {
        PixelShader = compile ps_2_0 WhitePixelShaderFunction();
    }
}