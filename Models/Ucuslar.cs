using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EthioAirLines.Models
{
    public class Ucuslar
    {
        public string Ucus_ID { get; set; }
        public string Kalkis_havaalani { get; set; }
        public string Varis_havaalani { get; set; }
        public DateTime Kalkis_saati { get; set; }
        public DateTime Varis_saati { get; set; }
        public string Ucak_ID { get; set; }
    }
}