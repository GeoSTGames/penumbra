using System;
using Microsoft.Xna.Framework;
using Vector4 = System.Numerics.Vector4;

namespace DeeZEngine.Engine.Extensions
{
    public static class NumericsVector4Extensions
    {

        public static Microsoft.Xna.Framework.Vector4 ToNumericsVector2(this Vector4 vector)
        {
            return new Microsoft.Xna.Framework.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

    }

}
