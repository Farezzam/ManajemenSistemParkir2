using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagerSistemParkir
{
    public partial class FormUtama: Form
    {
        public FormUtama()
        {
            InitializeComponent();
        }

        private void FormUtama_Load(object sender, EventArgs e)
        {

        }

        private void btnOperator_Click(object sender, EventArgs e)
        {
            Operator formOperator = new Operator();
            formOperator.ShowDialog();
        }

        private void btnKendaraan_Click(object sender, EventArgs e)
        {
            Form1 formKendaraan = new Form1();
            formKendaraan.ShowDialog();
        }

        private void btnTransaksi_Click(object sender, EventArgs e)
        {
            Transaksi formTransaksi = new Transaksi();
            formTransaksi.ShowDialog();
        }
    }
}
