using System;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Rendering;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    internal class MultiParticles : Particles
    {
        public virtual Particles InitWithTotalParticlesandImageGrid(int numberOfParticles, Image image)
        {
            imageGrid = image;
            drawer = new ImageMultiDrawer().InitWithImageandCapacity(imageGrid, numberOfParticles);
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
            totalParticles = numberOfParticles;
            particles = new Particle[totalParticles];
            colors = new RGBAColor[4 * totalParticles];
            if (particles == null || colors == null)
            {
                particles = null;
                colors = null;
                return null;
            }
            active = false;
            blendAdditive = false;
            OpenGL.GlGenBuffers(1, ref colorsID);
            return this;
        }

        public override void InitParticle(ref Particle particle)
        {
            Image image = imageGrid;
            int num = RND(image.texture.quadsCount - 1);
            Quad2D qt = image.texture.quads[num];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            CTRRectangle rectangle = image.texture.quadRects[num];
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            base.InitParticle(ref particle);
            particle.width = rectangle.w * particle.size;
            particle.height = rectangle.h * particle.size;
        }

        public override void UpdateParticle(ref Particle p, float delta)
        {
            if (p.life > 0f)
            {
                Vector vector = vectZero;
                if (p.pos.x != 0f || p.pos.y != 0f)
                {
                    vector = VectNormalize(p.pos);
                }
                Vector v = vector;
                vector = VectMult(vector, p.radialAccel);
                float num = v.x;
                v.x = 0f - v.y;
                v.y = num;
                v = VectMult(v, p.tangentialAccel);
                Vector v2 = VectAdd(VectAdd(vector, v), gravity);
                v2 = VectMult(v2, delta);
                p.dir = VectAdd(p.dir, v2);
                v2 = VectMult(p.dir, delta);
                p.pos = VectAdd(p.pos, v2);
                p.color.r += p.deltaColor.r * delta;
                p.color.g += p.deltaColor.g * delta;
                p.color.b += p.deltaColor.b * delta;
                p.color.a += p.deltaColor.a * delta;
                p.life -= delta;
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3D((double)(p.pos.x - (p.width / 2f)), (double)(p.pos.y - (p.height / 2f)), 0.0, p.width, p.height);
                for (int i = 0; i < 4; i++)
                {
                    colors[(particleIdx * 4) + i] = p.color;
                }
                particleIdx++;
                return;
            }
            if (particleIdx != particleCount - 1)
            {
                particles[particleIdx] = particles[particleCount - 1];
                drawer.vertices[particleIdx] = drawer.vertices[particleCount - 1];
                drawer.texCoordinates[particleIdx] = drawer.texCoordinates[particleCount - 1];
            }
            particleCount--;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (active && emissionRate != 0f)
            {
                float num = 1f / emissionRate;
                emitCounter += delta;
                while (particleCount < totalParticles && emitCounter > num)
                {
                    _ = AddParticle();
                    emitCounter -= num;
                }
                elapsed += delta;
                if (duration != -1f && duration < elapsed)
                {
                    StopSystem();
                }
            }
            particleIdx = 0;
            while (particleIdx < particleCount)
            {
                UpdateParticle(ref particles[particleIdx], delta);
            }
            if (Global.Renderer == null)
            {
                OpenGL.GlBindBuffer(2, colorsID);
                OpenGL.GlBufferData(2, colors, 3);
                OpenGL.GlBindBuffer(2, 0U);
            }
        }

        public override void Draw()
        {
            PreDraw();
            if (blendAdditive)
            {
                OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            }
            else
            {
                OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            }
            if (TryDrawTexturedParticles(particleIdx, useVertexColor: true))
            {
                PostDraw();
                return;
            }
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(drawer.image.texture.Name());
            OpenGL.GlVertexPointer(3, 5, 0, ToFloatArray(drawer.vertices));
            OpenGL.GlTexCoordPointer(2, 5, 0, ToFloatArray(drawer.texCoordinates));
            OpenGL.GlEnableClientState(13);
            OpenGL.GlBindBuffer(2, colorsID);
            OpenGL.GlColorPointer(4, 5, 0, colors);
            OpenGL.GlDrawElements(7, particleIdx * 6, drawer.indices);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            OpenGL.GlBindBuffer(2, 0U);
            OpenGL.GlDisableClientState(13);
            PostDraw();
        }

        protected bool TryDrawTexturedParticles(int quadCount, bool useVertexColor)
        {
            if (Global.Renderer == null || drawer?.image?.texture?.xnaTexture_ == null)
            {
                return false;
            }
            if (quadCount <= 0)
            {
                return false;
            }
            int maxQuads = drawer.vertices.Length;
            if (quadCount > maxQuads)
            {
                quadCount = maxQuads;
            }
            if (Global.Renderer is IQuadBatchRenderer quadRenderer)
            {
                Color batchColor = OpenGL.GetCurrentColor();
                Material batchMaterial = OpenGL.GetMaterialForCurrentState(useTexture: true, useVertexColor: useVertexColor, constantColor: useVertexColor ? null : batchColor);
                quadRenderer.DrawTexturedQuads(drawer.image.texture.xnaTexture_, drawer.vertices, drawer.texCoordinates, useVertexColor ? colors : null, quadCount, batchMaterial, OpenGL.GetModelViewMatrix());
                return true;
            }
            int maxQuads = drawer.vertices.Length;
            if (quadCount > maxQuads)
            {
                quadCount = maxQuads;
            }
            if (Global.Renderer is IQuadBatchRenderer quadRenderer)
            {
                Color batchColor = OpenGL.GetCurrentColor();
                Material batchMaterial = OpenGL.GetMaterialForCurrentState(useTexture: true, useVertexColor: useVertexColor, constantColor: useVertexColor ? null : batchColor);
                quadRenderer.DrawTexturedQuads(drawer.image.texture.xnaTexture_, drawer.vertices, drawer.texCoordinates, useVertexColor ? colors : null, quadCount, batchMaterial, OpenGL.GetModelViewMatrix());
                return true;
            }
            int vertexCount = quadCount * 4;
            VertexPositionColorTexture[] meshVertices = new VertexPositionColorTexture[vertexCount];
            Color currentColor = OpenGL.GetCurrentColor();
            int vertexIndex = 0;
            for (int i = 0; i < quadCount; i++)
            {
                Quad3D quadVertices = drawer.vertices[i];
                Quad2D quadTex = drawer.texCoordinates[i];
                float[] pos = quadVertices.ToFloatArray();
                float[] uv = quadTex.ToFloatArray();
                Color c0 = useVertexColor ? colors[(i * 4) + 0].ToXNA() : currentColor;
                Color c1 = useVertexColor ? colors[(i * 4) + 1].ToXNA() : currentColor;
                Color c2 = useVertexColor ? colors[(i * 4) + 2].ToXNA() : currentColor;
                Color c3 = useVertexColor ? colors[(i * 4) + 3].ToXNA() : currentColor;
                meshVertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[0], pos[1], pos[2]), c0, new Vector2(uv[0], uv[1]));
                meshVertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[3], pos[4], pos[5]), c1, new Vector2(uv[2], uv[3]));
                meshVertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[6], pos[7], pos[8]), c2, new Vector2(uv[4], uv[5]));
                meshVertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(pos[9], pos[10], pos[11]), c3, new Vector2(uv[6], uv[7]));
            }
            int indexCount = quadCount * 6;
            short[] drawIndices = drawer.indices;
            if (indexCount != drawIndices.Length)
            {
                short[] trimmed = new short[indexCount];
                Array.Copy(drawIndices, trimmed, indexCount);
                drawIndices = trimmed;
            }
            Material material = OpenGL.GetMaterialForCurrentState(useTexture: true, useVertexColor: useVertexColor, constantColor: useVertexColor ? null : currentColor);
            MeshDrawCommand command = new(meshVertices, drawIndices, drawer.image.texture.xnaTexture_, material, OpenGL.GetModelViewMatrix(), PrimitiveType.TriangleList, indexCount / 3, meshVertices.Length, indexCount);
            Global.Renderer.DrawMesh(command);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                drawer?.Dispose();
                drawer = null;
                imageGrid = null;
            }
            base.Dispose(disposing);
        }

        public ImageMultiDrawer drawer;

        public Image imageGrid;
    }
}
