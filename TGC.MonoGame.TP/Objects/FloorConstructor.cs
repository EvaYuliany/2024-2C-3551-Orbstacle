using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class FloorConstructor : IDisposable {

  private List<OrientedBoundingBox> BoundingBoxes =
      new List<OrientedBoundingBox>();
  private List<Vector3> Translations = new List<Vector3>();
  private List<Vector3> Scales = new List<Vector3>();
  private List<Vector3> Rotations = new List<Vector3>();
  private List<Vector3> Colors = new List<Vector3>();

  private float FloorUnit = 30;
  private float FloorThickness = 0.2f;

  private static float FloorInitialHeight = -15;
  private Vector3 FloorInitialPos = Vector3.Zero;

  private CubePrimitive Cube;

  private float CurrentHeight = FloorInitialHeight;
  Random random = new Random(0);

  public FloorConstructor(GraphicsDevice graphicsDevice) {
    Cube = new CubePrimitive(graphicsDevice);
  }

  public Vector3 AddBase(Vector2 offset) {
    Vector2 extra = FloorUnit * (offset.Equals(Vector2.UnitX) ||
                                         offset.Equals(Vector2.UnitY)
                                     ? offset * 0.01f
                                     : offset * -0.01f);
    Vector3 scale =
        new Vector3(FloorUnit + extra.X, FloorThickness * FloorUnit * 0.8f,
                    FloorUnit + extra.Y);
    Vector3 rotation = Vector3.Zero;

    int previousIndex = Translations.Count - 1;
    Vector3 previousTranslation =
        previousIndex == -1 ? Vector3.Zero : Translations[previousIndex];

    Vector3 translation =
        FloorInitialPos +
        new Vector3(FloorUnit * offset.X + previousTranslation.X, CurrentHeight,
                    FloorUnit * offset.Y + previousTranslation.Z);

    Translations.Add(translation);
    Rotations.Add(rotation);
    Scales.Add(scale);
    Vector3 color = new Vector3((float)random.NextDouble(), // R
                                (float)random.NextDouble(), // G
                                (float)random.NextDouble()  // B
    );
    Colors.Add(color);

    OrientedBoundingBox BB = OrientedBoundingBox.FromAABB(new BoundingBox(
        translation +
            new Vector3(-FloorUnit / 2, FloorThickness, -FloorUnit / 2),
        translation +
            new Vector3(FloorUnit / 2, FloorThickness, FloorUnit / 2)));

    BoundingBoxes.Add(BB);
    return translation;
  }

  public Vector3 AddSlope(Vector2 offset, bool up) {
    Vector2 extra = FloorUnit * (offset.Equals(Vector2.UnitX) ||
                                         offset.Equals(Vector2.UnitY)
                                     ? offset * 0.43f
                                     : offset * -0.43f);
    Vector3 scale = new Vector3(FloorUnit + extra.X, FloorThickness * FloorUnit,
                                FloorUnit + extra.Y);

    float rotation_angle = up ? -MathF.PI / 4 : MathF.PI / 4;
    Vector2 rotation_direction =
        Vector2.Transform(offset, Matrix.CreateRotationZ(-MathF.PI / 2));

    Vector3 rotation =
        new Vector3(rotation_direction.X, 0, rotation_direction.Y) *
        rotation_angle;

    int previousIndex = Translations.Count - 1;
    Vector3 previousTranslation =
        previousIndex == -1 ? Vector3.Zero : Translations[previousIndex];

    Vector3 translation =
        FloorInitialPos +
        new Vector3(FloorUnit * offset.X + previousTranslation.X,
                    CurrentHeight + (up ? FloorUnit / 2 : -FloorUnit / 2),
                    FloorUnit * offset.Y + previousTranslation.Z);

    Translations.Add(translation);
    Rotations.Add(rotation);
    Scales.Add(scale);

    Vector3 color = new Vector3((float)random.NextDouble(), // R
                                (float)random.NextDouble(), // G
                                (float)random.NextDouble()  // B
    );
    Colors.Add(color);
    OrientedBoundingBox BB =
        up ? OrientedBoundingBox.FromAABB(new BoundingBox(
                 translation + new Vector3(-FloorUnit / 2, -FloorThickness,
                                           -FloorUnit / 2),
                 translation +
                     new Vector3(FloorUnit / 2, FloorThickness, FloorUnit / 2)))
           : OrientedBoundingBox.FromAABB(new BoundingBox(
                 translation + new Vector3(-FloorUnit / 2, -FloorThickness,
                                           -FloorUnit / 2),
                 translation + new Vector3(FloorUnit / 2, FloorThickness,
                                           FloorUnit / 2)));

    BB.Rotate(Matrix.CreateRotationX(rotation.X));
    BB.Rotate(Matrix.CreateRotationY(rotation.Y));
    BB.Rotate(Matrix.CreateRotationZ(rotation.Z));

    BoundingBoxes.Add(BB);

    if (up)
      CurrentHeight += FloorUnit;
    else
      CurrentHeight -= FloorUnit;
    return translation;
  }

  public void Draw(Effect Effect) {
    for (int i = 0; i < Translations.Count; i++) {
      Effect.Parameters["DiffuseColor"].SetValue(Colors[i]);
      Effect.Parameters["World"].SetValue(
          Matrix.CreateScale(Scales[i]) *
          Matrix.CreateRotationX(Rotations[i].X) *
          Matrix.CreateRotationY(Rotations[i].Y) *
          Matrix.CreateRotationZ(Rotations[i].Z) *
          Matrix.CreateTranslation(Translations[i]));
      Cube.Draw(Effect);
    }
  }

  public bool Intersects(BoundingSphere m) {
    return BoundingBoxes.Exists((b) => b.Intersects(m));
  }

  public void Dispose() { Cube.Dispose(); }
}
}
