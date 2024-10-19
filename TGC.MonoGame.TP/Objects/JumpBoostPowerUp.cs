using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects {

public class JumpBoostPowerUp : PowerUp {
  float Boost;

  public JumpBoostPowerUp(GraphicsDevice graphicsDevice, Vector3 position,
                          float boost)
      : base(graphicsDevice, Color.Green, position) {
    Boost = boost;
  }

  override public void Collided(Player player) {
    player.JumpBoost = player.JumpBoost * Boost;
  }
}
}
