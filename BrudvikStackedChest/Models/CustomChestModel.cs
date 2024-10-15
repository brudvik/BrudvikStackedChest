using System.Collections.Generic;
using UnityEngine;

namespace BrudvikStackedChest.Models
{
    public class CustomChestModel
    {

        public string Name { get; set; } = "BrudvikStackedChest";
        public string Description { get; set; } = "BrudvikStackedChestDesc";
        public string Icon { get; set; } = "strg_049_round.png";
        public List<string> SpawnItems { get; set; } = new List<string>();
        public Color Color { get; set; } = new Color(0, 0, 0, 0.8f);
    }
}
