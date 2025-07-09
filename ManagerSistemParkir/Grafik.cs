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
using System.Windows.Forms.DataVisualization.Charting;

namespace ManagerSistemParkir
{
    public partial class Grafik: Form
    {
        Koneksi kn = new Koneksi();
        string connect = "";

        public Grafik()
        {
            InitializeComponent();
        }

        private void Grafik_Load(object sender, EventArgs e)
        {
            LoadOperatorToComboBox();
            cmbJenis.Items.AddRange(new string[] { "Semua", "Pemasukan", "Pengeluaran" });
            cmbJenis.SelectedIndex = 0;

            LoadChartData("Semua");

            cmbJenis.SelectedIndexChanged += cmbJenis_SelectedIndexChanged;
        }

        private void cmbJenis_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedJenis = cmbJenis.SelectedItem.ToString();
            LoadChartData(selectedJenis);
        }

        private void LoadChartData(string filter)
        {
            string selectedOperator = cmbOperator.SelectedItem?.ToString() ?? "Semua";

            chartTransaksi.Series.Clear();
            chartTransaksi.ChartAreas.Clear();
            chartTransaksi.Legends.Clear();
            chartTransaksi.Titles.Clear();

            ChartArea area = new ChartArea("AreaKeuangan");
            area.AxisX.Title = "Operator";
            area.AxisY.Title = "Total Pemasukan (Rp)";
            area.AxisX.LabelStyle.Angle = -45;
            area.BackColor = Color.Beige;

            chartTransaksi.ChartAreas.Add(area);

            Series series = new Series("Pemasukan")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.SteelBlue,
                IsValueShownAsLabel = true
            };

            string query = @"
                SELECT o.nama_operator, SUM(t.total_bayar) AS total_pemasukan
                FROM transaksi t
                JOIN operator o ON t.id_operator = o.id_operator
                {0}
                GROUP BY o.nama_operator
                ORDER BY o.nama_operator";

            string queryFilter = ""; // Renamed variable to avoid conflict
            List<SqlParameter> parameters = new List<SqlParameter>();

            if (selectedOperator != "Semua")
            {
                queryFilter = "WHERE o.nama_operator = @nama_operator";
                parameters.Add(new SqlParameter("@nama_operator", selectedOperator));
            }

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                SqlCommand cmd = new SqlCommand(string.Format(query, queryFilter), conn);
                if (parameters.Count > 0)
                    cmd.Parameters.AddRange(parameters.ToArray());

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string nama = reader["nama_operator"].ToString();
                    decimal total = Convert.ToDecimal(reader["total_pemasukan"]);
                    series.Points.AddXY(nama, total);
                }
            }

            chartTransaksi.Series.Add(series);
            chartTransaksi.Titles.Add("Grafik Pemasukan Berdasarkan Operator");
            chartTransaksi.Legends.Add(new Legend("Legenda"));
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void LoadOperatorToComboBox()
        {
            cmbOperator.Items.Clear();
            cmbOperator.Items.Add("Semua");

            string query = "SELECT nama_operator FROM operator";
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cmbOperator.Items.Add(reader["nama_operator"].ToString());
                }
            }

            cmbOperator.SelectedIndex = 0;
        }
    }

}
