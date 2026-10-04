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
    float2 TileUV : TEXCOORD0;
    float4 Data : COLOR0;
};


struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TileUV : TEXCOORD0;
    float4 Data : COLOR0;
};


VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);

    output.TileUV = input.TileUV;
    output.Data = input.Data;

    return output;
}


float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float2 tileUV = frac(input.TileUV);
    float alpha = input.Data.a;
    float tileId = round(input.Data.r * 255.0);

    float tileX = fmod(tileId, 16.0);
    float tileY = floor(tileId / 16.0);

    float2 uv = (float2(tileX, tileY) + tileUV) / 16.0;

    float4 color = Texture.Sample(TextureSampler, uv);

    color.a *= alpha;

    clip(color.a - 0.001);

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