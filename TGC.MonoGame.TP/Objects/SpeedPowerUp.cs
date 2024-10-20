using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class SpeedPowerUp : PowerUp {
  float Speed;

  public SpeedPowerUp(GraphicsDevice graphicsDevice, Vector3 position,
                      float speed)
      : base(graphicsDevice, Color.Blue, position) {
    Speed = speed;
  }

  override public void Collided(Player player) {
    player.Acceleration = player.Acceleration * Speed;
    player.MaxSpeed = player.MaxSpeed * Speed * 0.2f;
  }
}
}
