#nullable enable
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    /// <summary>
    /// Abstraction for renderer implementations that accept command structs and frame context.
    /// </summary>
    internal interface IRenderer : IDisposable
    {
        /// <summary>
        /// Initializes device resources for the renderer.
        /// </summary>
        /// <param name="device">Graphics device to bind resources against.</param>
        void Initialize(GraphicsDevice device);

        /// <summary>
        /// Begins a new frame with the provided context.
        /// </summary>
        /// <param name="context">Frame-scoped rendering context.</param>
        void BeginFrame(in RenderFrameContext context);

        /// <summary>
        /// Updates the view and projection matrices used by the renderer.
        /// </summary>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        void UpdateViewProjection(Matrix view, Matrix projection);

        /// <summary>
        /// Sets the active viewport and optional render target.
        /// </summary>
        /// <param name="viewport">Viewport to apply.</param>
        /// <param name="renderTarget">Optional render target override.</param>
        void SetViewport(Viewport viewport, RenderTarget2D? renderTarget = null);

        /// <summary>
        /// Updates the scissor rectangle.
        /// </summary>
        /// <param name="scissor">Optional scissor rectangle.</param>
        void SetScissor(Rectangle? scissor);

        /// <summary>
        /// Clears the current render target.
        /// </summary>
        /// <param name="color">Clear color.</param>
        void Clear(Color color);

        /// <summary>
        /// Draws a quad command.
        /// </summary>
        /// <param name="command">Quad draw command.</param>
        void DrawQuad(in QuadDrawCommand command);

        /// <summary>
        /// Draws a mesh command.
        /// </summary>
        /// <param name="command">Mesh draw command.</param>
        void DrawMesh(in MeshDrawCommand command);

        /// <summary>
        /// Draws a particle command.
        /// </summary>
        /// <param name="command">Particle draw command.</param>
        void DrawParticles(in ParticleDrawCommand command);

        /// <summary>
        /// Ends the current frame.
        /// </summary>
        void EndFrame();

        /// <summary>
        /// Gets lightweight renderer statistics for the current frame.
        /// </summary>
        RendererStats Stats { get; }
    }
}
