using System;
using System.Collections.Generic;

namespace TaikoLocalServer.Entities
{
    public partial class Card
    {
        public string AccessCode { get; set; } = null!;
        public uint Baid { get; set; }
    }
}
