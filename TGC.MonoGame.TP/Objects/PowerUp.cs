using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class PowerUp : IDisposable {
  private CubePrimitive Model;
  public BoundingBox bb;

  public Vector3 Position;
  private Color Color;

  public PowerUp(GraphicsDevice graphicsDevice, Vector3 position) {
    Model = new CubePrimitive(graphicsDevice);
    Vector3 bb_corner = Vector3.One * 0.5f;
    bb = new BoundingBox(position - bb_corner, position + bb_corner);
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
