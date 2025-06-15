using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EthioAirLines.Models
{
    public class Rezervasyonlar
    {
        public string Rezervasyon_ID { get; set; }
        public string Yolcu_ID { get; set; }
        public string Ucus_ID { get; set; }
        public string Koltuk_no { get; set; }
        public DateTime Tarih { get; set; }
        public string Durum { get; set; }

        public virtual Ucuslar Ucus { get; set; }
        public virtual Yolcular Yolcu { get; set; }
    }
}