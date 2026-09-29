using BrudvikStackedChest.Helpers;
using BrudvikStackedChest.Piece;
using BrudvikStackedChest.Utils;
using Jotunn.Entities;
using System;
using System.Collections.Generic;

namespace BrudvikStackedChest.Commands
{
    /// <summary>
    /// Console and chat command that lists how many items of each chest are unlimited.
    /// </summary>
    public class ProgressCommand : ConsoleCommand
    {
        private readonly ChestSupply supply;
        private readonly ChestProgressUi progressUi;
        private readonly Func<IEnumerable<CustomPieceExtended>> pieces;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressCommand"/> class.
        /// </summary>
        /// <param name="supply">Decides which items are unlimited.</param>
        /// <param name="progressUi">Formats the progress of a chest.</param>
        /// <param name="pieces">Provides the chest definitions.</param>
        public ProgressCommand(ChestSupply supply, ChestProgressUi progressUi, Func<IEnumerable<CustomPieceExtended>> pieces)
        {
            this.supply = supply;
            this.progressUi = progressUi;
            this.pieces = pieces;
        }

        /// <inheritdoc/>
        public override string Name => "bsc_progress";

        /// <inheritdoc/>
        public override string Help => Texts.Get("bsc_cmd_help");

        /// <inheritdoc/>
        public override void Run(string[] args)
        {
            Run(args, Console.instance);
        }

        /// <inheritdoc/>
        public override void Run(string[] args, Terminal context)
        {
            if (context == null) return;

            context.AddString(Texts.Get("bsc_cmd_mode", supply.Mode));
            if (!supply.IsReady)
            {
                context.AddString(Texts.Get("bsc_cmd_not_ready"));
                return;
            }

            foreach (var piece in pieces())
            {
                var category = piece.CustomPieceConfig.ItemCategory;
                if (category == Constants.ChestCategory.None) continue;

                var summary = progressUi.GetSummary(category) ?? Texts.Get("bsc_cmd_normal");
                context.AddString($"{Texts.Localize(piece.Tooltip)}: {summary}");
            }
        }
    }
}
