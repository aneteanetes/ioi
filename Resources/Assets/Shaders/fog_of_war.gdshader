shader_type canvas_item;

// Текстура маски тумана из C# (размером с сетку карты)
uniform sampler2D fog_texture : filter_nearest, repeat_disable;

void fragment() {
    // Читаем значение тумана по координатам ColorRect
    float fog_value = texture(fog_texture, UV).r;
    
    // По умолчанию пиксель тумана черный
    vec4 fog_color = vec4(0.0, 0.0, 0.0, 1.0);
    
    if (fog_value > 0.9) {
        // Полностью видимая зона — делаем ColorRect прозрачным в этом месте,
        // чтобы стал виден свет и мир под ним
        fog_color.a = 0.0; 
    } else if (fog_value > 0.4) {
        // Исследованная зона (полупрозрачная тень)
        fog_color.a = 0.75; // Настройте прозрачность (0.0 - видно всё, 1.0 - тьма)
    } else {
        // Неисследованная зона (абсолютная тьма)
        fog_color.a = 1.0;
    }
    
    COLOR = fog_color;
}
