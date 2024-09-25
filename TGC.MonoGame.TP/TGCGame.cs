using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Obstacles;
using TGC.MonoGame.TP.Objects;
using System.Collections.Generic;
using TGC.MonoGame.TP.Collisions;
using BepuPhysics.Collidables;
using System.Runtime.CompilerServices;

namespace TGC.MonoGame.TP {
  public class TGCGame : Game {
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderMusic = "Music/";
    public const string ContentFolderSounds = "Sounds/";
    public const string ContentFolderSpriteFonts = "SpriteFonts/";
    public const string ContentFolderTextures = "Textures/";
    public TGCGame() {
      Graphics = new GraphicsDeviceManager(this);

      Graphics.PreferredBackBufferWidth =
          GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
      Graphics.PreferredBackBufferHeight =
          GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

      Content.RootDirectory = "Content";
      IsMouseVisible = true;
    }

    private GraphicsDeviceManager Graphics;

    private Effect Effect;
    private Matrix View;
    private Matrix Projection;

    private Vector3 PlayerInitialPos = Vector3.Zero;
    private Effect PlayerEffect;
    private Player player;

    private List<Vector3> cubePositions;
    private List<Vector3> spherePositions;
    private List<Color> sphereColors;
    private List<Color> cubeColors;

    private float CameraAngle = -MathF.PI / 2;
    private float CameraRotationSpeed = 5f;
    private float CameraDistanceToPlayer = 15f;
    private float CameraUpAngle = 0;
    private Vector3 GetCameraPosition(float angle) {
      return new Vector3(MathF.Cos(angle) * CameraDistanceToPlayer, 3,
                        MathF.Sin(angle) * CameraDistanceToPlayer);
    }

  // CHECKPOINTS
    private CylinderPrimitive Cylinder { get; set;}
    private const float CheckPointHeight = 5000 ;
    private const float CheckPointRadius = 4f ;
    private Vector3 CheckPoint1Position{ get; set; }
    private BoundingBox CheckPoint1Collide { get; set; }

    private int CheckPoint1 ;
    private Matrix CheckPoimtMatrix { get; set; }
    private BoundingSphere SphereCollide { get; set; }


    //POWERUPS

    private BoundingBox PowerUp1Collide { get; set;}
    private int PowerUp1Count;
    private TeapotPrimitive PowerUp1 {get; set;}

    private float Gravity = 50f;
    private float RestartingY = -50f;

    private int numberOfCubes = 50;
    private int numberOfSpheres = 50;

    private Random random;

    private SpherePrimitive Sphere;

    private CubePrimitive Cube;
    private BoundingBox FloorBB;
    private Matrix FloorWorld = Matrix.Identity;
    private float FloorSize = 1000f;

    protected override void Initialize() {
      var rasterizerState = new RasterizerState();
      // rasterizerState.FillMode = FillMode.WireFrame;
      rasterizerState.CullMode = CullMode.None;
      GraphicsDevice.RasterizerState = rasterizerState;
      player = new Player(GraphicsDevice, Vector3.Zero, 15f, 1, Color.DarkBlue);
      View = Matrix.CreateLookAt(GetCameraPosition(CameraAngle) + player.Position,
                                player.Position + Vector3.UnitY * CameraUpAngle,
                                Vector3.Up);
      Projection = Matrix.CreatePerspectiveFieldOfView(
          MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

      random = new Random(0);
      cubePositions = new List<Vector3>();
      cubeColors = new List<Color>();
      sphereColors = new List<Color>();
      for (int i = 0; i < numberOfCubes; i++) {
        // Generar posiciones aleatorias dentro de un rango (por ejemplo, en un
        // área 100x100x100)
        var randomPosition =
            new Vector3((float)(random.NextDouble() * 100 - 50), // X
                        (float)(random.NextDouble() * 10),       // Y
                        (float)(random.NextDouble() * 100 - 50)  // Z
            );
        cubePositions.Add(randomPosition);

        // Generar color aleatorio
        var randomColor = new Color((float)random.NextDouble(), // R
                                    (float)random.NextDouble(), // G
                                    (float)random.NextDouble()  // B
        );
        cubeColors.Add(randomColor);
      }
      spherePositions = new List<Vector3>();
      for (int i = 0; i < numberOfSpheres; i++) {
        // Generar posiciones aleatorias dentro de un rango
        var randomPosition =
            new Vector3((float)(random.NextDouble() * 100 - 50), // X
                        (float)(random.NextDouble() * 10),       // Y
                        (float)(random.NextDouble() * 130 - 50)  // Z
            );

        spherePositions.Add(randomPosition);
        // Generar color aleatorio
        var randomColor = new Color((float)random.NextDouble(), // R
                                    (float)random.NextDouble(), // G
                                    (float)random.NextDouble()  // B
        );
        sphereColors.Add(randomColor);
      }

      FloorBB = new BoundingBox(new Vector3(-FloorSize / 2, -1, -FloorSize / 2),
                                new Vector3(FloorSize / 2, -1, FloorSize / 2));
      FloorWorld =
          Matrix.CreateScale(FloorSize, 0.2f, FloorSize) *
          Matrix.CreateTranslation(-Vector3.UnitY - new Vector3(0, 0.2f, 0));


      Cylinder = new CylinderPrimitive(GraphicsDevice , CheckPointHeight,CheckPointRadius,16);
      CheckPoimtMatrix = Matrix.Identity ;

      CheckPoint1 = 0;
      CheckPoint1Position = new Vector3(15,0,1);
      CheckPoint1Collide = new BoundingBox(new Vector3(13,0,1), new Vector3(17,1,1));
      
      SphereCollide = new BoundingSphere(player.Position , 1f); //No me funca

      PowerUp1 = new TeapotPrimitive(GraphicsDevice,1);
      PowerUp1Collide = new BoundingBox(new Vector3(-10,0,0),new Vector3(-10,1,0));
      PowerUp1Count = 0;
      base.Initialize();
    }

    protected override void LoadContent() {
      Sphere = new SpherePrimitive(GraphicsDevice);
      Cube = new CubePrimitive(GraphicsDevice);
      Cylinder = new CylinderPrimitive(GraphicsDevice , CheckPointHeight , 3f);
      Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
      PlayerEffect =
          Content.Load<Effect>(ContentFolderEffects + ("PlayerShade" + "r"));

      base.LoadContent();
    }

    protected override void Update(GameTime gameTime) {

      float dt = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
      var keyboardState = Keyboard.GetState();

      if (keyboardState.IsKeyDown(Keys.Escape))
        Exit();

      if (!player.Intersects(FloorBB)) {
        player.Velocity.Y -= Gravity * dt;
      } else {
        player.Velocity.Y *= -player.RestitutionCoeficient;
      }

      if (player.Position.Y <= RestartingY) {
        player.Position = PlayerInitialPos;
        player.Velocity = Vector3.Zero;
      }

      player.Update(dt, keyboardState);

      // Movimiento de la cámara con las flechas para facilidad de ver las cosas
      if (keyboardState.IsKeyDown(Keys.Up))
        CameraUpAngle += CameraRotationSpeed * dt;

      if (keyboardState.IsKeyDown(Keys.Down))
        CameraUpAngle -= CameraRotationSpeed * dt; // Mover la cámara hacia atrás

      if (keyboardState.IsKeyDown(Keys.Left))
        CameraAngle +=
            CameraRotationSpeed * dt; // Mover la cámara hacia la izquierda

      if (keyboardState.IsKeyDown(Keys.Right))
        CameraAngle -=
            CameraRotationSpeed * dt; // Mover la cámara hacia la derecha

      if(player.Intersects(CheckPoint1Collide)) {
        CheckPoint1=1;
      } //Desaparece

      if(player.Intersects(PowerUp1Collide)){ //PowerUp de Velocidad
        player.Velocity = new Vector3(player.Velocity.X *1.2f ,player.Velocity.Y ,player.Velocity.Z *1.2f);
        
      }

      base.Update(gameTime);
      View = Matrix.CreateLookAt(GetCameraPosition(CameraAngle) + player.Position,
                                player.Position + Vector3.UnitY * CameraUpAngle,
                                Vector3.Up);

              
    }

    protected override void Draw(GameTime gameTime) {
      GraphicsDevice.Clear(Color.Black);
      Effect.Parameters["View"].SetValue(View);
      Effect.Parameters["Projection"].SetValue(Projection);

      PlayerEffect.Parameters["View"].SetValue(View);
      PlayerEffect.Parameters["Projection"].SetValue(Projection);
      PlayerEffect.Parameters["DiffuseColor"].SetValue(Color.Blue.ToVector3());
      player.Draw(PlayerEffect);

      Effect.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());
      Effect.Parameters["World"].SetValue(FloorWorld);
      Cube.Draw(Effect);

      Effect.Parameters["DiffuseColor"].SetValue(Color.Salmon.ToVector3());
      Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(
          new Vector3(FloorSize / 2, -1, FloorSize / 2)));
      Cube.Draw(Effect);

      Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(
          new Vector3(-FloorSize / 2, -1, -FloorSize / 2)));
      Cube.Draw(Effect);

      Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(
          new Vector3(2, 0, 2))); // Ajusta la posición si es necesario

      for (int i = 0; i < cubePositions.Count; i++) {
        var position = cubePositions[i];
        var color = cubeColors[i];

        Matrix worldMatrix =
            Matrix.CreateScale(1f) * Matrix.CreateTranslation(position);
        Effect.Parameters["World"].SetValue(worldMatrix);
        Effect.Parameters["DiffuseColor"].SetValue(
            color.ToVector3()); // Usar el color aleatorio
        Cube.Draw(Effect);
      }

      for (int i = 0; i < spherePositions.Count; i++) {
        var position = spherePositions[i];
        var color = sphereColors[i];

        Matrix worldMatrix =
            Matrix.CreateScale(1f) * Matrix.CreateTranslation(position);
        Effect.Parameters["World"].SetValue(worldMatrix);
        Effect.Parameters["DiffuseColor"].SetValue(
            color.ToVector3()); // Usar el color aleatorio
        Sphere.Draw(Effect);
      }

      if (CheckPoint1==0) {
        var color = Color.Blue;
        Matrix worldMatrix =
            Matrix.CreateScale(1f) * Matrix.CreateTranslation(CheckPoint1Position);
        Effect.Parameters["World"].SetValue(worldMatrix);
        Effect.Parameters["DiffuseColor"].SetValue(
            color.ToVector3()); // Usar el color aleatorio
        Cylinder.Draw(Effect);
      }
      if(PowerUp1Count == 0){
        Matrix worldMatrix =
            Matrix.CreateScale(1f) * Matrix.CreateTranslation(new Vector3(-10,0,0));
        Effect.Parameters["World"].SetValue(worldMatrix);
      PowerUp1.Draw(Effect);
      }
      
      

    }

    protected override void UnloadContent() {
      Content.Unload();
      Sphere.Dispose();
      Cube.Dispose();
      base.UnloadContent();
    }

      private Vector3 Loss()  // Pierde y Resetea la Posicion
      {
        if(CheckPoint1 == 1){
            return CheckPoint1Position;
        }
        return new Vector3(0,0,0);
      }

      
  }
}


 
