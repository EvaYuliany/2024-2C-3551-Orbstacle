#region File Description

//-----------------------------------------------------------------------------
// BridgePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion Using Statements

namespace TGC.MonoGame.TP.Geometries
{
    /// <summary>
    ///     Geometric primitive class for drawing bridges.
    /// </summary>
    public class BridgePrimitive : GeometricPrimitive
    {
        /// <summary>
        ///     Constructs a new bridge primitive, with the specified size and tessellation level.
        ///     This is modified to look more like a bridge.
        /// </summary>
        public BridgePrimitive(GraphicsDevice graphicsDevice, float height = 0.1f, float diameter = 2f, int tessellation = 32)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException(nameof(tessellation));

            height /= 2;

            var radius = diameter / 2;

            // Create a ring of triangles around the outside of the bridge.
            for (var i = 0; i < tessellation; i++)
            {
                var normal = GetCircleVector(i, tessellation);

                AddVertex(normal * radius + Vector3.Up * height, Color.AliceBlue, normal);
                AddVertex(normal * radius + Vector3.Down * height, Color.DarkGray, normal);

                AddIndex(i * 2);
                AddIndex(i * 2 + 1);
                AddIndex((i * 2 + 2) % (tessellation * 2));

                AddIndex(i * 2 + 1);
                AddIndex((i * 2 + 3) % (tessellation * 2));
                AddIndex((i * 2 + 2) % (tessellation * 2));
            }

            // Create flat triangle fan caps to seal the top and bottom.
            CreateCap(tessellation, height, radius, Vector3.Up);
            CreateCap(tessellation, height, radius, Vector3.Down);

            InitializePrimitive(graphicsDevice);
        }

        /// <summary>
        ///     Helper method creates a triangle fan to close the ends of the bridge.
        /// </summary>
        private void CreateCap(int tessellation, float height, float radius, Vector3 normal)
        {
            // Create cap indices.
            for (var i = 0; i < tessellation - 2; i++)
                if (normal.Y > 0)
                {
                    AddIndex(CurrentVertex(false));
                    AddIndex(CurrentVertex(false) + (i + 1) % tessellation);
                    AddIndex(CurrentVertex(false) + (i + 2) % tessellation);
                }
                else
                {
                    AddIndex(CurrentVertex(false));
                    AddIndex(CurrentVertex(false) + (i + 2) % tessellation);
                    AddIndex(CurrentVertex(false) + (i + 1) % tessellation);
                }

            // Create cap vertices.
            for (var i = 0; i < tessellation; i++)
            {
                var position = GetCircleVector(i, tessellation) * radius + normal * height;

                AddVertex(position, Color.Azure, normal);
            }
        }

        /// <summary>
        ///     Helper method computes a point on a circle.
        /// </summary>
        private static Vector3 GetCircleVector(int i, int tessellation)
        {
            var angle = i * MathHelper.TwoPi / tessellation;

            var dx = (float)Math.Cos(angle);
            var dz = (float)Math.Sin(angle);

            return new Vector3(dx, 0, dz);
        }
        ///     Draws the bridge with a transformation to look like a bridge.
        ///     This includes rotation and scaling to simulate the bridge shape.
        /// </summary>
        public void Draw(Effect effect)
        {
            // Apply a rotation to make the bridge horizontal and scale to extend the length (like a bridge).
            Matrix bridgeTransformation = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateScale(5f, 1f, 1f);

            // Combine the transformation with the world matrix.
            effect.Parameters["World"]
             .SetValue(bridgeTransformation * effect.Parameters["World"].GetValueMatrix());

            // Use the combined matrix for rendering.
            base.Draw(effect);
        }
    }
}
