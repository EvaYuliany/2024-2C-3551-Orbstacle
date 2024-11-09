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
    private KeyboardState keyboardState;
    private KeyboardState previousKeyboardState;
    private Texture2D backgroundTexture;

    public Menu(TGCGame game) {
        this.game = game;
        IsActive = true; // Habilitar el menú por defecto
    }

    public void Initialize() {
        // Inicialización de cualquier recurso aquí si es necesario
    }

    public void LoadContent() {
        // Cargar la fuente del menú
        font = game.Content.Load<SpriteFont>(TGCGame.ContentFolderSpriteFonts + "MenuFont");
        
        // Crear un fondo semitransparente
        backgroundTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        backgroundTexture.SetData(new[] { new Color(0, 0, 0, 150) }); // Negro semitransparente
    }

    public void Update(GameTime gameTime, Player player) {
        keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down)) {
            selectedIndex = (selectedIndex + 1) % menuItems.Length;
        }
        if (keyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up)) {
            selectedIndex = (selectedIndex - 1 + menuItems.Length) % menuItems.Length;
        }

        if (keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter)) {
            Select(player);
        }
        previousKeyboardState = keyboardState;
    }

    private Material GetNextMaterial(Material m) {
        switch (m) {
            case Material.Rubber: return Material.Plastic;
            case Material.Plastic: return Material.Metal;
            case Material.Metal: return Material.Rubber;
        }
        return Material.Metal;
    }

    private string MaterialToString(Material m) {
        switch (m) {
            case Material.Rubber: return "Goma";
            case Material.Plastic: return "Plastico";
            case Material.Metal: return "Metal";
        }
        return "";
    }

    private string MaterialDescriptionToString(Material m) {
        switch (m) {
            case Material.Rubber: return "Goma - rebote: medio - velocidad maxima: 40";
            case Material.Plastic: return "Plastico - rebote: fuerte - velocidad maxima: 50";
            case Material.Metal: return "Metal - rebote: debil - velocidad maxima 30";
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
        
        Vector2 screenCenter = new Vector2(game.GraphicsDevice.Viewport.Width / 2f, game.GraphicsDevice.Viewport.Height / 2f);
        
        for (int i = 0; i < menuItems.Length; i++) {
            // Ajustar la escala y color del texto según si está seleccionado o no
            float scale = (i == selectedIndex) ? 1.2f : 1f;
            Color color = (i == selectedIndex) ? Color.BlueViolet : Color.White;

            // Sombra para el texto
            Vector2 textSize = font.MeasureString(menuItems[i]);
            
            // Dibujar sombra

            // Dibujar texto principal
            spriteBatch.DrawString(font, menuItems[i], new Vector2(100, 100 + i * 50), color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        string materialDetails = MaterialDescriptionToString(player.Material);
        Vector2 detailsSize = font.MeasureString(materialDetails);
        Vector2 detailsPosition = new Vector2(game.GraphicsDevice.Viewport.Width / 2f - detailsSize.X / 2, game.GraphicsDevice.Viewport.Height - 100);
        spriteBatch.DrawString(font, materialDetails, detailsPosition, Color.DarkCyan);

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
                player.SetMaterial(GetNextMaterial(player.Material)); // Cambiar de material
                break;
            case 2:
                game.Exit(); // Salir del juego
                break;
        }
    }

    public void UnloadContent() {
        // Liberar recursos si es necesario
        backgroundTexture?.Dispose();
    }
}
}
