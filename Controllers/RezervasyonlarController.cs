using EthioAirLines.Data;
using EthioAirLines.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc; // Important: Keep this for ASP.NET MVC 5

namespace EthioAirLines.Controllers
{
    public class RezervasyonlarController : Controller
    {
        // GET: Rezervasyonlar/Create
        public ActionResult Create(string ucusId)
        {
            if (string.IsNullOrEmpty(ucusId))
            {
                return RedirectToAction("Index", "Ucuslar");
            }

            // Get flight details
            Ucuslar flight = new Ucuslar();
            int aircraftCapacity = 0;
            List<int> occupiedSeats = new List<int>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Get flight and aircraft details
                var cmd = new MySqlCommand(@"
            SELECT u.*, uc.Kapasite 
            FROM UCUSLAR u 
            JOIN UCAKLAR uc ON u.Ucak_ID = uc.Ucak_ID 
            WHERE u.Ucus_ID = @UcusID", conn);
                cmd.Parameters.AddWithValue("@UcusID", ucusId);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    flight.Ucus_ID = reader["Ucus_ID"].ToString();
                    flight.Kalkis_havaalani = reader["Kalkis_havaalani"].ToString();
                    flight.Varis_havaalani = reader["Varis_havaalani"].ToString();
                    flight.Kalkis_saati = (DateTime)reader["Kalkis_saati"];
                    flight.Varis_saati = (DateTime)reader["Varis_saati"];
                    flight.Ucak_ID = reader["Ucak_ID"].ToString();
                    aircraftCapacity = Convert.ToInt32(reader["Kapasite"]);
                }
                reader.Close();

                // Get occupied seats
                cmd = new MySqlCommand("SELECT Koltuk_No FROM REZERVASYONLAR WHERE Ucus_ID = @UcusID", conn);
                cmd.Parameters.AddWithValue("@UcusID", ucusId);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    occupiedSeats.Add(Convert.ToInt32(reader["Koltuk_No"]));
                }
            }

            // Create list of available seats
            var availableSeats = Enumerable.Range(1, aircraftCapacity)
                .Where(seat => !occupiedSeats.Contains(seat))
                .ToList();

            ViewBag.AvailableSeats = new SelectList(availableSeats);
            ViewBag.Flight = flight;
            ViewBag.IsFull = !availableSeats.Any();

            return View(new Rezervasyonlar { Ucus_ID = ucusId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Rezervasyonlar rezervasyon, string yolcuAdi, string yolcuSoyad, string email, string telefon)
        {
            Ucuslar currentFlight = new Ucuslar();
            int currentAircraftCapacity = 0;
            List<int> currentOccupiedSeats = new List<int>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var flightCmd = new MySqlCommand(@"
                    SELECT u.*, uc.Kapasite 
                    FROM UCUSLAR u 
                    JOIN UCAKLAR uc ON u.Ucak_ID = uc.Ucak_ID 
                    WHERE u.Ucus_ID = @UcusID", conn);
                flightCmd.Parameters.AddWithValue("@UcusID", rezervasyon.Ucus_ID);
                var flightReader = flightCmd.ExecuteReader();
                if (flightReader.Read())
                {
                    currentFlight.Ucus_ID = flightReader["Ucus_ID"].ToString();
                    currentFlight.Kalkis_havaalani = flightReader["Kalkis_havaalani"].ToString();
                    currentFlight.Varis_havaalani = flightReader["Varis_havaalani"].ToString();
                    currentFlight.Kalkis_saati = (DateTime)flightReader["Kalkis_saati"];
                    currentFlight.Varis_saati = (DateTime)flightReader["Varis_saati"];
                    currentFlight.Ucak_ID = flightReader["Ucak_ID"].ToString();
                    currentAircraftCapacity = Convert.ToInt32(flightReader["Kapasite"]);
                }
                flightReader.Close();

                var occupiedSeatsCmd = new MySqlCommand("SELECT Koltuk_No FROM REZERVASYONLAR WHERE Ucus_ID = @UcusID", conn);
                occupiedSeatsCmd.Parameters.AddWithValue("@UcusID", rezervasyon.Ucus_ID);
                var occupiedSeatsReader = occupiedSeatsCmd.ExecuteReader();
                while (occupiedSeatsReader.Read())
                {
                    currentOccupiedSeats.Add(Convert.ToInt32(occupiedSeatsReader["Koltuk_No"]));
                }
            }

            var currentAvailableSeats = Enumerable.Range(1, currentAircraftCapacity)
                .Where(seat => !currentOccupiedSeats.Contains(seat))
                .ToList();

            ViewBag.AvailableSeats = new SelectList(currentAvailableSeats);
            ViewBag.Flight = currentFlight;
            ViewBag.IsFull = !currentAvailableSeats.Any();


            string yolcuId = "";
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // 1. Try to find existing passenger by Email
                    var findPassengerCmd = new MySqlCommand("SELECT Yolcu_ID FROM YOLCULAR WHERE Email = @Email", conn);
                    findPassengerCmd.Parameters.AddWithValue("@Email", email);
                    object existingYolcuId = findPassengerCmd.ExecuteScalar();

                    if (existingYolcuId != null)
                    {
                        yolcuId = existingYolcuId.ToString();
                        Debug.WriteLine($"[RezervasyonlarController.Create POST] Found existing passenger with ID: {yolcuId}");
                    }
                    else
                    {
                        string newYolcuId = Guid.NewGuid().ToString();
                        var insertPassengerCmd = new MySqlCommand("INSERT INTO YOLCULAR (Yolcu_ID, Adi, Soyad, Email, Telefon) VALUES (@Yolcu_ID, @Adi, @Soyad, @Email, @Telefon)", conn);
                        insertPassengerCmd.Parameters.AddWithValue("@Yolcu_ID", newYolcuId);
                        insertPassengerCmd.Parameters.AddWithValue("@Adi", yolcuAdi);
                        insertPassengerCmd.Parameters.AddWithValue("@Soyad", yolcuSoyad);
                        insertPassengerCmd.Parameters.AddWithValue("@Email", email);
                        insertPassengerCmd.Parameters.AddWithValue("@Telefon", telefon);
                        insertPassengerCmd.ExecuteNonQuery();
                        yolcuId = newYolcuId;
                        Debug.WriteLine($"[RezervasyonlarController.Create POST] Created new passenger with ID: {yolcuId}");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[RezervasyonlarController.Create POST] MySQL Error in passenger handling: {ex.Message}");
                if (ex.Number == 1062) // Duplicate entry
                {
                    ModelState.AddModelError("email", "This email is already registered. If you are an existing passenger, please ensure correct details.");
                }
                else
                {
                    ModelState.AddModelError("", "A database error occurred while processing passenger details: " + ex.Message);
                }
                return View(rezervasyon);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RezervasyonlarController.Create POST] General Error in passenger handling: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred while processing passenger details: " + ex.Message);
                return View(rezervasyon);
            }

            rezervasyon.Yolcu_ID = yolcuId;

            if (ModelState.IsValid)
            {
                try
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();

                        // Check if seat is still available (re-check in case another user booked it simultaneously)
                        var checkCmd = new MySqlCommand(
                            "SELECT COUNT(*) FROM REZERVASYONLAR WHERE Ucus_ID = @UcusID AND Koltuk_No = @KoltukNo",
                            conn);
                        checkCmd.Parameters.AddWithValue("@UcusID", rezervasyon.Ucus_ID);
                        checkCmd.Parameters.AddWithValue("@KoltukNo", rezervasyon.Koltuk_no);

                        int existingReservations = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (existingReservations > 0)
                        {
                            ModelState.AddModelError("Koltuk_No", "This seat is no longer available. Please select another seat.");
                            // Re-populate ViewBag data here as well for the re-rendered view
                            return View(rezervasyon);
                        }

                        // Insert new reservation - FIXED: Added Rezervasyon_ID to columns and @RezervasyonID parameter
                        string newRezervasyonId = Guid.NewGuid().ToString(); // Generate GUID for reservation ID
                        var cmd = new MySqlCommand(@"
                            INSERT INTO REZERVASYONLAR (Rezervasyon_ID, Ucus_ID, Yolcu_ID, Koltuk_No, Tarih, Durum)
                            VALUES (@RezervasyonID, @UcusID, @YolcuID, @KoltukNo, @Tarih, @Durum)", conn);

                        cmd.Parameters.AddWithValue("@RezervasyonID", newRezervasyonId); // Add parameter for Rezervasyon_ID
                        cmd.Parameters.AddWithValue("@UcusID", rezervasyon.Ucus_ID);
                        cmd.Parameters.AddWithValue("@YolcuID", rezervasyon.Yolcu_ID); // Use the resolved Yolcu_ID
                        cmd.Parameters.AddWithValue("@KoltukNo", rezervasyon.Koltuk_no);
                        cmd.Parameters.AddWithValue("@Tarih", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Durum", "Pasif"); // Set to "Pasif" initially

                        cmd.ExecuteNonQuery();
                    }
                    return RedirectToAction("List"); // Redirect to the list of reservations
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the reservation: " + ex.Message);
                    // Re-populate ViewBag data here as well for the re-rendered view
                    return View(rezervasyon);
                }
            }
            // If ModelState is not valid from the start, return view with errors
            return View(rezervasyon);
        }

        public ActionResult Cancel(string rezervasyonId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("UPDATE REZERVASYONLAR SET Durum = 'Iptal' WHERE Rezervasyon_ID = @Rezervasyon_ID", conn);
                cmd.Parameters.AddWithValue("@Rezervasyon_ID", rezervasyonId);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("List");
        }

        public ActionResult List(string flightId, string passengerName, string status)
        {
            // Pass current filter values back to the view
            ViewBag.CurrentFlightId = flightId;
            ViewBag.CurrentPassengerName = passengerName;
            ViewBag.CurrentStatus = status;

            // Prepare status options for dropdown
            var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Statuses" },
                new SelectListItem { Value = "Aktif", Text = "Aktif" },
                new SelectListItem { Value = "Pasif", Text = "Pasif" },
                new SelectListItem { Value = "Iptal", Text = "Iptal" }
            };
            ViewBag.StatusOptions = new SelectList(statusOptions, "Value", "Text", status);

            var rezervasyonlar = new List<RezervasyonViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var sql = new System.Text.StringBuilder(@"
                    SELECT r.Rezervasyon_ID, r.Ucus_ID, r.Yolcu_ID, y.Adi, y.Soyad, r.Koltuk_no, r.Tarih, r.Durum
                    FROM REZERVASYONLAR r
                    JOIN YOLCULAR y ON r.Yolcu_ID = y.Yolcu_ID
                    WHERE 1=1"); // Start with a true condition to easily append AND clauses

                var cmd = new MySqlCommand();
                cmd.Connection = conn;

                if (!string.IsNullOrEmpty(flightId))
                {
                    sql.Append(" AND r.Ucus_ID LIKE @UcusID");
                    cmd.Parameters.AddWithValue("@UcusID", "%" + flightId + "%");
                }
                if (!string.IsNullOrEmpty(passengerName))
                {
                    // Search in both Adi and Soyad
                    sql.Append(" AND (y.Adi LIKE @PassengerName OR y.Soyad LIKE @PassengerName)");
                    cmd.Parameters.AddWithValue("@PassengerName", "%" + passengerName + "%");
                }
                if (!string.IsNullOrEmpty(status))
                {
                    sql.Append(" AND r.Durum = @Durum");
                    cmd.Parameters.AddWithValue("@Durum", status);
                }

                sql.Append(" ORDER BY r.Tarih DESC");
                cmd.CommandText = sql.ToString();

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rezervasyonlar.Add(new RezervasyonViewModel
                    {
                        Rezervasyon_ID = reader["Rezervasyon_ID"]?.ToString(),
                        Ucus_ID = reader["Ucus_ID"]?.ToString(),
                        Yolcu_ID = reader["Yolcu_ID"]?.ToString(),
                        PassengerName = $"{reader["Adi"]?.ToString()} {reader["Soyad"]?.ToString()}",
                        Koltuk_no = reader["Koltuk_no"]?.ToString(),
                        Tarih = (DateTime)reader["Tarih"],
                        Durum = reader["Durum"]?.ToString()
                    });
                }
            }
            return View(rezervasyonlar);
        }
    }
}