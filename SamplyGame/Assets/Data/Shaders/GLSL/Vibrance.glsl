#include "Uniforms.glsl"
#include "Samplers.glsl"
#include "Transform.glsl"
#include "ScreenPos.glsl"
#include "Lighting.glsl"

varying vec2 vScreenPos;

uniform float amount = 1.0;
const vec4 coeff = vec4(0.299,0.587,0.114, 0.);

void VS()
{
    mat4 modelMatrix = iModelMatrix;
    vec3 worldPos = GetWorldPos(modelMatrix);
    gl_Position = GetClipPos(worldPos);
    vScreenPos = GetScreenPosPreDiv(gl_Position);
}

void PS()
{
    vec4 color = texture2D(sDiffMap, vScreenPos);

    float lum = dot(color, coeff);
    vec4 mask = (color - vec4(lum));
    mask = clamp(mask, 0.0, 1.0);
    float lumMask = dot(coeff, mask);
    lumMask = 1.0 - lumMask;
    gl_FragColor = mix(vec4(lum), color, 1.0 + amount * lumMask);
}