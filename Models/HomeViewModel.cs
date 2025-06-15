using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EthioAirLines.Models
{
    public class HomeViewModel
    {
        public List<Ucuslar> Flights { get; set; }
        public List<RezervasyonViewModel> Reservations { get; set; }
        public List<Ucaklar> Aircrafts { get; set; }
    }
}