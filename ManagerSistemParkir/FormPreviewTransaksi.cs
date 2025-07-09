using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerSistemParkir
{
    public partial class FormPreviewTransaksi : Form
    {
        private string connectionString = "Data Source=LAPTOP-JICJ6MBI\\FARISNAUFAL;Initial Catalog=ManajemenParkir2;Integrated Security=True;";

        public FormPreviewTransaksi(DataTable dt)
        {
            InitializeComponent();
            dgvPreviewTransaksi.DataSource = dt;
        }

        public FormPreviewTransaksi(DataTable dt, string connectionString) : this(dt)
        {
        }

        private void FormPreviewTransaksi_Load(object sender, EventArgs e)
        {
            dgvPreviewTransaksi.AutoResizeColumns();
        }

        private void btnKembali_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show("Apakah Anda yakin ingin mengimpor semua data transaksi ke database?", "Konfirmasi Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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

            DataTable dt = dgvPreviewTransaksi.DataSource as DataTable;
            if (dt == null)
            {
                MessageBox.Show("Data tidak tersedia untuk diimpor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                int rowNumber = i + 2; 

                try
                {
                    string platNomor = row[0]?.ToString()?.Trim() ?? "";
                    string namaOperator = row[1]?.ToString()?.Trim() ?? "";
                    string totalBayarStr = row[2]?.ToString()?.Trim() ?? "";
                    string waktuTransaksiStr = row[3]?.ToString()?.Trim() ?? "";

                    if (string.IsNullOrEmpty(platNomor) && string.IsNullOrEmpty(namaOperator) &&
                        string.IsNullOrEmpty(totalBayarStr) && string.IsNullOrEmpty(waktuTransaksiStr))
                        continue;

                    if (!ValidateTransaksi(platNomor, namaOperator, totalBayarStr, waktuTransaksiStr, out string validationError))
                    {
                        failCount++;
                        errorMessages.AppendLine($"Baris {rowNumber}: {validationError}");
                        continue;
                    }

                    string cleanTotalBayar = totalBayarStr.Replace(".", "")
                                                          .Replace(",", "")
                                                          .Replace(" ", "")
                                                          .Trim();

                    if (!decimal.TryParse(cleanTotalBayar, out decimal totalBayar))
                    {
                        failCount++;
                        errorMessages.AppendLine($"Baris {rowNumber}: Total bayar tidak valid: '{totalBayarStr}'");
                        continue;
                    }

                    if (!TryParseWaktuTransaksi(waktuTransaksiStr, out DateTime waktuTransaksi))
                    {
                        failCount++;
                        errorMessages.AppendLine($"Baris {rowNumber}: Format waktu transaksi tidak valid: '{waktuTransaksiStr}'");
                        continue;
                    }

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        int idKendaraan = 0;
                        using (SqlCommand cmdKendaraan = new SqlCommand(
                            "SELECT id_kendaraan FROM kendaraan WHERE REPLACE(UPPER(plat_nomor), ' ', '') = REPLACE(UPPER(@plat), ' ', '')", conn))
                        {
                            cmdKendaraan.Parameters.AddWithValue("@plat", platNomor);
                            object result = cmdKendaraan.ExecuteScalar();
                            if (result == null)
                            {
                                failCount++;
                                errorMessages.AppendLine($"Baris {rowNumber}: Plat nomor '{platNomor}' tidak ditemukan");
                                continue;
                            }
                            idKendaraan = Convert.ToInt32(result);
                        }

                        int idOperator = 0;
                        using (SqlCommand cmdOperator = new SqlCommand(
                            "SELECT id_operator FROM operator WHERE LOWER(LTRIM(RTRIM(nama_operator))) = LOWER(LTRIM(RTRIM(@nama)))", conn))
                        {
                            cmdOperator.Parameters.AddWithValue("@nama", namaOperator);
                            object result = cmdOperator.ExecuteScalar();
                            if (result == null)
                            {
                                failCount++;
                                errorMessages.AppendLine($"Baris {rowNumber}: Operator '{namaOperator}' tidak ditemukan");
                                continue;
                            }
                            idOperator = Convert.ToInt32(result);
                        }

                        using (SqlCommand cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) FROM transaksi 
                    WHERE id_kendaraan = @id_kendaraan 
                      AND id_operator = @id_operator 
                      AND total_bayar = @total_bayar 
                      AND ABS(DATEDIFF(MINUTE, waktu_transaksi, @waktu_transaksi)) <= 5", conn))
                        {
                            cmdCheck.Parameters.AddWithValue("@id_kendaraan", idKendaraan);
                            cmdCheck.Parameters.AddWithValue("@id_operator", idOperator);
                            cmdCheck.Parameters.AddWithValue("@total_bayar", totalBayar);
                            cmdCheck.Parameters.AddWithValue("@waktu_transaksi", waktuTransaksi);

                            int duplicateCount = (int)cmdCheck.ExecuteScalar();
                            if (duplicateCount > 0)
                            {
                                failCount++;
                                errorMessages.AppendLine($"Baris {rowNumber}: Duplikat transaksi (waktu: {waktuTransaksi:dd/MM/yyyy HH:mm})");
                                continue;
                            }
                        }

                        using (SqlCommand cmdInsert = new SqlCommand("sp_TambahTransaksi", conn))
                        {
                            cmdInsert.CommandType = CommandType.StoredProcedure;
                            cmdInsert.Parameters.AddWithValue("@id_kendaraan", idKendaraan);
                            cmdInsert.Parameters.AddWithValue("@id_operator", idOperator);
                            cmdInsert.Parameters.AddWithValue("@total_bayar", totalBayar);
                            cmdInsert.Parameters.AddWithValue("@waktu_transaksi", waktuTransaksi);

                            int result = cmdInsert.ExecuteNonQuery();
                            if (result > 0)
                                successCount++;
                            else
                            {
                                failCount++;
                                errorMessages.AppendLine($"Baris {rowNumber}: Gagal menyimpan transaksi");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    errorMessages.AppendLine($"Baris {rowNumber}: Error - {ex.Message}");
                }
            }

            string message = $"Import selesai.\nSukses: {successCount}\nGagal: {failCount}";
            if (errorMessages.Length > 0)
            {
                message += "\n\nBeberapa error:\n" + string.Join("\n", errorMessages.ToString().Split('\n').Take(10));
            }

            MessageBox.Show(message, "Hasil Import", MessageBoxButtons.OK,
                failCount == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (successCount > 0)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateTransaksi(string platNomor, string namaOperator, string totalBayarStr, string waktuTransaksiStr, out string errorMessage)
        {
            errorMessage = ""; 

            if (string.IsNullOrEmpty(platNomor) || string.IsNullOrEmpty(namaOperator) ||
                string.IsNullOrEmpty(totalBayarStr) || string.IsNullOrEmpty(waktuTransaksiStr))
            {
                errorMessage = "Ada data yang kosong";
                return false;
            }

            platNomor = platNomor.ToUpper().Trim();

            Regex platRegex = new Regex(@"^[A-Z]{1,2}\s*\d{1,4}\s*[A-Z]{1,3}$");
            if (!platRegex.IsMatch(platNomor))
            {
                errorMessage = $"Format plat nomor tidak valid: '{platNomor}'. Contoh yang benar: AB 1234 CD atau B 1234 ABC";
                return false;
            }

            namaOperator = namaOperator.Trim();
            if (string.IsNullOrWhiteSpace(namaOperator) || namaOperator.Length < 3)
            {
                errorMessage = $"Nama operator tidak valid: '{namaOperator}'. Minimal 3 karakter";
                return false;
            }

            if (!Regex.IsMatch(namaOperator, @"^[a-zA-Z\s]+$"))
            {
                errorMessage = $"Nama operator hanya boleh berisi huruf dan spasi: '{namaOperator}'";
                return false;
            }

            decimal totalBayar;
            string cleanTotalBayar = totalBayarStr.Trim()
                .Replace("Rp", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "");

            if (!decimal.TryParse(cleanTotalBayar, out totalBayar) || totalBayar <= 0)
            {
                errorMessage = $"Total bayar tidak valid: '{totalBayarStr}'. Harus berupa angka positif";
                return false;
            }

            if (totalBayar > 1000000)
            {
                errorMessage = $"Total bayar terlalu besar: {totalBayar:N0}. Maksimal Rp 1.000.000";
                return false;
            }

            DateTime waktuTransaksi;
            string cleanWaktu = waktuTransaksiStr.Trim();

            string[] formats =
            {
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy H:mm",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy H:mm",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "dd-MM-yyyy HH:mm:ss",
                "dd-MM-yyyy HH:mm",
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "yyyy-MM-dd",
                "dd-MM-yyyy"
            };

            bool dateTimeParsed = false;

            foreach (string format in formats)
            {
                if (DateTime.TryParseExact(cleanWaktu, format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out waktuTransaksi))
                {
                    dateTimeParsed = true;
                    break;
                }
            }

            if (!dateTimeParsed)
            {
                var indonesianCulture = new System.Globalization.CultureInfo("id-ID");
                dateTimeParsed = DateTime.TryParse(cleanWaktu, indonesianCulture,
                    System.Globalization.DateTimeStyles.None, out waktuTransaksi);
            }

            if (!dateTimeParsed)
            {
                dateTimeParsed = DateTime.TryParse(cleanWaktu, out waktuTransaksi);
            }

            if (!dateTimeParsed)
            {
                errorMessage = $"Format waktu transaksi tidak valid: '{waktuTransaksiStr}'. " +
                              "Format yang didukung: dd/MM/yyyy HH:mm, MM/dd/yyyy HH:mm, yyyy-MM-dd HH:mm, atau dd-MM-yyyy HH:mm";
                return false;
            }
            return true;
        }

        private bool TryParseWaktuTransaksi(string waktuTransaksiStr, out DateTime waktuTransaksi)
        {
            waktuTransaksi = DateTime.MinValue;
            string[] formats =
            {
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy H:mm",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy H:mm",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "dd-MM-yyyy HH:mm:ss",
                "dd-MM-yyyy HH:mm",
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "yyyy-MM-dd",
                "dd-MM-yyyy"
            };
            foreach (string format in formats)
            {
                if (DateTime.TryParseExact(waktuTransaksiStr.Trim(), format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out waktuTransaksi))
                {
                    return true;
                }
            }
            var indonesianCulture = new System.Globalization.CultureInfo("id-ID");
            return DateTime.TryParse(waktuTransaksiStr.Trim(), indonesianCulture,
                System.Globalization.DateTimeStyles.None, out waktuTransaksi);
        }
    }
}
