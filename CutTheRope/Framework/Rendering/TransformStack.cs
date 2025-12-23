using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Rendering
{
    /// <summary>
    /// Manages a matrix transformation stack, replacing the old OpenGL push/pop/translate/rotate/scale pattern.
    /// Game code uses this to build up transformations, then passes the final matrix to the renderer.
    /// </summary>
    internal sealed class TransformStack
    {
        private readonly Stack<Matrix> _stack = new();
        private Matrix _current = Matrix.Identity;

        /// <summary>
        /// Gets the current transformation matrix (top of stack).
        /// </summary>
        public Matrix Current => _current;

        /// <summary>
        /// Pushes the current transformation matrix onto the stack.
        /// </summary>
        public void Push()
        {
            _stack.Push(_current);
        }

        /// <summary>
        /// Pops the top transformation matrix from the stack and makes it current.
        /// </summary>
        public void Pop()
        {
            if (_stack.Count > 0)
            {
                _current = _stack.Pop();
            }
        }

        /// <summary>
        /// Resets the current transformation to identity.
        /// </summary>
        public void LoadIdentity()
        {
            _current = Matrix.Identity;
        }

        /// <summary>
        /// Applies a translation to the current transformation.
        /// </summary>
        public void Translate(float x, float y, float z = 0f)
        {
            _current = Matrix.CreateTranslation(x, y, z) * _current;
        }

        /// <summary>
        /// Applies a rotation (in degrees) around the Z axis to the current transformation.
        /// </summary>
        public void Rotate(float angleInDegrees, float x = 0f, float y = 0f, float z = 1f)
        {
            _current = Matrix.CreateRotationZ(MathHelper.ToRadians(angleInDegrees)) * _current;
        }

        /// <summary>
        /// Applies a scale to the current transformation.
        /// </summary>
        public void Scale(float x, float y, float z = 1f)
        {
            _current = Matrix.CreateScale(x, y, z) * _current;
        }

        /// <summary>
        /// Clears the entire stack and resets to identity.
        /// </summary>
        public void Clear()
        {
            _stack.Clear();
            _current = Matrix.Identity;
        }

        /// <summary>
        /// Gets the current stack depth (number of pushed matrices).
        /// </summary>
        public int Depth => _stack.Count;
    }
}
