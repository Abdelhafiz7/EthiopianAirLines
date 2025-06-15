using EthioAirLines.Data;
using EthioAirLines.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EthioAirLines.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var viewModel = new HomeViewModel();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Fetch Flights
                viewModel.Flights = new List<Ucuslar>();
                var cmdFlights = new MySqlCommand("SELECT * FROM UCUSLAR", conn);
                var readerFlights = cmdFlights.ExecuteReader();
                while (readerFlights.Read())
                {
                    viewModel.Flights.Add(new Ucuslar
                    {
                        Ucus_ID = readerFlights["Ucus_ID"].ToString(),
                        Kalkis_havaalani = readerFlights["Kalkis_havaalani"].ToString(),
                        Varis_havaalani = readerFlights["Varis_havaalani"].ToString(),
                        Kalkis_saati = (System.DateTime)readerFlights["Kalkis_saati"],
                        Varis_saati = (System.DateTime)readerFlights["Varis_saati"],
                        Ucak_ID = readerFlights["Ucak_ID"].ToString()
                    });
                }
                readerFlights.Close(); // Close reader before new command

                // Fetch Reservations (with Passenger Name)
                viewModel.Reservations = new List<RezervasyonViewModel>();
                var cmdReservations = new MySqlCommand(@"
                    SELECT r.Rezervasyon_ID, r.Ucus_ID, r.Yolcu_ID, y.Adi, y.Soyad, r.Koltuk_no, r.Tarih, r.Durum
                    FROM REZERVASYONLAR r
                    JOIN YOLCULAR y ON r.Yolcu_ID = y.Yolcu_ID
                ", conn);
                var readerReservations = cmdReservations.ExecuteReader();
                while (readerReservations.Read())
                {
                    viewModel.Reservations.Add(new RezervasyonViewModel
                    {
                        Rezervasyon_ID = readerReservations["Rezervasyon_ID"].ToString(),
                        Ucus_ID = readerReservations["Ucus_ID"].ToString(),
                        Yolcu_ID = readerReservations["Yolcu_ID"].ToString(),
                        PassengerName = readerReservations["Adi"].ToString() + " " + readerReservations["Soyad"].ToString(),
                        Koltuk_no = readerReservations["Koltuk_no"].ToString(),
                        Tarih = (DateTime)readerReservations["Tarih"],
                        Durum = readerReservations["Durum"].ToString()
                    });
                }
                readerReservations.Close(); // Close reader before new command

                // Fetch Aircrafts
                viewModel.Aircrafts = new List<Ucaklar>();
                var cmdAircrafts = new MySqlCommand("SELECT * FROM UCAKLAR", conn);
                var readerAircrafts = cmdAircrafts.ExecuteReader();
                while (readerAircrafts.Read())
                {
                    viewModel.Aircrafts.Add(new Ucaklar
                    {
                        Ucak_ID = readerAircrafts["Ucak_ID"].ToString(),
                        Model = readerAircrafts["Model"].ToString(),
                        Kapasite = int.Parse(readerAircrafts["Kapasite"].ToString())
                    });
                }
                readerAircrafts.Close();
            }

            return View(viewModel);
        }

        // About and Contact actions (keep if desired, otherwise remove)
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}