uniform float Time;
uniform float4 targetColor;
uniform float2 TextureSize;
uniform float base_amplitude;
uniform float base_frequency;

texture mainTex;
sampler mainTexSampler = sampler_state
{
    texture = <mainTex>;
};

float4 PixelShaderFunction1(float2 uv : TEXCOORD0) : COLOR
{
    float alpha = tex2D(mainTexSampler, uv).a;
    if (alpha == 0)
        return float4(0, 0, 0, 0);
    
    float opacity = 0.9;
    
    float wave_amplitude = 0.009;
    float wave_frequency = 6000.397;
    float wave_speed = .618;
    
    float2 wave_offset = float2(
        sin(uv.y * wave_frequency + Time * wave_speed),
        sin(uv.x * wave_frequency + Time * wave_speed)
    ) * wave_amplitude;

    float2 distorted_uv = uv + wave_offset;

    float2 red_uv = distorted_uv + wave_offset * 0;
    float2 blue_uv = distorted_uv - wave_offset * 0;
    float2 green_uv = distorted_uv;

    float r = tex2D(mainTexSampler, red_uv).r;
    float g = tex2D(mainTexSampler, green_uv).g;
    float b = tex2D(mainTexSampler, blue_uv).b;
    
    float intensity = (r + g + b) / 3.0; // Средняя интенсивность
    float3 screen_color = targetColor.rgb * intensity * 2.0; // Удваиваем интенсивность
    
    // Добавляем эффект свечения для более яркого отображения
    screen_color = saturate(screen_color + targetColor.rgb * intensity * 3);
    
    return float4(screen_color, .85);
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR
{
    float alpha = tex2D(mainTexSampler, uv).a;
    if (alpha == 0)
        return float4(0, 0, 0, 0);
    
    // Перевод в UV координаты
    float wave_amplitude = base_amplitude / TextureSize.y;
    float wave_frequency = TextureSize.y * base_frequency;
    float wave_speed = 0.618;
    
    // Создание волны
    float2 wave_offset = float2(
        sin(uv.y * wave_frequency + Time * wave_speed),
        sin(uv.x * wave_frequency + Time * wave_speed)
    ) * wave_amplitude;

    // Смещение UV
    float2 distorted_uv = uv + wave_offset;

    // Разделение цветовых каналов
    float2 red_uv = distorted_uv + wave_offset * 0;
    float2 green_uv = distorted_uv;
    float2 blue_uv = distorted_uv - wave_offset * 00;

    // Чтение текстуры
    float r = tex2D(mainTexSampler, red_uv).r;
    float g = tex2D(mainTexSampler, green_uv).g;
    float b = tex2D(mainTexSampler, blue_uv).b;
    
    // Формирование цвета
    float intensity = (r + g + b) / 3.0;
    float3 screen_color = targetColor.rgb * intensity * 2.0;
    screen_color = saturate(screen_color + targetColor.rgb * intensity * 0.5);
    
    return float4(screen_color, 0.85);
}

// Техника
technique Main
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}