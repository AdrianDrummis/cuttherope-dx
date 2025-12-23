using System;

using CutTheRope.Framework.Rendering;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering.Legacy
{
    /// <summary>
    /// Temporary bridge that allows the existing OpenGL-style code paths to feed the new renderer.
    /// In Phase 1 it only holds references; later phases will translate Gl* calls into renderer commands.
    /// </summary>
    internal static class LegacyGlAdapter
    {
        public static IRenderer? Renderer { get; private set; }

        public static bool IsAttached => Renderer != null;

        public static void Attach(IRenderer renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public static void Clear(Color color)
        {
            Renderer?.Clear(color);
        }

        public static void SetViewProjection(Matrix view, Matrix projection)
        {
            Renderer?.UpdateViewProjection(view, projection);
        }

        public static void SetViewport(Viewport viewport, RenderTarget2D? renderTarget = null)
        {
            Renderer?.SetViewport(viewport, renderTarget);
        }

        public static void SetScissor(Rectangle? scissor)
        {
            Renderer?.SetScissor(scissor);
        }

        public static void BeginFrame(Matrix view, Matrix projection, RenderTarget2D? renderTarget = null, Viewport? viewport = null, Rectangle? scissor = null)
        {
            Renderer?.BeginFrame(new RenderFrameContext(view, projection, renderTarget, viewport, scissor));
        }

        public static void EndFrame()
        {
            Renderer?.EndFrame();
        }

        public static void DrawTextured(VertexPositionNormalTexture[] vertices, short[]? indices, Matrix world, Material? material = null, PrimitiveType primitiveType = PrimitiveType.TriangleList)
        {
            if (Renderer == null || vertices.Length == 0)
            {
                return;
            }
            VertexPositionColorTexture[] converted = Convert(vertices);
            MeshDrawCommand command = new(
                converted,
                indices,
                material ?? MaterialPresets.TexturedAlphaBlend,
                world,
                primitiveType,
                GetPrimitiveCount(primitiveType, converted.Length, indices?.Length ?? 0)
            );
            Renderer.DrawMesh(command);
        }

        public static void DrawColored(VertexPositionColor[] vertices, short[]? indices, Matrix world, Material? material = null, PrimitiveType primitiveType = PrimitiveType.TriangleStrip)
        {
            if (Renderer == null || vertices.Length == 0)
            {
                return;
            }
            VertexPositionColorTexture[] converted = Convert(vertices);
            MeshDrawCommand command = new(
                converted,
                indices,
                material ?? MaterialPresets.SolidColorAlphaBlend,
                world,
                primitiveType,
                GetPrimitiveCount(primitiveType, converted.Length, indices?.Length ?? 0)
            );
            Renderer.DrawMesh(command);
        }

        private static VertexPositionColorTexture[] Convert(VertexPositionNormalTexture[] vertices)
        {
            VertexPositionColorTexture[] result = new VertexPositionColorTexture[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                result[i] = new VertexPositionColorTexture(vertices[i].Position, Color.White, vertices[i].TextureCoordinate);
            }
            return result;
        }

        private static VertexPositionColorTexture[] Convert(VertexPositionColor[] vertices)
        {
            VertexPositionColorTexture[] result = new VertexPositionColorTexture[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                result[i] = new VertexPositionColorTexture(vertices[i].Position, vertices[i].Color, Vector2.Zero);
            }
            return result;
        }

        private static int GetPrimitiveCount(PrimitiveType type, int vertexCount, int indexCount)
        {
            return type switch
            {
                PrimitiveType.TriangleStrip => (indexCount > 0 ? indexCount : vertexCount) - 2,
                PrimitiveType.TriangleList => (indexCount > 0 ? indexCount : vertexCount) / 3,
                _ => 0
            };
        }
    }
}
