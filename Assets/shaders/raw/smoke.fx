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
    
    // 1. Calculate Movement
    // We offset the UVs based on time * speed * direction
    float2 move = Direction * Speed * Time;
    float2 smokeUV = uv * Dispersion + move;

    // 2. Generate Noise Pattern
    // We use two fBm calls warped against each other for a "fluid" feel
    float q = fbm(smokeUV);
    float2 r = float2(fbm(smokeUV + q + Time * 0.1), fbm(smokeUV + q - Time * 0.1));
    float smokeShape = fbm(smokeUV + r);

    // 3. Apply Density and Intensity
    // Map the noise (0.0 to 1.0) to a density curve
    float alpha = smoothstep(0.0, 1.0 - (Density * 0.5), smokeShape); 
    
    // 4. Colorize
    float3 col = SmokeColor.rgb * smokeShape * Intensity;
    
    // 5. Apply Coverage (Vignette)
    // Calculates distance from center to fade edges if Coverage < 1.0
    float2 center = uv * 2.0 - 1.0;
    float dist = length(center);
    // Smooth fade out based on coverage parameter
    float mask = 1.0 - smoothstep(Coverage * 0.5, Coverage * 1.5, dist);
    
    alpha *= mask;

    // Return final color with calculated alpha
    return float4(col, alpha * SmokeColor.a);
}