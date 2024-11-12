#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Matrices de transformación
float4x4 WorldViewProjection;
float4x4 World;
float4x4 InverseTransposeWorld;

// Propiedades de iluminación
float3 ambientColor;
float3 diffuseColor;
float3 specularColor;
float KAmbient;
float KDiffuse;
float KSpecular;
float shininess;
float3 lightPosition;
float3 eyePosition;

// Propiedades de texturas y color base
float3 BaseColor;
float2 Tiling;

texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
    MIPFILTER = LINEAR;
};

texture NormalTexture;
sampler2D normalSampler = sampler_state
{
    Texture = (NormalTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

// Obtener normales desde el mapa de normales
float3 getNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

    float3 Q1 = ddx(worldPosition);
    float3 Q2 = ddy(worldPosition);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    worldNormal = normalize(worldNormal.xyz);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    return normalize(mul(tangentNormal, TBN));
}

// Estructuras de entrada y salida del Vertex Shader
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
};

// Vertex Shader
VertexShaderOutput CombinedVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPosition = mul(input.Position, World);
    output.Normal = mul(input.Normal, InverseTransposeWorld);
    output.TextureCoordinates = input.TextureCoordinates * Tiling;

    return output;
}

// Pixel Shader
float4 CombinedPS(VertexShaderOutput input) : COLOR
{
    // Direcciones de luz y cámara
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);

    // Obtener normal desde el mapa si está disponible
    float3 normal = getNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, normalize(input.Normal.xyz));

    // Obtener color de la textura, si existe; de lo contrario, usar BaseColor
    float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
    if (texelColor.a == 0) // Si no hay textura, usar el color base
    {
        texelColor = float4(BaseColor, 1.0);
    }

    // Cálculo de la luz difusa
    float NdotL = saturate(dot(normal, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

    // Cálculo de la luz especular
    float NdotH = dot(normal, halfVector);
    float3 specularLight = KSpecular * specularColor * pow(saturate(NdotH), shininess);

    // Cálculo final del color
    float4 finalColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);
    return finalColor;
}

technique CombinedTechnique
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL CombinedVS();
        PixelShader = compile PS_SHADERMODEL CombinedPS();
    }
};
