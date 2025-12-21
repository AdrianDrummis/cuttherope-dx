using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    /// <summary>
    /// Initial modern renderer implementation that mirrors the BasicEffect-based pipeline but with explicit materials and frame context.
    /// Later phases will replace per-draw user primitives with batching and GPU buffers.
    /// </summary>
    internal sealed class ModernRenderer : IRenderer
    {
        private GraphicsDevice? _graphicsDevice;

        private BasicEffect? _effectTexture;

        private BasicEffect? _effectTextureColor;

        private BasicEffect? _effectColor;

        private RenderFrameContext _frameContext;

        private RendererStats _stats;

        private RasterizerState? _rasterizerNoScissor;

        private RasterizerState? _rasterizerScissor;

        public RendererStats Stats => _stats;

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
            _stats = default;

            _graphicsDevice.SetRenderTarget(context.RenderTarget);
            if (context.Viewport.HasValue)
            {
                _graphicsDevice.Viewport = context.Viewport.Value;
            }
            if (context.Scissor.HasValue)
            {
                _graphicsDevice.ScissorRectangle = context.Scissor.Value;
            }

            ApplyMatrices(_effectTexture, context);
            ApplyMatrices(_effectTextureColor, context);
            ApplyMatrices(_effectColor, context);
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
                    _graphicsDevice.DrawUserIndexedPrimitives(command.PrimitiveType, command.Vertices, 0, command.Vertices.Length, command.Indices, 0, primitiveCount);
                }
                else
                {
                    int primitiveCount = command.PrimitiveType == PrimitiveType.TriangleStrip
                        ? command.Vertices.Length - 2
                        : command.Vertices.Length / 3;
                    _graphicsDevice.DrawUserPrimitives(command.PrimitiveType, command.Vertices, 0, primitiveCount);
                }
            }

            _stats = _stats with
            {
                DrawCalls = _stats.DrawCalls + 1,
                Vertices = _stats.Vertices + command.Vertices.Length,
                Indices = _stats.Indices + (command.Indices?.Length ?? 0)
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
            ConfigureEffect(effect, command.Material, null, command.World);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (command.Indices != null)
                {
                    _graphicsDevice.DrawUserIndexedPrimitives(command.PrimitiveType, command.Vertices, 0, command.Vertices.Length, command.Indices, 0, command.PrimitiveCount);
                }
                else
                {
                    _graphicsDevice.DrawUserPrimitives(command.PrimitiveType, command.Vertices, 0, command.PrimitiveCount);
                }
            }

            _stats = _stats with
            {
                DrawCalls = _stats.DrawCalls + 1,
                Vertices = _stats.Vertices + command.Vertices.Length,
                Indices = _stats.Indices + (command.Indices?.Length ?? 0)
            };
        }

        public void DrawParticles(in ParticleDrawCommand command)
        {
            // Phase 1: reuse mesh path; later phases will move to buffer-backed particle batching.
            MeshDrawCommand meshCommand = new(command.Vertices, command.Indices, command.Material, command.World, PrimitiveType.TriangleList, command.Indices.Length / 3);
            DrawMesh(meshCommand);
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
        }

        private static void ApplyMatrices(BasicEffect? effect, in RenderFrameContext context)
        {
            if (effect == null)
            {
                return;
            }
            effect.View = context.View;
            effect.Projection = context.Projection;
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
            }
            else if (material.ConstantColor.HasValue)
            {
                effect.DiffuseColor = material.ConstantColor.Value.ToVector3();
                effect.Alpha = material.ConstantColor.Value.A / 255f;
            }

            effect.World = world;
            _graphicsDevice!.BlendState = material.BlendState;
            _graphicsDevice.SamplerStates[0] = material.SamplerState;
            _graphicsDevice.RasterizerState = _frameContext.Scissor.HasValue ? _rasterizerScissor! : _rasterizerNoScissor!;
            _stats = _stats with { StateChanges = _stats.StateChanges + 1 };
        }
    }
}
