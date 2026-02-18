// Smoke.fx

// --- Inputs provided by WPF ---
sampler2D input : register(s0);

// Animation
float Time : register(c0);

// Appearance
float4 SmokeColor : register(c1);
float Intensity : register(c2);   // Brightness/Glow
float Density : register(c3);     // How "thick" the smoke is

// Movement
float2 Direction : register(c4);  // X, Y direction vector (-1.0 to 1.0)
float Speed : register(c5);

// Structure
float Dispersion : register(c6);  // Noise frequency (zoom)
float Coverage : register(c7);    // Screen coverage (vignette falloff)

// --- Noise Functions ---
// A simple pseudo-random hash function
float hash(float2 p) {
    float3 p3 = frac(float3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

// Value Noise: Smooth interpolation between random values
float noise(float2 p) {
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f); // Cubic smoothing
    
    return lerp(lerp(hash(i + float2(0.0, 0.0)), hash(i + float2(1.0, 0.0)), u.x),
                lerp(hash(i + float2(0.0, 1.0)), hash(i + float2(1.0, 1.0)), u.x), u.y);
}

// Fractal Brownian Motion: Layering noise to create "wisps"
float fbm(float2 uv) {
    float value = 0.0;
    float amplitude = 0.5;
    float2 shift = float2(100.0, 100.0);
    
    // Loop 5 times to create detailed wisps
    // We rotate the coordinates slightly to avoid grid artifacts
    float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.50));
    
    for (int i = 0; i < 5; ++i) {
        value += amplitude * noise(uv);
        uv = mul(rot, uv) * 2.0 + shift;
        amplitude *= 0.5;
    }
    return value;
}

// --- Main Shader ---
float4 main(float2 uv : TEXCOORD) : COLOR {
    // 0. Sample the original UI content (your Image/Grid)
    float4 originalColor = tex2D(input, uv);
    
    // 1. Calculate Movement (Existing code)
    float2 move = Direction * Speed * Time;
    float2 smokeUV = uv * Dispersion + move;

    // 2. Generate Noise Pattern (Existing code)
    float q = fbm(smokeUV);
    float2 r = float2(fbm(smokeUV + q + Time * 0.1), fbm(smokeUV + q - Time * 0.1));
    float smokeShape = fbm(smokeUV + r);

    // 3. Apply Density and Intensity
    float alpha = smoothstep(0.0, 1.0 - (Density * 0.5), smokeShape); 
    
    // 4. Colorize
    float3 smokeRGB = SmokeColor.rgb * smokeShape * Intensity;
    
    // 5. Apply Coverage (Vignette)
    float2 center = uv * 2.0 - 1.0;
    float dist = length(center);
    float mask = 1.0 - smoothstep(Coverage * 0.5, Coverage * 1.5, dist);
    
    float finalSmokeAlpha = alpha * mask * SmokeColor.a;

    // --- NEW BLENDING STEP ---
    // This overlays the smoke onto the original content
    float3 finalColor = lerp(originalColor.rgb, smokeRGB, finalSmokeAlpha);
    
    return float4(finalColor, originalColor.a);
}