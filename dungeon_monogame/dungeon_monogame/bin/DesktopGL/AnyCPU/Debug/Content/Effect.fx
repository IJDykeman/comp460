
struct VertexToPixel
{
	float4 Position   	: POSITION;
	float4 Normal   : TEXCOORD2;
	float4 Color		: COLOR0;
	float4 Paint		: COLOR1;
	float4 emissive		: COLOR2;
	float LightingFactor : TEXCOORD0;
	float2 TextureCoords: TEXCOORD1;
};

struct PixelToFrame
{
	float4 Color : COLOR0;
};





//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;

float4x4 xShadowView;
float4x4 xShadowProjection;
float4x4 xShadowWorld;
float3 xLightPos;
float2 ShadowMapPixelSize;
float2 ShadowMapSize;

float3 xLightDirection;
float xAmbient;
float xOpacity;
bool xEnableLighting;
bool xShowNormals;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSize;
float4 xTint;

//------- Texture Samplers --------

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture>; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = mirror; AddressV = mirror; };
//change LINEAR to POINT for blocky textures




//------- Technique: Colored --------

VertexToPixel ColoredVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float4 inColor : COLOR, float4 inPaint : COLOR1)
{
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	Output.Position = mul(inPos, preWorldViewProjection);
	//Output.Position3D = mul(inPos, xWorld);
	Output.TextureCoords = inPos;
	Output.Color = inColor;

	Output.Paint = inPaint;
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));
	Output.Normal = float4(Normal.xyz, 1);
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = saturate(dot(Normal, xLightDirection)) + xAmbient;
	


	return Output;
}


PixelToFrame ColoredPS(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;
	Output.Color = PSIn.Color;
	Output.Color = Output.Color * PSIn.LightingFactor;
	Output.Color = Output.Color * PSIn.Paint;
	//Output.Color = float4(PSIn.LightingFactor, PSIn.LightingFactor, 1, 1);
	//Output.Color = float4(PSIn.Normal.xyz/2.0 + .5, 1);
	return Output;
	
}


technique Colored
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 ColoredVS();
		PixelShader = compile  ps_3_0 ColoredPS();
	}
}



//------- Technique: Diffuse --------

VertexToPixel DiffuseVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float4 inColor : COLOR, float4 inPaint : COLOR1)
{
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
	return Output;
}



PixelToFrame DirectOutputPS(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;
	Output.Color = PSIn.Color;
	return Output;
	
}

technique Diffuse
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 DiffuseVS();
		PixelShader = compile  ps_3_0 DirectOutputPS();
	}
}

//------- Technique: Emissive --------

VertexToPixel EmissiveVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float4 inColor : COLOR, float4 inPaint : COLOR1, float4 emissive : COLOR2)
{
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul(xView, xProjection);
		float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
		Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = emissive;
	return Output;
}



PixelToFrame EmissivePS(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;
	Output.Color = PSIn.Color;
	return Output;

}

technique Emissive
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 EmissiveVS();
		PixelShader = compile  ps_3_0 EmissivePS();
	}
}

//------- Technique: Normal --------

VertexToPixel NormalVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float4 inColor : COLOR, float4 inPaint : COLOR1)
{
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = float4(inNormal.xyz / 2.0 + .5, 1);
	return Output;
}


technique Normal
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 NormalVS();
		PixelShader = compile  ps_3_0 DirectOutputPS();
	}
}



//------- Technique: Indirect --------

VertexToPixel IndirectVS(float4 inPos : POSITION, float3 inNormal : NORMAL, float4 inColor : COLOR, float4 inPaint : COLOR1)
{
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inPaint;
	return Output;
}


technique Indirect
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 IndirectVS();
		PixelShader = compile  ps_3_0 DirectOutputPS();
	}
}


