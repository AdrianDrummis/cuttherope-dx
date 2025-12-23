#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    /// <summary>
    /// Optional interface for renderers that accept batched textured quad draws.
    /// </summary>
    internal interface IQuadBatchRenderer
    {
        /// <summary>
        /// Draws a batch of textured quads.
        /// </summary>
        /// <param name="texture">Texture to bind for the draw.</param>
        /// <param name="positions">Quad positions (4 vertices per quad).</param>
        /// <param name="texCoords">Quad UV coordinates (4 vertices per quad).</param>
        /// <param name="colors">Optional per-vertex colors.</param>
        /// <param name="quadCount">Number of quads to draw.</param>
        /// <param name="material">Material state used for the draw.</param>
        /// <param name="world">World transform to apply.</param>
        void DrawTexturedQuads(Texture2D texture, Quad3D[] positions, Quad2D[] texCoords, RGBAColor[]? colors, int quadCount, Material material, Matrix world);
    }
}
