float _DissolveAmount : register(c0);

texture mainTex;
sampler mainTexSampler = sampler_state
{
    texture = <mainTex>;
};


texture noiseTex;
sampler noiseTexSampler = sampler_state
{
    texture = <noiseTex>;
};

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR
{
    float4 color = tex2D(mainTexSampler, uv);
    float noise = tex2D(noiseTexSampler, uv).r; // Assuming noise is grayscale

    if (noise < _DissolveAmount)
    {
        clip(-1); // Discard the fragment
    }

    return color;
}

technique Dissolve
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}