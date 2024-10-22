using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class Coin : IDisposable {
  private CylinderPrimitive Model;
  private BoundingCylinder bb;
  private Vector3 _position;
  private float Radius;
  private float Height;
  float Rotation = 0;
  float RotationSpeed = 2f;

  public Vector3 Position {
    get => _position;
    set {
      _position = value;
      bb = new BoundingCylinder(value, Radius, Height / 2);
    }
  }

  public Coin(GraphicsDevice graphicsDevice, Vector3 position) {
    Model = new CylinderPrimitive(graphicsDevice, 0.2f, 1f, 16);
    Height = 0.2f;
    Radius = 1f;
    Position = position;
  }

  public void Update(float dt) { Rotation += RotationSpeed * dt; }

  public void Draw(Effect Effect) {
    var color = Color.Gold;
    Matrix worldMatrix = Matrix.CreateRotationZ(MathF.PI / 2) *
                         Matrix.CreateRotationY(Rotation) *
                         Matrix.CreateTranslation(Position);
    Effect.Parameters["World"].SetValue(worldMatrix);
    Effect.Parameters["DiffuseColor"].SetValue(color.ToVector3());
    Model.Draw(Effect);
  }

  public bool Intersects(BoundingSphere m) { return bb.Intersects(m); }

  public void Dispose() { Model.Dispose(); }
}
}
