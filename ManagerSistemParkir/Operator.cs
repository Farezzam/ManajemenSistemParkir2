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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace ManagerSistemParkir
{
    public partial class Operator : Form
    {
        // Instansiasi class Koneksi untuk menyediakan connection string dinamis
        Koneksi kn = new Koneksi();
        string strkonek = "";
        private StringBuilder sqlPerformanceMessages = new StringBuilder();

        public Operator()
        {
            InitializeComponent();
            SetupDataGridViewColumns();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Event handler kosong, bisa diisi atau dihapus jika tidak perlu
        }

        private void Operator_Load(object sender, EventArgs e)
        {
            cmbShift.Items.Clear();
            cmbShift.Items.AddRange(new string[] { "Pagi", "Siang", "Malam" });
            cmbShift.SelectedIndex = 0;

            LoadData();
            ClearForm();
        }

        private void SetupDataGridViewColumns()
        {
            if (dgvOperator.Columns.Count > 0)
            {
                dgvOperator.Columns.Clear();
            }

            dgvOperator.AutoGenerateColumns = false;
            dgvOperator.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "id_operator", Width = 50, Name = "id_operator" });
            dgvOperator.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nama Operator", DataPropertyName = "nama_operator", Width = 150, Name = "nama_operator" });
            dgvOperator.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Shift", DataPropertyName = "shift", Width = 100, Name = "shift" });
        }

        public void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id_operator, nama_operator, shift FROM operator";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvOperator.DataSource = dt;
                    dgvOperator.ClearSelection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error load data: " + ex.Message, "Kesalahan Load Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            txtNama.Clear();
            cmbShift.SelectedIndex = 0;
            dgvOperator.ClearSelection();
            txtNama.Focus();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama operator harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string shift = cmbShift.SelectedItem.ToString();

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                try
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM operator WHERE nama_operator = @nama_operator";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@nama_operator", txtNama.Text.Trim());
                        int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (existingCount > 0)
                        {
                            MessageBox.Show("Nama operator sudah ada di database.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("TambahOperator", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@nama_operator", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@shift", shift);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Operator berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Gagal menambahkan operator.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show("Error database: " + sqlEx.Message, "Kesalahan SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Kesalahan Umum", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (dgvOperator.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih operator yang akan dihapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idOperator = -1;
            try
            {
                idOperator = Convert.ToInt32(dgvOperator.SelectedRows[0].Cells["id_operator"].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kesalahan saat mendapatkan ID Operator: {ex.Message}\n" +
                                "Pastikan kolom 'id_operator' sudah benar di DataGridView.",
                                "Kesalahan DataGrid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show("Apakah Anda yakin ingin menghapus operator ini?\n" +
                                                  "Perhatian: Jika operator memiliki data transaksi terkait, penghapusan akan gagal.",
                                                  "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("HapusOperator", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_operator", idOperator);
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Operator berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Gagal menghapus operator.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 547) // Foreign key violation
                    {
                        MessageBox.Show($"Error: Tidak dapat menghapus operator karena masih ada transaksi yang terkait dengannya.\n\nDetail Error: {sqlEx.Message}",
                                        "Kesalahan Referensi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Error database saat menghapus: " + sqlEx.Message, "Kesalahan SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Kesalahan Umum", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (dgvOperator.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih operator yang akan diubah.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama operator harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idOperator = -1;
            try
            {
                idOperator = Convert.ToInt32(dgvOperator.SelectedRows[0].Cells["id_operator"].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kesalahan saat mendapatkan ID Operator untuk perubahan: {ex.Message}", "Kesalahan DataGrid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string shift = cmbShift.SelectedItem.ToString();

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                try
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM operator WHERE nama_operator = @nama_operator AND id_operator <> @id_operator";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@nama_operator", txtNama.Text.Trim());
                        checkCmd.Parameters.AddWithValue("@id_operator", idOperator);
                        int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (existingCount > 0)
                        {
                            MessageBox.Show("Nama operator sudah ada untuk operator lain. Gunakan nama yang berbeda.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("PerbaruiOperator", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_operator", idOperator);
                        cmd.Parameters.AddWithValue("@nama_operator", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@shift", shift);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Data operator berhasil diubah.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Gagal mengubah data operator.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show("Error database saat mengubah: " + sqlEx.Message, "Kesalahan SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Kesalahan Umum", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearForm();
        }

        private void dgvOperator_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvOperator.Rows[e.RowIndex];
                txtNama.Text = row.Cells["nama_operator"].Value?.ToString();
                cmbShift.SelectedItem = row.Cells["shift"].Value?.ToString();
            }
        }

        private void btnkembali_Click(object sender, EventArgs e)
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
                        dt.Columns.Add(headerRow.GetCell(i)?.ToString() ?? $"Kolom{i}");
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
                                dataRow[j] = GetCellValue(cell, workbook);
                            }
                            else
                            {
                                dataRow[j] = DBNull.Value;
                            }
                        }
                        dt.Rows.Add(dataRow);
                    }
                }

                // Diasumsikan Anda memiliki FormPreviewOperator
                // FormPreviewOperator previewForm = new FormPreviewOperator(dt, this, kn.connectionString());
                // previewForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private object GetCellValue(ICell cell, IWorkbook workbook)
        {
            switch (cell.CellType)
            {
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        return cell.DateCellValue;
                    return cell.NumericCellValue;
                case CellType.Boolean:
                    return cell.BooleanCellValue;
                case CellType.Formula:
                    try
                    {
                        IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
                        return evaluator.EvaluateInCell(cell).ToString();
                    }
                    catch { return cell.ToString(); }
                default:
                    return cell.ToString();
            }
        }

        public void RefreshDataFromImport()
        {
            LoadData();
        }

        private void btnAnalisis_Click(object sender, EventArgs e)
        {
            PerformOperatorAnalysis();
        }

        private void PerformOperatorAnalysis()
        {
            sqlPerformanceMessages.Clear();
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

                    string analysisQuery = "SELECT id_operator, nama_operator, shift FROM operator;";
                    using (SqlCommand cmd = new SqlCommand(analysisQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) { /* Membaca data untuk memicu statistik */ }
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("SET STATISTICS IO OFF; SET STATISTICS TIME OFF;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                if (sqlPerformanceMessages.Length > 0)
                {
                    MessageBox.Show(sqlPerformanceMessages.ToString(), "Hasil Analisis Kinerja Query (STATISTICS IO/TIME)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Tidak ada statistik kinerja query yang didapatkan.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat melakukan analisis: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Conn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            sqlPerformanceMessages.AppendLine(e.Message);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            // Event handler kosong
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            FormReportOperator reportForm = new FormReportOperator();
            reportForm.ShowDialog();
        }
    }
}