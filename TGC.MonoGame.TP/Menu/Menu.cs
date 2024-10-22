using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;

namespace TGC.MonoGame.TP.MenuSpace
{
    public class Menu {
        private TGCGame game;
        private SpriteFont font;
        private string[] menuItems = { "Iniciar Juego", "Salir" };
        private int selectedIndex = 0;
        public bool IsActive { get; private set; }

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
        }

        public void Update(GameTime gameTime) {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Down)) {
                selectedIndex = (selectedIndex + 1) % menuItems.Length;
            }
            if (keyboardState.IsKeyDown(Keys.Up)) {
                selectedIndex = (selectedIndex - 1 + menuItems.Length) % menuItems.Length;
            }

            if (keyboardState.IsKeyDown(Keys.Enter)) {
                Select();
            }
        }

        public void Draw(GameTime gameTime) {
            var spriteBatch = new SpriteBatch(game.GraphicsDevice);
            spriteBatch.Begin();

            for (int i = 0; i < menuItems.Length; i++) {
                var color = (i == selectedIndex) ? Color.Yellow : Color.White; // Resaltar la opción seleccionada
                spriteBatch.DrawString(font, menuItems[i], new Vector2(100, 100 + i * 50), color);
            }

            spriteBatch.End();
        }

        private void Select() {
            switch (selectedIndex) {
                case 0:
                    IsActive = false; // Desactiva el menú para comenzar el juego
                    break;
                case 1:
                    game.Exit(); // Salir del juego
                    break;
            }
        }

        public void UnloadContent() {
            // Liberar recursos si es necesario
        }
    }
}
