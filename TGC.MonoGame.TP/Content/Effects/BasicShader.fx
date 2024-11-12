#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Variables globales
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseTransposeWorld;
float4x4 WorldViewProjection;
float2 Tiling;
float3 lightPosition;

float3 ambientColor; // Light's Ambient Color
float3 diffuseColor; // Light's Diffuse Color
float3 specularColor; // Light's Specular Color
float KAmbient; 
float KDiffuse; 
float KSpecular;
float shininess; 
float3 eyePosition; // Camera position
float3 LightDirection : LIGHTDIRECTION;

// Texturas
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

// Textura de Normal Map
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

// Función para obtener la normal desde el Normal Map
float3 getNormalFromMap(float2 textureCoordinates, float3 Position, float3 worldNormal)
{
    // Obtener el color del normal map (en el rango [-1, 1])
    float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

    // Calcular el espacio Tangente-Binormal-Normal (TBN)
    float3 Q1 = ddx(Position);
    float3 Q2 = ddy(Position);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    // Normalizar el normal y calcular TBN
    worldNormal = normalize(worldNormal);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    // Retornar la normal transformada al espacio tangente
    return normalize(mul(tangentNormal, TBN));
}

// Estructuras de Entrada y Salida del Vertex Shader
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
    float3 Normal : TEXCOORD3;  // Cambié esto a float3
};

// Vertex Shader
VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    // Calcular la posición en el espacio mundo
    float4 Position = mul(input.Position, World);
    output.Position = mul(mul(Position, View), Projection);
    output.TexCoord = input.TexCoord;

    // Transformar la dirección de la luz al espacio tangente
    float3x3 TBN = float3x3(input.Tangent, input.Binormal, input.Normal);
    output.TangentLightDir = mul(TBN, LightDirection);
    output.TangentViewDir = mul(TBN, normalize(-Position.xyz));

    // Normal en espacio mundial (ya está normalizada)
    output.Normal = normalize(input.Normal);

    return output;
}

// Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Obtener la normal desde el normal map
    float3 normalFromMap = getNormalFromMap(input.TexCoord, input.Position.xyz, input.Normal);

    // Calcular la intensidad de la luz usando la normal del normal map
    float lightIntensity = max(dot(normalFromMap, normalize(input.TangentLightDir)), 0.0);

    // Ajustar la intensidad de luz (la lógica de iluminación permanece igual)
    lightIntensity *= 2.0;

    // Luz ambiental
    float ambientLight = 0.2;
    lightIntensity += ambientLight;

    // Retornar el color final con iluminación
    return float4(diffuseColor * lightIntensity, 1.0);
}


VertexShaderOutput NormalMapVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Position = mul(input.Position, World);
    output.Normal = mul(input.Normal, InverseTransposeWorld);
    output.TexCoord = input.TexCoord * Tiling;
	
    return output;
}

float4 NormalMapPS(VertexShaderOutput input) : COLOR
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.Position.xyz);
    float3 viewDirection = normalize(eyePosition - input.Position.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    float3 normal =  getNormalFromMap(input.TexCoord, input.Position.xyz, normalize(input.Normal.xyz));

	// Get the texture texel
    float4 texelColor = tex2D(textureSampler, input.TexCoord);
    
	// Calculate the diffuse light
    float NdotL = saturate(dot(normal, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(normal, halfVector);
    float3 specularLight = KSpecular * specularColor * pow(NdotH, shininess);
    
    // Final calculation
    float4 finalColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);
    return finalColor;

}
// Técnica para el modelo básico
technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

// Técnica para Normal Mapping
technique NormalMapping
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL NormalMapVS();
        PixelShader = compile PS_SHADERMODEL NormalMapPS();
    }
};
