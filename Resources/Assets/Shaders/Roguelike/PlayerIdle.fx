float time;
sampler TextureSampler : register(s0);
float2 tileTopLeft;
float2 tileSizeUV;

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float atlasHeight = 1024.0;
    float tileHeight = 34.0;
    float localY = ((texCoord.y * atlasHeight) % tileHeight) / tileHeight;

    // 1. Считаем искажение
    float wave = sin(time * 2.5) * 0.03 * (1.0 - localY);
    float2 distortedCoord = texCoord + float2(wave, 0);

    // 2. ОГРАНИЧЕНИЕ (Clamp)
    // Не даем координате выйти за пределы текущего тайла по горизонтали
    float minX = floor(texCoord.x * (1024.0 / 34.0)) * (34.0 / 1024.0); // Начало тайла
    float maxX = minX + (34.0 / 1024.0); // Конец тайла
    
    // Если вышли за границы - пиксель станет прозрачным или возьмет крайний пиксель символа
    distortedCoord.x = clamp(distortedCoord.x, minX, maxX);

    // 3. Сэмплирование (теперь безопасно)
    float4 tex = tex2D(TextureSampler, distortedCoord);
    
    // Если после клэмпа мы всё еще за пределами реального рисунка символа 
    // (например, попали в пустые пиксели между символами), discard их
    if (distortedCoord.x <= minX || distortedCoord.x >= maxX)
        tex.a = 0;
    
    if (tex.a < 0.1)
        discard;

    // --- 3. ВАШ ГРАДИЕНТ (используем новые координаты) ---
    float pixelY = distortedCoord.y * atlasHeight;

    float3 top = float3(1.0, 1.0, 1.0);
    float3 bot = float3(0.3, 0.3, 0.3);
    float3 finalColor = lerp(top, bot, localY);

    // --- 4. ВАШ BEVEL (блик) ---
    // Сдвиг для блика тоже должен учитывать искажение, если оно сильное, 
    // но для 1 пикселя можно оставить так
    float above = tex2D(TextureSampler, distortedCoord - float2(0, 1.0 / atlasHeight)).a;
    if (tex.a > above)
        finalColor += 0.3;

    return float4(tex.rgb * finalColor, tex.a) * color;
}

// Техника (проход)
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}