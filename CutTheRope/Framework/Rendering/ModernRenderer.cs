#nullable enable
using System;

using CutTheRope.Framework;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    /// <summary>
    /// Initial modern renderer implementation that mirrors the BasicEffect-based pipeline but with explicit materials and frame context.
    /// Later phases will replace per-draw user primitives with batching and GPU buffers.
    /// </summary>
    internal sealed class ModernRenderer : IRenderer, IQuadBatchRenderer
    {
        private GraphicsDevice? _graphicsDevice;

        private BasicEffect? _effectTexture;

        private BasicEffect? _effectTextureColor;

        private BasicEffect? _effectColor;

        private RenderFrameContext _frameContext;
        private RasterizerState? _rasterizerNoScissor;

        private RasterizerState? _rasterizerScissor;

        private Matrix _currentView = Matrix.Identity;

        private Matrix _currentProjection = Matrix.Identity;

        private Viewport _currentViewport;

        private Rectangle? _currentScissor;

        private RenderTarget2D? _currentRenderTarget;

        private VertexPositionColorTexture[] _quadVertexCache = [];

        private short[] _quadIndexCache = [];

        private DynamicVertexBuffer? _vertexBuffer;

        private DynamicIndexBuffer? _indexBuffer;

        private int _vertexBufferCapacity;

        private int _indexBufferCapacity;

        private int _vertexBufferOffset;

        private int _indexBufferOffset;

        public RendererStats Stats { get; private set; }

        public void Initialize(GraphicsDevice device)
        {
            _graphicsDevice = device ?? throw new ArgumentNullException(nameof(device));
            _effectTexture = new BasicEffect(device)
            {
                TextureEnabled = true,
                VertexColorEnabled = false,
                View = Matrix.Identity,
                Projection = Matrix.Identity,
                World = Matrix.Identity
            };
            _effectTextureColor = new BasicEffect(device)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                View = Matrix.Identity,
                Projection = Matrix.Identity,
                World = Matrix.Identity
            };
            _effectColor = new BasicEffect(device)
            {
                TextureEnabled = false,
                VertexColorEnabled = true,
                View = Matrix.Identity,
                Projection = Matrix.Identity,
                World = Matrix.Identity
            };
            _rasterizerNoScissor = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = false
            };
            _rasterizerScissor = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };
        }

        public void BeginFrame(in RenderFrameContext context)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before BeginFrame.");
            }
            _frameContext = context;
            Stats = default;

            _currentRenderTarget = context.RenderTarget;
            _currentView = context.View;
            _currentProjection = context.Projection;
            _currentViewport = context.Viewport ?? _graphicsDevice.Viewport;
            _currentScissor = context.Scissor;

            _graphicsDevice.SetRenderTarget(_currentRenderTarget);
            _graphicsDevice.Viewport = _currentViewport;
            if (_currentScissor.HasValue)
            {
                _graphicsDevice.ScissorRectangle = _currentScissor.Value;
            }

            ApplyMatrices(_effectTexture);
            ApplyMatrices(_effectTextureColor);
            ApplyMatrices(_effectColor);
        }

        public void Clear(Color color)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before Clear.");
            }
            _graphicsDevice.Clear(color);
        }

        public void DrawQuad(in QuadDrawCommand command)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before drawing.");
            }
            if (command.Vertices.Length == 0)
            {
                return;
            }

            BasicEffect effect = GetEffectForMaterial(command.Material);
            ConfigureEffect(effect, command.Material, command.Texture, command.World);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (command.Indices != null)
                {
                    int primitiveCount = command.PrimitiveType == PrimitiveType.TriangleStrip
                        ? command.Indices.Length - 2
                        : command.Indices.Length / 3;
                    int vertexCount = command.Vertices.Length;
                    int indexCount = command.Indices.Length;
                    UploadVertices(command.Vertices, vertexCount, out int vertexOffset);
                    UploadIndices(command.Indices, indexCount, out int indexOffset);
                    _graphicsDevice.SetVertexBuffer(_vertexBuffer);
                    _graphicsDevice.Indices = _indexBuffer;
                    _graphicsDevice.DrawIndexedPrimitives(command.PrimitiveType, vertexOffset, indexOffset, primitiveCount);
                }
                else
                {
                    int primitiveCount = command.PrimitiveType == PrimitiveType.TriangleStrip
                        ? command.Vertices.Length - 2
                        : command.Vertices.Length / 3;
                    int vertexCount = command.Vertices.Length;
                    UploadVertices(command.Vertices, vertexCount, out int vertexOffset);
                    _graphicsDevice.SetVertexBuffer(_vertexBuffer);
                    _graphicsDevice.DrawPrimitives(command.PrimitiveType, vertexOffset, primitiveCount);
                }
            }

            Stats = Stats with
            {
                DrawCalls = Stats.DrawCalls + 1,
                Vertices = Stats.Vertices + command.Vertices.Length,
                Indices = Stats.Indices + (command.Indices?.Length ?? 0)
            };
        }

        public void DrawMesh(in MeshDrawCommand command)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before drawing.");
            }
            if (command.Vertices.Length == 0)
            {
                return;
            }

            BasicEffect effect = GetEffectForMaterial(command.Material);
            ConfigureEffect(effect, command.Material, command.Texture, command.World);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (command.Indices != null)
                {
                    UploadVertices(command.Vertices, command.VertexCount, out int vertexOffset);
                    UploadIndices(command.Indices, command.IndexCount, out int indexOffset);
                    _graphicsDevice.SetVertexBuffer(_vertexBuffer);
                    _graphicsDevice.Indices = _indexBuffer;
                    _graphicsDevice.DrawIndexedPrimitives(command.PrimitiveType, vertexOffset, indexOffset, command.PrimitiveCount);
                }
                else
                {
                    UploadVertices(command.Vertices, command.VertexCount, out int vertexOffset);
                    _graphicsDevice.SetVertexBuffer(_vertexBuffer);
                    _graphicsDevice.DrawPrimitives(command.PrimitiveType, vertexOffset, command.PrimitiveCount);
                }
            }

            Stats = Stats with
            {
                DrawCalls = Stats.DrawCalls + 1,
                Vertices = Stats.Vertices + command.VertexCount,
                Indices = Stats.Indices + command.IndexCount
            };
        }

        public void DrawParticles(in ParticleDrawCommand command)
        {
            // Phase 1: reuse mesh path; later phases will move to buffer-backed particle batching.
            MeshDrawCommand meshCommand = new(command.Vertices, command.Indices, null, command.Material, command.World, PrimitiveType.TriangleList, command.Indices.Length / 3, command.Vertices.Length, command.Indices.Length);
            DrawMesh(meshCommand);
        }

        public void DrawTexturedQuads(Texture2D texture, Quad3D[] positions, Quad2D[] texCoords, RGBAColor[]? colors, int quadCount, Material material, Matrix world)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before drawing.");
            }
            if (quadCount <= 0 || positions.Length < quadCount || texCoords.Length < quadCount)
            {
                return;
            }
            int vertexCount = quadCount * 4;
            int indexCount = quadCount * 6;

            if (_quadVertexCache.Length < vertexCount)
            {
                _quadVertexCache = new VertexPositionColorTexture[vertexCount];
            }
            if (_quadIndexCache.Length < indexCount)
            {
                _quadIndexCache = new short[indexCount];
                for (int i = 0; i < quadCount; i++)
                {
                    int baseVertex = i * 4;
                    int baseIndex = i * 6;
                    _quadIndexCache[baseIndex] = (short)baseVertex;
                    _quadIndexCache[baseIndex + 1] = (short)(baseVertex + 1);
                    _quadIndexCache[baseIndex + 2] = (short)(baseVertex + 2);
                    _quadIndexCache[baseIndex + 3] = (short)(baseVertex + 3);
                    _quadIndexCache[baseIndex + 4] = (short)(baseVertex + 2);
                    _quadIndexCache[baseIndex + 5] = (short)(baseVertex + 1);
                }
            }

            Color fallbackColor = material.ConstantColor ?? Color.White;
            bool useVertexColor = colors != null && colors.Length >= vertexCount;
            int vertexIndex = 0;
            for (int i = 0; i < quadCount; i++)
            {
                float[] pos = positions[i].ToFloatArray();
                Quad2D quadUv = texCoords[i];
                float tlX = quadUv.tlX;
                float tlY = quadUv.tlY;
                float trX = quadUv.trX;
                float trY = quadUv.trY;
                float blX = quadUv.blX;
                float blY = quadUv.blY;
                float brX = quadUv.brX;
                float brY = quadUv.brY;

                Color c0 = useVertexColor ? colors![(i * 4) + 0].ToXNA() : fallbackColor;
                Color c1 = useVertexColor ? colors![(i * 4) + 1].ToXNA() : fallbackColor;
                Color c2 = useVertexColor ? colors![(i * 4) + 2].ToXNA() : fallbackColor;
                Color c3 = useVertexColor ? colors![(i * 4) + 3].ToXNA() : fallbackColor;

                _quadVertexCache[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[0], pos[1], pos[2]), c0, new Vector2(tlX, tlY));
                _quadVertexCache[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[3], pos[4], pos[5]), c1, new Vector2(trX, trY));
                _quadVertexCache[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[6], pos[7], pos[8]), c2, new Vector2(blX, blY));
                _quadVertexCache[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[9], pos[10], pos[11]), c3, new Vector2(brX, brY));
            }

            MeshDrawCommand command = new(_quadVertexCache, _quadIndexCache, texture, material, world, PrimitiveType.TriangleList, indexCount / 3, vertexCount, indexCount);
            DrawMesh(command);
        }

        public void EndFrame()
        {
            // Placeholder for flush/restore logic. Later phases will flush batches and restore states as needed.
        }

        public void Dispose()
        {
            _effectTexture?.Dispose();
            _effectTextureColor?.Dispose();
            _effectColor?.Dispose();
            _rasterizerNoScissor?.Dispose();
            _rasterizerScissor?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }

        public void UpdateViewProjection(Matrix view, Matrix projection)
        {
            _currentView = view;
            _currentProjection = projection;
            ApplyMatrices(_effectTexture);
            ApplyMatrices(_effectTextureColor);
            ApplyMatrices(_effectColor);
        }

        public void SetViewport(Viewport viewport, RenderTarget2D? renderTarget = null)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before SetViewport.");
            }
            _currentViewport = viewport;
            _currentRenderTarget = renderTarget ?? _currentRenderTarget;
            _graphicsDevice.SetRenderTarget(_currentRenderTarget);
            _graphicsDevice.Viewport = _currentViewport;
            if (_currentScissor.HasValue)
            {
                _graphicsDevice.ScissorRectangle = _currentScissor.Value;
            }
        }

        public void SetScissor(Rectangle? scissor)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before SetScissor.");
            }
            _currentScissor = scissor;
            if (_currentScissor.HasValue)
            {
                _graphicsDevice.ScissorRectangle = _currentScissor.Value;
            }
            _graphicsDevice.RasterizerState = _currentScissor.HasValue ? _rasterizerScissor! : _rasterizerNoScissor!;
        }

        private void ApplyMatrices(BasicEffect? effect)
        {
            if (effect == null)
            {
                return;
            }
            effect.View = _currentView;
            effect.Projection = _currentProjection;
            effect.World = Matrix.Identity;
        }

        private BasicEffect GetEffectForMaterial(Material material)
        {
            BasicEffect? chosen = material.UseTexture
                ? material.UseVertexColor ? _effectTextureColor : _effectTexture
                : _effectColor;
            return chosen ?? throw new InvalidOperationException("Renderer effects are not initialized.");
        }

        private void ConfigureEffect(BasicEffect effect, Material material, Texture2D? texture, Matrix world)
        {
            if (material.UseTexture && texture != null)
            {
                effect.Texture = texture;
                if (material.ConstantColor.HasValue)
                {
                    effect.DiffuseColor = material.ConstantColor.Value.ToVector3();
                    effect.Alpha = material.ConstantColor.Value.A / 255f;
                }
                else if (material.UseVertexColor)
                {
                    effect.DiffuseColor = Vector3.One;
                    effect.Alpha = 1f;
                }
            }
            else if (material.ConstantColor.HasValue)
            {
                effect.DiffuseColor = material.ConstantColor.Value.ToVector3();
                effect.Alpha = material.ConstantColor.Value.A / 255f;
            }
            else if (material.UseVertexColor)
            {
                effect.DiffuseColor = Vector3.One;
                effect.Alpha = 1f;
            }

            effect.World = world;
            _graphicsDevice!.BlendState = material.BlendState;
            _graphicsDevice.SamplerStates[0] = material.SamplerState;
            _graphicsDevice.RasterizerState = _currentScissor.HasValue ? _rasterizerScissor! : _rasterizerNoScissor!;
            Stats = Stats with { StateChanges = Stats.StateChanges + 1 };
        }

        private void EnsureVertexBuffer(int vertexCount)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before drawing.");
            }
            if (_vertexBuffer == null || vertexCount > _vertexBufferCapacity)
            {
                _vertexBuffer?.Dispose();
                _vertexBufferCapacity = NextPowerOfTwo(vertexCount);
                _vertexBufferOffset = 0;
                _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, VertexPositionColorTexture.VertexDeclaration, _vertexBufferCapacity, BufferUsage.WriteOnly);
            }
        }

        private void EnsureIndexBuffer(int indexCount)
        {
            if (_graphicsDevice == null)
            {
                throw new InvalidOperationException("Renderer must be initialized before drawing.");
            }
            if (_indexBuffer == null || indexCount > _indexBufferCapacity)
            {
                _indexBuffer?.Dispose();
                _indexBufferCapacity = NextPowerOfTwo(indexCount);
                _indexBufferOffset = 0;
                _indexBuffer = new DynamicIndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, _indexBufferCapacity, BufferUsage.WriteOnly);
            }
        }

        private void UploadVertices(VertexPositionColorTexture[] vertices, int vertexCount, out int vertexOffset)
        {
            EnsureVertexBuffer(vertexCount);
            if (_vertexBufferOffset + vertexCount > _vertexBufferCapacity)
            {
                _vertexBufferOffset = 0;
            }
            SetDataOptions options = _vertexBufferOffset == 0 ? SetDataOptions.Discard : SetDataOptions.NoOverwrite;
            vertexOffset = _vertexBufferOffset;
            int stride = VertexPositionColorTexture.VertexDeclaration.VertexStride;
            _vertexBuffer!.SetData(vertexOffset * stride, vertices, 0, vertexCount, stride, options);
            _vertexBufferOffset += vertexCount;
        }

        private void UploadIndices(short[] indices, int indexCount, out int indexOffset)
        {
            EnsureIndexBuffer(indexCount);
            if (_indexBufferOffset + indexCount > _indexBufferCapacity)
            {
                _indexBufferOffset = 0;
            }
            SetDataOptions options = _indexBufferOffset == 0 ? SetDataOptions.Discard : SetDataOptions.NoOverwrite;
            indexOffset = _indexBufferOffset;
            int offsetInBytes = indexOffset * sizeof(short);
            _indexBuffer!.SetData(offsetInBytes, indices, 0, indexCount, options);
            _indexBufferOffset += indexCount;
        }

        private static int NextPowerOfTwo(int value)
        {
            int result = 1;
            while (result < value)
            {
                result <<= 1;
            }
            return result;
        }
    }
}
