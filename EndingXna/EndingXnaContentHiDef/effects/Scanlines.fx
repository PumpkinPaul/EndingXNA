
texture scanlineTexture;
float repeatY = 1.0f;
float value = 1.0f;

sampler TextureSampler;

sampler ScanlineSampler = sampler_state
{
    Texture = <scanlineTexture>;
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;  
    AddressU = Wrap;
    AddressV = Wrap;  
};

void VS(float4 inPosition : Position0, float2 inTex : TexCoord0, 
		out float4 oPosition : Position0, out float2 oTex : TexCoord0)
{
	oTex = inTex;
	oPosition = inPosition; 
}

float4 PS(float2 texCoord  : TEXCOORD0) : COLOR0
{		
  	float4 colour = tex2D(TextureSampler, texCoord);
  	texCoord.y *= repeatY;
  	float4 alpha = tex2D(ScanlineSampler, texCoord);
  		
	colour.rgb *= lerp(1.0, alpha.a, value);
		
  	return colour;
}

technique Scanlines
{
	pass P0
	{
		//VertexShader = compile vs_1_1 VS();
		Pixelshader = compile ps_2_0 PS();
	}
}