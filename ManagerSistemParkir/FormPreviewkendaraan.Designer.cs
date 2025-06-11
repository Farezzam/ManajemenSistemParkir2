namespace ManagerSistemParkir
{
    partial class FormPreviewkendaraan
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
            this.dgvPreviewKendaraan = new System.Windows.Forms.DataGridView();
            this.btnKembali = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreviewKendaraan)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvPreviewKendaraan
            // 
            this.dgvPreviewKendaraan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPreviewKendaraan.Location = new System.Drawing.Point(36, 30);
            this.dgvPreviewKendaraan.Name = "dgvPreviewKendaraan";
            this.dgvPreviewKendaraan.Size = new System.Drawing.Size(725, 393);
            this.dgvPreviewKendaraan.TabIndex = 0;
            // 
            // btnKembali
            // 
            this.btnKembali.Location = new System.Drawing.Point(686, 439);
            this.btnKembali.Name = "btnKembali";
            this.btnKembali.Size = new System.Drawing.Size(75, 23);
            this.btnKembali.TabIndex = 1;
            this.btnKembali.Text = "Back";
            this.btnKembali.UseVisualStyleBackColor = true;
            this.btnKembali.Click += new System.EventHandler(this.btnKembali_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(605, 439);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // FormPreviewkendaraan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 474);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnKembali);
            this.Controls.Add(this.dgvPreviewKendaraan);
            this.Name = "FormPreviewkendaraan";
            this.Text = "FormPreviewkendaraan";
            this.Load += new System.EventHandler(this.FormPreviewkendaraan_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreviewKendaraan)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvPreviewKendaraan;
        private System.Windows.Forms.Button btnKembali;
        private System.Windows.Forms.Button btnOK;
    }
}