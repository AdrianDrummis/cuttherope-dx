using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    /// <summary>
    /// Immutable per-frame context describing the current render target, view, and projection.
    /// </summary>
    internal readonly struct RenderFrameContext
    {
        public RenderFrameContext(Matrix view, Matrix projection, RenderTarget2D? renderTarget = null, Viewport? viewport = null, Rectangle? scissor = null)
        {
            View = view;
            Projection = projection;
            RenderTarget = renderTarget;
            Viewport = viewport;
            Scissor = scissor;
        }

        public Matrix View { get; }

        public Matrix Projection { get; }

        public RenderTarget2D? RenderTarget { get; }

        public Viewport? Viewport { get; }

        public Rectangle? Scissor { get; }
    }

    /// <summary>
    /// Simple material container to describe blend/sampler configuration and whether vertex color is used.
    /// </summary>
    internal sealed class Material
    {
        public Material(BlendState? blendState, SamplerState? samplerState, Effect? effect, Color? constantColor = null, bool useVertexColor = false, bool useTexture = true)
        {
            BlendState = blendState ?? BlendState.AlphaBlend;
            SamplerState = samplerState ?? SamplerState.LinearClamp;
            Effect = effect;
            ConstantColor = constantColor;
            UseVertexColor = useVertexColor;
            UseTexture = useTexture;
        }

        public BlendState BlendState { get; }

        public SamplerState SamplerState { get; }

        public Effect? Effect { get; }

        public Color? ConstantColor { get; }

        public bool UseVertexColor { get; }

        public bool UseTexture { get; }

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
    internal readonly struct QuadDrawCommand
    {
        public QuadDrawCommand(Texture2D texture, VertexPositionColorTexture[] vertices, short[]? indices, Material material, Matrix world, PrimitiveType primitiveType = PrimitiveType.TriangleStrip)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            Indices = indices;
            Material = material ?? throw new ArgumentNullException(nameof(material));
            World = world;
            PrimitiveType = primitiveType;
        }

        public Texture2D Texture { get; }

        public VertexPositionColorTexture[] Vertices { get; }

        public short[]? Indices { get; }

        public Material Material { get; }

        public Matrix World { get; }

        public PrimitiveType PrimitiveType { get; }
    }

    /// <summary>
    /// General mesh draw command. Will be routed to vertex/index buffers in later phases.
    /// </summary>
    internal readonly struct MeshDrawCommand
    {
        public MeshDrawCommand(VertexPositionColorTexture[] vertices, short[]? indices, Material material, Matrix world, PrimitiveType primitiveType, int primitiveCount)
        {
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            Indices = indices;
            Material = material ?? throw new ArgumentNullException(nameof(material));
            World = world;
            PrimitiveType = primitiveType;
            PrimitiveCount = primitiveCount;
        }

        public VertexPositionColorTexture[] Vertices { get; }

        public short[]? Indices { get; }

        public Material Material { get; }

        public Matrix World { get; }

        public PrimitiveType PrimitiveType { get; }

        public int PrimitiveCount { get; }
    }

    /// <summary>
    /// Placeholder particle batch command for later buffer-backed implementation.
    /// </summary>
    internal readonly struct ParticleDrawCommand
    {
        public ParticleDrawCommand(VertexPositionColorTexture[] vertices, short[] indices, Material material, Matrix world)
        {
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            Indices = indices ?? throw new ArgumentNullException(nameof(indices));
            Material = material ?? throw new ArgumentNullException(nameof(material));
            World = world;
        }

        public VertexPositionColorTexture[] Vertices { get; }

        public short[] Indices { get; }

        public Material Material { get; }

        public Matrix World { get; }
    }
}
