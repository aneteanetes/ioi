uniform float progress : register(c0); //hint_range(0.0, 1.0);
uniform float beam_size : register(c1); //hint_range(0.01, 0.15);
uniform float4 color : register(c2);
uniform float noise_desnity : register(c3);

texture mainTex;
sampler mainTexSampler = sampler_state
{
    texture = <mainTex>;
};

float2 random(float2 uv)
{
    uv = float2(dot(uv, float2(127.1, 311.7)),
               dot(uv, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(uv) * 43758.5453123);
}

float noise(float2 uv)
{
    float2 uv_index = floor(uv);
    float2 uv_fract = frac(uv);

    float2 blur = smoothstep(0.0, 1.0, uv_fract);
    
    return lerp(lerp(dot(random(uv_index + float2(0.0, 0.0)), uv_fract - float2(0.0, 0.0)),
                     dot(random(uv_index + float2(0.001, 0.0)), uv_fract - float2(0.001, 0.0)), blur.x),
                lerp(dot(random(uv_index + float2(0.0, 0.001)), uv_fract - float2(0.0, 0.001)),
                     dot(random(uv_index + float2(0.001, 0.001)), uv_fract - float2(0.001, 0.001)), blur.x), blur.y) * 0.5 + 0.5;
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR
{
    float4 tex = tex2D(mainTexSampler, uv);
    if (tex.a == 0)
        return float4(0, 0, 0, 0);
    
    // Инвертируем Y-координату для прогресса снизу вверх
    float inverted_y = 1.0 - uv.y;
    float noisevalue = noise(uv * noise_desnity) * inverted_y;
    
    // Изменяем условия для появления снизу
    float d1 = step(noisevalue, progress);
    float d2 = step(noisevalue, progress - beam_size);
    
    float3 beam = (d1 - d2) * color.rgb;
    
    tex.rgb += beam;
    tex.a *= d1; // Плавное появление пикселей
    
    return tex;
}

technique Teleportation
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}