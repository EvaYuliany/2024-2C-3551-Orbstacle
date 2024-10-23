using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Objects;

namespace TGC.MonoGame.TP.MenuSpace {
public class Menu {
  private TGCGame game;
  private SpriteFont font;
  private string[] menuItems = { "Iniciar Juego", "Material: ", "Salir" };
  private int selectedIndex = 0;
  public bool IsActive { get; set; }

  public Menu(TGCGame game) {
    this.game = game;
    IsActive = true; // Habilitar el menú por defecto
  }

  public void Initialize() {
    // Inicialización de cualquier recurso aquí si es necesario
  }

  public void LoadContent() {
    // Cargar la fuente del menú
    font = game.Content.Load<SpriteFont>(TGCGame.ContentFolderSpriteFonts +
                                         "MenuFont");
  }

  public void Update(GameTime gameTime, Player player) {
    var keyboardState = Keyboard.GetState();

    if (keyboardState.IsKeyDown(Keys.Down)) {
      selectedIndex = (selectedIndex + 1) % menuItems.Length;
    }
    if (keyboardState.IsKeyDown(Keys.Up)) {
      selectedIndex = (selectedIndex - 1 + menuItems.Length) % menuItems.Length;
    }

    if (keyboardState.IsKeyDown(Keys.Enter)) {
      Select(player);
    }
  }

  private Material GetNextMaterial(Material m) {
    switch (m) {
    case Material.Rubber:
      return Material.Plastic;
    case Material.Plastic:
      return Material.Metal;
    case Material.Metal:
      return Material.Rubber;
    }
    return Material.Metal;
  }

  private string MaterialToString(Material m) {
    switch (m) {
    case Material.Rubber:
      return "Goma";
    case Material.Plastic:
      return "Plastico";
    case Material.Metal:
      return "Metal";
    }
    return "";
  }

  public void Draw(GameTime gameTime, Player player) {
    var rasterState = game.GraphicsDevice.RasterizerState;
    var blendState = game.GraphicsDevice.BlendState;
    var depthStencilState = game.GraphicsDevice.DepthStencilState;

    var spriteBatch = new SpriteBatch(game.GraphicsDevice);

    spriteBatch.Begin();
    menuItems[1] = "Material: " + MaterialToString(player.Material);
    for (int i = 0; i < menuItems.Length; i++) {
      var color = (i == selectedIndex)
                      ? Color.Yellow
                      : Color.White; // Resaltar la opción seleccionada
      spriteBatch.DrawString(font, menuItems[i], new Vector2(100, 100 + i * 50),
                             color);
    }
    spriteBatch.End();

    game.GraphicsDevice.RasterizerState = rasterState;
    game.GraphicsDevice.BlendState = blendState;
    game.GraphicsDevice.DepthStencilState = depthStencilState;
  }

  private void Select(Player player) {
    switch (selectedIndex) {
    case 0:
      IsActive = false; // Desactiva el menú para comenzar el juego
      break;
    case 1:
      player.SetMaterial(
          GetNextMaterial(player.Material)); // Cambiar de material
      break;
    case 2:
      game.Exit(); // Salir del juego
      break;
    }
  }

  public void UnloadContent() {
    // Liberar recursos si es necesario
  }
}
}
