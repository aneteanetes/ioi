shader_type canvas_item;

uniform float pixel_size : hint_range(1.0, 100.0) = 3.0;

void fragment() {
    vec2 size = vec2(textureSize(TEXTURE, 0));
    vec2 uv = floor(UV * size / pixel_size) * pixel_size / size;
    COLOR = texture(TEXTURE, uv);
}
