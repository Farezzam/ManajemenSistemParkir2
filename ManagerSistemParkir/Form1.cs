using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerSistemParkir
{
    public partial class Form1 : Form
    {
        // Perbaikan nama database: ManajemenParkir2 (sesuai dengan script SQL)
        private string connectionString = "Data Source=LAPTOP-JICJ6MBI\\FARISNAUFAL;Initial Catalog=ManajemenParkir2;Integrated Security=True;";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void ClearForm()
        {
            txtKendaraan.Clear();
            txtPlat.Clear();
            txtMasuk.Clear();
            txtKeluar.Clear();

            txtPlat.Focus();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id_kendaraan, plat_nomor, jenis_kendaraan, waktu_masuk, waktu_keluar FROM kendaraan";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvDataParkir.AutoGenerateColumns = true;
                    dgvDataParkir.DataSource = dt;

                    // Menyembunyikan kolom ID jika diperlukan
                    if (dgvDataParkir.Columns.Contains("id_kendaraan"))
                    {
                        dgvDataParkir.Columns["id_kendaraan"].Visible = false;
                    }

                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnTambah_Click_1(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    if (string.IsNullOrEmpty(txtPlat.Text) || string.IsNullOrEmpty(txtKendaraan.Text) || string.IsNullOrEmpty(txtMasuk.Text))
                    {
                        MessageBox.Show("Semua data harus diisi", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Validasi jenis kendaraan
                    string jenisKendaraan = txtKendaraan.Text.Trim();
                    if (jenisKendaraan != "Motor" && jenisKendaraan != "Mobil")
                    {
                        MessageBox.Show("Jenis kendaraan harus 'Motor' atau 'Mobil'", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Validasi format tanggal
                    DateTime waktuMasuk;
                    if (!DateTime.TryParse(txtMasuk.Text.Trim(), out waktuMasuk))
                    {
                        MessageBox.Show("Format waktu masuk tidak valid. Gunakan format tanggal yang benar", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    conn.Open();
                    string query = "INSERT INTO kendaraan (plat_nomor, jenis_kendaraan, waktu_masuk) VALUES (@plat_nomor, @jenis_kendaraan, @waktu_masuk)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@plat_nomor", txtPlat.Text.Trim());
                        cmd.Parameters.AddWithValue("@jenis_kendaraan", jenisKendaraan);
                        cmd.Parameters.AddWithValue("@waktu_masuk", waktuMasuk);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data berhasil ditambahkan", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Data gagal ditambahkan", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnHapus_Click_1(object sender, EventArgs e)
        {
            if (dgvDataParkir.SelectedRows.Count > 0)
            {
                DialogResult confirm = MessageBox.Show("Apakah Anda yakin ingin menghapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        try
                        {
                            // Perbaikan: Menggunakan nama kolom yang benar dan tabel yang benar
                            int idKendaraan = Convert.ToInt32(dgvDataParkir.SelectedRows[0].Cells["id_kendaraan"].Value);

                            conn.Open();
                            // Perbaikan: Query DELETE untuk tabel kendaraan dan parameter yang benar
                            string query = "DELETE FROM kendaraan WHERE id_kendaraan = @id_kendaraan";

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@id_kendaraan", idKendaraan);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Data berhasil dihapus", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                    ClearForm();
                                }
                                else
                                {
                                    MessageBox.Show("Data gagal dihapus", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Pilih data yang akan dihapus", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            LoadData();

            MessageBox.Show($"Jumlah kolom : {dgvDataParkir.ColumnCount}\nJumlah Baris : {dgvDataParkir.RowCount}",
                "Debugging DataGridView", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvDataParkir_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDataParkir.Rows[e.RowIndex];

                txtPlat.Text = row.Cells["plat_nomor"].Value.ToString();
                txtKendaraan.Text = row.Cells["jenis_kendaraan"].Value.ToString();

                if (row.Cells["waktu_masuk"].Value != DBNull.Value)
                {
                    DateTime waktuMasuk = Convert.ToDateTime(row.Cells["waktu_masuk"].Value);
                    txtMasuk.Text = waktuMasuk.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtMasuk.Text = string.Empty;
                }

                if (row.Cells["waktu_keluar"].Value != DBNull.Value)
                {
                    DateTime waktuKeluar = Convert.ToDateTime(row.Cells["waktu_keluar"].Value);
                    txtKeluar.Text = waktuKeluar.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtKeluar.Text = string.Empty;
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (dgvDataParkir.SelectedRows.Count > 0)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        // Validasi input
                        if (string.IsNullOrEmpty(txtPlat.Text) || string.IsNullOrEmpty(txtKendaraan.Text) || string.IsNullOrEmpty(txtMasuk.Text))
                        {
                            MessageBox.Show("Semua data harus diisi", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Validasi jenis kendaraan
                        string jenisKendaraan = txtKendaraan.Text.Trim();
                        if (jenisKendaraan != "Motor" && jenisKendaraan != "Mobil")
                        {
                            MessageBox.Show("Jenis kendaraan harus 'Motor' atau 'Mobil'", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Validasi format tanggal
                        DateTime waktuMasuk;
                        if (!DateTime.TryParse(txtMasuk.Text.Trim(), out waktuMasuk))
                        {
                            MessageBox.Show("Format waktu masuk tidak valid. Gunakan format tanggal yang benar (yyyy-MM-dd HH:mm:ss)", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Dapatkan ID kendaraan dari baris yang dipilih
                        int idKendaraan = Convert.ToInt32(dgvDataParkir.SelectedRows[0].Cells["id_kendaraan"].Value);

                        conn.Open();
                        string query = "UPDATE kendaraan SET plat_nomor = @plat_nomor, jenis_kendaraan = @jenis_kendaraan, waktu_masuk = @waktu_masuk WHERE id_kendaraan = @id_kendaraan";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id_kendaraan", idKendaraan);
                            cmd.Parameters.AddWithValue("@plat_nomor", txtPlat.Text.Trim());
                            cmd.Parameters.AddWithValue("@jenis_kendaraan", jenisKendaraan);
                            cmd.Parameters.AddWithValue("@waktu_masuk", waktuMasuk);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data berhasil diubah", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();
                                ClearForm();
                            }
                            else
                            {
                                MessageBox.Show("Data gagal diubah", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Pilih data yang akan diubah", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}