using System;

namespace BrudvikStackedChest.Events
{
    public class ContainerAwakePatchEvent : EventArgs
    {
        public Container Container { get; set; }
    }
}
