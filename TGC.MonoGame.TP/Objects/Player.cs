using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class Player : IDisposable {

  private SpherePrimitive Model;
  private BoundingSphere BoundingSphere;
  private Matrix World;

  public Vector3 Position;
  public Vector3 Velocity = Vector3.Zero;
  private float Acceleration;

  private float JumpBoost = 20f;
  private float Friction = 7f;
  private float MaxSpeed = 50f;
  public float RestitutionCoeficient = 0.8f;

  private float Radius;
  private Color Color;

  public Player(GraphicsDevice graphicsDevice, Vector3 position,
                float acceleration, float radius, Color color) {
    Model = new SpherePrimitive(graphicsDevice);
    BoundingSphere = new BoundingSphere(position, radius);
    Position = position;
    Acceleration = acceleration;
    Radius = radius;
    Color = color;
  }

  public void Update(float dt, KeyboardState keyboardState) {
    float dv = dt * Acceleration;

    if (keyboardState.IsKeyDown(Keys.W)) {
      Velocity.Z += dv;
      if (Velocity.Z > MaxSpeed)
        Velocity.Z = MaxSpeed;
    }

    if (keyboardState.IsKeyDown(Keys.S)) {
      Velocity.Z -= dv;
      if (Velocity.Z < -MaxSpeed)
        Velocity.Z = -MaxSpeed;
    }

    if (keyboardState.IsKeyDown(Keys.A)) {
      Velocity.X += dv;
      if (Velocity.X > MaxSpeed)
        Velocity.X = MaxSpeed;

    } 
    if (keyboardState.IsKeyDown(Keys.D)) {
      Velocity.X -= dv;
      if (Velocity.X < -MaxSpeed)
        Velocity.X = -MaxSpeed;
    }

    if (Velocity.X != 0) {
      if (Velocity.X < 0)
        Velocity.X += dt * Friction;
      if (Velocity.X > 0)
        Velocity.X -= dt * Friction;
    }

    if (Velocity.Z != 0) {
      if (Velocity.Z < 0)
        Velocity.Z += dt * Friction;
      if (Velocity.Z > 0)
        Velocity.Z -= dt * Friction;
    }

    if (keyboardState.IsKeyDown(Keys.Space)) {
      if (Velocity.Y <= 0 && Velocity.Y >= -0.001)
        Velocity.Y = JumpBoost;
    }

    Position += Velocity * dt;
    World = Matrix.CreateTranslation(Position);
    BoundingSphere = new BoundingSphere(Position, Radius);
  }

  public void Draw(Effect Effect) {
    Effect.Parameters["World"].SetValue(World);
    Model.Draw(Effect);
  }

  public bool Intersects(BoundingBox m) { return BoundingSphere.Intersects(m); }

  public void Dispose() { Model.Dispose(); }
}
}
