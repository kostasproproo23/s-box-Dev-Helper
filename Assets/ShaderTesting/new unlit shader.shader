FEATURES
{
    Feature ( F_TINT_BY_MASK, 0..1, "Custom Features" );

    #include "common/features.hlsl"
}

MODES
{
	Forward();
    Depth();
}   

COMMON
{
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
    #include "common/vertex.hlsl"

    PixelInput MainVs( VertexInput i )
    {
        PixelInput o = ProcessVertex( i );

        return FinalizeVertex( o );
    }
}    

PS
{   
    #include "common/pixel.hlsl"

    float3 OverrideColor < Attribute( "MyOverrideColor); Default3(1,1,1) >;

    StaticCombo( S_TINT_BY_MASK, F_TINT_BY_MASK, Sys( ALL ));

    CreateInputTexture2D( AlbedoMap, Srgb, 8, "", "_color", "My Material,10/10", Default3( 1.0, 1.0, 1.0 ) );

    // Add a float variable that will control the scale of our texture on a mesh.
    float TextureScale < UiType( Slider ); Range( 0, 5 ); Default( 1 ); UiGroup("My Material,10/11"); >;
    float3 TextureTint < UiType( Color ); Default3( 1, 1, 1 ); UiGroup( "My Material,10/12" ); >;

    #if ( S_TINT_BY_MASK )
        CreateInputTexture2D( TintMask, Linear, 8, "", "_tint", "My Material,10/20", Default( 1 ));
        Texture2D g_tColor < Channel( RGB, Box( AlbedoMap ), Srgb ); Channel( A, Box(TintMask), Linear); OutputFormat( BC7 ); SrgbRead( true ); >;
    #else
        Texture2D g_tColor < Channel( RGB, Box( AlbedoMap ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
    #endif

    //Scroll Speed 
    int2 TextureScrollDirection < UiType( Slider ); Range2( -1, -1, 1, 1 ); Default2( 0, 0 ); UiGroup( "Texture Scroll,20/10" ); >;
    float2 TextureScrollSpeed < UiType( Slider ); Range2( 0, 0, 4, 4 ); Default2( 1, 1 ); UiGroup( "Texture Scroll,20/20" ); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        float2 uv = i.vTextureCoords.xy * TextureScale;
        uv += g_flTime * TextureScrollSpeed * TextureScrollDirection;
        
        float4 MyTexture = g_tColor.Sample( g_sAniso, uv ).rgb;
    
        #if (S_TINT_BY_MASK)
            MyTexture.rgb = lerp(MyTexture.rgb, MyTexture.rgb * TextureTint, MyTexture.a);
        #else
            MyTexture.rgb *= TextureTint;
        #endif

        return float4(D_OVERRIDE_COLOR ? OverrideColor : MyTexture.rgb , 1 );
    }
}
   