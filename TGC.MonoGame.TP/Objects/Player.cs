using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public enum Material { Metal, Plastic, Rubber }

struct MaterialProperties {
  public float jump_boost;
  public float acceleration;
  public float friction;
  public float max_speed;
  public float restitution_coeficient;
  public Color color;
}
public class Player : IDisposable {

MaterialProperties PlasticProperties =
    new MaterialProperties { jump_boost = 20f,
                             acceleration = 15f,
                             friction = 7f,
                             max_speed = 50f,
                             restitution_coeficient = 0.8f,
                             color = Color.Red };

MaterialProperties MetalProperties =
    new MaterialProperties { jump_boost = 50f,
                             acceleration = 15f,
                             friction = 7f,
                             max_speed = 50f,
                             restitution_coeficient = 0.8f,
                             color = Color.Blue };

MaterialProperties RubberProperties =
    new MaterialProperties { jump_boost = 50f,
                             acceleration = 15f,
                             friction = 7f,
                             max_speed = 50f,
                             restitution_coeficient = 0.8f,
                             color = Color.Green };


  private SpherePrimitive Model;
  public BoundingSphere BoundingSphere;
  private Matrix World;

  public Vector3 Position;
  public Vector3 Velocity = Vector3.Zero;

  public Material Material;

  private float Radius;

  private Color Color;
  public float JumpBoost;
  public float Friction;
  public float MaxSpeed;
  public float Acceleration;
  public float RestitutionCoeficient;

  public Player(GraphicsDevice graphicsDevice, Vector3 position,
                Material material, float radius) {
    Model = new SpherePrimitive(graphicsDevice);
    BoundingSphere = new BoundingSphere(position, radius);
    Position = position;
    Radius = radius;
    SetMaterial(material);
  }

  private void SetMaterialProps(MaterialProperties p) {
    Color = p.color;
    JumpBoost = p.jump_boost;
    Friction = p.friction;
    MaxSpeed = p.max_speed;
    Acceleration = p.acceleration;
    RestitutionCoeficient = p.restitution_coeficient;
  }

  public void SetMaterial(Material material) {
    Material = material;
    switch (material) {
    case Material.Metal:
      SetMaterialProps(MetalProperties);
      break;
    case Material.Plastic:
      SetMaterialProps(PlasticProperties);
      break;
    case Material.Rubber:
      SetMaterialProps(RubberProperties);
      break;
    }
  }

  public void Update(float dt, KeyboardState keyboardState) {

    if (keyboardState.IsKeyDown(Keys.W))
      Velocity.X += dt * Acceleration;

    if (keyboardState.IsKeyDown(Keys.S))
      Velocity.X -= dt * Acceleration;

    if (keyboardState.IsKeyDown(Keys.A))
      Velocity.Z -= dt * Acceleration;

    if (keyboardState.IsKeyDown(Keys.D))
      Velocity.Z += dt * Acceleration;

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

    Position +=
        new Vector3(Velocity.X > 0 ? MathF.Min(Velocity.X, MaxSpeed)
                                   : MathF.Max(Velocity.X, -MaxSpeed),
                    Velocity.Y,
                    Velocity.Z > 0 ? MathF.Min(Velocity.Z, MaxSpeed)
                                   : MathF.Max(Velocity.Z, -MaxSpeed)) *
        dt;

    World = Matrix.CreateRotationX(Position.Z) *
            Matrix.CreateRotationZ(-Position.X) *
            Matrix.CreateTranslation(Position);
    BoundingSphere = new BoundingSphere(Position, Radius);
  }

  public void Jump() { Velocity.Y = JumpBoost; }

  public void Draw(Effect Effect) {
    Effect.Parameters["World"].SetValue(World);
    Effect.Parameters["DiffuseColor"].SetValue(Color.ToVector3());
    Model.Draw(Effect);
  }

  public bool Intersects(BoundingBox m) { return BoundingSphere.Intersects(m); }

  public void Dispose() { Model.Dispose(); }
}
}
