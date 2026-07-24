sampler TextureSampler : register(s0);

// Параметры, которые мы обновим перед отрисовкой
float2 TextYRange; // x = StartY на экране, y = EndY на экране
float2 InvAtlasSize; // 1.0 / ширина_атласа, 1.0 / высота_атласа

// Входные данные от SpriteBatch (MonoGame/XNA)
struct VertexToPixel
{
    float4 Position : SV_Position; // Позиция пикселя на экране
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 MainPS(VertexToPixel input) : COLOR0
{
    float4 tex = tex2D(TextureSampler, input.TexCoord);
    if (tex.a < 0.1)
        discard;

    // Вычисляем градиент по экранным координатам
    // input.Position.y — это координата текущего пикселя на экране в px
    float localY = (input.Position.y - TextYRange.x) / (TextYRange.y - TextYRange.x);
    localY = saturate(localY);

    float3 top = float3(1.0, 0.9, 0.4);
    float3 bot = float3(0.6, 0.4, 0.1);
    float3 finalColor = lerp(top, bot, localY);

    // Bevel (используем прозрачность соседнего пикселя в атласе)
    float above = tex2D(TextureSampler, input.TexCoord - float2(0, InvAtlasSize.y)).a;
    if (tex.a > above)
        finalColor += 0.3;

    return float4(tex.rgb * finalColor, tex.a) * input.Color;
}

// Техника (проход)
technique Lighting
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 MainPS();
    }
}