using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Obstacles;
using TGC.MonoGame.TP.Objects;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.TP.SkyBoxSpace;

namespace TGC.MonoGame.TP {
public class TGCGame : Game {
  public const string ContentFolder3D = "Models/";
  public const string ContentFolderEffects = "Effects/";
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
  private Song Song { get; set; }
  private List<Vector3> cubePositions;
  private List<Vector3> spherePositions;
  private List<Color> sphereColors;
  private List<Color> cubeColors;

  private float CameraAngle = MathF.PI;
  private float CameraRotationSpeed = 5f;
  private float CameraDistanceToPlayer = 15f;
  private float CameraUpAngle = 0;
  private Vector3 GetCameraPosition(float angle) {
    return new Vector3(MathF.Cos(angle) * CameraDistanceToPlayer, 3,
                       MathF.Sin(angle) * CameraDistanceToPlayer);
  }

  private float Gravity = 50f;
  private float RestartingY = -200f;

  private int numberOfCubes = 50;
  private int numberOfSpheres = 50;

  private Random random;

  private SpherePrimitive Sphere;

  private CubePrimitive Cube;

  private FloorConstructor FloorConstructor;

  private Checkpoint check;
  private SpeedPowerUp powerup;
  private JumpBoostPowerUp jpowerup;

  private Coin coin;
  private float Points = 0;

  private Pendulum pendulum;
  private SkyBox SkyBox { get; set; }
  private Model SkyBoxModel { get; set; }
  private Effect SkyBoxEffect { get; set; }
  private TextureCube SkyBoxTexture { get; set; }

  protected override void Initialize() {
    var rasterizerState = new RasterizerState();
    // rasterizerState.FillMode = FillMode.WireFrame;
    rasterizerState.CullMode = CullMode.None;
    GraphicsDevice.RasterizerState = rasterizerState;
    player = new Player(GraphicsDevice, Vector3.Zero, Material.Metal, 1);
    SkyBox = new SkyBox(SkyBoxModel,SkyBoxTexture,SkyBoxEffect);

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

    powerup = new SpeedPowerUp(GraphicsDevice, Vector3.UnitY, 2.5f);
    jpowerup = new JumpBoostPowerUp(GraphicsDevice, Vector3.UnitY, 1.2f);
    check = new Checkpoint(GraphicsDevice, Vector3.Zero, 7, 5000);
    coin = new Coin(GraphicsDevice, Vector3.UnitY * 1.2f);
    pendulum = new Pendulum(GraphicsDevice, Vector3.UnitY * 20, 0, MathF.PI / 2,
                            MathF.PI / 2, -MathF.PI / 2, 15, 10, Color.Red,
                            Color.Blue, 1);

    FloorConstructor = new FloorConstructor(GraphicsDevice);
    (int, Vector2,
     bool)[] Track = { (0, Vector2.Zero, false),   (0, Vector2.UnitX, false),
                       (0, Vector2.UnitX, false),  (0, Vector2.UnitY, false),
                       (0, Vector2.UnitX, false),  (1, Vector2.UnitY, true),
                       (0, Vector2.UnitY, false),  (1, Vector2.UnitY, false),
                       (1, Vector2.UnitY, false),  (1, Vector2.UnitY, false),
                       (0, Vector2.UnitY, false),  (1, Vector2.UnitY, true),
                       (1, Vector2.UnitY, true),   (1, Vector2.UnitY, true),
                       (0, Vector2.UnitY, false),  (0, Vector2.UnitX, false),
                       (0, Vector2.UnitX, false),  (0, Vector2.UnitX, false),
                       (1, Vector2.UnitX, true),   (0, Vector2.UnitX, false),
                       (0, Vector2.UnitY, false),  (0, Vector2.UnitY, false),
                       (0, Vector2.UnitY, false),  (0, -Vector2.UnitX, false),
                       (0, -Vector2.UnitX, false), (0, -Vector2.UnitX, false),
                       (1, -Vector2.UnitX, true),  (0, -Vector2.UnitX, true),
                       (1, -Vector2.UnitX, true) };

    List<Vector3> TrackPositions = new List<Vector3>();

    for (int i = 0; i < Track.Length; i++) {
      (int type, Vector2 offset, bool up) = Track[i];
      if (type == 1) {
        TrackPositions.Add(FloorConstructor.AddSlope(offset, up));
      } else {
        TrackPositions.Add(FloorConstructor.AddBase(offset));
      }
    }

    coin.Position += TrackPositions[1];
    powerup.Position += TrackPositions[2];
    jpowerup.Position += TrackPositions[3];
    pendulum.Position += TrackPositions[4];
    check.Position += TrackPositions[6];

    base.Initialize();
  }

  protected override void LoadContent() {
    SkyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
    SkyBoxModel = Content.Load<Model>(ContentFolder3D + "skybox/cube");
    SkyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skybox");
    SkyBox = new SkyBox(SkyBoxModel,SkyBoxTexture,SkyBoxEffect);

    Sphere = new SpherePrimitive(GraphicsDevice);
    Cube = new CubePrimitive(GraphicsDevice);
    Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
    PlayerEffect =  Content.Load<Effect>(ContentFolderEffects + ("PlayerShade" + "r"));
    Song = Content.Load<Song>(ContentFolderSounds + "retro-2");
    MediaPlayer.IsRepeating = true; 
    MediaPlayer.Play(Song);

    base.LoadContent();
  }

  protected override void Update(GameTime gameTime) {
    float dt = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
    var keyboardState = Keyboard.GetState();

    if (keyboardState.IsKeyDown(Keys.Escape))
      Exit();

    player.Update(dt, keyboardState, CameraAngle);
    pendulum.Update(dt);
    coin.Update(dt);
    CheckCollisions(dt, keyboardState);
    CameraMovement(dt, keyboardState);

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
    pendulum.Draw(Effect);

    FloorConstructor.Draw(Effect);
    powerup.Draw(Effect);
    jpowerup.Draw(Effect);
    check.Draw(Effect);
    coin.Draw(Effect);
    SkyBox.Draw(View,Projection, GetCameraPosition(CameraUpAngle));

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
                              // Cube.Draw(Effect);
    }

    for (int i = 0; i < spherePositions.Count; i++) {
      var position = spherePositions[i];
      var color = sphereColors[i];

      Matrix worldMatrix =
          Matrix.CreateScale(1f) * Matrix.CreateTranslation(position);
      Effect.Parameters["World"].SetValue(worldMatrix);
      Effect.Parameters["DiffuseColor"].SetValue(
          color.ToVector3()); // Usar el color aleatorio
                              // Sphere.Draw(Effect);
    }
  }

  public void CameraMovement(float dt, KeyboardState keyboardState) {
    if (keyboardState.IsKeyDown(Keys.Up))
      CameraUpAngle += CameraRotationSpeed * dt;

    if (keyboardState.IsKeyDown(Keys.Down))
      CameraUpAngle -= CameraRotationSpeed * dt;

    if (keyboardState.IsKeyDown(Keys.Left))
      CameraAngle -= CameraRotationSpeed * dt;

    if (keyboardState.IsKeyDown(Keys.Right))
      CameraAngle += CameraRotationSpeed * dt;
  }

  public void CheckCollisions(float dt, KeyboardState keyboardState) {
    powerup.CheckCollision(player);
    jpowerup.CheckCollision(player);

    if (coin.Intersects(player.BoundingSphere)) {
      Points++;
      coin.Dispose();
    }

    if (pendulum.Intersects(player.BoundingSphere)) {
      player.Velocity += Vector3.UnitX * pendulum.Speed * 2;
    }

    if (check.Intersects(player.BoundingSphere)) { // checkpoint
      PlayerInitialPos = check.Position;
    }

    (bool PlayerIntersectsFloor, Floor IntersectingFloor) =
        FloorConstructor.Intersects(player.BoundingSphere);
    if (PlayerIntersectsFloor) {
      if (keyboardState.IsKeyDown(Keys.Space)) {
        player.Jump();
      }

      if (player.Velocity.Y < 0.1)
        player.Velocity =
            Vector3.Reflect(player.Velocity, IntersectingFloor.Normal) *
            player.RestitutionCoeficient;

    } else {
      player.Velocity.Y -= Gravity * dt;
    }

    if (player.Position.Y <= RestartingY) {
      player.Position = PlayerInitialPos;
      player.Velocity = Vector3.Zero;
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
