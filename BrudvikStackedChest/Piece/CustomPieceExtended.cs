using BrudvikStackedChest.Constants;
using BrudvikStackedChest.Extensions;
using BrudvikStackedChest.Piece;
using Jotunn.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// Extends the CustomPiece class to add additional functionality for custom pieces.
    /// </summary>
    public class CustomPieceExtended : CustomPiece
    {
        // Configuration for the custom piece
        public readonly CustomPieceConfigExtended CustomPieceConfig;

        // Indicates whether the piece has been spawned
        public bool Spawned { get; set; } = false;

        // Properties
        public Color Color { get; set; }
        public string Icon { get; set; }
        public string Tooltip { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPieceExtended"/> class.
        /// </summary>
        /// <param name="name">The name of the custom piece.</param>
        /// <param name="prefabName">The name of the prefab associated with the custom piece.</param>
        /// <param name="config">The configuration for the custom piece.</param>
        public CustomPieceExtended(string name, string prefabName, CustomPieceConfigExtended config) : base(name, prefabName, config)
        {
            this.CustomPieceConfig = config;

            // Initialize properties
            this.Color = Color.white; 
            this.Icon = string.Empty; 
            this.Tooltip = string.Empty;
            this.Rows = 8; 
            this.Columns = 8; 
        }

        // Additional methods to apply the properties
        public void ApplyProperties()
        {
            ChangeColor(this.Color);
            AddIconToChest(this.Icon, DefaultValues.ChestIconScale);
            SetTooltip(this.Tooltip);
            SetRowsAndColumns(this.Rows, this.Columns);
        }

        /// <summary>
        /// Adds an icon to the front of the custom piece.
        /// </summary>
        /// <param name="iconSprite">The sprite to be used as the icon.</param>
        /// <param name="scale">The scale of the icon.</param>
        private void AddIconToFront(Sprite iconSprite, Vector3 scale)
        {
            // Create a new GameObject for the icon and set it as a child of the current GameObject
            GameObject iconObject = new GameObject("Icon", typeof(SpriteRenderer))
            {
                transform =
                {
                    parent = this.Piece.gameObject.transform,
                    localPosition = new Vector3(0.20f, 0.5f, 0.48f),
                    localScale = scale
                }
            };

            // Set the sprite to the provided icon sprite
            iconObject.GetComponent<SpriteRenderer>().sprite = iconSprite;
        }

        /// <summary>
        /// Sets the number of rows and columns for the container component of the piece.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        public void SetRowsAndColumns(int rows, int columns)
        {
            // Access the Container component and set the number of columns and rows if it exists
            if (this.Piece.TryGetComponent<Container>(out var container))
            {
                container.m_width = columns;
                container.m_height = rows;
            }
        }

        /// <summary>
        /// Adds an icon to the chest with the specified path and scale.
        /// </summary>
        /// <param name="iconPath">The path to the icon.</param>
        /// <param name="scale">The scale of the icon.</param>
        public void AddIconToChest(string iconPath, Vector3 scale)
        {
            // Prepend the icon path with the PluginName if it is set
            string iconPathWithPlugin = this.CustomPieceConfig.PluginName != null
                ? $"{CustomPieceConfig.PluginName}.Assets.{iconPath}"
                : iconPath;

            // Load the icon sprite from the specified path and add it to the front of the chest
            AddIconToFront(
                AssetUtilsExtended.LoadTextureFromEmbeddedResource(iconPathWithPlugin).ConvertToSprite(),
                scale
            );
        }

        /// <summary>
        /// Sets the tooltip text for the custom piece.
        /// </summary>
        /// <param name="text">The tooltip text.</param>
        public void SetTooltip(string text)
        {
            // Access the Container component and set the name.
            if (this.Piece.TryGetComponent<Container>(out var container))
            {
                container.m_name = text;
            }

            // Check if HoverText component already exists, if not, add it
            var hoverTextComponent = this.Piece.gameObject.GetComponent<HoverText>() ?? this.Piece.gameObject.AddComponent<HoverText>();
           
            // Set the hover text
            hoverTextComponent.m_text = text;
        }

        /// <summary>
        /// Changes the color of the custom piece.
        /// </summary>
        /// <param name="color">The new color.</param>
        public void ChangeColor(Color color)
        {
            // Get all renderers in the current GameObject
            var renderers = this.Piece.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    // Create a new material with the specified color
                    var newMaterial = new Material(renderer.material)
                    {
                        color = color
                    };
                    // Assign the new material to the renderer
                    renderer.material = newMaterial;
                }
            }
        }
    }
}