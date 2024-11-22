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
  public Vector3 Normal;

  private CubePrimitive Cube;

  public Floor(CubePrimitive cube, Vector3 translation, Vector3 scale,
               Vector3 rotation) {
    Translation = translation;
    Scale = scale;
    Rotation = rotation;
    Cube = cube;

    Matrix rotation_matrix = Matrix.CreateRotationX(Rotation.X) *
                             Matrix.CreateRotationY(Rotation.Y) *
                             Matrix.CreateRotationZ(Rotation.Z);

    Normal =
        Vector3.Normalize(Vector3.Transform(Vector3.UnitY, rotation_matrix) +
                          Vector3.UnitY * scale.Y * 0.5f);

    Vector3 bb = scale * 0.5f;
    BoundingBox = OrientedBoundingBox.FromAABB(
        new BoundingBox(Translation - bb, Translation + bb));
  }

  public void Draw(Effect Effect, Matrix View, Matrix Projection) {
    Matrix worldMatrix = Matrix.CreateScale(Scale) *
                         Matrix.CreateRotationX(Rotation.X) *
                         Matrix.CreateRotationY(Rotation.Y) *
                         Matrix.CreateRotationZ(Rotation.Z) *
                         Matrix.CreateTranslation(Translation);
    Effect.Parameters["InverseTransposeWorld"].SetValue(
        Matrix.Transpose(Matrix.Invert(worldMatrix)));
    Effect.Parameters["WorldViewProjection"].SetValue(worldMatrix * View *
                                                      Projection);
    Effect.Parameters["World"].SetValue(worldMatrix);
    Cube.Draw(Effect);
  }

  public bool Intersects(BoundingSphere m) { return BoundingBox.Intersects(m); }

  public void Dispose() { Cube.Dispose(); }
}
}
