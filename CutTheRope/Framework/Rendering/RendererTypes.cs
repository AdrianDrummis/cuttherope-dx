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
        public Matrix View { get; } = view;

        public Matrix Projection { get; } = projection;

        public RenderTarget2D? RenderTarget { get; } = renderTarget;

        public Viewport? Viewport { get; } = viewport;

        public Rectangle? Scissor { get; } = scissor;
    }

    /// <summary>
    /// Simple material container to describe blend/sampler configuration and whether vertex color is used.
    /// </summary>
    internal sealed class Material(BlendState? blendState, SamplerState? samplerState, Effect? effect, Color? constantColor = null, bool useVertexColor = false, bool useTexture = true)
    {
        public BlendState BlendState { get; } = blendState ?? BlendState.AlphaBlend;

        public SamplerState SamplerState { get; } = samplerState ?? SamplerState.LinearClamp;

        public Effect? Effect { get; } = effect;

        public Color? ConstantColor { get; } = constantColor;

        public bool UseVertexColor { get; } = useVertexColor;

        public bool UseTexture { get; } = useTexture;

        public static Material Textured(Effect? effect = null, BlendState? blend = null, SamplerState? sampler = null, Color? tint = null, bool useVertexColor = false)
        {
            return new Material(blend, sampler, effect, tint, useVertexColor, useTexture: true);
        }

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
        public Texture2D Texture { get; } = texture ?? throw new ArgumentNullException(nameof(texture));

        public VertexPositionColorTexture[] Vertices { get; } = vertices ?? throw new ArgumentNullException(nameof(vertices));

        public short[]? Indices { get; } = indices;

        public Material Material { get; } = material ?? throw new ArgumentNullException(nameof(material));

        public Matrix World { get; } = world;

        public PrimitiveType PrimitiveType { get; } = primitiveType;
    }

    /// <summary>
    /// General mesh draw command. Will be routed to vertex/index buffers in later phases.
    /// </summary>
    internal readonly struct MeshDrawCommand(VertexPositionColorTexture[] vertices, short[]? indices, Texture2D? texture, Material material, Matrix world, PrimitiveType primitiveType, int primitiveCount, int vertexCount, int indexCount)
    {
        public VertexPositionColorTexture[] Vertices { get; } = vertices ?? throw new ArgumentNullException(nameof(vertices));

        public short[]? Indices { get; } = indices;

        public Texture2D? Texture { get; } = texture;

        public Material Material { get; } = material ?? throw new ArgumentNullException(nameof(material));

        public Matrix World { get; } = world;

        public PrimitiveType PrimitiveType { get; } = primitiveType;

        public int PrimitiveCount { get; } = primitiveCount;

        public int VertexCount { get; } = vertexCount;

        public int IndexCount { get; } = indexCount;
    }

    /// <summary>
    /// Placeholder particle batch command for later buffer-backed implementation.
    /// </summary>
    internal readonly struct ParticleDrawCommand(VertexPositionColorTexture[] vertices, short[] indices, Material material, Matrix world)
    {
        public VertexPositionColorTexture[] Vertices { get; } = vertices ?? throw new ArgumentNullException(nameof(vertices));

        public short[] Indices { get; } = indices ?? throw new ArgumentNullException(nameof(indices));

        public Material Material { get; } = material ?? throw new ArgumentNullException(nameof(material));

        public Matrix World { get; } = world;
    }
}
