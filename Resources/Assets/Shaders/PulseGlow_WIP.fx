uniform float4 glow_color : register(c0);// float4(1.0, 0.9, 0.5, 1.0);
uniform float intensity : register(c1); //hint_range(0.0, 5.0) = 1.5;
uniform float spread : register(c2); //hint_range(0.1, 2.0) = 1.0;
uniform float pulse_speed : register(c3); //hint_range(0.0, 10.0) = 1.0;
uniform float timeSeconds : register(c4); //hint_range(0.0, 10.0) = 1.0;

texture mainTex;
sampler mainTexSampler = sampler_state
{
    texture = <mainTex>;
};

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR
{
    float2 centered_uv = uv - float2(0.5, 0.5);
    float dist = length(centered_uv) * spread;
    float alpha = max(0.0, 1.0 - dist);
    alpha *= (1.0 + 0.2 * sin(timeSeconds * pulse_speed));
    alpha = clamp(alpha * intensity, 0.0, 1.0);
    
    return glow_color * alpha;
}

technique T0
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}