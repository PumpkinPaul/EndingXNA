sampler s0 : register0;

float2 Viewport;
float2 CrtStrength; // (0.15 is a good value)
float2 Bulge;		// (0.95 is a good value)

/*
	Distortion fragment shader, for drawing distortion maps
	requires: TEXCOORD0, distortion
	in: strengthOut
*/
float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float2 tex = texCoord - 0.5;

	float image_aspect = Viewport.x / Viewport.y;

	float k = 0.0; // lens distortion coefficient
    float kcube = CrtStrength;  // cubic distortion value
        
    float r2 = image_aspect * image_aspect * tex.x * tex.x + tex.y * tex.y;   
		     
    float f = 0;
 
    //only compute the cubic distortion if necessary 
    if (kcube == 0.0) f = 1 + r2 * k;
    else f = 1 + r2 * (k + kcube * sqrt(r2));

	//Bulge!
	f = f * Bulge;
        
    // get the right pixel for the current position
    float x = f * tex.x + 0.5;
    float y = f * tex.y + 0.5;

    float3 inputDistord = tex2D(s0, float2(x, y));
 
    return float4(inputDistord.r, inputDistord.g, inputDistord.b,1);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}