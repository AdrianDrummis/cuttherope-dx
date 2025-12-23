#nullable enable
using CutTheRope.Framework;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Rendering
{
    internal interface IQuadBatchRenderer
    {
        void DrawTexturedQuads(Texture2D texture, Quad3D[] positions, Quad2D[] texCoords, RGBAColor[]? colors, int quadCount, Material material, Matrix world);
    }
}
