#region File Description

//-----------------------------------------------------------------------------
// SpaceShipPrimitive.cs
//
// Custom geometric primitive for drawing a simple spaceship model.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion Using Statements

namespace TGC.MonoGame.TP.Geometries
{
    /// <summary>
    ///     Geometric primitive class for drawing a simple spaceship.
    /// </summary>
    public class SpaceShipPrimitive : GeometricPrimitive
    {
        public SpaceShipPrimitive(GraphicsDevice graphicsDevice) : this(graphicsDevice, Color.White, Color.White, Color.White)
        {
        }

        public SpaceShipPrimitive(GraphicsDevice graphicsDevice, Color color1, Color color2, Color color3)
        {
            // Define the vertices for a simple triangular spaceship
            // Front face (triangle)
            Vector3 frontVertex1 = new Vector3(0, 1, 0);
            Vector3 frontVertex2 = new Vector3(-1, -1, 1);
            Vector3 frontVertex3 = new Vector3(1, -1, 1);

            // Back face (triangle)
            Vector3 backVertex1 = new Vector3(0, 1, -2);
            Vector3 backVertex2 = new Vector3(-1, -1, -2);
            Vector3 backVertex3 = new Vector3(1, -1, -2);

            // Define the normals for each face
            Vector3 frontNormal = Vector3.UnitZ;
            Vector3 backNormal = -Vector3.UnitZ;
            Vector3 topNormal = Vector3.UnitY;

            // Front face
            AddVertex(frontVertex1, color1, frontNormal);
            AddVertex(frontVertex2, color2, frontNormal);
            AddVertex(frontVertex3, color3, frontNormal);
            AddIndex(CurrentVertex(false) + 0);
            AddIndex(CurrentVertex(false) + 1);
            AddIndex(CurrentVertex(false) + 2);

            // Back face
            AddVertex(backVertex1, color1, backNormal);
            AddVertex(backVertex2, color2, backNormal);
            AddVertex(backVertex3, color3, backNormal);
            AddIndex(CurrentVertex(false) + 0);
            AddIndex(CurrentVertex(false) + 1);
            AddIndex(CurrentVertex(false) + 2);

            // Sides (connecting front to back)
            AddVertex(frontVertex1, color1, topNormal);
            AddVertex(frontVertex2, color2, topNormal);
            AddVertex(backVertex2, color3, topNormal);
            AddIndex(CurrentVertex(false) + 0);
            AddIndex(CurrentVertex(false) + 1);
            AddIndex(CurrentVertex(false) + 2);

            AddVertex(frontVertex1, color1, topNormal);
            AddVertex(backVertex2, color2, topNormal);
            AddVertex(backVertex1, color3, topNormal);
            AddIndex(CurrentVertex(false) + 0);
            AddIndex(CurrentVertex(false) + 1);
            AddIndex(CurrentVertex(false) + 2);

            AddVertex(frontVertex3, color1, topNormal);
            AddVertex(frontVertex2, color2, topNormal);
            AddVertex(backVertex2, color3, topNormal);
            AddIndex(CurrentVertex(false) + 0);
            AddIndex(CurrentVertex(false) + 1);
            AddIndex(CurrentVertex(false) + 2);

            AddVertex(frontVertex3, color1, topNormal);
            AddVertex(backVertex2, color2, topNormal);
            AddVertex(backVertex3, color3, topNormal);
            AddIndex(CurrentVertex(false) + 0);
            AddIndex(CurrentVertex(false) + 1);
            AddIndex(CurrentVertex(false) + 2);

            InitializePrimitive(graphicsDevice);
        }
    }
}
