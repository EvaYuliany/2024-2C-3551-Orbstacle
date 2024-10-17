using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Geometries;

namespace TGC.MonoGame.TP.Obstacles {

public class Pendulum : IDisposable {

  public BoundingSphere BoundingSphere;
  private CylinderPrimitive RodModel;
  private SpherePrimitive BallModel;
  private Color RodColor;
  private Color BallColor;

  private Matrix RodWorld;
  private Matrix BallWorld;
  private float RodLength;
  private float BallRadius;

  public Vector3 Position;
  public float RotationAngle;
  private float AngleOffset;

  private float MaxAngle;
  private float MinAngle;
  public float Speed;
  private Vector3 SphereCenter;

  public Pendulum(GraphicsDevice graphicsDevice, Vector3 vertex_position,
                  float rotation_angle, float starting_offset, float max_angle,
                  float min_angle, float r_length, float b_radius,
                  Color r_color, Color b_color, float speed) {
    RodModel = new CylinderPrimitive(graphicsDevice, 1, b_radius / 5);
    BallModel = new SpherePrimitive(graphicsDevice);
    RodColor = r_color;
    BallColor = b_color;
    RodLength = r_length;
    BallRadius = b_radius;
    Position = vertex_position;
    AngleOffset = starting_offset;
    RotationAngle = rotation_angle;
    MaxAngle = max_angle;
    MinAngle = min_angle;
    Speed = speed;

    Matrix Translation =
        Matrix.CreateTranslation(-Vector3.UnitY * RodLength / 2) *
        Matrix.CreateRotationZ(AngleOffset) *
        Matrix.CreateRotationY(RotationAngle) *
        Matrix.CreateTranslation(Position);

    BallWorld = Matrix.CreateScale(BallRadius) *
                Matrix.CreateTranslation(-Vector3.UnitY * RodLength / 2) *
                Translation;
    RodWorld = Matrix.CreateScale(new Vector3(1, RodLength, 1)) * Translation;

    SphereCenter = Vector3.Transform(Vector3.Zero, BallWorld);
    BoundingSphere = new BoundingSphere(SphereCenter, BallRadius/2);
  }

  public void Update(float dt) {
    if (Speed > 0 && AngleOffset >= MaxAngle ||
        Speed < 0 && AngleOffset <= MinAngle)
      Speed *= -1;

    AngleOffset += Speed * dt;

    Matrix Translation =
        Matrix.CreateTranslation(-Vector3.UnitY * RodLength / 2) *
        Matrix.CreateRotationZ(AngleOffset) *
        Matrix.CreateRotationY(RotationAngle) *
        Matrix.CreateTranslation(Position);

    BallWorld = Matrix.CreateScale(BallRadius) *
                Matrix.CreateTranslation(-Vector3.UnitY * RodLength / 2) *
                Translation;
    RodWorld = Matrix.CreateScale(new Vector3(1, RodLength, 1)) * Translation;

    SphereCenter = Vector3.Transform(Vector3.Zero, BallWorld);
    BoundingSphere = new BoundingSphere(SphereCenter, BallRadius/2);
  }

  public bool Intersects(BoundingSphere m) {
    return BoundingSphere.Intersects(m);
  }

  public void Draw(Effect Effect) {
    Effect.Parameters["DiffuseColor"].SetValue(RodColor.ToVector3());
    Effect.Parameters["World"].SetValue(RodWorld);
    RodModel.Draw(Effect);

    Effect.Parameters["DiffuseColor"].SetValue(BallColor.ToVector3());
    Effect.Parameters["World"].SetValue(BallWorld);
    BallModel.Draw(Effect);

  }

  public void Dispose() {
    BallModel.Dispose();
    RodModel.Dispose();
  }
}
}
