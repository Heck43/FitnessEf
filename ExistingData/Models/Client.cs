using System;
using System.Collections.Generic;

namespace FitnessEf.ExistingData.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Status { get; set; } = null!;
}
