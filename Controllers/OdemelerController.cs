using EthioAirLines.Data;
using MySql.Data.MySqlClient;
using System;
using System.Web.Mvc;

namespace EthioAirLines.Controllers
{
    public class OdemelerController : Controller
    {
        public ActionResult Create(string rezervasyonId)
        {
            ViewBag.RezervasyonId = rezervasyonId;
            return View();
        }

        [HttpPost]
        public ActionResult Create(string rezervasyonId, decimal tutar, string odemeTipi)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("INSERT INTO ODEMELER (Odeme_ID, Rezervasyon_ID, Tutar, Tarih, Odeme_durumu, Odeme_tipi) VALUES (@Odeme_ID, @Rezervasyon_ID, @Tutar, @Tarih, @Odeme_durumu, @Odeme_tipi)", conn);
                cmd.Parameters.AddWithValue("@Odeme_ID", Guid.NewGuid().ToString());
                cmd.Parameters.AddWithValue("@Rezervasyon_ID", rezervasyonId);
                cmd.Parameters.AddWithValue("@Tutar", tutar);
                cmd.Parameters.AddWithValue("@Tarih", DateTime.Now);
                cmd.Parameters.AddWithValue("@Odeme_durumu", "Odenmis");
                cmd.Parameters.AddWithValue("@Odeme_tipi", odemeTipi);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index", "Ucuslar");
        }
    }
}