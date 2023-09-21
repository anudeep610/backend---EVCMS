using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Reservation
{
    public int Reservationid { get; set; }

    public string? Userid { get; set; }

    public int? Stationid { get; set; }

    public DateTime Starttime { get; set; }

    public DateTime Endtime { get; set; }

    public virtual Chargingstation? Station { get; set; }

    public virtual User? User { get; set; }
}
