
using System.Drawing;
using System.Windows.Forms;

namespace RodizioSmartRestuarant
{
    partial class Receipt
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
            this.pictureBoxImage = new System.Windows.Forms.PictureBox();
            this.experimentbutton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.LabelName = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelSection = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxImage
            // 
            this.pictureBoxImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxImage.Location = new System.Drawing.Point(59, 64);
            this.pictureBoxImage.Name = "pictureBoxImage";
            this.pictureBoxImage.Size = new System.Drawing.Size(169, 134);
            this.pictureBoxImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxImage.TabIndex = 0;
            this.pictureBoxImage.TabStop = false;
            // 
            // experimentbutton
            // 
            this.experimentbutton.BackColor = System.Drawing.SystemColors.HotTrack;
            this.experimentbutton.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.experimentbutton.Location = new System.Drawing.Point(906, 528);
            this.experimentbutton.Name = "experimentbutton";
            this.experimentbutton.Size = new System.Drawing.Size(75, 23);
            this.experimentbutton.TabIndex = 1;
            this.experimentbutton.Text = "PAyme";
            this.experimentbutton.UseVisualStyleBackColor = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // LabelName
            // 
            this.LabelName.AutoSize = true;
            this.LabelName.Location = new System.Drawing.Point(475, 64);
            this.LabelName.Name = "LabelName";
            this.LabelName.Size = new System.Drawing.Size(45, 17);
            this.LabelName.TabIndex = 2;
            this.LabelName.Text = "Name";
            this.LabelName.Click += new System.EventHandler(this.Name_Click);
            // 
            // textBoxName
            // 
            this.textBoxName.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxName.Location = new System.Drawing.Point(615, 85);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(234, 15);
            this.textBoxName.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Location = new System.Drawing.Point(615, 97);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(261, 1);
            this.panel1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Location = new System.Drawing.Point(478, 104);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(277, 1);
            this.panel2.TabIndex = 5;
            // 
            // labelSection
            // 
            this.labelSection.AutoSize = true;
            this.labelSection.Location = new System.Drawing.Point(843, 64);
            this.labelSection.Name = "labelSection";
            this.labelSection.Size = new System.Drawing.Size(55, 17);
            this.labelSection.TabIndex = 6;
            this.labelSection.Text = "Section";
            // 
            // Receipt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1057, 630);
            this.Controls.Add(this.labelSection);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.LabelName);
            this.Controls.Add(this.experimentbutton);
            this.Controls.Add(this.pictureBoxImage);
            this.Name = "Receipt";
            this.Text = "Receipt";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxImage;
        private System.Windows.Forms.Button experimentbutton;

        private void uploadImage()
        {
            try
            {
                openFileDialog1.Filter = "JPG FILES(*.jpg)|PNG FILES(*.png)";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    pictureBoxImage.Image = Image.FromFile(openFileDialog1.FileName);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error uploading image"+ex.ToString());
            }
        }

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Label LabelName;
        private TextBox textBoxName;
        private Panel panel1;
        private Panel panel2;
        private Label labelSection;
    }
}