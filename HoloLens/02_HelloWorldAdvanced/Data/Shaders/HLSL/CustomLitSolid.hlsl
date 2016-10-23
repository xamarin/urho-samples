#include "Uniforms.hlsl"
#include "Samplers.hlsl"
#include "Transform.hlsl"
#include "ScreenPos.hlsl"
#include "Lighting.hlsl"

// parameters for PS
// defined in materials (see Materials/Earth.xml)
#ifdef COMPILEPS
uniform float cCloudsFactor;
uniform float4 cSpecColor;
uniform float2 cCloudsOffset;
#endif

void PS(
	float4 iTexCoord : TEXCOORD0,
	float4 iTangent : TEXCOORD3,
	float3 iNormal : TEXCOORD1,
	float4 iWorldPos : TEXCOORD2,
	float4 iScreenPos : TEXCOORD5,
	out float4 oColor : OUTCOLOR0)
{
	float4 earthDiff = Sample2D(DiffMap, iTexCoord.xy);
	float4 clouds = Sample2D(EnvMap, iTexCoord.xy + cCloudsOffset);
	float4 night = Sample2D(EmissiveMap, iTexCoord.xy);
	float3 specColor = cSpecColor.rgb * Sample2D(SpecMap, iTexCoord.xy).rgb;
	float3x3 tbn = float3x3(iTangent.xyz, float3(iTexCoord.zw, iTangent.w), iNormal);
	float3 normal = normalize(mul(DecodeNormal(Sample2D(NormalMap, iTexCoord.xy)), tbn));

	float3 lightDir;
	float3 finalColor;

	// Earth texture
	finalColor = earthDiff.rgb;
	// Specular map
	finalColor += specColor * cLightColor.a;
	// Normal map
	finalColor *= GetDiffuse(normal, iWorldPos.xyz, lightDir);
	// Clouds:
	finalColor += clouds.rgb * cCloudsFactor;
	// Nigth lamps
	finalColor += night.rgb;

	// Return final color
	oColor = float4(finalColor, 1.0);
}


// Default LitSolid Vertex Shader impl:
void VS(float4 iPos : POSITION,
		float3 iNormal : NORMAL,
		float2 iTexCoord : TEXCOORD0,
		float4 iTangent : TANGENT,
	out float4 oTexCoord : TEXCOORD0,
	out float4 oTangent : TEXCOORD3,
	out float3 oNormal : TEXCOORD1,
	out float4 oWorldPos : TEXCOORD2,
	out float4 oScreenPos : TEXCOORD5,
	out float4 oPos : OUTPOSITION)
{
	float3 worldPos = GetWorldPos(iModelMatrix);
	oPos = GetClipPos(worldPos);
	oNormal = GetWorldNormal(iModelMatrix);
	oWorldPos = float4(worldPos, GetDepth(oPos));
	float3 tangent = GetWorldTangent(iModelMatrix);
	float3 bitangent = cross(tangent, oNormal) * iTangent.w;
	oTexCoord = float4(GetTexCoord(iTexCoord), bitangent.xy);
	oTangent = float4(tangent, bitangent.z);
}