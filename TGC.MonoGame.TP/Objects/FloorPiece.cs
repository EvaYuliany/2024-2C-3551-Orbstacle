using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class Floor : IDisposable {

  public OrientedBoundingBox BoundingBox;

  public Vector3 Translation;
  public Vector3 Scale;
  public Vector3 Rotation;
  public Vector3 Color;

  private CubePrimitive Cube;

  public Floor(CubePrimitive cube, Vector3 translation, Vector3 scale,
               Vector3 rotation, Vector3 color) {
    Translation = translation;
    Scale = scale;
    Rotation = rotation;
    Color = color;
    Cube = cube;

    Vector3 bb = scale * 0.5f;
    BoundingBox = OrientedBoundingBox.FromAABB(
        new BoundingBox(Translation - bb, Translation + bb));
  }

  public void Draw(Effect Effect) {
    Effect.Parameters["DiffuseColor"].SetValue(Color);
    Effect.Parameters["World"].SetValue(Matrix.CreateScale(Scale) *
                                        Matrix.CreateRotationX(Rotation.X) *
                                        Matrix.CreateRotationY(Rotation.Y) *
                                        Matrix.CreateRotationZ(Rotation.Z) *
                                        Matrix.CreateTranslation(Translation));
    Cube.Draw(Effect);
  }

  public bool Intersects(BoundingSphere m) { return BoundingBox.Intersects(m); }

  public void Dispose() { Cube.Dispose(); }
}
}
