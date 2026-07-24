sampler TextureSampler : register(s0);

float4 MainPS(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 tex = tex2D(TextureSampler, texCoord);
    if (tex.a < 0.1)
        discard;
    
    float atlasHeight = 1024.0;
    float tileHeight = 34.0;

    // coords
    float pixelY = (texCoord.y + 1) * atlasHeight;
    float localY = (pixelY % tileHeight) / tileHeight;
    
    // gradient (gold/bronze)
    float3 top = float3(1.0, 1.0, 1.0);
    float3 bot = float3(0.3, 0.3, 0.3);
    
    //float3 top = float3(1.0, 0.9, 0.4);
    //float3 bot = float3(0.6, 0.4, 0.1);
    
    //float3 top = float3(0.95, 0.92, 0.82); // crema/gold
    //float3 bot = float3(0.55, 0.48, 0.38); // gray/brown
    
    //float3 top = float3(0.85, 0.75, 0.45); // dark gold
    //float3 bot = float3(0.35, 0.18, 0.08); // dark brown
    float3 finalColor = lerp(top, bot, localY);

    // Bevel
    float above = tex2D(TextureSampler, texCoord - float2(0, 1.0 / atlasHeight)).a;
    if (tex.a > above)
        finalColor += 0.3; // glare

    return float4(tex.rgb * finalColor, tex.a) * color;
}

// Техника (проход)
technique Lighting
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}