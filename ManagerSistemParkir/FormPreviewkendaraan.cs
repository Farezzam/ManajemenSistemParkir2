using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel; 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerSistemParkir
{
    public partial class FormPreviewkendaraan : Form
    {
        private string connectionString = "Data Source=LAPTOP-JICJ6MBI\\FARISNAUFAL;Initial Catalog=ManajemenParkir2;Integrated Security=True;";
        private string errorMessage;
        private string masuk;
        private string keluar;

        public FormPreviewkendaraan(DataTable dt)
        {
            InitializeComponent();
            dgvPreviewKendaraan.DataSource = dt;

        }

        private void FormPreviewkendaraan_Load(object sender, EventArgs e)
        {
            dgvPreviewKendaraan.AutoResizeColumns();
        }

        private void btnKembali_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show("Apakah Anda yakin ingin mengimpor semua data ke database?", "Konfirmasi Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        private void ImportDataToDatabase()
        {
            int successCount = 0;
            int failCount = 0;
            StringBuilder errorMessages = new StringBuilder();

            DataTable dt = dgvPreviewKendaraan.DataSource as DataTable;

            if (dt == null)
            {
                MessageBox.Show("Data tidak tersedia untuk diimpor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int colPlat = -1, colJenis = -1, colMasuk = -1, colKeluar = -1;

            if (dt.Columns.Contains("Plat Nomor")) colPlat = dt.Columns["Plat Nomor"].Ordinal;
            else if (dt.Columns.Contains("plat_nomor")) colPlat = dt.Columns["plat_nomor"].Ordinal; 

            if (dt.Columns.Contains("Jenis Kendaraan")) colJenis = dt.Columns["Jenis Kendaraan"].Ordinal;
            else if (dt.Columns.Contains("jenis_kendaraan")) colJenis = dt.Columns["jenis_kendaraan"].Ordinal;

            if (dt.Columns.Contains("Waktu Masuk")) colMasuk = dt.Columns["Waktu Masuk"].Ordinal;
            else if (dt.Columns.Contains("waktu_masuk")) colMasuk = dt.Columns["waktu_masuk"].Ordinal;

            if (dt.Columns.Contains("Waktu Keluar")) colKeluar = dt.Columns["Waktu Keluar"].Ordinal;
            else if (dt.Columns.Contains("waktu_keluar")) colKeluar = dt.Columns["waktu_keluar"].Ordinal;

            if (colPlat == -1 || colJenis == -1 || colMasuk == -1 || colKeluar == -1)
            {
                MessageBox.Show("Salah satu atau lebih kolom penting (Plat Nomor, Jenis Kendaraan, Waktu Masuk, Waktu Keluar) tidak ditemukan di file Excel. Pastikan nama header kolom sesuai.", "Kesalahan Header Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            foreach (DataRow row in dt.Rows)
            {
                string plat = row[colPlat]?.ToString()?.Trim();
                string jenis = row[colJenis]?.ToString()?.Trim();
                string masukStr = row[colMasuk]?.ToString()?.Trim();
                string keluarStr = row[colKeluar]?.ToString()?.Trim();

                if (string.IsNullOrEmpty(plat) && string.IsNullOrEmpty(jenis) &&
                    string.IsNullOrEmpty(masukStr) && string.IsNullOrEmpty(keluarStr))
                {
                    continue;
                }

                string validationError;
                if (!ValidateKendaraan(plat, jenis, masukStr, keluarStr, out validationError))
                {
                    failCount++;
                    errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2} (data: {plat}, {jenis}, {masukStr}, {keluarStr}): {validationError}");
                    continue;
                }

                DateTime waktuMasuk = DateTime.Parse(masukStr); 
                DateTime waktuKeluar = DateTime.Parse(keluarStr); 

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();

                        // Cek apakah plat nomor sudah ada
                        string checkQuery = "SELECT COUNT(*) FROM kendaraan WHERE plat_nomor = @plat";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@plat", plat.ToUpper());
                            int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (existingCount > 0)
                            {
                                failCount++;
                                errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Plat nomor {plat} sudah ada di database");
                                continue;
                            }
                        }

                        string insertQuery = @"INSERT INTO kendaraan (plat_nomor, jenis_kendaraan, waktu_masuk, waktu_keluar)
                                                 VALUES (@plat_nomor, @jenis_kendaraan, @waktu_masuk, @waktu_keluar)";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@plat_nomor", plat.ToUpper());
                            cmd.Parameters.AddWithValue("@jenis_kendaraan", jenis);
                            cmd.Parameters.AddWithValue("@waktu_masuk", waktuMasuk);
                            cmd.Parameters.AddWithValue("@waktu_keluar", waktuKeluar);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                successCount++;
                            }
                            else
                            {
                                failCount++;
                                errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Gagal menyimpan ke database (tidak ada baris terpengaruh)");
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        failCount++;
                        errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Database Error: {sqlEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Terjadi kesalahan umum: {ex.Message}");
                    }
                } 
            }

            string message = $"Import selesai.\nSukses: {successCount} baris\nGagal: {failCount} baris.";

            if (errorMessages.Length > 0)
            {
                if (errorMessages.Length > 1000)
                {
                    message += "\n\nDetail error (dipotong karena terlalu panjang):\n" + errorMessages.ToString().Substring(0, 1000) + "...";
                }
                else
                {
                    message += "\n\nDetail error:\n" + errorMessages.ToString();
                }
            }

            MessageBox.Show(message, "Import Data", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (failCount == 0)
            {
                this.DialogResult = DialogResult.OK; 
                this.Close();
            }
        }

        private bool ValidateKendaraan(string plat, string jenis, string masukStr, string keluarStr, out string validationError)
        {
            validationError = "";

            if (string.IsNullOrEmpty(plat) || string.IsNullOrEmpty(jenis) ||
                string.IsNullOrEmpty(masukStr) || string.IsNullOrEmpty(keluarStr))
            {
                validationError = "Ada data yang kosong";
                return false;
            }

            plat = plat.ToUpper();
            Regex platRegex = new Regex(@"^[A-Z]{1,2}\s\d{1,4}\s[A-Z]{1,3}$");
            if (!platRegex.IsMatch(plat))
            {
                validationError = $"Format plat nomor tidak valid: '{plat}'. Contoh: AB 1234 CD";
                return false;
            }

            if (jenis != "Motor" && jenis != "Mobil")
            {
                validationError = $"Jenis kendaraan harus 'Motor' atau 'Mobil', bukan: '{jenis}'";
                return false;
            }

            DateTime masukDt, keluarDt;
            string[] formats =
            {
                "yyyy-MM-dd HH:mm:ss",
                "dd/MM/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm",
                "MM/dd/yyyy HH:mm",
                "yyyy-MM-dd H:mm", 
                "dd/MM/yyyy H:mm",
                "MM/dd/yyyy H:mm",
                "yyyy-MM-dd HH:mm", 
                "dd/MM/yyyy", 
                "MM/dd/yyyy" 
            };

            if (!DateTime.TryParseExact(masukStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out masukDt))
            {
                if (!DateTime.TryParse(masukStr, out masukDt))
                {
                    validationError = $"Format waktu masuk tidak valid: '{masukStr}'. Pastikan format tanggal dan waktu benar. Contoh: 'yyyy-MM-dd HH:mm:ss' atau 'dd/MM/yyyy HH:mm'.";
                    return false;
                }
            }

            if (!DateTime.TryParseExact(keluarStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out keluarDt))
            {
                if (!DateTime.TryParse(keluarStr, out keluarDt))
                {
                    validationError = $"Format waktu keluar tidak valid: '{keluarStr}'. Pastikan format tanggal dan waktu benar. Contoh: 'yyyy-MM-dd HH:mm:ss' atau 'dd/MM/yyyy HH:mm'.";
                    return false;
                }
            }

            if (keluarDt <= masukDt)
            {
                validationError = "Waktu keluar harus lebih besar dari waktu masuk";
                return false;
            }

            int currentYear = DateTime.Now.Year; 
            if (masukDt.Year < 2010 || masukDt.Year > currentYear)
            {
                validationError = $"Tahun waktu masuk ({masukDt.Year}) harus antara 2010 dan {currentYear}";
                return false;
            }

            if (keluarDt.Year < 2010 || keluarDt.Year > currentYear)
            {
                validationError = $"Tahun waktu keluar ({keluarDt.Year}) harus antara 2010 dan {currentYear}";
                return false;
            }

            return true;
        }
    }
}
