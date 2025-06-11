using NPOI.POIFS.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerSistemParkir
{
    public partial class FormPreviewOperator : Form
    {
        private string connectionString = "Data Source=LAPTOP-JICJ6MBI\\FARISNAUFAL;Initial Catalog=ManajemenParkir2;Integrated Security=True;";
        private Operator parentForm;

        public FormPreviewOperator(DataTable dt, Operator parent, string connString)
        {
            InitializeComponent();
            dgvPreviewOperator.DataSource = dt;
            parentForm = parent;
            connectionString = connString;

        }

        private void FormPreviewOperator_Load(object sender, EventArgs e)
        {
            dgvPreviewOperator.AutoResizeColumns();
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

            DataTable dt = dgvPreviewOperator.DataSource as DataTable;

            if (dt == null)
            {
                MessageBox.Show("Data tidak tersedia untuk diimpor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int colNama = -1, colShift = -1;

            if (dt.Columns.Contains("Nama Operator")) colNama = dt.Columns["Nama Operator"].Ordinal;
            else if (dt.Columns.Contains("nama_operator")) colNama = dt.Columns["nama_operator"].Ordinal; 

            if (dt.Columns.Contains("Shift")) colShift = dt.Columns["Shift"].Ordinal;
            else if (dt.Columns.Contains("shift")) colShift = dt.Columns["shift"].Ordinal;

            if (colNama == -1 || colShift == -1)
            {
                MessageBox.Show("Salah satu atau lebih kolom penting (Nama Operator, Shift) tidak ditemukan di file Excel. Pastikan nama header kolom sesuai.", "Kesalahan Header Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row == null || row.ItemArray.All(field => field == null || string.IsNullOrWhiteSpace(field.ToString())))
                        {
                            continue; 
                        }

                        string nama = "";
                        string shift = "";

                        try
                        {
                            nama = row[colNama]?.ToString()?.Trim();
                            shift = row[colShift]?.ToString()?.Trim();
                        }
                        catch (IndexOutOfRangeException)
                        {
                            failCount++;
                            errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Kolom tidak ditemukan atau indeks salah.");
                            continue;
                        }


                        string validationError;
                        if (!ValidateOperator(nama, shift, out validationError))
                        {
                            failCount++;
                            errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: {validationError}");
                            continue;
                        }

                        try
                        {
                            string checkQuery = "SELECT COUNT(*) FROM operator WHERE nama_operator = @nama";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@nama", nama);
                                int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                                if (existingCount > 0)
                                {
                                    failCount++;
                                    errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Nama operator '{nama}' sudah ada di database.");
                                    continue; 
                                }
                            }

                            string insertQuery = "INSERT INTO operator (nama_operator, shift) VALUES (@nama, @shift)";
                            using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@nama", nama);
                                cmd.Parameters.AddWithValue("@shift", shift);

                                int result = cmd.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    successCount++;
                                }
                                else
                                {
                                    failCount++;
                                    errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Gagal menyimpan ke database tanpa error eksplisit.");
                                }
                            }
                        }
                        catch (SqlException sqlEx)
                        {
                            failCount++;
                            errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Gagal menyimpan ke database. Error: {sqlEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            errorMessages.AppendLine($"Baris {dt.Rows.IndexOf(row) + 2}: Error tak terduga: {ex.Message}");
                        }
                    } 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi error saat memproses impor: " + ex.Message, "Kesalahan Impor Umum", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            } 

            string message = $"Import selesai.\nSukses: {successCount} baris\nGagal: {failCount} baris.";

            if (errorMessages.Length > 0)
            {
                if (errorMessages.Length > 1000)
                {
                    message += "\n\nDetail error (dipersingkat):\n" + errorMessages.ToString().Substring(0, 1000) + "...\n(Lihat log untuk detail lebih lanjut)";
                }
                else
                {
                    message += "\n\nDetail error:\n" + errorMessages.ToString();
                }
            }

            MessageBox.Show(message, "Import Data", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (successCount > 0)
            {
                parentForm?.LoadData(); 
            }
            this.Close(); 
        }

        private bool ValidateOperator(string nama, string shift, out string validationError)
        {
            validationError = "";

            if (string.IsNullOrWhiteSpace(nama) || string.IsNullOrWhiteSpace(shift))
            {
                validationError = "Nama operator dan shift tidak boleh kosong.";
                return false;
            }

            if (nama.Length < 2)
            {
                validationError = $"Nama operator terlalu pendek: '{nama}'. Minimal 2 karakter.";
                return false;
            }

            string[] validShifts = { "Pagi", "Siang", "Malam" };
            if (!validShifts.Contains(shift, StringComparer.OrdinalIgnoreCase)) 
            {
                validationError = $"Shift tidak valid: '{shift}'. Harus salah satu dari: Pagi, Siang, Malam.";
                return false;
            }

            return true;
        }
    }
}
