using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    /// <summary>
    /// Provides cached Material instances for common rendering configurations.
    /// Avoids allocating new Material objects per-draw when blend modes change frequently.
    /// </summary>
    internal static class MaterialPresets
    {
        // Textured materials with different blend modes
        public static readonly Material TexturedAlphaBlend = Material.Textured(
            blend: BlendState.AlphaBlend,
            sampler: SamplerState.LinearClamp
        );

        public static readonly Material TexturedAdditive = Material.Textured(
            blend: BlendState.Additive,
            sampler: SamplerState.LinearClamp
        );

        public static readonly Material TexturedOpaque = Material.Textured(
            blend: BlendState.Opaque,
            sampler: SamplerState.LinearClamp
        );

        public static readonly Material TexturedNonPremultiplied = Material.Textured(
            blend: BlendState.NonPremultiplied,
            sampler: SamplerState.LinearClamp
        );

        // Textured materials with vertex color
        public static readonly Material TexturedVertexColorAlphaBlend = new Material(
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            effect: null,
            constantColor: null,
            useVertexColor: true,
            useTexture: true
        );

        public static readonly Material TexturedVertexColorAdditive = new Material(
            BlendState.Additive,
            SamplerState.LinearClamp,
            effect: null,
            constantColor: null,
            useVertexColor: true,
            useTexture: true
        );

        // Solid color materials
        public static readonly Material SolidColorAlphaBlend = Material.SolidColor(
            blend: BlendState.AlphaBlend
        );

        public static readonly Material SolidColorAdditive = Material.SolidColor(
            blend: BlendState.Additive
        );

        // Common blend mode mappings from OpenGL BlendingFactor combinations
        // GL_ONE, GL_ONE_MINUS_SRC_ALPHA -> Premultiplied alpha
        public static readonly Material TexturedPremultiplied = Material.Textured(
            blend: new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorSourceBlend = Blend.One
            },
            sampler: SamplerState.LinearClamp
        );

        // GL_SRC_ALPHA, GL_ONE -> Additive with source alpha
        public static readonly Material TexturedAdditiveAlpha = Material.Textured(
            blend: new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.SourceAlpha,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend = Blend.SourceAlpha
            },
            sampler: SamplerState.LinearClamp
        );
    }
}