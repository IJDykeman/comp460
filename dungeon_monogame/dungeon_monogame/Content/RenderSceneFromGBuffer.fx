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
float lightIntensity;
float3 lightPosition;

//how far does this light reach
//float lightRadius;
//control the brightness of the light
//float lightIntensity = 1.0f;


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

float4 DirectionalLightPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	
	//get normal data from the normalMap
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	float4 colorData = tex2D(colorSampler, input.TexCoord);
	float4 emissiveData = tex2D(emissiveSampler, input.TexCoord);
	emissiveData = emissiveData;
	float3 lightVector = -normalize(lightDirection);
	//compute diffuse light
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	float NdL = max(0, dot(normal, lightVector)) + .3;
	float3 diffuseLight = NdL * colorData.rgb;
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
	//surface-to-light vector

	//reflexion vector
	float3 reflectionVector = normalize(reflect(lightVector, normal));
	//camera-to-surface vector
	float3 directionToCamera = normalize(cameraPosition - position);
	//compute specular light
	float specularLight = specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower);
	//output the two lights
	return float4(diffuseLight.rgb * lightIntensity + (emissiveData.rgb * 2), 1);
}

float4x4 InvertView;
float4x4 InvertProjection;
float lightRadius;

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



technique DirectionalLightTechnique
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 DirectionalLightPixelShaderFunction();
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



