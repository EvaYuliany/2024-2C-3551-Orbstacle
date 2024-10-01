using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class Checkpoint : IDisposable {
  private CylinderPrimitive Model;
  private BoundingCylinder bb;

  public Vector3 Position;

  private float Radius;
  private float Height;
  private Color Color;

  public Checkpoint(GraphicsDevice graphicsDevice, Vector3 position,
                    float radius, float height) {
    Model = new CylinderPrimitive(graphicsDevice, height, radius * 2, 16);
    Height = height;
    Radius = radius;
    bb = new BoundingCylinder(position, radius, height / 2);
    Position = position;
  }

  public void Draw(Effect Effect) {
    var color = Color.Blue;
    Matrix worldMatrix = Matrix.CreateTranslation(Position);
    Effect.Parameters["World"].SetValue(worldMatrix);
    Effect.Parameters["DiffuseColor"].SetValue(color.ToVector3());
    Model.Draw(Effect);
  }

  public bool Intersects(BoundingSphere m) { return bb.Intersects(m); }

  public void Dispose() { Model.Dispose(); }
}
}