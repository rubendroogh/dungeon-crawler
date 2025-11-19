shader_type canvas_item;

uniform float enable_outline : hint_range(0.0, 1.0) = 1.0; // 1 = on, 0 = off
uniform float outline_size : hint_range(0.0, 32.0) = 2.0;  // pixels
uniform vec4 outline_color : source_color = vec4(0, 0, 0, 1);

void vertex() {
	if (enable_outline > .5) {
		vec2 aspect = vec2(TEXTURE_PIXEL_SIZE.x / TEXTURE_PIXEL_SIZE.y, 1.0);
        VERTEX += (UV * 2.0 - 1.0) * outline_size * aspect;
	}
}

void fragment() {
    vec2 uv = UV;

	if (enable_outline < .5)
	{
		COLOR *= texture(TEXTURE, uv);
	}
	else
	{
		// Compute UV distance to the edges
		vec2 uv_offset = outline_size * TEXTURE_PIXEL_SIZE;

		if (uv.x < uv_offset.x || uv.x > 1.0 - uv_offset.x ||
		    uv.y < uv_offset.y || uv.y > 1.0 - uv_offset.y) {
		    COLOR = outline_color;
		} else {
		    COLOR = texture(TEXTURE, uv) - outline_color;
		}
	}
}
