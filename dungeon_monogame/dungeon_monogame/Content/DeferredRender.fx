
struct VertexToPixel
{
	float4 Position   	: POSITION;
	float4 Position2   	: TEXCOORD0;
	float4 Normal   : TEXCOORD2;
	float4 Color		: COLOR0;
	float4 Paint		: COLOR1;
	//float LightingFactor : TEXCOORD0;
	float2 TextureCoords: TEXCOORD1;
	float2 Depth: TEXCOORD3;
};

struct PixelToFrame
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
};





//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;

//float4x4 xShadowView;
//float4x4 xShadowProjection;
//float4x4 xShadowWorld;
//float3 xLightPos;
//float2 ShadowMapPixelSize;
//float2 ShadowMapSize;

float3 xLightDirection;
float xAmbient;
//float xOpacity;
//bool xEnableLighting;
//bool xShowNormals;
//float3 xCamPos;
//float3 xCamUp;
//float xPointSpriteSize;
//float4 xTint;

//------- Texture Samplers --------

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture>; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = mirror; AddressV = mirror; };
//change LINEAR to POINT for blocky textures




//------- Technique: RenderGBuffer --------

VertexToPixel ColoredVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float4 inColor : COLOR, float4 inPaint : COLOR1)
{
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	inPos = float4(inPos.xyz,1);
	float4 pos = mul(inPos, preWorldViewProjection);
	Output.Position = pos;
	Output.Position2 = pos;
	//Output.Position3D = mul(inPos, xWorld);
	Output.TextureCoords = inPos;
	Output.Color = inColor;

	Output.Paint = inPaint;
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));
	Output.Normal = float4(Normal.xyz, 1);

	Output.Depth.x = Output.Position.z;
	Output.Depth.y = Output.Position.w;
	//Output.LightingFactor = 1;
	//Output.LightingFactor = saturate(dot(Normal, xLightDirection)) + xAmbient;



	return Output;
}


PixelToFrame GBufferPS(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;
	Output.Color = PSIn.Color;
	//Output.Color = Output.Color * PSIn.LightingFactor;
	Output.Color = Output.Color * PSIn.Paint;
	//Output.Color = float4(PSIn.LightingFactor, PSIn.LightingFactor, 1, 1);
	//Output.Color = float4(PSIn.Normal.xyz/2.0 + .5, 1);
	Output.Normal = PSIn.Normal / 2 + .5;
	//Output.Depth.x = PSIn.Position2.z;
	//Output.Depth.y = PSIn.Position2.w;
	Output.Depth = PSIn.Depth.x / PSIn.Depth.y;
	//Output.Position = PSIn.Position2;
	return Output;

}


technique RenderGBuffer
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 ColoredVS();
		PixelShader = compile  ps_2_0 GBufferPS();
	}
}

