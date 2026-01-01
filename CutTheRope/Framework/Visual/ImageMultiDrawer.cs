using System;

using CutTheRope.Desktop;
using CutTheRope.Framework.Rendering;
using CutTheRope.Framework.Rendering.Legacy;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    internal sealed class ImageMultiDrawer : BaseElement
    {
        public ImageMultiDrawer InitWithImageandCapacity(Image i, int n)
        {
            image = i;
            numberOfQuadsToDraw = -1;
            totalQuads = n;
            texCoordinates = new Quad2D[totalQuads];
            vertices = new Quad3D[totalQuads];
            indices = new short[totalQuads * 6];
            InitIndices();
            return this;
        }

        private void FreeWithCheck()
        {
            texCoordinates = null;
            vertices = null;
            indices = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FreeWithCheck();
                image = null;
                verticesOptimized = null;
            }
            base.Dispose(disposing);
        }

        private void InitIndices()
        {
            for (int i = 0; i < totalQuads; i++)
            {
                indices[i * 6] = (short)(i * 4);
                indices[(i * 6) + 1] = (short)((i * 4) + 1);
                indices[(i * 6) + 2] = (short)((i * 4) + 2);
                indices[(i * 6) + 3] = (short)((i * 4) + 3);
                indices[(i * 6) + 4] = (short)((i * 4) + 2);
                indices[(i * 6) + 5] = (short)((i * 4) + 1);
            }
        }

        public void SetTextureQuadatVertexQuadatIndex(Quad2D qt, Quad3D qv, int n)
        {
            if (n >= totalQuads)
            {
                ResizeCapacity(n + 1);
            }
            texCoordinates[n] = qt;
            vertices[n] = qv;
        }

        public void MapTextureQuadAtXYatIndex(int q, float dx, float dy, int n)
        {
            if (n >= totalQuads)
            {
                ResizeCapacity(n + 1);
            }
            texCoordinates[n] = image.texture.quads[q];
            vertices[n] = Quad3D.MakeQuad3D((double)(dx + image.texture.quadOffsets[q].x), (double)(dy + image.texture.quadOffsets[q].y), 0.0, image.texture.quadRects[q].w, image.texture.quadRects[q].h);
        }

        private void DrawNumberOfQuads(int n)
        {
            if (TryDrawTexturedQuadBatch(n))
            {
                return;
            }
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(image.texture.Name());
            OpenGL.GlVertexPointer(3, 5, 0, ToFloatArray(vertices));
            OpenGL.GlTexCoordPointer(2, 5, 0, ToFloatArray(texCoordinates));
            OpenGL.GlDrawElements(7, n * 6, indices);
        }

        public void Optimize(VertexPositionNormalTexture[] v)
        {
            if (v != null && verticesOptimized == null)
            {
                verticesOptimized = v;
            }
        }

        public void DrawAllQuads()
        {
            if (verticesOptimized == null)
            {
                DrawNumberOfQuads(totalQuads);
                return;
            }
            if (TryDrawOptimized())
            {
                return;
            }
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(image.texture.Name());
            OpenGL.Optimized_DrawTriangleList(verticesOptimized, indices);
        }

        public override void Draw()
        {
            PreDraw();
            OpenGL.GlTranslatef(drawX, drawY, 0f);
            if (numberOfQuadsToDraw == -1)
            {
                DrawAllQuads();
            }
            else if (numberOfQuadsToDraw > 0)
            {
                DrawNumberOfQuads(numberOfQuadsToDraw);
            }
            OpenGL.GlTranslatef(0f - drawX, 0f - drawY, 0f);
            PostDraw();
        }

        private bool TryDrawTexturedQuadBatch(int quadCount)
        {
            if (Global.Renderer == null || image?.texture?.xnaTexture_ == null)
            {
                return false;
            }
            if (quadCount <= 0 || quadCount > totalQuads)
            {
                return false;
            }
            int vertexCount = quadCount * 4;
            VertexPositionColorTexture[] meshVertices = new VertexPositionColorTexture[vertexCount];
            int vertexIndex = 0;
            for (int i = 0; i < quadCount; i++)
            {
                Quad3D quadVertices = vertices[i];
                Quad2D quadTex = texCoordinates[i];
                float[] pos = quadVertices.ToFloatArray();
                float[] uv = quadTex.ToFloatArray();
                Color color = OpenGL.GetCurrentColor();
                meshVertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[0], pos[1], pos[2]), color, new Vector2(uv[0], uv[1]));
                meshVertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[3], pos[4], pos[5]), color, new Vector2(uv[2], uv[3]));
                meshVertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[6], pos[7], pos[8]), color, new Vector2(uv[4], uv[5]));
                meshVertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[9], pos[10], pos[11]), color, new Vector2(uv[6], uv[7]));
            }
            if (vertexIndex == 0)
            {
                return false;
            }
            if (vertexIndex != meshVertices.Length)
            {
                VertexPositionColorTexture[] trimmed = new VertexPositionColorTexture[vertexIndex];
                Array.Copy(meshVertices, trimmed, vertexIndex);
                meshVertices = trimmed;
            }
            int indexCount = quadCount * 6;
            short[] drawIndices = indices;
            if (indices.Length != indexCount)
            {
                drawIndices = new short[indexCount];
                Array.Copy(indices, drawIndices, indexCount);
            }
            Material material = OpenGL.GetMaterialForCurrentState(useTexture: true, useVertexColor: false, constantColor: OpenGL.GetCurrentColor());
            MeshDrawCommand command = new(meshVertices, drawIndices, image.texture.xnaTexture_, material, OpenGL.GetModelViewMatrix(), PrimitiveType.TriangleList, indexCount / 3, meshVertices.Length, indexCount);
            Global.Renderer.DrawMesh(command);
            return true;
        }

        private bool TryDrawOptimized()
        {
            if (Global.Renderer == null || verticesOptimized == null || image?.texture?.xnaTexture_ == null)
            {
                return false;
            }
            Material material = OpenGL.GetMaterialForCurrentState(useTexture: true, useVertexColor: false, constantColor: OpenGL.GetCurrentColor());
            MeshDrawCommand command = new(LegacyGlAdapter.Convert(verticesOptimized), indices, image.texture.xnaTexture_, material, OpenGL.GetModelViewMatrix(), PrimitiveType.TriangleList, indices.Length / 3, verticesOptimized.Length, indices.Length);
            Global.Renderer.DrawMesh(command);
            return true;
        }

        private void ResizeCapacity(int n)
        {
            if (n != totalQuads)
            {
                totalQuads = n;
                texCoordinates = new Quad2D[totalQuads];
                vertices = new Quad3D[totalQuads];
                indices = new short[totalQuads * 6];
                if (texCoordinates == null || vertices == null || indices == null)
                {
                    FreeWithCheck();
                }
                InitIndices();
            }
        }

        public Image image;

        public int totalQuads;

        public Quad2D[] texCoordinates;

        public Quad3D[] vertices;

        public short[] indices;

        public int numberOfQuadsToDraw;

        private VertexPositionNormalTexture[] verticesOptimized;
    }
}
