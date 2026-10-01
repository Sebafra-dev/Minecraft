#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

Texture2D Texture;

SamplerState TextureSampler
{
    Texture = <Texture>;

    AddressU = Wrap;
    AddressV = Wrap;

    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 AtlasUV : TEXCOORD0;
    float2 TileUV : TEXCOORD1;
    float Alpha : TEXCOORD2;
};


struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 AtlasUV : TEXCOORD0;
    float2 TileUV : TEXCOORD1;
    float Alpha : TEXCOORD2;
};


VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);

    output.AtlasUV = input.AtlasUV;
    output.TileUV = input.TileUV;
    output.Alpha = input.Alpha;

    return output;
}


float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float2 tileUV = frac(input.TileUV);

    float2 tileSize = float2(
        8.0 / 128.0,
        8.0 / 128.0
    );

    float2 uv = input.AtlasUV + tileUV * tileSize;

    float4 color = Texture.Sample(TextureSampler, uv);

    color.a *= input.Alpha;
    
    clip(color.a - 0.1);

    return color;
}


technique VoxelTechnique
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};