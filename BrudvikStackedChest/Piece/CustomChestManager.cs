using BrudvikStackedChest.Helpers;
using BrudvikStackedChest.Models;
using Jotunn.Configs;
using Jotunn.Managers;

namespace BrudvikStackedChest.Piece
{
    /// <summary>
    /// Manages the creation and configuration of custom chests.
    /// </summary>
    public class CustomChestManager
    {
        // Reference to the PieceManager for adding custom pieces
        private readonly PieceManager pieceManager;

        // Reference to the SpriteLoader for loading icons
        private readonly SpriteLoader spriteLoader;

        // The name of the plugin, used for resource identification
        private readonly string pluginName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomChestManager"/> class.
        /// </summary>
        /// <param name="pieceManager">The PieceManager for adding custom pieces.</param>
        /// <param name="spriteLoader">The SpriteLoader for loading icons.</param>
        /// <param name="pluginName">The name of the plugin, used for resource identification.</param>
        public CustomChestManager(PieceManager pieceManager, SpriteLoader spriteLoader, string pluginName)
        {
            this.pieceManager = pieceManager;
            this.spriteLoader = spriteLoader;
            this.pluginName = pluginName;
        }

        /// <summary>
        /// Adds a custom chest piece based on the provided model.
        /// </summary>
        /// <param name="model">The model containing the configuration for the custom chest.</param>
        /// <returns>The created <see cref="CustomPieceExtended"/> object.</returns>
        public CustomPieceExtended AddCustomChest(CustomChestModel model)
        {
            // Create a new configuration for the custom chest piece
            var pieceConfig = new CustomPieceConfigExtended
            {
                Name = model.Name, 
                Description = model.Description, 
                PieceTable = "Hammer", 
                Category = "Chests", 
                PluginName = pluginName, 
                Icon = spriteLoader.Load(model.Icon), 
                Requirements = new[]
                {
                    new RequirementConfig
                    {
                        Item = "Wood", 
                        Amount = 10, 
                        Recover = true 
                    }
                },
                SpawnItems = model.SpawnItems 
            };

            //// Create a new custom piece with the specified name and base piece
            var piece = new CustomPieceExtended(model.Name, "piece_chest", pieceConfig)
            {
                Color = model.Color,
                Icon = model.Icon,
                Tooltip = model.Description,
                Rows = 8,
                Columns = 8
            };

            // Apply the properties to the custom piece
            piece.ApplyProperties();

            // Add the custom piece to the PieceManager
            pieceManager.AddPiece(piece);

            // Log the initiation of the custom chest piece
            Jotunn.Logger.LogInfo($"{model.Name} chest piece is initiated");
            return piece; 
        }
    }
}
