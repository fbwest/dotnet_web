﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace West.Shared;

public partial class Shipper
{
    [Key]
    public int ShipperId { get; set; }

    [Required]
    [Column(TypeName = "nvarchar (40)")]
	[StringLength(40)]
    public string CompanyName { get; set; } = null!;

    [Column(TypeName = "nvarchar (24)")]
	[StringLength(24)]
    public string? Phone { get; set; }

    [InverseProperty("ShipViaNavigation")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
