using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace TidesOfTime.Common.Rendering.ProceduralGore
{
    /// <summary>
    /// Instance data for a procedural gore particle. Contains a world transformation matrix and the subdivision data used in the particle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Instance
    {
        public Matrix World;

        public Vector4 InstanceUV;

        public Instance(Matrix world, Vector4 instanceUV)
        {
            World = world;
            InstanceUV = instanceUV;
        }
    }
}
