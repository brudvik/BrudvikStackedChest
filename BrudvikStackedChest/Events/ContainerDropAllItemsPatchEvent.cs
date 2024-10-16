using System;

namespace BrudvikStackedChest.Events
{
    public class ContainerDropAllItemsPatchEvent : EventArgs
    {
        public Container Container { get; set; }
    }
}
