using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class FloorConstructor : IDisposable {

  private List<Floor> Floors = new List<Floor>();
  private List<bool> Slopes = new List<bool>();

  private float FloorUnit = 30;
  private float FloorThickness = 0.2f;

  private static float FloorInitialHeight = -10;
  private Vector3 FloorInitialPos = Vector3.Zero;

  private CubePrimitive Cube;

  private float CurrentHeight = FloorInitialHeight;

  public FloorConstructor(GraphicsDevice graphicsDevice) {
    Cube = new CubePrimitive(graphicsDevice);
  }

  public Vector3 AddBase(Vector2 offset) {
    Vector2 extra = FloorUnit * (offset.Equals(Vector2.UnitX) ||
                                         offset.Equals(Vector2.UnitY)
                                     ? offset * 0.01f
                                     : offset * -0.01f);
    Vector3 scale = new Vector3(FloorUnit + extra.X, FloorThickness * FloorUnit,
                                FloorUnit + extra.Y);
    Vector3 rotation = Vector3.Zero;

    int previousIndex = Floors.Count - 1;
    Vector3 previousTranslation =
        previousIndex == -1 ? Vector3.Zero : Floors[previousIndex].Translation;

    Vector3 translation =
        FloorInitialPos +
        new Vector3(FloorUnit * offset.X + previousTranslation.X, CurrentHeight,
                    FloorUnit * offset.Y + previousTranslation.Z);

    Vector3 color = new Vector3(0.5f, 0.5f, 0.5f);

    Floor floor = new Floor(Cube, translation, scale, rotation, color);
    Floors.Add(floor);
    Slopes.Add(false);

    return translation + Vector3.UnitY * FloorThickness * FloorUnit / 2;
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

    int previousIndex = Floors.Count - 1;
    Vector3 previousTranslation =
        previousIndex == -1 ? Vector3.Zero : Floors[previousIndex].Translation;

    Vector3 translation =
        FloorInitialPos +
        new Vector3(FloorUnit * offset.X + previousTranslation.X,
                    CurrentHeight + (up ? FloorUnit / 2 : -FloorUnit / 2),
                    FloorUnit * offset.Y + previousTranslation.Z);

    Vector3 color = new Vector3(0.5f, 0.5f, 0.5f);

    Floor floor = new Floor(Cube, translation, scale, rotation, color);
    Floors.Add(floor);
    Slopes.Add(true);
    Matrix rotation_matrix = Matrix.CreateRotationX(-rotation.X) *
                             Matrix.CreateRotationY(rotation.Y) *
                             Matrix.CreateRotationZ(rotation.Z);

    floor.BoundingBox.Rotate(rotation_matrix);

    if (up)
      CurrentHeight += FloorUnit;
    else
      CurrentHeight -= FloorUnit;

    return translation + Vector3.UnitY * FloorThickness * FloorUnit / 2;
  }

  public void Draw(Effect Effect, Matrix View, Matrix Projection) {
    for (int i = 0; i < Floors.Count; i++) {
      Floors[i].Draw(Effect, View, Projection);
    }
  }

public (bool, Floor) Intersects(BoundingSphere m) {
    int intersecting_floor = Floors.FindIndex((b) => b.Intersects(m));
    if (intersecting_floor == -1)
        return (false, null);

    Floor intersectingFloor = Floors[intersecting_floor];
    
    if (Slopes[intersecting_floor]) {
        Vector3 normal = Vector3.Cross(intersectingFloor.Scale.X * Vector3.UnitX, intersectingFloor.Scale.Z * Vector3.UnitZ);
        normal.Normalize(); 
        intersectingFloor.Normal = normal; 
    }

    return (true, intersectingFloor);
}


  public void Dispose() { Cube.Dispose(); }
}
}
