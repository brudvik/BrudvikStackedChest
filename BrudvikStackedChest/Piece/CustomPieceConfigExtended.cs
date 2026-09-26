using BrudvikStackedChest.Constants;
using Jotunn.Configs;

namespace BrudvikStackedChest.Piece
{
    public class CustomPieceConfigExtended : PieceConfig
    {
        public string? PluginName { get; set; }
        public ChestCategory ItemCategory { get; set; } = ChestCategory.None;
    }
}
