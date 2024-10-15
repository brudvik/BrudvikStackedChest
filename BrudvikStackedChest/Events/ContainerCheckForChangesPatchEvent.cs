using System;

namespace BrudvikStackedChest.Events
{
    public class ContainerCheckForChangesPatchEvent : EventArgs
    {
        public Container Container { get; set; }
    }
}
