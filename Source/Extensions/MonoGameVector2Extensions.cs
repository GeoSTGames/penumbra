using System;
using Microsoft.Xna.Framework;

namespace DeeZEngine.Engine.Extensions
{
    public static class MonoGameVector2Extensions
    {

        public static float ToRotation(this Vector2 vector)
        {
            float angle = (float)Math.Atan2(vector.X, -vector.Y);
            return angle;
        }

        public static System.Numerics.Vector2 ToNumericsVector2(this Vector2 vector)
        {
            return new System.Numerics.Vector2(vector.X, vector.Y);
        }

    }

}