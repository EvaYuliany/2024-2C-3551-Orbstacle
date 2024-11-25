using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Obstacles;
using TGC.MonoGame.TP.Objects;
using TGC.MonoGame.TP.MenuSpace;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.TP.SkyBoxSpace;

namespace TGC.MonoGame.TP {
public class TGCGame : Game {
  public const string ContentFolder3D = "Models/";
  public const string ContentFolderEffects = "Effects/";
  public const string ContentFolderSounds = "Sounds/";
  public const string ContentFolderSpriteFonts = "Font/";
  public const string ContentFolderTextures = "Textures/";

  private GraphicsDeviceManager Graphics;

  public TGCGame() {
    Graphics = new GraphicsDeviceManager(this);

    Graphics.PreferredBackBufferWidth =
        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
    Graphics.PreferredBackBufferHeight =
        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }

  Vector3 ambientColor = new Vector3(0.868f, 0.696f, 0.336f);
  Vector3 diffuseColor = new Vector3(1f, 0.182f, 0.157f);
  Vector3 specularColor = Vector3.One;
  Vector3 lightPosition = Vector3.UnitY * 500;

  float kAmbient = 1f;
  float kDiffuse = 0.6f;
  float kSpecular = 0.8f;
  float shininess = 16f;

  Vector2 tiling = Vector2.One * 0.5f;

  private const int EnvironmentMapSize = 2048;
  Vector3 EnvironmentUpDirection = Vector3.UnitY;
  Vector3 EnvironmentFrontDirection = Vector3.UnitX;

  private const int ShadowMapSize = 2048;
  Vector3 ShadowUpDirection = Vector3.UnitX;
  Vector3 ShadowFrontDirection = -Vector3.UnitY;

  private Effect ShadowNormalEffect;
  private Effect ShadowBlinnEffect;
  private Effect EnvironmentEffect;

  private Matrix View;
  private Matrix Projection;

  private Matrix EnvironmentView;
  private Matrix EnvironmentProjection;

  private Matrix ShadowView;
  private Matrix ShadowProjection;

  private FullScreenQuad fullScreenQuad;

  private RenderTargetCube EnvironmentRenderTarget;
  private RenderTarget2D ShadowRenderTarget;

  private Menu menu;

  private Player player;
  private float Points = 0;
  private Vector3 PlayerInitialPos = Vector3.Zero;

  private List<Vector3> cubePositions;
  private List<Vector3> spherePositions;
  private List<Color> sphereColors;
  private List<Color> cubeColors;

  private float CameraAngle = MathF.PI;
  private float CameraRotationSpeed = 5f;
  private float CameraDistanceToPlayer = 15f;
  private float CameraUpAngle = 0;
  private Vector3 GetCameraPosition(float angle) {
    if (menu.IsActive)
      return new Vector3(-50, 50, 50);
    return new Vector3(MathF.Cos(angle) * CameraDistanceToPlayer, 3,
                       MathF.Sin(angle) * CameraDistanceToPlayer);
  }

  private float Gravity = 50f;
  private float RestartingY = -200f;

  private int numberOfCubes = 100;
  private int numberOfSpheres = 100;

  private Random random;

  private SpherePrimitive Sphere;
  private CubePrimitive Cube;
  private FloorConstructor FloorConstructor;

  private Pendulum pendulum;
  private Checkpoint check;
  private SpeedPowerUp powerup;
  private JumpBoostPowerUp jpowerup;
  private List<Coin> coins;

  private SkyBox SkyBox { get; set; }
  private Model SkyBoxModel { get; set; }
  private Effect SkyBoxEffect { get; set; }
  private TextureCube SkyBoxTexture { get; set; }

  private Song Song { get; set; }
  private Texture2D FloorNormalMap { get; set; }
  private Texture2D FloorTexture { get; set; }

  protected override void Initialize() {
    menu = new Menu(this);
    var rasterizerState = new RasterizerState();
    // rasterizerState.FillMode = FillMode.WireFrame;
    rasterizerState.CullMode = CullMode.None;
    GraphicsDevice.RasterizerState = rasterizerState;
    player = new Player(GraphicsDevice, Vector3.Zero, Material.Metal, 1);

    View = Matrix.CreateLookAt(GetCameraPosition(CameraAngle) + player.Position,
                               player.Position + Vector3.UnitY * CameraUpAngle,
                               Vector3.Up);
    EnvironmentView = Matrix.CreateLookAt(
        player.Position, player.Position + EnvironmentFrontDirection,
        EnvironmentUpDirection);
    ShadowView = Matrix.CreateLookAt(
        lightPosition, lightPosition + ShadowFrontDirection, ShadowUpDirection);

    Projection = Matrix.CreatePerspectiveFieldOfView(
        MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1500);
    EnvironmentProjection =
        Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 3000f);
    ShadowProjection =
        Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 5f, 3000f);

    EnvironmentRenderTarget = new RenderTargetCube(
        GraphicsDevice, EnvironmentMapSize, false, SurfaceFormat.Color,
        DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
    ShadowRenderTarget =
        new RenderTarget2D(GraphicsDevice, ShadowMapSize, ShadowMapSize, false,
                           SurfaceFormat.Single, DepthFormat.Depth24, 0,
                           RenderTargetUsage.PlatformContents);

    random = new Random(0);
    cubePositions = new List<Vector3>();
    cubeColors = new List<Color>();
    sphereColors = new List<Color>();
    for (int i = 0; i < numberOfCubes; i++) {
      var randomPosition = new Vector3((float)(random.NextDouble() * 600), // X
                                       (float)(random.NextDouble() * 20),  // Y
                                       (float)(random.NextDouble() * 600)  // Z
      );
      cubePositions.Add(randomPosition);
      var randomColor = new Color((float)random.NextDouble(), // R
                                  (float)random.NextDouble(), // G
                                  (float)random.NextDouble()  // B
      );
      cubeColors.Add(randomColor);
    }
    spherePositions = new List<Vector3>();
    for (int i = 0; i < numberOfSpheres; i++) {
      var randomPosition = new Vector3((float)(random.NextDouble() * 600), // X
                                       (float)(random.NextDouble() * 50),  // Y
                                       (float)(random.NextDouble() * 600)  // Z
      );
      spherePositions.Add(randomPosition);
      var randomColor = new Color((float)random.NextDouble(), // R
                                  (float)random.NextDouble(), // G
                                  (float)random.NextDouble()  // B
      );
      sphereColors.Add(randomColor);
    }

    powerup = new SpeedPowerUp(GraphicsDevice, Vector3.UnitY, 2.5f);
    jpowerup = new JumpBoostPowerUp(GraphicsDevice, Vector3.UnitY, 1.2f);
    check = new Checkpoint(GraphicsDevice, Vector3.Zero, 7, 5000);
    coins = new List<Coin>();
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
        Vector3 center = FloorConstructor.AddBase(offset);
        coins.Add(new Coin(GraphicsDevice, center + Vector3.UnitY * 1.2f));
        TrackPositions.Add(center);
      }
    }

    powerup.Position += TrackPositions[2];
    jpowerup.Position += TrackPositions[3];
    pendulum.Position += TrackPositions[4];
    check.Position += TrackPositions[6] + Vector3.UnitY * 0.5f;
    menu.Initialize();
    base.Initialize();
  }

  protected override void LoadContent() {
    FloorNormalMap =
        Content.Load<Texture2D>(ContentFolderTextures + "tiling-normal");
    FloorTexture =
        Content.Load<Texture2D>(ContentFolderTextures + "tiling-base");

    SkyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
    SkyBoxModel = Content.Load<Model>(ContentFolder3D + "skybox/cube");
    SkyBoxTexture =
        Content.Load<TextureCube>(ContentFolderTextures + ("skybo" + "x"));
    SkyBox = new SkyBox(SkyBoxModel, SkyBoxTexture, SkyBoxEffect);

    Sphere = new SpherePrimitive(GraphicsDevice);
    Cube = new CubePrimitive(GraphicsDevice);
    fullScreenQuad = new FullScreenQuad(GraphicsDevice);

    ShadowBlinnEffect =
        Content.Load<Effect>(ContentFolderEffects + "ShadowBlinnShader");
    ShadowNormalEffect =
        Content.Load<Effect>(ContentFolderEffects + "ShadowNormalShader");
    EnvironmentEffect =
        Content.Load<Effect>(ContentFolderEffects + "EnvironmentShader");

    SetBlinnEffect(ShadowBlinnEffect);
    SetBlinnEffect(ShadowNormalEffect);
    SetNormalEffect(ShadowNormalEffect);

    Song = Content.Load<Song>(ContentFolderSounds + "retro-2");
    MediaPlayer.IsRepeating = true;
    MediaPlayer.Volume = 0.2f;
    MediaPlayer.Play(Song);

    menu.LoadContent();
    base.LoadContent();
  }
  protected override void Update(GameTime gameTime) {
    if (menu.IsActive)
      menu.Update(gameTime, player);
    float dt = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
    var keyboardState = Keyboard.GetState();

    if (keyboardState.IsKeyDown(Keys.Escape))
      menu.IsActive = true;

    if (!menu.IsActive)
      player.Update(dt, keyboardState, CameraAngle);
    pendulum.Update(dt);
    for (int i = 0; i < coins.Count; i++) {
      coins[i].Update(dt);
    }
    if (!menu.IsActive) {
      CheckCollisions(dt, keyboardState, gameTime);
      CameraMovement(dt, keyboardState);
    }

    ShadowBlinnEffect.Parameters["eyePosition"].SetValue(
        GetCameraPosition(CameraAngle));
    ShadowNormalEffect.Parameters["eyePosition"].SetValue(
        GetCameraPosition(CameraAngle));
    EnvironmentEffect.Parameters["eyePosition"].SetValue(
        GetCameraPosition(CameraAngle));

    base.Update(gameTime);
    View = Matrix.CreateLookAt(GetCameraPosition(CameraAngle) + player.Position,
                               player.Position + Vector3.UnitY * CameraUpAngle,
                               Vector3.Up);
    EnvironmentView = Matrix.CreateLookAt(
        player.Position, player.Position + EnvironmentFrontDirection,
        EnvironmentUpDirection);
    ShadowView = Matrix.CreateLookAt(
        lightPosition, lightPosition + ShadowFrontDirection, ShadowUpDirection);
  }

  protected void DrawShadows(GameTime gameTime) {
    GraphicsDevice.SetRenderTarget(ShadowRenderTarget);
    GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                         Color.Black, 1f, 0);

    ShadowBlinnEffect.CurrentTechnique =
        ShadowBlinnEffect.Techniques["DepthPass"];

    DrawScene(gameTime, ShadowBlinnEffect, ShadowView, ShadowProjection);
    ShadowBlinnEffect.Parameters["BaseColor"].SetValue(Color.White.ToVector3());
    FloorConstructor.Draw(ShadowBlinnEffect, ShadowView, ShadowProjection);
  }

  protected void DrawScene(GameTime gameTime, Effect effect, Matrix View,
                           Matrix Projection) {

    pendulum.Draw(effect, View, Projection);
    powerup.Draw(effect, View, Projection);
    jpowerup.Draw(effect, View, Projection);
    check.Draw(effect, View, Projection);

    foreach (var coin in coins) {
      coin.Draw(effect, View, Projection);

      if (!menu.IsActive) {
        SkyBox.Draw(View, Projection, GetCameraPosition(CameraAngle));
      }
    }
  }

  protected void SetNormalEffect(Effect effect) {
    effect.Parameters["NormalTexture"].SetValue(FloorNormalMap);
    effect.Parameters["ModelTexture"].SetValue(FloorTexture);
  }

  protected void SetBlinnEffect(Effect effect) {
    effect.Parameters["lightPosition"].SetValue(lightPosition);
    effect.Parameters["ambientColor"].SetValue(ambientColor);
    effect.Parameters["diffuseColor"].SetValue(diffuseColor);
    effect.Parameters["specularColor"].SetValue(specularColor);
    effect.Parameters["KAmbient"].SetValue(kAmbient);
    effect.Parameters["KDiffuse"].SetValue(kDiffuse);
    effect.Parameters["KSpecular"].SetValue(kSpecular);
    effect.Parameters["shininess"].SetValue(shininess);
  }

  protected void SetShadowEffect(Effect effect) {
    effect.CurrentTechnique = effect.Techniques["DrawShadowedPCF"];
    effect.Parameters["shadowMap"].SetValue(ShadowRenderTarget);
    effect.Parameters["lightPosition"].SetValue(lightPosition);
    effect.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowMapSize);
    effect.Parameters["LightViewProjection"].SetValue(ShadowView *
                                                      ShadowProjection);
  }

  protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(Color.Black);

    DrawShadows(gameTime);
    GraphicsDevice.SetRenderTarget(null);
    SetShadowEffect(ShadowNormalEffect);
    SetShadowEffect(ShadowBlinnEffect);

    DrawScene(gameTime, ShadowBlinnEffect, View, Projection);
    FloorConstructor.Draw(ShadowNormalEffect, View, Projection);
    if (menu.IsActive) {
      menu.Draw(gameTime, player);
    } else {
      switch (player.Material) {
      case Material.Metal: {
        for (var face = CubeMapFace.PositiveX; face <= CubeMapFace.NegativeZ;
             face++) {
          GraphicsDevice.SetRenderTarget(EnvironmentRenderTarget, face);
          GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                               Color.Black, 1f, 0);

          SetCubemapCameraForOrientation(face);
          EnvironmentView = Matrix.CreateLookAt(
              player.Position, player.Position + EnvironmentFrontDirection,
              EnvironmentUpDirection);

          DrawScene(gameTime, EnvironmentEffect, EnvironmentView,
                    EnvironmentProjection);
          FloorConstructor.Draw(EnvironmentEffect, EnvironmentView,
                                EnvironmentProjection);
        }
        GraphicsDevice.SetRenderTarget(null);
        EnvironmentEffect.Parameters["environmentMap"].SetValue(
            EnvironmentRenderTarget);
        player.Draw(EnvironmentEffect, View, Projection);
        break;
      }
      case Material.Plastic: {
        ShadowBlinnEffect.Parameters["BaseColor"].SetValue(
            Color.Red.ToVector3());
        player.Draw(ShadowBlinnEffect, View, Projection);
        break;
      }
      case Material.Rubber: {
        ShadowBlinnEffect.Parameters["BaseColor"].SetValue(
            Color.Green.ToVector3());
        player.Draw(ShadowBlinnEffect, View, Projection);
        break;
      }
      }
    }
  }

  private void SetCubemapCameraForOrientation(CubeMapFace face) {
    switch (face) {
    default:
    case CubeMapFace.PositiveX:
      EnvironmentFrontDirection = -Vector3.UnitX;
      EnvironmentUpDirection = Vector3.Down;
      break;

    case CubeMapFace.NegativeX:
      EnvironmentFrontDirection = Vector3.UnitX;
      EnvironmentUpDirection = Vector3.Down;
      break;

    case CubeMapFace.PositiveY:
      EnvironmentFrontDirection = Vector3.Down;
      EnvironmentUpDirection = Vector3.UnitZ;
      break;

    case CubeMapFace.NegativeY:
      EnvironmentFrontDirection = Vector3.Up;
      EnvironmentUpDirection = -Vector3.UnitZ;
      break;

    case CubeMapFace.PositiveZ:
      EnvironmentFrontDirection = -Vector3.UnitZ;
      EnvironmentUpDirection = Vector3.Down;
      break;

    case CubeMapFace.NegativeZ:
      EnvironmentFrontDirection = Vector3.UnitZ;
      EnvironmentUpDirection = Vector3.Down;
      break;
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

  public void CheckCollisions(float dt, KeyboardState keyboardState,
                              GameTime gameTime) {
    powerup.CheckCollision(player, gameTime);
    jpowerup.CheckCollision(player, gameTime);

    for (int i = 0; i < coins.Count; i++) {
      if (coins[i].Intersects(player.BoundingSphere)) {
        Points++;
        coins[i].Dispose();
        coins.RemoveAt(i);
      }
    }

    if (pendulum.Intersects(player.BoundingSphere)) {
      player.Velocity += Vector3.UnitX * pendulum.Speed * 3;
    }

    if (check.Intersects(player.BoundingSphere)) {
      PlayerInitialPos = check.Position;
      check.Dispose();
    }

    (bool PlayerIntersectsFloor, Floor IntersectingFloor) =
        FloorConstructor.Intersects(player.BoundingSphere);
    if (PlayerIntersectsFloor) {
      if (keyboardState.IsKeyDown(Keys.Space)) {
        player.Jump();
      }

      if (player.Velocity.Y < 0) {
        player.Velocity =
            Vector3.Reflect(player.Velocity * player.RestitutionCoeficient,
                            IntersectingFloor.Normal);
      }

      if (player.Velocity.Y >= 0) {
        player.Velocity += Vector3.UnitY * player.RestitutionCoeficient;
        Vector3 grav = -20 * Gravity * Vector3.UnitY;
        Vector3 acc = grav - (Vector3.Dot(IntersectingFloor.Normal, grav) *
                              IntersectingFloor.Normal);
        player.Velocity += acc * dt;
      }

    } else {
      player.Velocity.Y -= Gravity * dt;
    }

    if (player.Position.Y <= RestartingY) {
      player.SetMaterial(player.Material);
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
