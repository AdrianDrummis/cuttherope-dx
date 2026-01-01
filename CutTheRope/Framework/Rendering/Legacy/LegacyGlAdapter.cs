#nullable enable
using System;

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
        private static readonly TransformStack _transformStack = new();
        private static Color _currentColor = Color.White;
        private static BlendState? _currentBlendState = BlendState.AlphaBlend;
        private static bool _texturingEnabled = false;
        private static bool _blendingEnabled = false;

        public static IRenderer? Renderer { get; private set; }

        public static bool IsAttached => Renderer != null;

        /// <summary>
        /// Gets the current model-view transformation matrix from the transform stack.
        /// </summary>
        public static Matrix CurrentTransform => _transformStack.Current;

        /// <summary>
        /// Gets the current color state.
        /// </summary>
        public static Color CurrentColor => _currentColor;

        /// <summary>
        /// Gets the current blend state.
        /// </summary>
        public static BlendState? CurrentBlendState => _currentBlendState;

        /// <summary>
        /// Gets whether texturing is enabled.
        /// </summary>
        public static bool IsTexturingEnabled => _texturingEnabled;

        /// <summary>
        /// Gets whether blending is enabled.
        /// </summary>
        public static bool IsBlendingEnabled => _blendingEnabled;

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

        /// <summary>
        /// Pushes the current transformation matrix onto the stack.
        /// </summary>
        public static void PushMatrix()
        {
            _transformStack.Push();
        }

        /// <summary>
        /// Pops the top transformation matrix from the stack.
        /// </summary>
        public static void PopMatrix()
        {
            _transformStack.Pop();
        }

        /// <summary>
        /// Resets the current transformation to identity.
        /// </summary>
        public static void LoadIdentity()
        {
            _transformStack.LoadIdentity();
        }

        /// <summary>
        /// Applies a translation to the current transformation.
        /// </summary>
        public static void Translate(float x, float y, float z = 0f)
        {
            _transformStack.Translate(x, y, z);
        }

        /// <summary>
        /// Applies a rotation (in degrees) around the Z axis.
        /// </summary>
        public static void Rotate(float angleInDegrees, float x = 0f, float y = 0f, float z = 1f)
        {
            _transformStack.Rotate(angleInDegrees, x, y, z);
        }

        /// <summary>
        /// Applies a scale to the current transformation.
        /// </summary>
        public static void Scale(float x, float y, float z = 1f)
        {
            _transformStack.Scale(x, y, z);
        }

        /// <summary>
        /// Sets the current color state.
        /// </summary>
        public static void SetColor(Color color)
        {
            _currentColor = color;
        }

        /// <summary>
        /// Sets the blend state from OpenGL blend factors.
        /// </summary>
        public static void SetBlendFunc(BlendState blendState)
        {
            _currentBlendState = blendState;
        }

        /// <summary>
        /// Enables or disables texturing.
        /// </summary>
        public static void SetTexturingEnabled(bool enabled)
        {
            _texturingEnabled = enabled;
        }

        /// <summary>
        /// Enables or disables blending.
        /// </summary>
        public static void SetBlendingEnabled(bool enabled)
        {
            _blendingEnabled = enabled;
        }

        public static void DrawTextured(Texture2D texture, VertexPositionNormalTexture[] vertices, short[]? indices, Matrix world, Material? material = null, PrimitiveType primitiveType = PrimitiveType.TriangleList)
        {
            if (Renderer == null || vertices.Length == 0)
            {
                return;
            }
            VertexPositionColorTexture[] converted = Convert(vertices);
            int indexCount = indices?.Length ?? 0;
            MeshDrawCommand command = new(
                converted,
                indices,
                texture,
                material ?? MaterialPresets.TexturedAlphaBlend,
                world,
                primitiveType,
                GetPrimitiveCount(primitiveType, converted.Length, indexCount),
                converted.Length,
                indexCount
            );
            Renderer.DrawMesh(command);
        }

        public static void DrawTexturedColored(Texture2D texture, VertexPositionColorTexture[] vertices, short[]? indices, Matrix world, Material? material = null, PrimitiveType primitiveType = PrimitiveType.TriangleList)
        {
            if (Renderer == null || vertices.Length == 0)
            {
                return;
            }
            int indexCount = indices?.Length ?? 0;
            MeshDrawCommand command = new(
                vertices,
                indices,
                texture,
                material ?? MaterialPresets.TexturedVertexColorAlphaBlend,
                world,
                primitiveType,
                GetPrimitiveCount(primitiveType, vertices.Length, indexCount),
                vertices.Length,
                indexCount
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
            int indexCount = indices?.Length ?? 0;
            MeshDrawCommand command = new(
                converted,
                indices,
                null,
                material ?? MaterialPresets.SolidColorAlphaBlend,
                world,
                primitiveType,
                GetPrimitiveCount(primitiveType, converted.Length, indexCount),
                converted.Length,
                indexCount
            );
            Renderer.DrawMesh(command);
        }

        public static VertexPositionColorTexture[] Convert(VertexPositionNormalTexture[] vertices)
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
            int count = indexCount > 0 ? indexCount : vertexCount;
            return type switch
            {
                PrimitiveType.TriangleStrip => count - 2,
                PrimitiveType.TriangleList => count / 3,
                PrimitiveType.LineList => count / 2,
                PrimitiveType.LineStrip => count - 1,
                PrimitiveType.PointList => count,
                _ => 0
            };
        }
    }
}
