float fillPercent; // [0,100]
float4 maskColor;

texture mainTex;
sampler mainTexSampler = sampler_state
{
    texture = <mainTex>;
};

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR
{
    float4 color = tex2D(mainTexSampler, uv);
    
    float2 center = float2(0.5, 0.5);
    float2 dir = uv - center;
    
    float aspect = 1.0;
    dir.x *= aspect;
    
    float angle = atan2(dir.y, dir.x*-1) + 3.14159265359 / 2.0;
    if (angle < 0)
        angle += 6.28318530718;
    if (angle >= 6.28318530718)
        angle -= 6.28318530718;
    float normalizedAngle = angle / 6.28318530718;
    
    float fillFactor = fillPercent / 100.0;
    
    float mask = step(normalizedAngle, fillFactor);
    
    return lerp(color, maskColor, mask * maskColor.a);
}

technique Main
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}