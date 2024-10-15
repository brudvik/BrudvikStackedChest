using Jotunn.Configs;
using System.Collections.Generic;

namespace BrudvikStackedChest.Piece
{
    public class CustomPieceConfigExtended : PieceConfig
    {
        public string? PluginName { get; set; }
        public List<string> SpawnItems { get; set; } = new List<string>();
    }
}
