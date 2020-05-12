using System;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine.Engine.Extensions
{

    public static class FloatExtensions
    {

        public static Vector2 ToDirectionVector(this float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }

        public static void ToDirectionVector(this float angle, out Vector2 vector)
        {
            vector = new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }

    }

}
