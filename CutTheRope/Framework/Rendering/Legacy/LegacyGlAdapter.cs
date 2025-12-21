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
        public static IRenderer? Renderer { get; private set; }

        public static bool IsAttached => Renderer != null;

        public static void Attach(IRenderer renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public static void BeginFrame(Matrix view, Matrix projection, RenderTarget2D? renderTarget = null, Viewport? viewport = null, Rectangle? scissor = null)
        {
            Renderer?.BeginFrame(new RenderFrameContext(view, projection, renderTarget, viewport, scissor));
        }

        public static void EndFrame()
        {
            Renderer?.EndFrame();
        }
    }
}
