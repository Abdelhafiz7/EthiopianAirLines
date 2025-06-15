using EthioAirLines.Data;
using EthioAirLines.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EthioAirLines.Controllers
{
    public class YolcularController : Controller
    {
        // GET: Yolcular (List all passengers)
        public ActionResult Index()
        {
            List<Yolcular> passengers = new List<Yolcular>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM YOLCULAR", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    passengers.Add(new Yolcular
                    {
                        Yolcu_ID = reader["Yolcu_ID"].ToString(),
                        Adi = reader["Adi"].ToString(),
                        Soyad = reader["Soyad"].ToString(),
                        Email = reader["Email"].ToString(),
                        Telefon = reader["Telefon"].ToString()
                    });
                }
            }
            return View(passengers);
        }

        // GET: Yolcular/Create (Show form to add new passenger)
        public ActionResult Create()
        {
            return View();
        }

        // POST: Yolcular/Create (Handle form submission for new passenger)
        [HttpPost]
        [ValidateAntiForgeryToken] // Added for security
        public ActionResult Create(string yolcuId, string adi, string soyad, string email, string telefon)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    // Call the AddPassenger stored procedure
                    var cmd = new MySqlCommand("AddPassenger", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure; // Specify it's a stored procedure

                    cmd.Parameters.AddWithValue("p_Yolcu_ID", yolcuId);
                    cmd.Parameters.AddWithValue("p_Adi", adi);
                    cmd.Parameters.AddWithValue("p_Soyad", soyad);
                    cmd.Parameters.AddWithValue("p_Email", email);
                    cmd.Parameters.AddWithValue("p_Telefon", telefon);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index"); // Redirect to passenger list after creation
            }
            catch (MySqlException ex)
            {
                // Handle duplicate entry for Email or ID
                if (ex.Number == 1062) // MySQL error code for duplicate entry
                {
                    ModelState.AddModelError("", "A passenger with this ID or Email already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while adding the passenger: " + ex.Message);
                }
                // Return to the form with errors, potentially re-populating if needed
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                // Return to the form with errors
                return View();
            }
        }

        // GET: Yolcular/Edit/5 (Show form to edit existing passenger)
        public ActionResult Edit(string id)
        {
            Yolcular passenger = null;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM YOLCULAR WHERE Yolcu_ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    passenger = new Yolcular
                    {
                        Yolcu_ID = reader["Yolcu_ID"].ToString(),
                        Adi = reader["Adi"].ToString(),
                        Soyad = reader["Soyad"].ToString(),
                        Email = reader["Email"].ToString(),
                        Telefon = reader["Telefon"].ToString()
                    };
                }
            }
            if (passenger == null)
            {
                // If passenger not found, return a 404 or redirect
                return HttpNotFound();
            }
            return View(passenger); // Pass the retrieved passenger model to the view
        }

        // POST: Yolcular/Edit/5 (Handle form submission for editing passenger)
        [HttpPost]
        [ValidateAntiForgeryToken] // Ensure CSRF protection
        public ActionResult Edit(Yolcular model) // Model Binder will populate 'model' from form inputs
        {
            // If model binding fails significantly, 'model' might be null, but usually it's instantiated.
            // Check ModelState validity
            if (!ModelState.IsValid)
            {
                // If there are validation errors, return the view with the model to show errors
                return View(model);
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("UPDATE YOLCULAR SET Adi = @Adi, Soyad = @Soyad, Email = @Email, Telefon = @Telefon WHERE Yolcu_ID = @Yolcu_ID", conn);
                    cmd.Parameters.AddWithValue("@Adi", model.Adi);
                    cmd.Parameters.AddWithValue("@Soyad", model.Soyad);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Telefon", model.Telefon);
                    cmd.Parameters.AddWithValue("@Yolcu_ID", model.Yolcu_ID); // Crucial for WHERE clause
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index"); // Redirect to passenger list after successful update
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // MySQL error code for duplicate entry (e.g., duplicate email if unique)
                {
                    ModelState.AddModelError("Email", "This email address is already in use by another passenger.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while updating the passenger: " + ex.Message);
                }
                return View(model); // Return to the form with current data and error messages
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                return View(model); // Return to the form with current data and error messages
            }
        }

        // GET: Yolcular/Delete/5 (Show confirmation page for deletion)
        public ActionResult Delete(string id)
        {
            Yolcular passenger = null;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT * FROM YOLCULAR WHERE Yolcu_ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    passenger = new Yolcular
                    {
                        Yolcu_ID = reader["Yolcu_ID"].ToString(),
                        Adi = reader["Adi"].ToString(),
                        Soyad = reader["Soyad"].ToString(),
                        Email = reader["Email"].ToString(),
                        Telefon = reader["Telefon"].ToString()
                    };
                }
            }
            if (passenger == null)
            {
                return HttpNotFound(); // Return 404 if passenger not found
            }
            return View(passenger);
        }

        // POST: Yolcular/Delete/5 (Handle actual deletion)
        [HttpPost, ActionName("Delete")] // Use ActionName to allow same method name for GET/POST
        [ValidateAntiForgeryToken] // Ensure CSRF protection
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM YOLCULAR WHERE Yolcu_ID = @ID", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index"); // Redirect to passenger list after deletion
            }
            catch (MySqlException ex)
            {
                ModelState.AddModelError("", "An error occurred while deleting the passenger: " + ex.Message);
                // Re-fetch passenger details to display on error page
                return Delete(id); // Return to the confirmation page with error
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                return Delete(id);
            }
        }
    }
}