using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Chargingstation
{
    public int Stationid { get; set; }

    public string? Userid { get; set; }

    public string Location { get; set; } = null!;

    public int Ports { get; set; }

    public decimal Chargingrate { get; set; }

    public string Availability { get; set; } = null!;

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual User? User { get; set; }
}
