#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;

float Time = 0;
float3 LightDirection : LIGHTDIRECTION;

texture NormalTexture : register(t1);
sampler NormalSampler = sampler_state
{
    Texture = (NormalTexture);
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	 float3 Normal : NORMAL;
	 float2 TexCoord : TEXCOORD0;  
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
	    float2 TexCoord : TEXCOORD0;

};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);

    float4 viewPosition = mul(worldPosition, View);	

    output.Position = mul(viewPosition, Projection);
    output.Normal = normalize(mul(input.Normal, (float3x3)World));
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	    float3 normalFromMap = tex2D(NormalSampler, input.TexCoord).xyz * 2.0f - 1.0f;  // Mapeamos a rango [-1,1]
    normalFromMap = normalize(normalFromMap);

	 // Intensidad de la luz usando el producto punto entre la dirección de la luz y la normal de la superficie
    float lightIntensity = max(dot(normalFromMap,LightDirection), 0.0);

    // Usamos un factor para aumentar la intensidad de la luz (por ejemplo, multiplicar por 2)
    lightIntensity = lightIntensity * 2.0;

    // Definir una luz ambiental mínima (para evitar que se vea negra)
    float ambientLight = 1; // Puedes ajustar este valor
    lightIntensity += ambientLight;

    // Modificamos el color de la superficie con la luz calculada
    return float4(DiffuseColor * lightIntensity, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
