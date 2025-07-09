using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerSistemParkir
{
    public partial class Form1 : Form
    {
        Koneksi kn = new Koneksi();
        private StringBuilder sqlStatisticsOutput = new StringBuilder();

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
            try
            {
                using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                {
                    conn.Open();
                    string query = "SELECT id_kendaraan, plat_nomor, jenis_kendaraan, waktu_masuk, waktu_keluar FROM kendaraan";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvDataParkir.AutoGenerateColumns = true;
                    dgvDataParkir.DataSource = dt;

                    if (dgvDataParkir.Columns.Contains("id_kendaraan"))
                    {
                        dgvDataParkir.Columns["id_kendaraan"].Visible = false;
                    }
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat memuat data: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTambah_Click_1(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                try
                {
                    if (string.IsNullOrEmpty(txtPlat.Text) || string.IsNullOrEmpty(txtKendaraan.Text) ||
                        string.IsNullOrEmpty(txtMasuk.Text) || string.IsNullOrEmpty(txtKeluar.Text))
                    {
                        MessageBox.Show("Semua data harus diisi", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string jenisKendaraan = txtKendaraan.Text.Trim();
                    if (jenisKendaraan != "Motor" && jenisKendaraan != "Mobil")
                    {
                        MessageBox.Show("Jenis kendaraan harus 'Motor' atau 'Mobil'", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string platNomor = txtPlat.Text.Trim().ToUpper();
                    txtPlat.Text = platNomor; 

                    Regex platRegex = new Regex(@"^[A-Z]{1,2}\s\d{1,4}\s[A-Z]{1,3}$");
                    if (!platRegex.IsMatch(platNomor))
                    {
                        MessageBox.Show("Format plat nomor tidak valid. Contoh format yang benar: AB 1234 CD", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DateTime waktuMasuk;
                    string[] formats = { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "MM/dd/yyyy HH:mm" };
                    if (!DateTime.TryParseExact(txtMasuk.Text.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out waktuMasuk) &&
                        !DateTime.TryParse(txtMasuk.Text.Trim(), out waktuMasuk)) 
                    {
                        MessageBox.Show("Format waktu masuk tidak valid. Gunakan format seperti 'YYYY-MM-DD HH:MM:SS'", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DateTime waktuKeluar;
                    if (!DateTime.TryParseExact(txtKeluar.Text.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out waktuKeluar) &&
                        !DateTime.TryParse(txtKeluar.Text.Trim(), out waktuKeluar)) 
                    {
                        MessageBox.Show("Format waktu keluar tidak valid. Gunakan format seperti 'YYYY-MM-DD HH:MM:SS'", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (waktuKeluar <= waktuMasuk)
                    {
                        MessageBox.Show("Waktu keluar harus lebih besar dari waktu masuk", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int currentYear = DateTime.Now.Year;
                    if (waktuMasuk.Year < 2010 || waktuMasuk.Year > currentYear)
                    {
                        MessageBox.Show($"Waktu masuk harus antara tahun 2010 dan {currentYear}", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (waktuKeluar.Year < 2010 || waktuKeluar.Year > currentYear)
                    {
                        MessageBox.Show($"Waktu keluar harus antara tahun 2010 dan {currentYear}", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    conn.Open();

                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM kendaraan WHERE plat_nomor = @plat", conn);
                    checkCmd.Parameters.AddWithValue("@plat", platNomor);
                    int existingCount = (int)checkCmd.ExecuteScalar();

                    if (existingCount > 0)
                    {
                        MessageBox.Show("Plat nomor sudah ada di database", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (SqlCommand cmd = new SqlCommand("sp_TambahKendaraan", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@plat_nomor", platNomor);
                        cmd.Parameters.AddWithValue("@jenis_kendaraan", jenisKendaraan);
                        cmd.Parameters.AddWithValue("@waktu_masuk", waktuMasuk);
                        cmd.Parameters.AddWithValue("@waktu_keluar", waktuKeluar);

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
                catch (SqlException sqlEx)
                {
                    MessageBox.Show("Database Error: " + sqlEx.Message, "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                    {
                        try
                        {
                            int idKendaraan = Convert.ToInt32(dgvDataParkir.SelectedRows[0].Cells["id_kendaraan"].Value);

                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand("sp_HapusKendaraan", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
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
                                    MessageBox.Show("Data gagal dihapus atau data tidak ditemukan", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        catch (SqlException sqlEx)
                        {
                            if (sqlEx.Number == 547) 
                            {
                                MessageBox.Show("Gagal menghapus data. Data kendaraan ini masih memiliki transaksi terkait. " +
                                                "Harap hapus transaksi terkait terlebih dahulu atau hubungi administrator database.",
                                                "Kesalahan Integritas Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                MessageBox.Show("Database Error saat menghapus: " + sqlEx.Message, "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Terjadi kesalahan saat menghapus data: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                txtPlat.Text = row.Cells["plat_nomor"].Value?.ToString() ?? string.Empty; 
                txtKendaraan.Text = row.Cells["jenis_kendaraan"].Value?.ToString() ?? string.Empty;

                if (row.Cells["waktu_masuk"].Value != DBNull.Value && row.Cells["waktu_masuk"].Value != null)
                {
                    DateTime waktuMasuk = Convert.ToDateTime(row.Cells["waktu_masuk"].Value);
                    txtMasuk.Text = waktuMasuk.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    txtMasuk.Text = string.Empty;
                }

                if (row.Cells["waktu_keluar"].Value != DBNull.Value && row.Cells["waktu_keluar"].Value != null)
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
                using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                {
                    try
                    {
                        if (string.IsNullOrEmpty(txtPlat.Text) || string.IsNullOrEmpty(txtKendaraan.Text) ||
                            string.IsNullOrEmpty(txtMasuk.Text) || string.IsNullOrEmpty(txtKeluar.Text))
                        {
                            MessageBox.Show("Semua data harus diisi", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string jenisKendaraan = txtKendaraan.Text.Trim();
                        if (jenisKendaraan != "Motor" && jenisKendaraan != "Mobil")
                        {
                            MessageBox.Show("Jenis kendaraan harus 'Motor' atau 'Mobil'", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        DateTime waktuMasuk;
                        string[] formats = { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm", "MM/dd/yyyy HH:mm" };
                        if (!DateTime.TryParseExact(txtMasuk.Text.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out waktuMasuk) &&
                            !DateTime.TryParse(txtMasuk.Text.Trim(), out waktuMasuk))
                        {
                            MessageBox.Show("Format waktu masuk tidak valid. Gunakan format seperti 'YYYY-MM-DD HH:MM:SS'", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        DateTime waktuKeluar;
                        if (!DateTime.TryParseExact(txtKeluar.Text.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out waktuKeluar) &&
                            !DateTime.TryParse(txtKeluar.Text.Trim(), out waktuKeluar))
                        {
                            MessageBox.Show("Format waktu keluar tidak valid. Gunakan format seperti 'YYYY-MM-DD HH:MM:SS'", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (waktuKeluar <= waktuMasuk)
                        {
                            MessageBox.Show("Waktu keluar harus lebih besar dari waktu masuk", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        int currentYear = DateTime.Now.Year;
                        if (waktuMasuk.Year < 2010 || waktuMasuk.Year > currentYear)
                        {
                            MessageBox.Show($"Waktu masuk harus antara tahun 2010 dan {currentYear}", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (waktuKeluar.Year < 2010 || waktuKeluar.Year > currentYear)
                        {
                            MessageBox.Show($"Waktu keluar harus antara tahun 2010 dan {currentYear}", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }


                        int idKendaraan = Convert.ToInt32(dgvDataParkir.SelectedRows[0].Cells["id_kendaraan"].Value);

                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("sp_UbahKendaraan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@id_kendaraan", idKendaraan);
                            cmd.Parameters.AddWithValue("@plat_nomor", txtPlat.Text.Trim().ToUpper());
                            cmd.Parameters.AddWithValue("@jenis_kendaraan", jenisKendaraan);
                            cmd.Parameters.AddWithValue("@waktu_masuk", waktuMasuk);
                            cmd.Parameters.AddWithValue("@waktu_keluar", waktuKeluar);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data berhasil diubah", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();
                                ClearForm();
                            }
                            else
                            {
                                MessageBox.Show("Data gagal diubah atau data tidak ditemukan", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        MessageBox.Show("Database Error saat mengubah: " + sqlEx.Message, "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Terjadi kesalahan saat mengubah data: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Pilih data yang akan diubah", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnKembali_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                PreviewData(filePath);
            }
        }

        private void PreviewData(string filePath)
        {
            try
            {
                DataTable dt = new DataTable();

                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(file);
                    ISheet sheet = workbook.GetSheetAt(0);

                    IRow headerRow = sheet.GetRow(0);
                    int cellCount = headerRow.LastCellNum;

                    for (int i = 0; i < cellCount; i++)
                    {
                        ICell cell = headerRow.GetCell(i);
                        string columnName = cell?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(columnName))
                        {
                            columnName = $"Kolom_{i}"; 
                        }
                        if (dt.Columns.Contains(columnName))
                        {
                            int counter = 1;
                            string tempColumnName = columnName;
                            while (dt.Columns.Contains(tempColumnName))
                            {
                                tempColumnName = $"{columnName}_{counter++}";
                            }
                            columnName = tempColumnName;
                        }
                        dt.Columns.Add(columnName);
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;

                        DataRow dataRow = dt.NewRow();
                        for (int j = 0; j < cellCount; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell != null)
                            {
                                switch (cell.CellType)
                                {
                                    case CellType.String:
                                        dataRow[j] = cell.StringCellValue;
                                        break;
                                    case CellType.Numeric:
                                        if (DateUtil.IsCellDateFormatted(cell))
                                        {
                                            dataRow[j] = cell.DateCellValue;
                                        }
                                        else
                                        {
                                            dataRow[j] = cell.NumericCellValue;
                                        }
                                        break;
                                    case CellType.Boolean:
                                        dataRow[j] = cell.BooleanCellValue;
                                        break;
                                    case CellType.Formula:
                                        try
                                        {
                                            dataRow[j] = cell.NumericCellValue; 
                                        }
                                        catch
                                        {
                                            dataRow[j] = cell.StringCellValue; 
                                        }
                                        break;
                                    default:
                                        dataRow[j] = cell.ToString();
                                        break;
                                }
                            }
                            else
                            {
                                dataRow[j] = DBNull.Value;
                            }
                        }
                        dt.Rows.Add(dataRow);
                    }
                }

                FormPreviewkendaraan previewForm = new FormPreviewkendaraan(dt);

                if (previewForm.ShowDialog() == DialogResult.OK)
                {
                    LoadData(); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnalisis_Click(object sender, EventArgs e)
        {
            PerformParkingAnalysis();
        }

        private void PerformParkingAnalysis()
        {
            sqlStatisticsOutput.Clear();

            try
            {
                using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                {
                    conn.InfoMessage += new SqlInfoMessageEventHandler(Conn_InfoMessage);
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SET STATISTICS IO ON; SET STATISTICS TIME ON;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    string analysisQuery = "SELECT id_kendaraan, plat_nomor, jenis_kendaraan, waktu_masuk, waktu_keluar FROM kendaraan;";
                    using (SqlCommand cmd = new SqlCommand(analysisQuery, conn))
                    {

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                            }
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("SET STATISTICS IO OFF; SET STATISTICS TIME OFF;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                if (sqlStatisticsOutput.Length > 0)
                {
                    MessageBox.Show(sqlStatisticsOutput.ToString(), "Hasil Analisis Kinerja Query (STATISTICS IO/TIME)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Tidak ada statistik kinerja query yang didapatkan. Pastikan query menghasilkan output statistik.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Database Error saat melakukan analisis: " + sqlEx.Message, "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat melakukan analisis: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Conn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if (e.Message.Contains("SQL Server parse and compile time") ||
                e.Message.Contains("Table") && e.Message.Contains("scan count") ||
                e.Message.Contains("CPU time") && e.Message.Contains("elapsed time"))
            {
                sqlStatisticsOutput.AppendLine(e.Message);
            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            FormReportkendaraan reportViewerForm = new FormReportkendaraan();
            reportViewerForm.ShowDialog();
        }
    }
}