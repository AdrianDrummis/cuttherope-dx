namespace CutTheRope.Framework.Core
{
    public class VectorClass
    {
        public VectorClass()
        {
        }

        public VectorClass(Vector Value)
        {
            v = Value;
        }

#pragma warning disable CA1051
        public Vector v;
#pragma warning restore CA1051
    }
}
