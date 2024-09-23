using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects
{

    public class Player : IDisposable
    {

        private SpherePrimitive Model;
        private BoundingSphere BoundingSphere;
        private Matrix World;

        public Vector3 Position;
        private float Speed;
        private float Radius;

        private Color Color;

        public Player(GraphicsDevice graphicsDevice, Vector3 position, float speed,
                      float radius, Color color)
        {
            Model = new SpherePrimitive(graphicsDevice);
            BoundingSphere = new BoundingSphere(position, radius);
            Position = position;
            Speed = speed;
            Radius = radius;
            Color = color;
        }

        public void Update(float dt, KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.W))
                Position.Z += Speed * dt;

            if (keyboardState.IsKeyDown(Keys.S))
                Position.Z -= Speed * dt;

            if (keyboardState.IsKeyDown(Keys.D))
                Position.X -= Speed * dt;

            if (keyboardState.IsKeyDown(Keys.A))
                Position.X += Speed * dt;

            if (keyboardState.IsKeyDown(Keys.Space))
                Position.Y += Speed * dt;

            if (keyboardState.IsKeyDown(Keys.LeftShift))
                Position.Y -= Speed * dt;

            World = Matrix.CreateTranslation(Position);
            BoundingSphere = new BoundingSphere(Position, Radius);
        }

        public void Draw(Effect Effect)
        {
            Effect.Parameters["World"].SetValue(World);
            Model.Draw(Effect);
        }

        public bool Intersects(BoundingBox m) { return BoundingSphere.Intersects(m); }

        public void Dispose() { Model.Dispose(); }
    }
}
