shader_type spatial;
render_mode unshaded, cull_disabled;

uniform vec3 grid_color : source_color = vec3(0.25, 0.3, 0.35);
uniform vec3 thick_color : source_color = vec3(0.15, 0.2, 0.25);
uniform float grid_size = 1.0;
uniform float thick_every = 10.0;

void fragment() {
    vec3 pos = VERTEX;
    
    float dist_x = mod(abs(pos.x), grid_size * thick_every);
    float dist_z = mod(abs(pos.z), grid_size * thick_every);
    
    float line_x = step(dist_x, grid_size * 0.05);
    float line_z = step(dist_z, grid_size * 0.05);
    
    float thin_x = step(mod(abs(pos.x), grid_size), grid_size * 0.02);
    float thin_z = step(mod(abs(pos.z), grid_size), grid_size * 0.02);
    
    float grid = max(max(line_x, line_z), max(thin_x, thin_z));
    
    vec3 color = mix(thick_color, grid_color, step(0.5, mod(floor(pos.x / grid_size) + floor(pos.z / grid_size), 2.0)));
    ALBEDO = mix(color, grid_color, grid);
}