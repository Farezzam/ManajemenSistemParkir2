namespace ManagerSistemParkir
{
    partial class FormUtama
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label01 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnOperator = new System.Windows.Forms.Button();
            this.btnKendaraan = new System.Windows.Forms.Button();
            this.btnTransaksi = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label01
            // 
            this.label01.AutoSize = true;
            this.label01.BackColor = System.Drawing.Color.Transparent;
            this.label01.ForeColor = System.Drawing.SystemColors.Control;
            this.label01.Location = new System.Drawing.Point(164, 26);
            this.label01.Name = "label01";
            this.label01.Size = new System.Drawing.Size(126, 13);
            this.label01.TabIndex = 0;
            this.label01.Text = "Manajemen Sistem Parkir";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(131, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Operator";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(131, 185);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "kendaraan";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(131, 245);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Transaksi";
            // 
            // btnOperator
            // 
            this.btnOperator.Location = new System.Drawing.Point(256, 125);
            this.btnOperator.Name = "btnOperator";
            this.btnOperator.Size = new System.Drawing.Size(75, 23);
            this.btnOperator.TabIndex = 4;
            this.btnOperator.Text = "Go";
            this.btnOperator.UseVisualStyleBackColor = true;
            this.btnOperator.Click += new System.EventHandler(this.btnOperator_Click);
            // 
            // btnKendaraan
            // 
            this.btnKendaraan.Location = new System.Drawing.Point(256, 180);
            this.btnKendaraan.Name = "btnKendaraan";
            this.btnKendaraan.Size = new System.Drawing.Size(75, 23);
            this.btnKendaraan.TabIndex = 5;
            this.btnKendaraan.Text = "Go";
            this.btnKendaraan.UseVisualStyleBackColor = true;
            this.btnKendaraan.Click += new System.EventHandler(this.btnKendaraan_Click);
            // 
            // btnTransaksi
            // 
            this.btnTransaksi.Location = new System.Drawing.Point(256, 240);
            this.btnTransaksi.Name = "btnTransaksi";
            this.btnTransaksi.Size = new System.Drawing.Size(75, 23);
            this.btnTransaksi.TabIndex = 6;
            this.btnTransaksi.Text = "Go";
            this.btnTransaksi.UseVisualStyleBackColor = true;
            this.btnTransaksi.Click += new System.EventHandler(this.btnTransaksi_Click);
            // 
            // FormUtama
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::ManagerSistemParkir.Properties.Resources.minimalism_texture_geometry_wallpaper_preview;
            this.ClientSize = new System.Drawing.Size(481, 413);
            this.Controls.Add(this.btnTransaksi);
            this.Controls.Add(this.btnKendaraan);
            this.Controls.Add(this.btnOperator);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label01);
            this.Name = "FormUtama";
            this.Text = "FormUtama";
            this.Load += new System.EventHandler(this.FormUtama_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label01;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnOperator;
        private System.Windows.Forms.Button btnKendaraan;
        private System.Windows.Forms.Button btnTransaksi;
    }
}