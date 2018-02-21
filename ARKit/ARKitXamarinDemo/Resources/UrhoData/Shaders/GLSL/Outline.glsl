#include "Uniforms.glsl"
#include "Transform.glsl"
//#ifdef COMPILEVS
uniform float cOutlineWidth;
//#endif
//#ifdef COMPILEPS
uniform vec4 cOutlineColor;// = vec4(1.0, 1.0, 1.0, 1.0);
//#endif
void VS()
{
    mat4 modelMatrix = iModelMatrix;
    vec3 worldPos = GetWorldPos(modelMatrix);
    vec3 vNormal = GetWorldNormal(modelMatrix);
    // Scale along normal
    worldPos += vNormal * cOutlineWidth;
    gl_Position = GetClipPos(worldPos);
}
void PS()
{
    gl_FragColor = cOutlineColor;
}