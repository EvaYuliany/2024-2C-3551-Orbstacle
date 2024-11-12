#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;
float3 LightDirection : LIGHTDIRECTION;

// Texturas y samplers
texture NormalTexture : register(t1);
sampler NormalSampler = sampler_state
{
    Texture = <NormalTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 TangentLightDir : TEXCOORD1;
    float3 TangentViewDir : TEXCOORD2;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    // Calcular la posición en el mundo
    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(mul(worldPosition, View), Projection);
    output.TexCoord = input.TexCoord;

    // Matriz TBN (Tangent, Binormal, Normal)
    float3x3 TBN = float3x3(normalize(input.Tangent), normalize(input.Binormal), normalize(input.Normal));

    // Transformar la dirección de la luz y la dirección de la vista al espacio del Tangente
    output.TangentLightDir = mul(TBN, normalize(LightDirection));
    output.TangentViewDir = mul(TBN, normalize(-worldPosition.xyz));

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Obtener la normal desde el normal map y convertirla de [0,1] a [-1,1]
    float3 normalFromMap = tex2D(NormalSampler, input.TexCoord).xyz * 2.0f - 1.0f;
    normalFromMap = normalize(normalFromMap);

    // Normalizar direcciones de luz y vista
    float3 lightDir = normalize(input.TangentLightDir);
    float3 viewDir = normalize(input.TangentViewDir);

    // Calcular iluminación difusa usando la normal del normal map
    float diffuseIntensity = max(dot(normalFromMap, lightDir), 0.0);

    // Calcular componente especular (Phong)
    float3 halfVector = normalize(lightDir + viewDir);
    float specularIntensity = pow(max(dot(normalFromMap, halfVector), 0.0), 16.0); // Ajusta el exponente para brillo especular

    // Luz ambiental
    float ambientLight = 0.2;
    float totalIntensity = ambientLight + diffuseIntensity + specularIntensity;

    // Aplicar el color difuso
    return float4(DiffuseColor * totalIntensity, 1.0);
}

technique NormalMapping
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
