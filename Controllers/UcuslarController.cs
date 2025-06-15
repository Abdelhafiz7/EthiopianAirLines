using EthioAirLines.Data;
using EthioAirLines.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;

namespace EthioAirLines.Controllers
{
    public class UcuslarController : Controller
    {
        // GET: Ucuslar
        public ActionResult Index(string from, string to)
        {
            List<Ucuslar> flights = new List<Ucuslar>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                StringBuilder queryBuilder = new StringBuilder("SELECT * FROM UCUSLAR WHERE 1=1");

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                if (!string.IsNullOrEmpty(from))
                {
                    queryBuilder.Append(" AND Kalkis_havaalani LIKE @From");
                    cmd.Parameters.AddWithValue("@From", "%" + from + "%");
                }

                if (!string.IsNullOrEmpty(to))
                {
                    queryBuilder.Append(" AND Varis_havaalani LIKE @To");
                    cmd.Parameters.AddWithValue("@To", "%" + to + "%");
                }

                cmd.CommandText = queryBuilder.ToString();

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    flights.Add(new Ucuslar
                    {
                        Ucus_ID = reader["Ucus_ID"].ToString(),
                        Kalkis_havaalani = reader["Kalkis_havaalani"].ToString(),
                        Varis_havaalani = reader["Varis_havaalani"].ToString(),
                        Kalkis_saati = (System.DateTime)reader["Kalkis_saati"],
                        Varis_saati = (System.DateTime)reader["Varis_saati"],
                        Ucak_ID = reader["Ucak_ID"].ToString()
                    });
                }
            }
            return View(flights);
        }

        // GET: Ucuslar/Create
        public ActionResult Create()
        {
            List<Ucaklar> aircrafts = new List<Ucaklar>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM UCAKLAR", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    aircrafts.Add(new Ucaklar
                    {
                        Ucak_ID = reader["Ucak_ID"].ToString(),
                        Model = reader["Model"].ToString(),
                        Kapasite = int.Parse(reader["Kapasite"].ToString())
                    });
                }
            }
            ViewBag.Aircrafts = aircrafts;
            return View();
        }

        // POST: Ucuslar/Create
        [HttpPost]
        public ActionResult Create(string ucusId, string kalkis, string varis, DateTime kalkisSaati, DateTime varisSaati, string ucakId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("INSERT INTO UCUSLAR (Ucus_ID, Kalkis_havaalani, Varis_havaalani, Kalkis_saati, " +
                                           "Varis_saati, Ucak_ID) VALUES (@Ucus_ID, @Kalkis, @Varis, " +
                                           "@Kalkis_saati, @Varis_saati, @Ucak_ID)", conn);
                cmd.Parameters.AddWithValue("@Ucus_ID", ucusId);
                cmd.Parameters.AddWithValue("@Kalkis", kalkis);
                cmd.Parameters.AddWithValue("@Varis", varis);
                cmd.Parameters.AddWithValue("@Kalkis_saati", kalkisSaati);
                cmd.Parameters.AddWithValue("@Varis_saati", varisSaati);
                cmd.Parameters.AddWithValue("@Ucak_ID", ucakId);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // GET: Ucuslar/Edit/5
        public ActionResult Edit(string id)
        {
            Ucuslar flight = new Ucuslar();
            if (!string.IsNullOrEmpty(id))
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM UCUSLAR WHERE Ucus_ID = @ID", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        flight.Ucus_ID = reader["Ucus_ID"].ToString();
                        flight.Kalkis_havaalani = reader["Kalkis_havaalani"].ToString();
                        flight.Varis_havaalani = reader["Varis_havaalani"].ToString();
                        flight.Kalkis_saati = (DateTime)reader["Kalkis_saati"];
                        flight.Varis_saati = (DateTime)reader["Varis_saati"];
                        flight.Ucak_ID = reader["Ucak_ID"].ToString();
                    }
                }

                // Get aircraft list for dropdown
                List<Ucaklar> aircrafts = new List<Ucaklar>();
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM UCAKLAR", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        aircrafts.Add(new Ucaklar
                        {
                            Ucak_ID = reader["Ucak_ID"].ToString(),
                            Model = reader["Model"].ToString(),
                            Kapasite = int.Parse(reader["Kapasite"].ToString())
                        });
                    }
                }
                ViewBag.Aircrafts = aircrafts;
            }
            return View(flight);
        }

        // POST: Ucuslar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Ucuslar model)
        {
            if (!ModelState.IsValid)
            {
                // Reload aircraft list for dropdown
                List<Ucaklar> aircrafts = new List<Ucaklar>();
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM UCAKLAR", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        aircrafts.Add(new Ucaklar
                        {
                            Ucak_ID = reader["Ucak_ID"].ToString(),
                            Model = reader["Model"].ToString(),
                            Kapasite = int.Parse(reader["Kapasite"].ToString())
                        });
                    }
                }
                ViewBag.Aircrafts = aircrafts;
                return View(model);
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("UPDATE UCUSLAR SET Kalkis_havaalani = @Kalkis, Varis_havaalani = @Varis, " +
                                             "Kalkis_saati = @Kalkis_saati, Varis_saati = @Varis_saati, Ucak_ID = @Ucak_ID " +
                                             "WHERE Ucus_ID = @Ucus_ID", conn);
                    cmd.Parameters.AddWithValue("@Kalkis", model.Kalkis_havaalani);
                    cmd.Parameters.AddWithValue("@Varis", model.Varis_havaalani);
                    cmd.Parameters.AddWithValue("@Kalkis_saati", model.Kalkis_saati);
                    cmd.Parameters.AddWithValue("@Varis_saati", model.Varis_saati);
                    cmd.Parameters.AddWithValue("@Ucak_ID", model.Ucak_ID);
                    cmd.Parameters.AddWithValue("@Ucus_ID", model.Ucus_ID);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the flight: " + ex.Message);
                // Reload aircraft list for dropdown
                List<Ucaklar> aircrafts = new List<Ucaklar>();
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM UCAKLAR", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        aircrafts.Add(new Ucaklar
                        {
                            Ucak_ID = reader["Ucak_ID"].ToString(),
                            Model = reader["Model"].ToString(),
                            Kapasite = int.Parse(reader["Kapasite"].ToString())
                        });
                    }
                }
                ViewBag.Aircrafts = aircrafts;
                return View(model);
            }
        }

        // GET: Ucuslar/Delete/5
        public ActionResult Delete(string id)
        {
            Ucuslar flight = new Ucuslar();
            if (!string.IsNullOrEmpty(id))
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT * FROM UCUSLAR WHERE Ucus_ID = @ID", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        flight.Ucus_ID = reader["Ucus_ID"].ToString();
                        flight.Kalkis_havaalani = reader["Kalkis_havaalani"].ToString();
                        flight.Varis_havaalani = reader["Varis_havaalani"].ToString();
                        flight.Kalkis_saati = (DateTime)reader["Kalkis_saati"];
                        flight.Varis_saati = (DateTime)reader["Varis_saati"];
                        flight.Ucak_ID = reader["Ucak_ID"].ToString();
                    }
                }
            }
            return View(flight);
        }

        // POST: Ucuslar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM UCUSLAR WHERE Ucus_ID = @ID", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while deleting the flight. It might have related reservations: " + ex.Message);
                Ucuslar flightToDelete = new Ucuslar();
                if (!string.IsNullOrEmpty(id))
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        var cmd = new MySqlCommand("SELECT * FROM UCUSLAR WHERE Ucus_ID = @ID", conn);
                        cmd.Parameters.AddWithValue("@ID", id);
                        var reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            flightToDelete.Ucus_ID = reader["Ucus_ID"].ToString();
                            flightToDelete.Kalkis_havaalani = reader["Kalkis_havaalani"].ToString();
                            flightToDelete.Varis_havaalani = reader["Varis_havaalani"].ToString();
                            flightToDelete.Kalkis_saati = (DateTime)reader["Kalkis_saati"];
                            flightToDelete.Varis_saati = (DateTime)reader["Varis_saati"];
                            flightToDelete.Ucak_ID = reader["Ucak_ID"].ToString();
                        }
                    }
                }
                return View("Delete", flightToDelete);
            }
        }
    }
}