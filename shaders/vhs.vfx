HEADER
{
    CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
    Description = "VHS";
}

FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    VrForward();                                                    // Indicates this shader will be used for main rendering
    
    ToolsWireframe( "vr_tools_wireframe.vfx" );                     // Allows for mat_wireframe to work
    ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" );     // Shows how expensive drawing is in debug view
    Default();
}

//=========================================================================================================================
COMMON
{
    #include "system.fxc"
    #include "common.fxc"

    #define VertexInput VS_INPUT
    #define PixelInput PS_INPUT

    #define VertexOutput VS_OUTPUT
    #define PixelOutput PS_OUTPUT
}

//=========================================================================================================================

struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;    
};

//=========================================================================================================================

struct PixelInput
{
    float4 vPositionPs : SV_Position;
    float2 vTexCoord : TEXCOORD0;
};

//=========================================================================================================================

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        o.vPositionPs = float4(i.vPositionOs.xyz, 1.0f);
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}

//=========================================================================================================================

PS
{
    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );

    CreateTexture2D( g_FrameBuffer )< Attribute("FrameBufferCopyTexture"); >; 
    struct PixelOutput
    {
        float4 vColor : SV_Target0;
    };

    PixelOutput MainPs( PixelInput i )
    {
        float2 screenUvs = i.vPositionPs.xy * g_vInvGBufferSize.xy;

        PixelOutput o;
        o.vColor.rgb = 1 - Tex2D(g_FrameBuffer, screenUvs.xy).rgb; // invert screen color
        o.vColor.a = 1.0f;

        return o;
    }
}