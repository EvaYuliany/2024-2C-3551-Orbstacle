using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

abstract public class PowerUp : IDisposable {
  private CubePrimitive Model;
  private bool active = false;
  private Vector3 _position;
  private Color Color;
  private BoundingBox bb;
  private float lastCollision = 0;

  abstract public float duration { get; set; }

  public Vector3 Position {
    get => _position;
    set {
      _position = value;
      Vector3 bb_corner = Vector3.One * 0.5f;
      bb = new BoundingBox(value - bb_corner, value + bb_corner);
    }
  }

  public PowerUp(GraphicsDevice graphicsDevice, Color color, Vector3 position) {
    Model = new CubePrimitive(graphicsDevice);
    Color = color;
    Position = position;
  }

  public void Draw(Effect Effect) {
    Matrix worldMatrix = Matrix.CreateTranslation(Position);
    Effect.Parameters["World"].SetValue(worldMatrix);
    Effect.Parameters["DiffuseColor"].SetValue(Color.ToVector3());
    Model.Draw(Effect);
  }

  abstract public void Collided(Player player);
  abstract public void Deactivate(Player player);

  public void CheckCollision(Player player, GameTime gameTime) {
    if (bb.Intersects(player.BoundingSphere)) {
      if (!active) {
        active = true;
        Collided(player);
      }

      lastCollision = gameTime.TotalGameTime.Seconds;
    } else {
      if (gameTime.TotalGameTime.Seconds - lastCollision > duration) {
        active = false;
        Deactivate(player);
      }
    }
  }

  public void Dispose() { Model.Dispose(); }
}
}
