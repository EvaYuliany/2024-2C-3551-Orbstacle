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
  public override float duration {
    get { return 10; }
    set {}
  }

  public JumpBoostPowerUp(GraphicsDevice graphicsDevice, Vector3 position,
                          float boost)
      : base(graphicsDevice, Color.Green, position) {
    Boost = boost;
  }

  override public void Collided(Player player) {
    player.JumpBoost = player.JumpBoost * Boost;
  }

  override public void Deactivate(Player player) {
    player.JumpBoost = player.JumpBoost / Boost;
  }
}
}
