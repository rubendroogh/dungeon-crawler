shader_type canvas_item;

uniform float enable_outline : hint_range(0.0, 1.0) = 1.0; // 1 = on, 0 = off
uniform float outline_size : hint_range(0.0, 32.0) = 2.0;  // pixels
uniform vec4 outline_color : source_color = vec4(0, 0, 0, 1);

void vertex() {
	if (enable_outline > .5) {
		VERTEX += (UV * 2.0 - 1.0) * outline_size;
	}
}

void fragment() {
    vec2 uv = UV;
	
	if (enable_outline < .5)
	{
		COLOR = texture(TEXTURE, uv);
	}
	else
	{
		vec2 texture_pixel_size = vec2(1.0) / (vec2(1.0) / TEXTURE_PIXEL_SIZE + vec2(outline_size * 2.0));
		uv = (uv - texture_pixel_size * outline_size) * TEXTURE_PIXEL_SIZE / texture_pixel_size;
		
		if (uv != clamp(uv, vec2(0.0), vec2(1.0))) {
			COLOR = outline_color;
		} else {
			COLOR = texture(TEXTURE, uv);
		}
	}
}
