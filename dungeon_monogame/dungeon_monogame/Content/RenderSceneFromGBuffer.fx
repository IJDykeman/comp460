//direction of the light
float3 lightDirection;
//color of the light 
float4 lightColor;
//position of the camera, for specular light
float3 cameraPosition;
//this is used to compute the world-position
float4x4 InvertViewProjection;
// diffuse color, and specularIntensity in the alpha channel
texture colorMap;
// normals, and specularPower in the alpha channel
texture normalMap;
//depth
texture depthMap;
texture positionMap;
texture emissiveMap;
texture shadowDepthMap;
// used to compute distances to lights
float4x4 lightView;
float4x4 lightProjection;
float lightIntensity;
float3 lightPosition;

//how far does this light reach
//float lightRadius;
//control the brightness of the light
//float lightIntensity = 1.0f;
float4x4 InvertView;
float4x4 InvertProjection;
float lightRadius;

sampler colorSampler = sampler_state
{
	Texture = (colorMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};
sampler depthSampler = sampler_state
{
	Texture = (depthMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
	Texture = (normalMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};


sampler positionSampler = sampler_state
{
	Texture = (positionMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};


sampler emissiveSampler = sampler_state
{
	Texture = (emissiveMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

sampler shadowDepthMapSampler = sampler_state
{
	Texture = (shadowDepthMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};



struct VertexShaderInput
{
	float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};
float2 halfPixel;
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = float4(input.Position, 1);
	//align texture coordinates
	output.TexCoord = input.TexCoord - halfPixel;
	return output;
}

float4 EmmissiveMaterialsPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 emissiveData = tex2D(emissiveSampler, input.TexCoord);
	return float4((emissiveData.rgb * 2), 1);
}

float4 DirectionalLightPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	
	//get normal data from the normalMap
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	float4 colorData = tex2D(colorSampler, input.TexCoord);
	float4 emissiveData = tex2D(emissiveSampler, input.TexCoord);
	//compute diffuse light
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	float NdL = max(0, -dot(normal, lightDirection));
	float3 diffuseLight = NdL * colorData.rgb;
	//return float4(diffuseLight.rgb, 1);

	//tranform normal back into [-1,1] range


	float depthVal = tex2D(depthSampler, input.TexCoord).r;
	//compute screen-space position
	float4 position;
	float4 position2;

	position.x = input.TexCoord.x * 2.0f - 1.0f;
	position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	position.z = depthVal;
	position.w = 1.0f;
	//transform to world space
	position = mul(position, InvertViewProjection);
	position /= position.w;

	position2=position;
	position2.z *=1;


	//surface-to-light vector
	float4x4 lightViewProjection = mul(lightView, lightProjection);
	float4 shadowMapTexCoord = mul(position, lightViewProjection);
	shadowMapTexCoord.x = shadowMapTexCoord.x/shadowMapTexCoord.w/2 +  .5;
	shadowMapTexCoord.y = -shadowMapTexCoord.y/shadowMapTexCoord.w/2 + .5;
	float4 shadowMapDepthData = tex2D(shadowDepthMapSampler, shadowMapTexCoord); 


	float3 result = (diffuseLight.rgb * lightIntensity + (emissiveData.rgb));
	float4 lightViewProjPos = mul(position, lightViewProjection);
	float d = (lightViewProjPos.z / lightViewProjPos.w);
	float s = shadowMapDepthData.r;
	if (
		(s + .003f< d) ||
		//(s + .003f< d-100000) ||
		shadowMapTexCoord.x > 1 || shadowMapTexCoord.y > 1 || shadowMapTexCoord.x < 0 || shadowMapTexCoord.y < 0 ){
		result *=0;
	}
	if(d < 0 || d > 1 ){
		result *=0;
	}

	return float4(result, 1);
}


float4 SpotLightPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	
	//get normal data from the normalMap
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	float4 colorData = tex2D(colorSampler, input.TexCoord);
	float4 emissiveData = tex2D(emissiveSampler, input.TexCoord);
	//compute diffuse light
	float3 normal = 2.0f * normalData.xyz - 1.0f;
	


	float depthVal = tex2D(depthSampler, input.TexCoord).r;
	//compute screen-space position
	float4 position;
	float4 position2;

	position.x = input.TexCoord.x * 2.0f - 1.0f;
	position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	position.z = depthVal;
	position.w = 1.0f;
	//transform to world space
	position = mul(position, InvertViewProjection);
	position /= position.w;

	position2=position;
	position2.z *=1;


	float3 lightVector = lightPosition - position;
	float attenuation = saturate(pow(max(0, 1.0f - length(lightVector) / lightRadius),2));
	//normalize light vector
	lightVector = normalize(lightVector);
	//compute diffuse light
	float NdL = max(.05, dot(normal, lightVector));
	//NdL = sqrt(-1 / (NdL + 1) + 1);
	float3 diffuseLight = NdL * colorData.rgb;
	float3 result = (diffuseLight.rgb * lightIntensity*attenuation + (emissiveData.rgb));

	float4x4 lightViewProjection = mul(lightView, lightProjection);
	float4 shadowMapTexCoord = mul(position, lightViewProjection);
	shadowMapTexCoord.x = shadowMapTexCoord.x/shadowMapTexCoord.w/2 +  .5;
	shadowMapTexCoord.y = -shadowMapTexCoord.y/shadowMapTexCoord.w/2 + .5;
	float4 shadowMapDepthData = tex2D(shadowDepthMapSampler, shadowMapTexCoord); 



	float4 lightViewProjPos = mul(position, lightViewProjection);
	float d = (lightViewProjPos.z / lightViewProjPos.w);
	float s = shadowMapDepthData.r;
	if (
		(s + .003f< d) ||
		//(s + .003f< d-100000) ||
		shadowMapTexCoord.x > 1 || shadowMapTexCoord.y > 1 || shadowMapTexCoord.x < 0 || shadowMapTexCoord.y < 0 ){
		result *=0;
	}
	if(d < 0 || d > 1 ){
		result *=0;
	}

	return float4(result, 1);
}




float4 PointLightPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//float lightRadius = 10;
	
	//get normal data from the normalMap
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	float4 colorData = tex2D(colorSampler, input.TexCoord);
	float4 positionData = tex2D(positionSampler, input.TexCoord);

	//compute diffuse light
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	//return float4(diffuseLight.rgb, 1);

	//tranform normal back into [-1,1] range


	//get specular power, and get it into [0,255] range]
	float specularPower = normalData.a * 255;
	//get specular intensity from the colorMap
	float specularIntensity = tex2D(colorSampler, input.TexCoord).a;
	//read depth
	float depthVal = tex2D(depthSampler, input.TexCoord).r;
	//compute screen-space position
	float4 position;
	position.x = input.TexCoord.x * 2.0f - 1.0f;
	position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	position.z = depthVal;
	position.w = 1.0f;
	//transform to world space
	position = mul(position, InvertViewProjection);
	position /= position.w;

	float3 lightVector = lightPosition - position;
	float attenuation = saturate(pow(max(0, 1.0f - length(lightVector) / lightRadius),2));
	//normalize light vector
	lightVector = normalize(lightVector);
	//compute diffuse light
	float NdL = max(.05, dot(normal, lightVector));
	NdL = sqrt(-1 / (NdL + 1) + 1);
	float3 diffuseLight = NdL * colorData.rgb;
		//reflection vector
		float3 reflectionVector = normalize(reflect(-lightVector, normal));
		//camera-to-surface vector
		float3 directionToCamera = normalize(cameraPosition - position);
		//compute specular light
		float specularLight = specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower);
	//take into account attenuation and lightIntensity.
	float3 finalColor = attenuation * lightIntensity * diffuseLight.rgb;
	//return float4(diffuseLight.rgb * 0 + position.xyz / 10,1);
	return float4(finalColor.rgb * lightColor, 1);
}


float4 HemisphereLightPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//float lightRadius = 10;
	
	//get normal data from the normalMap
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	float4 colorData = tex2D(colorSampler, input.TexCoord);
	float4 positionData = tex2D(positionSampler, input.TexCoord);

	//compute diffuse light
	float3 normal = 2.0f * normalData.xyz - 1.0f;




	//get specular power, and get it into [0,255] range]
	float specularPower = normalData.a * 255;
	//get specular intensity from the colorMap
	float specularIntensity = tex2D(colorSampler, input.TexCoord).a;
	//read depth
	float depthVal = tex2D(depthSampler, input.TexCoord).r;
	//compute screen-space position
	float4 position;
	position.x = input.TexCoord.x * 2.0f - 1.0f;
	position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	position.z = depthVal;
	position.w = 1.0f;
	//transform to world space
	position = mul(position, InvertViewProjection);
	position /= position.w;

	float3 lightVector = lightPosition - position;
	float attenuation = saturate(pow(max(0, 1.0f - length(lightVector) / lightRadius),2));
	//normalize light vector
	lightVector = normalize(lightVector);
	//compute diffuse light
	float NdL = max(.05, dot(normal, lightVector));
	NdL = sqrt(-1 / (NdL + 1) + 1);
	float3 diffuseLight = NdL * colorData.rgb;
		//reflection vector
		float3 reflectionVector = normalize(reflect(-lightVector, normal));
		//camera-to-surface vector
		float3 directionToCamera = normalize(cameraPosition - position);
		//compute specular light
		float specularLight = specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower);
	//take into account attenuation and lightIntensity.
	float3 finalColor = attenuation * lightIntensity * diffuseLight.rgb;
	//return float4(diffuseLight.rgb * 0 + position.xyz / 10,1);
	if (dot(-lightDirection,lightVector) <0){
		return 0;
	}
	return float4(finalColor.rgb * lightColor, 1);
}



technique DirectionalLightTechnique
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 DirectionalLightPixelShaderFunction();
	}
}



technique SpotLightTechnique
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 SpotLightPixelShaderFunction();
	}
}


technique EmmissiveMaterialsTechnique
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 EmmissiveMaterialsPixelShaderFunction();
	}
}

technique PointLightTechnique
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PointLightPixelShaderFunction();
	}
}

technique HemisphereLightTechnique
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 HemisphereLightPixelShaderFunction();
	}
}



