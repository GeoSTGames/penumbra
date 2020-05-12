using System;
using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace DeeZEngine.Engine.Extensions
{
    public static class Vector2Extensions
    {

        public static float ToRotation(this Vector2 vector)
        {
            float angle = (float) Math.Atan2(vector.X, -vector.Y);
            return angle;
        }

        public static Microsoft.Xna.Framework.Vector2 ToMonoGameVector2(this Vector2 vector)
        {
            return new Microsoft.Xna.Framework.Vector2(vector.X, vector.Y);
        }

    }

}
