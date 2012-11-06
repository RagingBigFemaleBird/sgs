//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- MonoChromeEffect
//
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Shader constant register mappings (scalars - float, double, Point, Color, Point3D, etc.)
//-----------------------------------------------------------------------------------------

float4 filterColor : register(C0);
float strength : register(C1);

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D  implicitInputSampler : register(S0);

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
   float2 texuv = uv;
   float4 color = tex2D(implicitInputSampler, texuv);
   float4 gray = color.r * 0.30 + color.g * 0.59 + color.b * 0.11;
   float4 result;
   float factor = 1.0 - strength;
   result.r = (color.r - gray) * factor + gray;
   result.g = (color.g - gray) * factor + gray;
   result.b = (color.b - gray) * factor + gray;
   result.a = color.a;
   return result;
}