uniform float TIME : register(c0);

texture mainTex;
sampler mainTexSampler = sampler_state
{
    texture = <mainTex>;
};


// Constants for noise generation
static const float3x3 NOISE_MATRIX =
{
    13.323122, 21.1212, 21.8112, 
    23.5112, 28.7312, 14.7212, 
    21.71123, 11.9312, 61.3934 
};

// Helper function to generate snowflake pattern
float3 generate_snowflake(float2 coord, float layer_index, float time, float wind_strength)
{

    // Snow appearance parameters
    float spread = 0.5; // Snowflake spread : hint_range(0.0, 1.5)
    float size = 0.5; // Snowflake size: hint_range(0.01, 5.0)

    // Snow movement parameters
    float speed = 0.5; // Fall speed : hint_range(0.0, 10.0)
    
    
    // Scale coordinates based on layer depth
    float layer_scale = 1.0 + layer_index * 0.5 / (max(size, 0.01) * 2.0);
    float2 scaled_coord = coord * layer_scale;

    // Apply movement (falling + wind)
    float2 movement = float2(
        scaled_coord.y * (spread * fmod(layer_index * 7.238917, 1.0) - spread * 0.5) +
        (-wind_strength) * speed * time * 0.5, // Reverse the wind direction by negating wind_strength
        -speed * time / (1.0 + layer_index * 0.5 * 0.03)
    );
    float2 final_coord = scaled_coord + movement;
        
    // Generate noise pattern
    float3 noise_input = float3(floor(final_coord), 31.189 + layer_index);
    float3 noise_val = floor(noise_input) * 0.00001 + frac(noise_input);
    float3 random = frac((31415.9 + noise_val) / mul(NOISE_MATRIX, noise_val));

    // Calculate snowflake shape
    float2 shape = abs(fmod(final_coord, 1.0) - 0.5 + 0.9 * random.xy - 0.45);
    shape += 0.01 * abs(2.0 * frac(10.0 * final_coord.yx) - 1.0);

    // Calculate edge softness
    float depth_offset = 5.0 * sin(time * 0.1);
    float edge_softness = 0.005 + 0.05 * min(0.5 * abs(layer_index - 5.0 - depth_offset), 1.0);

    // Calculate final shape
    float shape_value = 0.6 * max(shape.x - shape.y, shape.x + shape.y) + max(shape.x, shape.y) - 0.01;

    return smoothstep(edge_softness, -edge_softness, shape_value) *
                (random.x / (1.0 + 0.02 * layer_index * 0.5));
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR
{
    int num_of_layers = 40; // Depth layers
    float4 snow_color = float4(1.0, 1.0, 1.0, 1.0); // Snow color  : source_color
    float snow_transparency = 0.2; // Intensity of the snow transparency : hint_range(-0.5, 1.0)
    float wind = 0.0; // Wind direction and strength  : hint_range(-2.0, 2.0)
    
    float3 snow_accumulation = float3(0,0,0);

    // Generate snow for each layer
    for (int i = 0; i < num_of_layers; i++)
    {
        snow_accumulation += generate_snowflake(uv, float(i), TIME, wind);
    }

    // Calculate final color
    float snow_intensity = clamp(length(snow_accumulation), 0.0, 1.0);
    float4 base_color = tex2D(mainTexSampler, uv);

    // Apply transparency effect to the snow color
    float4 transparency_color = float4(snow_color.rgb * (1.0 + snow_transparency), snow_intensity);

    return lerp(base_color, transparency_color, snow_intensity);
}

technique T0
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}