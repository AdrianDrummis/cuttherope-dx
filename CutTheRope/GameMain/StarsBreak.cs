using CutTheRope.Desktop;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class StarsBreak : RotateableMultiParticles
    {
        public override Particles InitWithTotalParticlesandImageGrid(int p, Image grid)
        {
            if (base.InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }
            duration = 2f;
            gravity.x = 0f;
            gravity.y = 200f;
            angle = -90f;
            angleVar = 50f;
            speed = 150f;
            speedVar = 70f;
            radialAccel = 0f;
            radialAccelVar = 1f;
            tangentialAccel = 0f;
            tangentialAccelVar = 1f;
            x = SCREEN_WIDTH / 2f;
            y = SCREEN_HEIGHT / 2f;
            posVar.x = SCREEN_WIDTH / 2f;
            posVar.y = SCREEN_HEIGHT / 2f;
            life = 4f;
            lifeVar = 0f;
            size = 1f;
            sizeVar = 0f;
            emissionRate = 100f;
            startColor.RedColor = 1f;
            startColor.GreenColor = 1f;
            startColor.BlueColor = 1f;
            startColor.Alpha = 1f;
            startColorVar.RedColor = 0f;
            startColorVar.GreenColor = 0f;
            startColorVar.BlueColor = 0f;
            startColorVar.Alpha = 0f;
            endColor.RedColor = 1f;
            endColor.GreenColor = 1f;
            endColor.BlueColor = 1f;
            endColor.Alpha = 1f;
            endColorVar.RedColor = 0f;
            endColorVar.GreenColor = 0f;
            endColorVar.BlueColor = 0f;
            endColorVar.Alpha = 0f;
            rotateSpeed = 0f;
            rotateSpeedVar = 600f;
            blendAdditive = true;
            return this;
        }

        public override void Draw()
        {
            PreDraw();
            OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(drawer.image.texture.Name());
            OpenGL.GlVertexPointer(3, 5, 0, ToFloatArray(drawer.vertices));
            OpenGL.GlTexCoordPointer(2, 5, 0, ToFloatArray(drawer.texCoordinates));
            OpenGL.GlEnableClientState(13);
            OpenGL.GlBindBuffer(2, colorsID);
            OpenGL.GlColorPointer(4, 5, 0, colors);
            OpenGL.GlDrawElements(7, particleIdx * 6, drawer.indices);
            OpenGL.GlBindBuffer(2, 0U);
            OpenGL.GlDisableClientState(13);
            PostDraw();
        }
    }
}
