///======================================================
/// Custom standard procedural skybox.
/// Add moon, night horizon color and stars cubemap.
///======================================================

Shader "BasicTimeOfDay/Custom/Skybox/Procedural"  
{

Properties 
{
	[KeywordEnum(None, Simple, High Quality)] _SunDisk ("Sun", Int) = 2
	[HideInInspector]_SunSize("Sun Size", Range(0,1)) = 0.04
	
	[HideInInspector]_AtmosphereThickness("Atmoshpere Thickness", Range(0,5)) = 1.0
	[HideInInspector]_SkyTint("Sky Tint", Color) = (.5, .5, .5, 1)
	[HideInInspector]_GroundColor("Ground", Color) = (.369, .349, .341, 1)
	[HideInInspector]_Exposure("Exposure", Range(0, 8)) = 1.3


	//[HideInInspector]_SunDir("Sun Direction", Vector) = (0, 0, 0, 0)
	//[HideInInspector]_SunColor("Sun Color", Color) = (.5, .5, .5, 1)

	//[HideInInspector]_NightHorizonExponent("Night Horizon Exponent", Range(0,30)) = 1.0
	//[HideInInspector]_NightHorizonColor("_Night Horizon Color", Color) = (.2, .2, .2, 1)


	//[HideInInspector]_MoonDir("Sun Direction", Vector) = (0, 0, 0, 0)
	//[HideInInspector]_MoonSize("Moon Size", Range(0,2)) = 0.5

	//[HideInInspector]_MoonTexture("Moon Texture", 2D) = "White"{}
	//[HideInInspector]_MoonColor("Sun Color", Color) = (.5, .5, .5, 1)
	//[HideInInspector]_MoonIntesity("Moon Intensity", Range(0,2)) = 1.0

	//[HideInInspector]_MoonHaloPower("Moon Halo Power", Range(0,300)) = 200
	//[HideInInspector]_MoonHaloColor("Moon Halo Color", Color) = (.5, .5, .5, 1)


	//[HideInInspector][Gamma]_StarsIntensity("Outer Space Intensity", Range(0, 30)) = 1
    //[HideInInspector]_StarsCubeMap("Outer Space CubeMap", Cube) = "grey" {}
    //[HideInInspector]_StarsColor("Outer Space Color", Color) = (.5, .5, .5, 1)

}

SubShader 
{
	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off ZWrite Off

	Pass 
	{

		CGPROGRAM
		#pragma glsl
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "Lighting.cginc"

		#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
		#pragma multi_compile _SUNDISK_NONE _SUNDISK_SIMPLE _SUNDISK_HIGH_QUALITY

		#pragma multi_compile __ USEMOON 
		#pragma multi_compile __ USEMOONHALO
		#pragma multi_compile __ USESTARS

		uniform half  _Exposure;		// HDR exposure
		uniform half3 _GroundColor;
		uniform half  _SunSize;
		uniform half3 _SkyTint;
		uniform half  _AtmosphereThickness;

		// Sun.
		//==============================
		uniform float3   _SunDir;
		uniform half4    _SunColor;
		uniform float4x4 _SunMatrix;
		//==============================

		// Night horizon.
		//===================================
		uniform float  _NightHorizonExponent;
		uniform half4  _NightHorizonColor;
		//===================================

		// Moon.
		//==============================
		uniform float3    _MoonDir;
		uniform float4x4  _MoonMatrix;
		uniform float     _MoonSize;

		uniform sampler2D _MoonTexture;
		uniform float4    _MoonColor;
		uniform float     _MoonIntensity;

		uniform float     _MoonHaloPower;
		uniform float4    _MoonHaloColor;
		uniform float     _MoonHaloIntensity;
		//===================================

		// Stars.
		//=======================================
		uniform samplerCUBE _StarsCubemap;
		uniform samplerCUBE _StarsNoiseCubemap;
		uniform float4x4    _StarsMatrix;
		uniform float4x4    _StarsNoiseMatrix;
		uniform half        _StarsIntensity;
		uniform half4       _StarsColor;
		uniform float       _StarsTwinkle;
		half4               _StarsCubemap_HDR;
		//=======================================


	#if defined(UNITY_COLORSPACE_GAMMA)
		#define GAMMA 2
		#define COLOR_2_GAMMA(color) color
		#define COLOR_2_LINEAR(color) color*color
		#define LINEAR_2_OUTPUT(color) sqrt(color)
	#else
		#define GAMMA 2.2
		// HACK: to get gfx-tests in Gamma mode to agree until UNITY_ACTIVE_COLORSPACE_IS_GAMMA is working properly
		#define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
		#define COLOR_2_LINEAR(color) color
		#define LINEAR_2_LINEAR(color) color
	#endif

		// RGB wavelengths
		// .35 (.62=158), .43 (.68=174), .525 (.75=190)
		static const float3 kDefaultScatteringWavelength = float3(.65, .57, .475);
		static const float3 kVariableRangeForScatteringWavelength = float3(.15, .15, .15);

		#define OUTER_RADIUS 1.025
		static const float kOuterRadius = OUTER_RADIUS;
		static const float kOuterRadius2 = OUTER_RADIUS*OUTER_RADIUS;
		static const float kInnerRadius = 1.0;
		static const float kInnerRadius2 = 1.0;

		static const float kCameraHeight = 0.0001;

		#define kRAYLEIGH (lerp(0, 0.0025, pow(_AtmosphereThickness,2.5)))		// Rayleigh constant
		#define kMIE 0.0010      		// Mie constant
		#define kSUN_BRIGHTNESS 20.0 	// Sun brightness

		#define kMAX_SCATTER 50.0 // Maximum scattering value, to prevent math overflows on Adrenos

		static const half kSunScale = 400.0 * kSUN_BRIGHTNESS;
		static const float kKmESun = kMIE * kSUN_BRIGHTNESS;
		static const float kKm4PI = kMIE * 4.0 * 3.14159265;
		static const float kScale = 1.0 / (OUTER_RADIUS - 1.0);
		static const float kScaleDepth = 0.25;
		static const float kScaleOverScaleDepth = (1.0 / (OUTER_RADIUS - 1.0)) / 0.25;
		static const float kSamples = 2.0; // THIS IS UNROLLED MANUALLY, DON'T TOUCH

		#define MIE_G (-0.990)
		#define MIE_G2 0.9801

		#define SKY_GROUND_THRESHOLD 0.02

		// fine tuning of performance. You can override defines here if you want some specific setup
		// or keep as is and allow later code to set it according to target api

		// if set vprog will output color in final color space (instead of linear always)
		// in case of rendering in gamma mode that means that we will do lerps in gamma mode too, so there will be tiny difference around horizon
		// #define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 0

		// sun disk rendering:
		// no sun disk - the fastest option
		#define SKYBOX_SUNDISK_NONE 0
		// simplistic sun disk - without mie phase function
		#define SKYBOX_SUNDISK_SIMPLE 1
		// full calculation - uses mie phase function
		#define SKYBOX_SUNDISK_HQ 2

		// uncomment this line and change SKYBOX_SUNDISK_SIMPLE to override material settings
		// #define SKYBOX_SUNDISK SKYBOX_SUNDISK_SIMPLE

	#ifndef SKYBOX_SUNDISK
		#if defined(_SUNDISK_NONE)
			#define SKYBOX_SUNDISK SKYBOX_SUNDISK_NONE
		#elif defined(_SUNDISK_SIMPLE)
			#define SKYBOX_SUNDISK SKYBOX_SUNDISK_SIMPLE
		#else
			#define SKYBOX_SUNDISK SKYBOX_SUNDISK_HQ
		#endif
	#endif

	#ifndef SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
		#if defined(SHADER_API_MOBILE)
			#define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 1
		#else
			#define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 0
		#endif
	#endif

		// Calculates the Rayleigh phase function
		half getRayleighPhase(half eyeCos2)
		{
			return 0.75 + 0.75*eyeCos2;
		}
		half getRayleighPhase(half3 light, half3 ray)
		{
			half eyeCos	= dot(light, ray);
			return getRayleighPhase(eyeCos * eyeCos);
		}


		struct appdata_t
		{
			float4 vertex : POSITION;
		};

		struct v2f
		{
			float4	pos				: SV_POSITION;

		#if SKYBOX_SUNDISK == SKYBOX_SUNDISK_HQ
			// for HQ sun disk, we need vertex itself to calculate ray-dir per-pixel
			half3	vertex			: TEXCOORD0;
		#elif SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
			half3	rayDir			: TEXCOORD0;
		#else
			// as we dont need sun disk we need just rayDir.y (sky/ground threshold)
			half	skyGroundFactor	: TEXCOORD0;
		#endif

			// calculate sky colors in vprog
			half3	groundColor		: TEXCOORD1;
			half3	skyColor		: TEXCOORD2;

		#if SKYBOX_SUNDISK != SKYBOX_SUNDISK_NONE
			half3	sunColor		: TEXCOORD3;
		#endif

		 	float3 worldPos         : TEXCOORD4;

		#if defined(USEMOON) 
			float4  moonCoords      : TEXCOORD5;
		#endif

		#if defined(USESTARS) 
			float3  starsCoords        : TEXCOORD6;
			float3  starsNoiseCoords   : TEXCOORD7;
		#endif

		};


		float scale(float inCos)
		{
			float x = 1.0 - inCos;
		#if defined(SHADER_API_N3DS)
			// The polynomial expansion here generates too many swizzle instructions for the 3DS vertex assembler
			// Approximate by removing x^1 and x^2
			return 0.25 * exp(-0.00287 + x*x*x*(-6.80 + x*5.25));
		#else
			return 0.25 * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
		#endif
		}

		v2f vert (appdata_t v)
		{
			v2f OUT;
			OUT.pos = UnityObjectToClipPos(v.vertex);


			OUT.worldPos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);

			// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
			float3 eyeRay = normalize(OUT.worldPos);


			// Moon coords.
		#if defined(USEMOON)
			OUT.moonCoords.w   = saturate(dot(_MoonDir.xyz, OUT.worldPos));
			OUT.moonCoords.xyz= mul((float3x3)_MoonMatrix, v.vertex.xyz) / _MoonSize + 0.5;
		#endif

			// Stars coords.
		#if defined(USESTARS) 
			float3 sunCoords = mul((float3x3)_SunMatrix, v.vertex.xyz);
			OUT.starsCoords  = mul((float3x3)_StarsMatrix, sunCoords.xyz);
			OUT.starsNoiseCoords = mul((float3x3)_StarsNoiseMatrix, sunCoords.xyz);
		#endif


			float3 kSkyTintInGammaSpace = COLOR_2_GAMMA(_SkyTint); // convert tint from Linear back to Gamma
			float3 kScatteringWavelength = lerp (
				kDefaultScatteringWavelength-kVariableRangeForScatteringWavelength,
				kDefaultScatteringWavelength+kVariableRangeForScatteringWavelength,
				half3(1,1,1) - kSkyTintInGammaSpace); // using Tint in sRGB gamma allows for more visually linear interpolation and to keep (.5) at (128, gray in sRGB) point
			float3 kInvWavelength = 1.0 / pow(kScatteringWavelength, 4);

			float kKrESun = kRAYLEIGH * kSUN_BRIGHTNESS;
			float kKr4PI = kRAYLEIGH * 4.0 * 3.14159265;

			float3 cameraPos = float3(0,kInnerRadius + kCameraHeight,0); 	// The camera's current position


			float far = 0.0;
			half3 cIn, cOut;

			if(eyeRay.y >= 0.0)
			{
				// Sky
				// Calculate the length of the "atmosphere"
				far = sqrt(kOuterRadius2 + kInnerRadius2 * eyeRay.y * eyeRay.y - kInnerRadius2) - kInnerRadius * eyeRay.y;

				float3 pos = cameraPos + far * eyeRay;

				// Calculate the ray's starting position, then calculate its scattering offset
				float height = kInnerRadius + kCameraHeight;
				float depth = exp(kScaleOverScaleDepth * (-kCameraHeight));
				float startAngle = dot(eyeRay, cameraPos) / height;
				float startOffset = depth*scale(startAngle);


				// Initialize the scattering loop variables
				float sampleLength = far / kSamples;
				float scaledLength = sampleLength * kScale;
				float3 sampleRay = eyeRay * sampleLength;
				float3 samplePoint = cameraPos + sampleRay * 0.5;

				// Now loop through the sample rays
				float3 frontColor = float3(0.0, 0.0, 0.0);
				// Weird workaround: WP8 and desktop FL_9_1 do not like the for loop here
				// (but an almost identical loop is perfectly fine in the ground calculations below)
				// Just unrolling this manually seems to make everything fine again.
//				for(int i=0; i<int(kSamples); i++)
				{
					float height = length(samplePoint);
					float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
					float lightAngle = dot(_SunDir.xyz, samplePoint) / height;
					float cameraAngle = dot(eyeRay, samplePoint) / height;
					float scatter = (startOffset + depth*(scale(lightAngle) - scale(cameraAngle)));
					float3 attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));

					frontColor += attenuate * (depth * scaledLength);
					samplePoint += sampleRay;
				}
				{
					float height = length(samplePoint);
					float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
					float lightAngle = dot(_SunDir.xyz, samplePoint) / height;
					float cameraAngle = dot(eyeRay, samplePoint) / height;
					float scatter = (startOffset + depth*(scale(lightAngle) - scale(cameraAngle)));
					float3 attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));

					frontColor += attenuate * (depth * scaledLength);
					samplePoint += sampleRay;
				}

				// Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
				cIn = frontColor * (kInvWavelength * kKrESun);
				cOut = frontColor * kKmESun;
			}
			else
			{
				// Ground
				far = (-kCameraHeight) / (min(-0.001, eyeRay.y));

				float3 pos = cameraPos + far * eyeRay;

				// Calculate the ray's starting position, then calculate its scattering offset
				float depth = exp((-kCameraHeight) * (1.0/kScaleDepth));
				float cameraAngle = dot(-eyeRay, pos);
				float lightAngle = dot(_SunDir.xyz, pos);
				float cameraScale = scale(cameraAngle);
				float lightScale = scale(lightAngle);
				float cameraOffset = depth*cameraScale;
				float temp = (lightScale + cameraScale);

				// Initialize the scattering loop variables
				float sampleLength = far / kSamples;
				float scaledLength = sampleLength * kScale;
				float3 sampleRay = eyeRay * sampleLength;
				float3 samplePoint = cameraPos + sampleRay * 0.5;

				// Now loop through the sample rays
				float3 frontColor = float3(0.0, 0.0, 0.0);
				float3 attenuate;
//				for(int i=0; i<int(kSamples); i++) // Loop removed because we kept hitting SM2.0 temp variable limits. Doesn't affect the image too much.
				{
					float height = length(samplePoint);
					float depth = exp(kScaleOverScaleDepth * (kInnerRadius - height));
					float scatter = depth*temp - cameraOffset;
					attenuate = exp(-clamp(scatter, 0.0, kMAX_SCATTER) * (kInvWavelength * kKr4PI + kKm4PI));
					frontColor += attenuate * (depth * scaledLength);
					samplePoint += sampleRay;
				}

				cIn = frontColor * (kInvWavelength * kKrESun + kKmESun);
				cOut = clamp(attenuate, 0.0, 1.0);
			}

		#if SKYBOX_SUNDISK == SKYBOX_SUNDISK_HQ
			OUT.vertex 			= -v.vertex;
		#elif SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
			OUT.rayDir 			= half3(-eyeRay);
		#else
			OUT.skyGroundFactor	= -eyeRay.y / SKY_GROUND_THRESHOLD;
		#endif

			// if we want to calculate color in vprog:
			// 1. in case of linear: multiply by _Exposure in here (even in case of lerp it will be common multiplier, so we can skip mul in fshader)
			// 2. in case of gamma and SKYBOX_COLOR_IN_TARGET_COLOR_SPACE: do sqrt right away instead of doing that in fshader

			OUT.groundColor	= _Exposure * (cIn + COLOR_2_LINEAR(_GroundColor) * cOut);
			OUT.skyColor	= _Exposure * (cIn * getRayleighPhase(_SunDir.xyz, -eyeRay));

		#if SKYBOX_SUNDISK != SKYBOX_SUNDISK_NONE
			OUT.sunColor	= _Exposure * (cOut * _SunColor.xyz);
		#endif

		#if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
			OUT.groundColor	= sqrt(OUT.groundColor);
			OUT.skyColor	= sqrt(OUT.skyColor);
			#if SKYBOX_SUNDISK != SKYBOX_SUNDISK_NONE
				OUT.sunColor= sqrt(OUT.sunColor);
			#endif
		#endif

			return OUT;
		}


		// Calculates the Mie phase function
		half getMiePhase(half eyeCos, half eyeCos2)
		{
			half temp = 1.0 + MIE_G2 - 2.0 * MIE_G * eyeCos;
			temp = pow(temp, pow(_SunSize,0.65) * 10);
			temp = max(temp,1.0e-4); // prevent division by zero, esp. in half precision
			temp = 1.5 * ((1.0 - MIE_G2) / (2.0 + MIE_G2)) * (1.0 + eyeCos2) / temp;
			#if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
				temp = pow(temp, .454545);
			#endif
			return temp;
		}

		half calcSunSpot(half3 vec1, half3 vec2)
		{
			half3 delta = vec1 - vec2;
			half dist = length(delta);
			half spot = 1.0 - smoothstep(0.0, _SunSize, dist);
			return kSunScale * spot * spot;
		}

		half4 frag (v2f IN) : SV_Target
		{
			half3 col = half3(0.0, 0.0, 0.0);

		// if y > 1 [eyeRay.y < -SKY_GROUND_THRESHOLD] - ground
		// if y >= 0 and < 1 [eyeRay.y <= 0 and > -SKY_GROUND_THRESHOLD] - horizon
		// if y < 0 [eyeRay.y > 0] - sky


		#if SKYBOX_SUNDISK == SKYBOX_SUNDISK_HQ
			half3 ray = normalize(mul((float3x3)unity_ObjectToWorld, IN.vertex));
			half y = ray.y / SKY_GROUND_THRESHOLD;
		#elif SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
			half3 ray = IN.rayDir.xyz;
			half y = ray.y / SKY_GROUND_THRESHOLD;
		#else
			half y = IN.skyGroundFactor;
		#endif

		    half3 worldPosNormal = normalize(IN.worldPos);


		    // Night horizon color.
			//====================================================
			half3 nightHorizonColor = (pow(1 - saturate(abs(worldPosNormal.y)), _NightHorizonExponent) *_NightHorizonColor);
			IN.skyColor +=   _Exposure * nightHorizonColor;
			//====================================================


			// Moon.
			//====================================================
			#if defined(USEMOON)  
			half4 moonColor  = tex2D(_MoonTexture, IN.moonCoords.xy) * _MoonColor;
			moonColor.rgb    *= _MoonIntensity;
			moonColor.rgb    *= IN.moonCoords.w;
			half  moonMask   = 1.0 - (moonColor.a);
			#if defined(USEMOONHALO)
			moonColor.rgb      += (_MoonHaloIntensity *  pow(max(0,-dot(worldPosNormal.xyz, -_MoonDir.xyz)) , _MoonHaloPower) * _MoonHaloColor).rgb;
			#endif
			IN.skyColor   += _Exposure * moonColor.rgb;
			#endif
			//====================================================

			// Stars.
			//====================================================
			#if defined(USESTARS) 
			half4  starsCube  = texCUBE(_StarsCubemap, IN.starsCoords);
			half3  starsColor = DecodeHDR(starsCube,  _StarsCubemap_HDR)* _StarsColor.rgb;

			#if defined(USEMOON) 
			starsColor.rgb   *= (unity_ColorSpaceDouble.rgb) *  moonMask;
			#else
			starsColor.rgb   *= (unity_ColorSpaceDouble.rgb);
			#endif

			starsColor.rgb *= _StarsIntensity;

			half4 starsNoise = texCUBE(_StarsNoiseCubemap, IN.starsNoiseCoords);
			starsColor.rgb *=  lerp(1,2*starsNoise.rgb,_StarsTwinkle);

			IN.skyColor      += _Exposure * starsColor.rgb;
			//IN.skyColor.rgb += starsNoise;
			#endif
			//====================================================

			// if we did precalculate color in vprog: just do lerp between them
			col = lerp(IN.skyColor, IN.groundColor, saturate(y));

		#if SKYBOX_SUNDISK != SKYBOX_SUNDISK_NONE
			if(y < 0.0)
			{
			#if SKYBOX_SUNDISK == SKYBOX_SUNDISK_SIMPLE
				half mie = calcSunSpot(_SunDir.xyz, -ray);
			#else // SKYBOX_SUNDISK_HQ
				half eyeCos = dot(_SunDir.xyz, ray);
				half eyeCos2 = eyeCos * eyeCos;
				half mie = getMiePhase(eyeCos, eyeCos2);
			#endif

				col += mie * IN.sunColor;
			}
		#endif

		#if defined(UNITY_COLORSPACE_GAMMA) && !SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
			col = LINEAR_2_OUTPUT(col);
		#endif

			return half4(col,1);

		}
		ENDCG
	}
}


Fallback Off

}
