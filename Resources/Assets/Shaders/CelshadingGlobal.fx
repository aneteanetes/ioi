sampler SceneTexture : register(s0);

// Параметры для гибкости (можно передать из C#)
float2 ScreenSize; // Ширина и высота RenderTarget (например, 1920, 1080)
//float Time; // Передавай GameTime.TotalSeconds.Seconds из C#

float4 MainPS(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 tex = tex2D(SceneTexture, texCoord);
    
    if (tex.a <= 0.0)
        return float4(0, 0, 0, 0);

    float localY = texCoord.y;

    // Цвета: чистое золото (от светлого к насыщенному темному)
    // Убрали коричневый, добавив больше "золотого" веса в нижнюю точку
    float3 top = float3(0.85, 0.65, 0.25); // Теплое золото
    float3 bot = float3(0.55, 0.40, 0.10); // Глубокое золото (вместо коричневого)
    
    float3 gradient = lerp(top, bot, localY);

    // Bevel (фаска)
    float pixelStepY = 1.0 / ScreenSize.y;
    float above = tex2D(SceneTexture, texCoord - float2(0, pixelStepY)).a;

    float3 finalColor = gradient;
    if (tex.a > above)
        finalColor += 0.22;

    return float4(tex.rgb * finalColor, tex.a) * color;
 
    //float4 tex = tex2D(SceneTexture, texCoord);
    //if (tex.a <= 0.0)
    //    return float4(0, 0, 0, 0);

    //float y = texCoord.y;

    //// 1. Твое "Чистое золото"
    //float3 top = float3(0.85, 0.65, 0.25);
    //float3 bot = float3(0.55, 0.40, 0.10);
    //float3 finalColor = lerp(top, bot, y);

    //// 2. Динамический блик (бегущая полоса)
    //// Скорость и частота настраиваются через sin и Time
    //float shinePos = frac(Time * 0.2); // Полоса пробегает раз в 5 секунд
    //float shineWidth = 0.05; // Ширина полоски
    //float shine = smoothstep(shinePos - shineWidth, shinePos, y) -
    //              smoothstep(shinePos, shinePos + shineWidth, y);
    
    //finalColor += max(0, shine * 0.3); // Добавляем яркости там, где проходит блик

    //// 3. Bevel (фаска)
    //float pixelStepY = 1.0 / ScreenSize.y;
    //float above = tex2D(SceneTexture, texCoord - float2(0, pixelStepY)).a;
    //if (tex.a > above)
    //    finalColor += 0.22;

    //// 4. Виньетка (затемнение по углам для атмосферы)
    //float dist = distance(texCoord, float2(0.5, 0.5));
    //finalColor *= smoothstep(1.2, 0.5, dist);

    //return float4(tex.rgb * finalColor, tex.a) * color;
}

technique PostProcess
{
    pass Pass1
    {
        // Для пост-эффектов лучше ps_3_0, если железо позволяет
        PixelShader = compile ps_3_0 MainPS();
    }
}