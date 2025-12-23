#nullable enable
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    /// <summary>
    /// Immutable per-frame context describing the current render target, view, and projection.
    /// </summary>
    internal readonly struct RenderFrameContext(Matrix view, Matrix projection, RenderTarget2D? renderTarget = null, Viewport? viewport = null, Rectangle? scissor = null)
    {
        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        public Matrix View { get; } = view;

        /// <summary>
        /// Gets the projection matrix.
        /// </summary>
        public Matrix Projection { get; } = projection;

        /// <summary>
        /// Gets the optional render target.
        /// </summary>
        public RenderTarget2D? RenderTarget { get; } = renderTarget;

        /// <summary>
        /// Gets the optional viewport.
        /// </summary>
        public Viewport? Viewport { get; } = viewport;

        /// <summary>
        /// Gets the optional scissor rectangle.
        /// </summary>
        public Rectangle? Scissor { get; } = scissor;
    }

    /// <summary>
    /// Simple material container to describe blend/sampler configuration and whether vertex color is used.
    /// </summary>
    internal sealed class Material(BlendState? blendState, SamplerState? samplerState, Effect? effect, Color? constantColor = null, bool useVertexColor = false, bool useTexture = true)
    {
        /// <summary>
        /// Gets the blend state to use for the draw.
        /// </summary>
        public BlendState BlendState { get; } = blendState ?? BlendState.AlphaBlend;

        /// <summary>
        /// Gets the sampler state to use for the draw.
        /// </summary>
        public SamplerState SamplerState { get; } = samplerState ?? SamplerState.LinearClamp;

        /// <summary>
        /// Gets the optional custom effect.
        /// </summary>
        public Effect? Effect { get; } = effect;

        /// <summary>
        /// Gets the optional constant color tint.
        /// </summary>
        public Color? ConstantColor { get; } = constantColor;

        /// <summary>
        /// Gets whether vertex colors are used.
        /// </summary>
        public bool UseVertexColor { get; } = useVertexColor;

        /// <summary>
        /// Gets whether a texture is used.
        /// </summary>
        public bool UseTexture { get; } = useTexture;

        /// <summary>
        /// Creates a textured material configuration.
        /// </summary>
        public static Material Textured(Effect? effect = null, BlendState? blend = null, SamplerState? sampler = null, Color? tint = null, bool useVertexColor = false)
        {
            return new Material(blend, sampler, effect, tint, useVertexColor, useTexture: true);
        }

        /// <summary>
        /// Creates a solid color material configuration.
        /// </summary>
        public static Material SolidColor(Effect? effect = null, BlendState? blend = null)
        {
            return new Material(blend ?? BlendState.AlphaBlend, SamplerState.LinearClamp, effect, Color.White, useVertexColor: true, useTexture: false);
        }
    }

    /// <summary>
    /// Aggregated renderer counters for lightweight instrumentation.
    /// </summary>
    internal readonly record struct RendererStats(int DrawCalls, int StateChanges, int Vertices, int Indices);

    /// <summary>
    /// Describes a textured/colored quad draw. Uses user primitives for now; batching will replace this in later phases.
    /// </summary>
    internal readonly struct QuadDrawCommand(Texture2D texture, VertexPositionColorTexture[] vertices, short[]? indices, Material material, Matrix world, PrimitiveType primitiveType = PrimitiveType.TriangleStrip)
    {
        /// <summary>
        /// Gets the texture for the draw.
        /// </summary>
        public Texture2D Texture { get; } = texture ?? throw new ArgumentNullException(nameof(texture));

        /// <summary>
        /// Gets the vertices for the draw.
        /// </summary>
        public VertexPositionColorTexture[] Vertices { get; } = vertices ?? throw new ArgumentNullException(nameof(vertices));

        /// <summary>
        /// Gets the optional indices for the draw.
        /// </summary>
        public short[]? Indices { get; } = indices;

        /// <summary>
        /// Gets the material describing render state.
        /// </summary>
        public Material Material { get; } = material ?? throw new ArgumentNullException(nameof(material));

        /// <summary>
        /// Gets the world transform.
        /// </summary>
        public Matrix World { get; } = world;

        /// <summary>
        /// Gets the primitive topology.
        /// </summary>
        public PrimitiveType PrimitiveType { get; } = primitiveType;
    }

    /// <summary>
    /// General mesh draw command. Will be routed to vertex/index buffers in later phases.
    /// </summary>
    internal readonly struct MeshDrawCommand(VertexPositionColorTexture[] vertices, short[]? indices, Texture2D? texture, Material material, Matrix world, PrimitiveType primitiveType, int primitiveCount, int vertexCount, int indexCount)
    {
        /// <summary>
        /// Gets the vertices for the draw.
        /// </summary>
        public VertexPositionColorTexture[] Vertices { get; } = vertices ?? throw new ArgumentNullException(nameof(vertices));

        /// <summary>
        /// Gets the optional indices for the draw.
        /// </summary>
        public short[]? Indices { get; } = indices;

        /// <summary>
        /// Gets the optional texture for the draw.
        /// </summary>
        public Texture2D? Texture { get; } = texture;

        /// <summary>
        /// Gets the material describing render state.
        /// </summary>
        public Material Material { get; } = material ?? throw new ArgumentNullException(nameof(material));

        /// <summary>
        /// Gets the world transform.
        /// </summary>
        public Matrix World { get; } = world;

        /// <summary>
        /// Gets the primitive topology.
        /// </summary>
        public PrimitiveType PrimitiveType { get; } = primitiveType;

        /// <summary>
        /// Gets the number of primitives to draw.
        /// </summary>
        public int PrimitiveCount { get; } = primitiveCount;

        /// <summary>
        /// Gets the number of vertices to use.
        /// </summary>
        public int VertexCount { get; } = vertexCount;

        /// <summary>
        /// Gets the number of indices to use.
        /// </summary>
        public int IndexCount { get; } = indexCount;
    }

    /// <summary>
    /// Placeholder particle batch command for later buffer-backed implementation.
    /// </summary>
    internal readonly struct ParticleDrawCommand(VertexPositionColorTexture[] vertices, short[] indices, Material material, Matrix world)
    {
        /// <summary>
        /// Gets the vertices for the draw.
        /// </summary>
        public VertexPositionColorTexture[] Vertices { get; } = vertices ?? throw new ArgumentNullException(nameof(vertices));

        /// <summary>
        /// Gets the indices for the draw.
        /// </summary>
        public short[] Indices { get; } = indices ?? throw new ArgumentNullException(nameof(indices));

        /// <summary>
        /// Gets the material describing render state.
        /// </summary>
        public Material Material { get; } = material ?? throw new ArgumentNullException(nameof(material));

        /// <summary>
        /// Gets the world transform.
        /// </summary>
        public Matrix World { get; } = world;
    }
}
