using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Billing
{
    public int Billingid { get; set; }

    public string? Userid { get; set; }

    public int? Stationid { get; set; }

    public DateTime Billingdate { get; set; }

    public decimal Totalamount { get; set; }

    public string Paymentstatus { get; set; } = null!;

    public virtual Chargingstation? Station { get; set; }

    public virtual User? User { get; set; }
}
