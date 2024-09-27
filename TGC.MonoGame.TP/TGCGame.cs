using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Obstacles;
using TGC.MonoGame.TP.Objects;
using System.Collections.Generic;

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

  private FloorConstructor FloorConstructor;
  private Vector3 LastFloor;

  protected override void Initialize() {
    var rasterizerState = new RasterizerState();
    // rasterizerState.FillMode = FillMode.WireFrame;
    rasterizerState.CullMode = CullMode.None;
    GraphicsDevice.RasterizerState = rasterizerState;
    player = new Player(GraphicsDevice, Vector3.Zero, Material.Metal, 1);
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

    FloorConstructor = new FloorConstructor(GraphicsDevice);
    FloorConstructor.AddBase(Vector2.Zero);
    FloorConstructor.AddBase(Vector2.UnitX);
    FloorConstructor.AddBase(Vector2.UnitX);
    FloorConstructor.AddBase(Vector2.UnitY);
    FloorConstructor.AddSlope(Vector2.UnitY, true);
    LastFloor = FloorConstructor.AddBase(Vector2.UnitY);
    FloorConstructor.AddSlope(Vector2.UnitY, false);
    FloorConstructor.AddSlope(Vector2.UnitY, false);
    FloorConstructor.AddSlope(Vector2.UnitY, false);
    FloorConstructor.AddBase(Vector2.UnitY);
    FloorConstructor.AddSlope(Vector2.UnitY, true);
    FloorConstructor.AddSlope(Vector2.UnitY, true);
    FloorConstructor.AddSlope(Vector2.UnitY, true);
    FloorConstructor.AddBase(Vector2.UnitY);
    FloorConstructor.AddBase(Vector2.UnitX);
    FloorConstructor.AddBase(Vector2.UnitX);
    FloorConstructor.AddBase(Vector2.UnitX);
    FloorConstructor.AddSlope(Vector2.UnitX, true);
    FloorConstructor.AddBase(Vector2.UnitX);
    FloorConstructor.AddBase(Vector2.UnitY);
    FloorConstructor.AddBase(Vector2.UnitY);
    FloorConstructor.AddBase(Vector2.UnitY);
    FloorConstructor.AddBase(-Vector2.UnitX);
    FloorConstructor.AddBase(-Vector2.UnitX);
    FloorConstructor.AddBase(-Vector2.UnitX);
    FloorConstructor.AddSlope(-Vector2.UnitX,true);
    FloorConstructor.AddSlope(-Vector2.UnitX,true);
    FloorConstructor.AddSlope(-Vector2.UnitX,true);
    base.Initialize();
  }

  protected override void LoadContent() {
    Sphere = new SpherePrimitive(GraphicsDevice);
    Cube = new CubePrimitive(GraphicsDevice);
    Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
    PlayerEffect = Content.Load<Effect>(ContentFolderEffects + ("PlayerShade" +
                                                                "r"));

    base.LoadContent();
  }

  protected override void Update(GameTime gameTime) {

    float dt = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
    var keyboardState = Keyboard.GetState();

    if (keyboardState.IsKeyDown(Keys.Escape))
      Exit();

    if (!FloorConstructor.Intersects(player.BoundingSphere)) {
      player.Velocity.Y -= Gravity * dt;
    } else {

      if (keyboardState.IsKeyDown(Keys.Space)) {
        player.Jump();
      }

      if (player.Velocity.Y < 0)
        player.Velocity.Y *= -player.RestitutionCoeficient();
    }

    if (player.Position.Y <= RestartingY) {
      player.Position = PlayerInitialPos;
      player.Velocity = Vector3.Zero;
    }

    player.Update(dt, keyboardState, CameraAngle);

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

    // Vector3 forwardDirection = new Vector3(MathF.Cos(CameraAngle), 0, MathF.Sin(CameraAngle));

    // float movementSpeed = 10f;
    // if (keyboardState.IsKeyDown(Keys.W)) {
    //     player.Position += forwardDirection * movementSpeed * dt;
    // }

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
    player.Draw(PlayerEffect);

    FloorConstructor.Draw(Effect);

    Effect.Parameters["World"].SetValue(Matrix.CreateTranslation(
        new Vector3(2, 0, 2))); // Ajusta la posición si es necesario

    Effect.Parameters["World"].SetValue(Matrix.CreateScale(0.3f) *
                                        Matrix.CreateTranslation(LastFloor));
    Effect.Parameters["DiffuseColor"].SetValue(Color.Yellow.ToVector3());
    Cube.Draw(Effect);

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
  }

  protected override void UnloadContent() {
    Content.Unload();
    Sphere.Dispose();
    Cube.Dispose();
    FloorConstructor.Dispose();
    base.UnloadContent();
  }
}
}
