using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EthioAirLines.Models
{
    public class RezervasyonViewModel
    {
        public string Rezervasyon_ID { get; set; }
        public string Ucus_ID { get; set; }
        public string Yolcu_ID { get; set; }
        public string PassengerName { get; set; }
        public string Koltuk_no { get; set; }
        public DateTime Tarih { get; set; }
        public string Durum { get; set; }

        
    }
}