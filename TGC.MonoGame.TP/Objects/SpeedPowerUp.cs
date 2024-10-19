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
    player.Velocity = new Vector3(player.Velocity.X * Speed, player.Velocity.Y,
                                  player.Velocity.Z * Speed);
  }
}
}
