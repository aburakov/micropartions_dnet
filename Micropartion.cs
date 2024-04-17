using System;
using System.Collections.Generic;

namespace Micropartions;

public partial class Micropartion
{
    public string Micropartionguid { get; set; } = null!;

    public string? Boxserial { get; set; }

    public string Skuserial { get; set; } = null!;

    public string Operationguid { get; set; } = null!;

    public long Operationnumber { get; set; }
}
