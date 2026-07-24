uniform bool shaking : register(c0);//false;
uniform float amplitude : register(c1);// = 1.0;
uniform float noise_scale : register(c2);// = 20.0;
uniform float noise_speed : register(c3); // = 20.0;
uniform float samplerWidth : register(c4);
uniform float samplerHeight : register(c5);
uniform float timeSeconds : register(c6);

texture mainTex;
sampler mainTexSampler = sampler_state
{
    texture = <mainTex>;
};

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float a = hash(i + float2(0.0, 0.0));
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    float2 u = f * f * (3.0 - 2.0 * f);
    return lerp(a, b, u.x) +
  (c - a) * u.y * (1.0 - u.x) +
  (d - b) * u.x * u.y;
}


float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR
{
    if (shaking)
    {
        float4 orig = tex2D(mainTexSampler, uv);
        if (orig.a <= 0.0)
        {
            discard;
        }
    }
    
    float nx = noise((uv * noise_scale) + float2(timeSeconds * noise_speed, 0.0));
    float ny = noise(uv * noise_scale + float2(0.0, timeSeconds * noise_speed));
    float2 rand_dir = normalize(float2(nx * 2.0 - 1.0, ny * 2.0 - 1.0));
    
    float2 example = uv * noise_scale;

    float doubleValue = timeSeconds * noise_speed;
    
    float n = noise(uv * noise_scale + float2(doubleValue, doubleValue));
    
    float off = (n * 2.0 - 1.0) * amplitude;
    
    float2 uv2 = uv + rand_dir * off * float2(1 / samplerWidth, 1 / samplerHeight);
    return tex2D(mainTexSampler, uv2);
}

technique T0
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}