﻿using System.ComponentModel.DataAnnotations;

namespace DF.Telegram.IP
{
    public class CreateUpdateDynamicIPDto
    {
        [Required]
        [StringLength(15)]
        public string? IP { get; set; }
        [Required]
        [StringLength(5)]
        public string? Port { get; set; }
    }
}
