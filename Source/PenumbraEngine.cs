﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics;
using Penumbra.Graphics.Helpers;
using Penumbra.Graphics.Providers;
using Penumbra.Utilities;

namespace Penumbra
{
    internal class PenumbraEngine
    {
        private BufferedShadowRenderHelper _bufferedShadowRenderHelper;

        private readonly LightmapTextureBuffer _textureBuffer = new LightmapTextureBuffer();
        private RenderProcessProvider _renderProcessProvider;
        private PrimitiveRenderHelper _primitiveRenderHelper;

        private Color _ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public PenumbraEngine(Projections projections)
        {
            Camera = new Camera(projections);
            Camera.YInverted += (s, e) => { Hulls.ForEach(h => h.Load(Camera.InvertedY)); };
            Hulls.CollectionChanged += (s, e) =>
            {
                e.NewItems?.ForEach<Hull>(i =>
                {
                    Check.ArgumentNotNull(i, "hull");
                    i.Load(Camera.InvertedY);
                });
            };
        }

        public bool DebugDraw { get; set; } = true;

        public Color AmbientColor
        {
            get { return new Color(_ambientColor.R, _ambientColor.G, _ambientColor.B); }
            set { _ambientColor = new Color(value, 1f); }
        }

        public Matrix ViewProjection
        {
            get { return Camera.CustomWorld; }
            set { Camera.CustomWorld = value; }
        }

        internal ShaderParameterCollection ShaderParameters { get; } = new ShaderParameterCollection();
        internal ObservableCollection<Light> Lights { get; } = new ObservableCollection<Light>();
        internal ObservableCollection<Hull> Hulls { get; } = new ObservableCollection<Hull>();
        internal Camera Camera { get; }
        internal GraphicsDevice GraphicsDevice { get; private set; }

        public void Load(GraphicsDevice device, GraphicsDeviceManager deviceManager, ContentManager content)
        {
            GraphicsDevice = device;
            
            Camera.Load(GraphicsDevice, deviceManager);
            _textureBuffer.Load(GraphicsDevice, deviceManager);
            _renderProcessProvider = new RenderProcessProvider(GraphicsDevice, content, Camera);
            _primitiveRenderHelper = new PrimitiveRenderHelper(GraphicsDevice, this);
            _bufferedShadowRenderHelper = new BufferedShadowRenderHelper(GraphicsDevice, this);

            // Setup logging for debug purposes.
            Logger.Add(new DelegateLogger(x => Debug.WriteLine(x)));
        }

        public void PreRender()
        {
            GraphicsDevice.SetRenderTarget(_textureBuffer.Scene);
        }

        public void Render()
        {
            // Switch render target to lightmap.
            GraphicsDevice.SetRenderTarget(_textureBuffer.LightMap);
            GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target, AmbientColor, 1f, 0);

            ShaderParameters.SetMatrix(ShaderParameter.ProjectionTransform, ref Camera.WorldViewProjection);

            // Generate lightmap.
            for (int i = 0; i < Lights.Count; i++)
            {
                Light light = Lights[i];
                if (!light.Enabled) continue;
                             
                bool skip = false;
                for (int j = 0; j < Hulls.Count; j++)
                {
                    if (light.IsInside(Hulls[j]))
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip) continue;

                // TODO: Cache and/or spatial tree?

                // Clear stencil.
                // TODO: use incremental stencil values to avoid clearing every light?
                if (light.ShadowType == ShadowType.Occluded)
                    GraphicsDevice.Clear(ClearOptions.Stencil, AmbientColor, 1f, 0);

                // Set scissor rectangle.
                // DO NOT USE params overload. Causes unnecessary garbage.                
                GraphicsDevice.ScissorRectangle = Camera.GetScissorRectangle(light);

                // Draw shadows for light.
                if (light.CastsShadows)
                {
                    _bufferedShadowRenderHelper.DrawShadows(
                        light,
                        _renderProcessProvider.Umbra(light.ShadowType),
                        _renderProcessProvider.Penumbra(light.ShadowType),
                        _renderProcessProvider.Antumbra(light.ShadowType),
                        _renderProcessProvider.Solid(light.ShadowType));
                }

                // Draw light.                
                ShaderParameters.SetVector3(ShaderParameter.LightColor, light.Color.ToVector3());
                ShaderParameters.SetSingle(ShaderParameter.LightIntensity, light.IntensityFactor);
                _primitiveRenderHelper.DrawQuad(_renderProcessProvider.Light, light.Position, light.Range * 2);

                // Draw light source (for debugging purposes only).
                _primitiveRenderHelper.DrawCircle(_renderProcessProvider.LightSource, light.Position, light.Radius);

                // Clear alpha.                
                _primitiveRenderHelper.DrawFullscreenQuad(_renderProcessProvider.ClearAlpha);

                // Clear light's dirty flags.
                light.DirtyFlags &= 0;
            }

            // Switch render target back to default.
            GraphicsDevice.SetRenderTarget(null);

            // Present lightmap.            
            _primitiveRenderHelper.DrawFullscreenQuad(_renderProcessProvider.Present, _textureBuffer.Scene);
            _primitiveRenderHelper.DrawFullscreenQuad(_renderProcessProvider.PresentLightmap, _textureBuffer.LightMap);

            // Clear hulls dirty flags.
            for (int j = 0; j < Hulls.Count; j++)
            {
                Hull hull = Hulls[j];
                hull.DirtyFlags &= 0;
            }
        }
    }
}