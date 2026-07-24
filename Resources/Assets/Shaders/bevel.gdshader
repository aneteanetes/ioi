shader_type canvas_item;

void fragment() {
	// Читаем пиксель из прозрачного вьюпорта
	vec4 tex = texture(TEXTURE, UV);
	
	// Теперь фон реально прозрачный (tex.a == 0.0), поэтому проверка сработает идеально!
	if (tex.a <= 0.0) {
		// Рисуем честный, глубокий черный цвет задника рогалика
		COLOR = vec4(0.0, 0.0, 0.0, 1.0);
	} else {
		// Общий вертикальный градиент на весь экран игры
		float localY = UV.y;
		
		vec3 top = vec3(0.85, 0.65, 0.25);
		vec3 bot = vec3(0.55, 0.40, 0.10);
		vec3 gradient = mix(top, bot, localY);
		
		// Расчет фаски относительно пикселей экрана
		float pixelStepY = TEXTURE_PIXEL_SIZE.y;
		
		// Проверяем прозрачность пикселя строго над монолитным контуром всей стены
		float above = texture(TEXTURE, UV - vec2(0.0, pixelStepY)).a;
		
		vec3 finalColor = gradient;
		
		// Если пиксель есть, а НАД ним пустота (неважно, это один тайл или стык трех тайлов)
		if (tex.a > above) {
			finalColor += 0.25; // Сочный блик на макушке объекта
		}
		
		COLOR = vec4(tex.rgb * finalColor, tex.a) * COLOR;
	}
}
