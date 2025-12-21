using System;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    internal interface IRenderer : IDisposable
    {
        void Initialize(GraphicsDevice device);

        void BeginFrame(in RenderFrameContext context);

        void DrawQuad(in QuadDrawCommand command);

        void DrawMesh(in MeshDrawCommand command);

        void DrawParticles(in ParticleDrawCommand command);

        void EndFrame();

        RendererStats Stats { get; }
    }
}
