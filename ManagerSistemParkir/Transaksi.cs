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
using System.Diagnostics; 

namespace ManagerSistemParkir
{
    public partial class Transaksi : Form
    {
        Koneksi kn = new Koneksi();
        private StringBuilder sqlPerformanceMessages = new StringBuilder(); 

        public Transaksi()
        {
            InitializeComponent();
        }

        private void Transaksi_Load(object sender, EventArgs e)
        {
            LoadComboBoxData();
            LoadData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                {
                    conn.Open();

                    // Load Kendaraan
                    SqlCommand cmdKendaraan = new SqlCommand("SELECT id_kendaraan, plat_nomor FROM kendaraan", conn);
                    SqlDataReader drKendaraan = cmdKendaraan.ExecuteReader();
                    Dictionary<int, string> kendaraanDict = new Dictionary<int, string>();
                    while (drKendaraan.Read())
                    {
                        kendaraanDict.Add((int)drKendaraan["id_kendaraan"], drKendaraan["plat_nomor"].ToString());
                    }
                    cmbKendaraan.DataSource = new BindingSource(kendaraanDict, null);
                    cmbKendaraan.DisplayMember = "Value";
                    cmbKendaraan.ValueMember = "Key";
                    drKendaraan.Close(); // Penting untuk menutup reader sebelum membuka reader lain

                    // Load Operator
                    SqlCommand cmdOperator = new SqlCommand("SELECT id_operator, nama_operator FROM operator", conn);
                    SqlDataReader drOperator = cmdOperator.ExecuteReader();
                    Dictionary<int, string> operatorDict = new Dictionary<int, string>();
                    while (drOperator.Read())
                    {
                        operatorDict.Add((int)drOperator["id_operator"], drOperator["nama_operator"].ToString());
                    }
                    cmbOperator.DataSource = new BindingSource(operatorDict, null);
                    cmbOperator.DisplayMember = "Value";
                    cmbOperator.ValueMember = "Key";
                    drOperator.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat memuat data ComboBox: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                {
                    conn.Open();
                    string query = @"
                        SELECT t.id_transaksi, k.plat_nomor, o.nama_operator, t.total_bayar, t.waktu_transaksi 
                        FROM transaksi t
                        JOIN kendaraan k ON t.id_kendaraan = k.id_kendaraan
                        JOIN operator o ON t.id_operator = o.id_operator";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvTransaksi.AutoGenerateColumns = true;
                    dgvTransaksi.DataSource = dt;

                    if (dgvTransaksi.Columns.Contains("id_transaksi"))
                    {
                        dgvTransaksi.Columns["id_transaksi"].Visible = false;
                    }
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat memuat data: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            if (cmbKendaraan.Items.Count > 0)
            {
                cmbKendaraan.SelectedIndex = 0;
            }
            if (cmbOperator.Items.Count > 0)
            {
                cmbOperator.SelectedIndex = 0;
            }
            txtTotalBayar.Clear();
            dtpTransaksi.Value = DateTime.Now;
            dgvTransaksi.ClearSelection();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtTotalBayar.Text))
                {
                    MessageBox.Show("Total bayar harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal totalBayar;
                if (!decimal.TryParse(txtTotalBayar.Text.Trim(), out totalBayar) || totalBayar <= 0)
                {
                    MessageBox.Show("Total bayar harus berupa angka positif!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbKendaraan.SelectedItem == null || cmbOperator.SelectedItem == null)
                {
                    MessageBox.Show("Pilih kendaraan dan operator!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                {
                    conn.Open();
                    string query = "sp_TambahTransaksi";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_kendaraan", ((KeyValuePair<int, string>)cmbKendaraan.SelectedItem).Key);
                        cmd.Parameters.AddWithValue("@id_operator", ((KeyValuePair<int, string>)cmbOperator.SelectedItem).Key);
                        cmd.Parameters.AddWithValue("@total_bayar", totalBayar);
                        cmd.Parameters.AddWithValue("@waktu_transaksi", dtpTransaksi.Value);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Transaksi berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Gagal menambahkan transaksi.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (dgvTransaksi.SelectedRows.Count > 0)
            {
                DialogResult confirm = MessageBox.Show("Yakin ingin menghapus transaksi ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        int idTransaksi = Convert.ToInt32(dgvTransaksi.SelectedRows[0].Cells["id_transaksi"].Value);
                        using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                        {
                            conn.Open();
                            string query = "sp_HapusTransaksi";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@id_transaksi", idTransaksi);

                                int result = cmd.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    MessageBox.Show("Transaksi berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                    ClearForm();
                                }
                                else
                                {
                                    MessageBox.Show("Gagal menghapus transaksi.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Number == 547)
                        {
                            MessageBox.Show("Tidak dapat menghapus transaksi karena ada data terkait di tabel lain (misal: laporan).", "Kesalahan Relasi Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Error database saat menghapus: " + sqlEx.Message, "Kesalahan SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Pilih baris transaksi terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (dgvTransaksi.SelectedRows.Count > 0)
            {
                try
                {
                    if (string.IsNullOrEmpty(txtTotalBayar.Text))
                    {
                        MessageBox.Show("Total bayar harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    decimal totalBayar;
                    if (!decimal.TryParse(txtTotalBayar.Text.Trim(), out totalBayar) || totalBayar <= 0)
                    {
                        MessageBox.Show("Total bayar harus berupa angka positif!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int idTransaksi = Convert.ToInt32(dgvTransaksi.SelectedRows[0].Cells["id_transaksi"].Value);

                    using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                    {
                        conn.Open();
                        string query = "sp_UbahTransaksi";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@id_transaksi", idTransaksi);
                            cmd.Parameters.AddWithValue("@id_kendaraan", ((KeyValuePair<int, string>)cmbKendaraan.SelectedItem).Key);
                            cmd.Parameters.AddWithValue("@id_operator", ((KeyValuePair<int, string>)cmbOperator.SelectedItem).Key);
                            cmd.Parameters.AddWithValue("@total_bayar", totalBayar);
                            cmd.Parameters.AddWithValue("@waktu_transaksi", dtpTransaksi.Value);

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Data transaksi berhasil diubah!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();
                                ClearForm();
                            }
                            else
                            {
                                MessageBox.Show("Gagal mengubah data transaksi.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Pilih baris transaksi yang akan diubah.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            MessageBox.Show($"Jumlah kolom : {dgvTransaksi.ColumnCount}\nJumlah Baris : {dgvTransaksi.RowCount}",
                "Debugging DataGridView", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvTransaksi_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dgvTransaksi.Rows[e.RowIndex];
                    string plat = row.Cells["plat_nomor"].Value?.ToString();
                    string nama = row.Cells["nama_operator"].Value?.ToString();

                    for (int i = 0; i < cmbKendaraan.Items.Count; i++)
                    {
                        var item = (KeyValuePair<int, string>)cmbKendaraan.Items[i];
                        if (item.Value == plat)
                        {
                            cmbKendaraan.SelectedIndex = i;
                            break;
                        }
                    }

                    for (int i = 0; i < cmbOperator.Items.Count; i++)
                    {
                        var item = (KeyValuePair<int, string>)cmbOperator.Items[i];
                        if (item.Value == nama)
                        {
                            cmbOperator.SelectedIndex = i;
                            break;
                        }
                    }

                    txtTotalBayar.Text = row.Cells["total_bayar"].Value?.ToString();

                    if (row.Cells["waktu_transaksi"].Value != DBNull.Value)
                    {
                        dtpTransaksi.Value = Convert.ToDateTime(row.Cells["waktu_transaksi"].Value);
                    }
                    else
                    {
                        dtpTransaksi.Value = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saat memuat data ke form: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            openFileDialog.Title = "Pilih File Excel Transaksi";

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
                                            XSSFFormulaEvaluator evaluator = new XSSFFormulaEvaluator(workbook);
                                            CellValue cellValue = evaluator.Evaluate(cell);
                                            switch (cellValue.CellType)
                                            {
                                                case CellType.String:
                                                    dataRow[j] = cellValue.StringValue;
                                                    break;
                                                case CellType.Numeric:
                                                    if (DateUtil.IsCellDateFormatted(cell))
                                                    {
                                                        dataRow[j] = DateTime.FromOADate(cellValue.NumberValue);
                                                    }
                                                    else
                                                    {
                                                        dataRow[j] = cellValue.NumberValue;
                                                    }
                                                    break;
                                                case CellType.Boolean:
                                                    dataRow[j] = cellValue.BooleanValue;
                                                    break;
                                                default:
                                                    dataRow[j] = cell.ToString();
                                                    break;
                                            }
                                        }
                                        catch
                                        {
                                            dataRow[j] = cell.ToString();
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

                FormPreviewTransaksi previewForm = new FormPreviewTransaksi(dt, kn.connectionString());

                if (previewForm.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                    LoadComboBoxData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Debug.WriteLine("SQL Info Message (Transaksi): " + e.Message);
            sqlPerformanceMessages.AppendLine(e.Message);
        }

        private void btnAnalisis_Click(object sender, EventArgs e)
        {
            PerformTransactionAnalysis();
        }

        private void PerformTransactionAnalysis()
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

                    string analysisQuery = @"
                SELECT t.id_transaksi, k.plat_nomor, o.nama_operator, t.total_bayar, t.waktu_transaksi 
                FROM transaksi t
                JOIN kendaraan k ON t.id_kendaraan = k.id_kendaraan
                JOIN operator o ON t.id_operator = o.id_operator
                ORDER BY t.waktu_transaksi DESC;";

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

                if (sqlPerformanceMessages.Length > 0)
                {
                    MessageBox.Show(sqlPerformanceMessages.ToString(), "Hasil Analisis Kinerja Query Transaksi (STATISTICS IO/TIME)", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                sqlPerformanceMessages.AppendLine(e.Message);
            }
        }

        private void btnDetail_Click(object sender, EventArgs e)
        {
            FormReportViewer reportViewerForm = new FormReportViewer();
            reportViewerForm.ShowDialog();
        }

        private void btnGrafik_Click(object sender, EventArgs e)
        {
            Grafik grafikForm = new Grafik();
            grafikForm.ShowDialog();
        }
    }
}