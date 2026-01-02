// https://godotshaders.com/shader/planet-coffee/

shader_type canvas_item;

uniform vec3 colorA : source_color = vec3(0.2, 0.0, 0.1);
uniform vec3 colorB : source_color = vec3(0.7, 0.3, 0.4);
uniform vec3 colorC : source_color = vec3(1.0, 0.2, 0.4);
uniform bool discomode = false;
uniform float speed : hint_range(0.1, 2.0) = 1.0;

#define iTime (TIME * speed)

float rand(vec4 p) {
    return fract(sin(p.x*1234. + p.y*2345. + p.z*3456. + p.w*4567.) * 5678.);
}

float smoothnoise(vec4 p) {
    const vec2 e = vec2(0.0, 1.0);
    vec4 i = floor(p);    // integer
    vec4 f = fract(p);    // fract
    
    f = f*f*(3. - 2.*f);
    
    return mix(mix(mix(mix(rand(i + e.xxxx),
                           rand(i + e.yxxx), f.x),
                       mix(rand(i + e.xyxx),
                           rand(i + e.yyxx), f.x), f.y),
                   mix(mix(rand(i + e.xxyx),
                           rand(i + e.yxyx), f.x),
                       mix(rand(i + e.xyyx),
                           rand(i + e.yyyx), f.x), f.y), f.z),
               mix(mix(mix(rand(i + e.xxxy),
                           rand(i + e.yxxy), f.x),
                       mix(rand(i + e.xyxy),
                           rand(i + e.yyxy), f.x), f.y),
                   mix(mix(rand(i + e.xxyy),
                           rand(i + e.yxyy), f.x),
                       mix(rand(i + e.xyyy),
                           rand(i + e.yyyy), f.x), f.y), f.z), f.w);
}

float fbm(vec3 x) {
    float v = 0.0;
    float a = 0.5;
    vec3 shift = vec3(1.0);
    for (int i = 0; i < 10; ++i) {
        v += a * smoothnoise(vec4(x, cos(iTime * 0.002) * 200.0));
        x = x * 2.0 + shift;
        a *= 0.5;
    }
    return v;
}

vec3 uvTo3D(vec2 uv) {
    float theta = uv.x * 2.0 * 3.14159265359; // Longitud
    float phi = uv.y * 3.14159265359; // Latitud
    float x = sin(phi) * cos(theta);
    float y = sin(phi) * sin(theta);
    float z = cos(phi);
    return vec3(x, y, z);
}

float max3 (vec3 v) {
  return max (max (v.x, v.y), v.z);
}

void fragment() {
    vec3 col_a = colorA;
    vec3 col_b = colorB;
    vec3 col_c = colorC;
    
    if(discomode){
        col_a = vec3(sin(iTime), sin(iTime + 7.0), cos(iTime));
        col_b = vec3(cos(iTime), cos(iTime + 7.0), sin(iTime));
        col_c = vec3(sin(iTime), cos(iTime), sin(iTime + 0.5));
    }
    
    vec2 uv = UV;
    vec3 pos = uvTo3D(uv);
    
    pos.y += sin(iTime / 5.0);
    pos.x += cos(iTime / 5.0);
    pos.z += sin(iTime / 5.0);
    
    float fbmm = fbm(pos);
    vec3 q = vec3(fbmm, sin(fbmm), cos(fbmm));
    vec3 r = vec3(fbmm, sin(fbmm), cos(fbmm));
    float v = fbm(pos + 5.0 * r + iTime * 0.005);
    
    vec3 col_top = vec3(1.0);
    vec3 col_bot = vec3(0.0);
    
    vec3 res_color = mix(col_a, col_b, clamp(r, 0.0, 1.0));
    res_color = mix(res_color, col_c, clamp(q, 0.0, 1.0));
    
    float poss = v * 2.0 - 1.0;
    res_color = mix(res_color, col_top, clamp(poss, 0.0, 1.0));
    res_color = mix(res_color, col_bot, clamp(-poss, 0.0, 1.0));
    
    res_color = res_color / max(max3(res_color), 0.001);
    
    res_color = (clamp((0.4 * pow(v, 3.0) + pow(v, 2.0) + 0.5 * v), 0.0, 1.0) * 0.9 + 0.1) * res_color;

    COLOR = vec4(res_color, 1.0);
}