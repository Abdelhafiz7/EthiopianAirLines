using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EthioAirLines.Models
{
    public class Odemeler
    {
        public string Odeme_ID { get; set; }
        public string Rezervasyon_ID { get; set; }
        public decimal Tutar { get; set; }
        public DateTime Tarih { get; set; }
        public string Odeme_durumu { get; set; }
        public string Odeme_tipi { get; set; }
    }
}