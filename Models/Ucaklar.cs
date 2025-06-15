using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EthioAirLines.Models
{
    public class Ucaklar
    {
        [Key]
        public string Ucak_ID { get; set; }
        public string Model { get; set; }
        public int Kapasite { get; set; }

    }
}