using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class User
{
    public string Userid { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();

    public virtual ICollection<Chargingstation> Chargingstations { get; set; } = new List<Chargingstation>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
