using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Objects
{

  public enum Material {
    Metal,
    Plastic,
    Rubber
  }

  struct MaterialProperties { 
    public float jump_boost;
    public float acceleration;
    public float friction;
    public float max_speed;
    public float restitution_coeficient;
    public Color color;
  }

    public class Player : IDisposable
    {


        private SpherePrimitive Model;
        public BoundingSphere BoundingSphere;
        private Matrix World;

        public Vector3 Position;
        public Vector3 Velocity = Vector3.Zero;

        public Material Material;
        private static MaterialProperties Props;

        public float RestitutionCoeficient(){return Props.restitution_coeficient;}
        private float Radius;


        public Player(GraphicsDevice graphicsDevice, Vector3 position,
                      Material material, float radius)
        {
            Model = new SpherePrimitive(graphicsDevice);
            BoundingSphere = new BoundingSphere(position, radius);
            Position = position;
            Radius = radius;
            Material = material;
            SetMaterialProps(material);
        }

        public void SetMaterial(Material material){
          Material = material;
          SetMaterialProps(material);
        }

        public void Update(float dt, KeyboardState keyboardState)
        {

            if (keyboardState.IsKeyDown(Keys.W))
                Velocity.Z += dt * Props.acceleration;

            if (keyboardState.IsKeyDown(Keys.S))
                Velocity.Z -= dt * Props.acceleration;

            if (keyboardState.IsKeyDown(Keys.A))
                Velocity.X += dt * Props.acceleration;

            if (keyboardState.IsKeyDown(Keys.D))
                Velocity.X -= dt * Props.acceleration;

            if (Velocity.X != 0)
            {
                if (Velocity.X < 0)
                    Velocity.X += dt * Props.friction;
                if (Velocity.X > 0)
                    Velocity.X -= dt * Props.friction;
            }

            if (Velocity.Z != 0)
            {
                if (Velocity.Z < 0)
                    Velocity.Z += dt * Props.friction;
                if (Velocity.Z > 0)
                    Velocity.Z -= dt * Props.friction;
            }


            Position += new Vector3(Velocity.X > 0 ? MathF.Min(Velocity.X, Props.max_speed)
                                                   : MathF.Max(Velocity.X, -Props.max_speed),
                                    Velocity.Y,
                                    Velocity.Z > 0 ? MathF.Min(Velocity.Z, Props.max_speed)
                                                   : MathF.Max(Velocity.Z, -Props.max_speed)) *
                        dt;

            World = Matrix.CreateRotationX(Position.Z) *
                    Matrix.CreateRotationZ(-Position.X) *
                    Matrix.CreateTranslation(Position);
            BoundingSphere = new BoundingSphere(Position, Radius);
        }

        public void Jump()
        {
          Velocity.Y = Props.jump_boost;
        }

        public void Draw(Effect Effect)
        {
            Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["DiffuseColor"].SetValue(Props.color.ToVector3());
            Model.Draw(Effect);
        }

        public bool Intersects(BoundingBox m) { return BoundingSphere.Intersects(m); }

        private void SetMaterialProps(Material material){
          switch(material)
          {
            case Material.Metal:
              Props = new MaterialProperties{ 
                jump_boost=50f,
                  acceleration=15f,
                  friction=7f,
                  max_speed=50f,
                  restitution_coeficient=0.8f,
                  color= Color.Blue
              };
              break;
            case Material.Plastic:
              Props = new MaterialProperties{ 
                jump_boost=20f,
                  acceleration=15f,
                  friction=7f,
                  max_speed=50f,
                  restitution_coeficient=0.8f,
                  color= Color.Red
              };
              break;
            case Material.Rubber:
              Props = new MaterialProperties{ 
                jump_boost=20f,
                  acceleration=15f,
                  friction=7f,
                  max_speed=50f,
                  restitution_coeficient=0.8f,
                  color= Color.Green
              };
              break;
          }
        }

        public void Dispose() { Model.Dispose(); }
    }
}
