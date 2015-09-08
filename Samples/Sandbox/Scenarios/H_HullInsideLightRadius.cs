﻿using System;
using Microsoft.Xna.Framework;
using Penumbra;
using Penumbra.Core;

namespace Sandbox.Scenarios
{
    class H_HullInsideLightRadius : Scenario
    {
        private const float MinLightRadius = 50;
        private const float MaxLightRadius = 600;
        private const float RadiusSpeed = 2f;

        private Light _light;
        private bool _isRadiusIncreasing;
        private float _progress;

        public override void Activate(PenumbraComponent penumbra)
        {
            _light = new Light
            {
                Position = new Vector2(-100, 0),
                Color = Color.White,
                Range = MaxLightRadius,
                Radius = MinLightRadius
            };
            penumbra.Lights.Add(_light);
            penumbra.Hulls.Add(new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(50f)
            });
        }

        public override void Update(float deltaSeconds)
        {
            _progress = Math.Min(_progress + deltaSeconds / RadiusSpeed, 1f);

            _light.Radius = _isRadiusIncreasing
                ? MathHelper.Lerp(MinLightRadius, MaxLightRadius, _progress)
                : MathHelper.Lerp(MaxLightRadius, MinLightRadius, _progress);

            if (_progress >= 1f)
            {
                _progress = 0;
                _isRadiusIncreasing = !_isRadiusIncreasing;
            }
        }
    }
}