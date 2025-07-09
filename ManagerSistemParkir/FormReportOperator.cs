using Microsoft.Reporting.WinForms;
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
    public partial class FormReportOperator : Form
    {
        Koneksi kn = new Koneksi();
        string connect = "";

        public FormReportOperator()
        {
            InitializeComponent();
        }

        private void FormReportOperator_Load(object sender, EventArgs e)
        {
            SetupReportViewer();
            this.reportViewer1.RefreshReport();
        }

        private void SetupReportViewer()
        {
            connect = kn.connectionString();
            string sqlQuery = @"
                SELECT
                    id_operator,
                    nama_operator,
                    shift
                FROM
                    operator;";

            DataTable dtReportData = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(kn.connectionString()))
                {
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        connection.Open(); 
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dtReportData); 
                    }
                }
                
                ReportDataSource rds = new ReportDataSource("DataSetOperator", dtReportData);
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);
                reportViewer1.LocalReport.ReportPath = @"D:\Tugas\Semester 4\PABD\Repository\ManagerSistemParkir\ManagerSistemParkir\ReportOperator.rdlc";
                reportViewer1.LocalReport.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat memuat laporan operator:\n\n" + ex.Message +
                                "\n\n----- PANDUAN PEMECAHAN MASALAH ----- " +
                                "\n1. **Koneksi Database:** Pastikan 'Data Source' di connection string Anda benar (saat ini: LAPTOP-JICJ6MBI\\FARISNAUFAL) dan database 'ManajemenParkir2' dapat diakses." +
                                "\n2. **Kueri SQL:** Jalankan kueri 'SELECT id_operator, nama_operator, shift FROM operator;' langsung di SQL Server Management Studio (SSMS). Pastikan tidak ada error dan ada data yang kembali." +
                                "\n3. **Lokasi File RDLC:** Pastikan file 'ReportOperator.rdlc' *benar-benar ada* di jalur yang Anda tentukan:\n   " + @"D:\Tugas\Semester 4\PABD\Repository\ManagerSistemParkir\ManagerSistemParkir\ReportOperator.rdlc" +
                                "\n   Jika Anda mengubah lokasi file RDLC, perbarui path di kode ini." +
                                "\n4. **Nama Dataset di RDLC:** Buka 'ReportOperator.rdlc' di Visual Studio, periksa panel 'Report Data'. Nama dataset yang Anda gunakan di sana (yang terhubung ke data Anda) *harus sama persis* dengan string di kode: 'DataSetOperator'." +
                                "\n5. **Kolom di RDLC:** Pastikan dataset di RDLC Anda memiliki kolom (id_operator, nama_operator, shift) yang cocok dengan kueri SQL, dan kolom-kolom tersebut sudah diseret ke desain laporan Anda (misalnya di dalam tabel).",
                                "Error Laporan Operator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}