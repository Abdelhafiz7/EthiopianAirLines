using EthioAirLines.Data;
using EthioAirLines.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 
using System.Diagnostics; 
using System.Linq;
using System.Web.Mvc;

namespace EthioAirLines.Controllers
{
    public class UcaklarController : Controller
    {
        public ActionResult Index()
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
                        Ucak_ID = reader["Ucak_ID"]?.ToString(),
                        Model = reader["Model"]?.ToString(),
                        Kapasite = reader["Kapasite"] != DBNull.Value ? Convert.ToInt32(reader["Kapasite"]) : 0
                    });
                }
            }
            return View(aircrafts);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ucaklar model) 
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("INSERT INTO UCAKLAR (Ucak_ID, Model, Kapasite) VALUES (@Ucak_ID, @Model, @Kapasite)", conn);
                    cmd.Parameters.AddWithValue("@Ucak_ID", model.Ucak_ID);
                    cmd.Parameters.AddWithValue("@Model", model.Model);
                    cmd.Parameters.AddWithValue("@Kapasite", model.Kapasite);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    ModelState.AddModelError("Ucak_ID", "An aircraft with this ID already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while adding the aircraft: " + ex.Message);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                return View(model);
            }
        }

        public ActionResult Edit(string id)
        {
            Ucaklar aircraft = new Ucaklar(); 

            Debug.WriteLine($"[UcaklarController.Edit GET] Attempting to load ID: {id}");

            if (!string.IsNullOrEmpty(id))
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT Ucak_ID, Model, Kapasite FROM UCAKLAR WHERE Ucak_ID = @ID", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        aircraft.Ucak_ID = reader["Ucak_ID"]?.ToString();
                        aircraft.Model = reader["Model"]?.ToString();
                        aircraft.Kapasite = reader["Kapasite"] != DBNull.Value ? Convert.ToInt32(reader["Kapasite"]) : 0;
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Aircraft with ID '{id}' not found.");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "No Aircraft ID provided for editing.");
                Debug.WriteLine("[UcaklarController.Edit GET] No ID provided.");
            }

            return View(aircraft);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Ucaklar model)
        {
            Debug.WriteLine($"[UcaklarController.Edit POST] Received model.Ucak_ID: {model?.Ucak_ID ?? "NULL"}, Model: {model?.Model ?? "NULL"}, Kapasite: {model?.Kapasite.ToString() ?? "NULL"}");

            if (!ModelState.IsValid)
            {
                Debug.WriteLine("[UcaklarController.Edit POST] ModelState is NOT valid.");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        Debug.WriteLine($"  Error for {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                return View(model);
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM UCAKLAR WHERE Model = @Model AND Ucak_ID != @Ucak_ID", conn);
                    checkCmd.Parameters.AddWithValue("@Model", model.Model);
                    checkCmd.Parameters.AddWithValue("@Ucak_ID", model.Ucak_ID);
                    int duplicateCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (duplicateCount > 0)
                    {
                        ModelState.AddModelError("Model", "This model already exists for another aircraft.");
                        return View(model);
                    }

                    var cmd = new MySqlCommand("UPDATE UCAKLAR SET Model = @Model, Kapasite = @Kapasite WHERE Ucak_ID = @Ucak_ID", conn);
                    cmd.Parameters.AddWithValue("@Model", model.Model);
                    cmd.Parameters.AddWithValue("@Kapasite", model.Kapasite);
                    cmd.Parameters.AddWithValue("@Ucak_ID", model.Ucak_ID);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    Debug.WriteLine($"[UcaklarController.Edit POST] Rows affected: {rowsAffected}");

                    if (rowsAffected == 0)
                    {
                        ModelState.AddModelError("", "Aircraft not found or no changes were made. Please verify the ID.");
                        return View(model);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"[UcaklarController.Edit POST] MySQL Error: {ex.Message}");
                ModelState.AddModelError("", "A database error occurred: " + ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UcaklarController.Edit POST] General Error: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
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
                    else
                    {
                        ModelState.AddModelError("", $"Flight with ID '{id}' not found for deletion.");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "No Flight ID provided for deletion.");
            }
            return View(flight);
        }

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
            catch (MySqlException ex)
            {
                ModelState.AddModelError("", "An error occurred while deleting the flight. It might have related records: " + ex.Message);
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
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred while deleting the flight: " + ex.Message);
                return View("Delete", new Ucuslar { Ucus_ID = id });
            }
        }
    }
}