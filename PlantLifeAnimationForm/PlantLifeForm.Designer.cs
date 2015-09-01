namespace PlantLifeAnimationForm
{
    partial class PlantLifeForm
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
        public void InitializeComponent()
        {
            this.peoplePicture = new System.Windows.Forms.PictureBox();
            this.PlantLifePicture = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.peoplePicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlantLifePicture)).BeginInit();
            this.SuspendLayout();
            // 
            // peoplePicture
            // 
            this.peoplePicture.Location = new System.Drawing.Point(0, 0);
            this.peoplePicture.Name = "peoplePicture";
            this.peoplePicture.Size = new System.Drawing.Size(353, 239);
            this.peoplePicture.TabIndex = 0;
            this.peoplePicture.TabStop = false;
            // 
            // PlantLifePicture
            // 
            this.PlantLifePicture.Location = new System.Drawing.Point(0, 0);
            this.PlantLifePicture.Name = "PlantLifePicture";
            this.PlantLifePicture.Size = new System.Drawing.Size(1062, 573);
            this.PlantLifePicture.TabIndex = 1;
            this.PlantLifePicture.TabStop = false;
            this.PlantLifePicture.Click += new System.EventHandler(this.PlantLifePicture_Click);
            // 
            // PlantLifeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1064, 577);
            this.Controls.Add(this.peoplePicture);
            this.Controls.Add(this.PlantLifePicture);
            this.Name = "PlantLifeForm";
            this.Text = "PlantLifeForm";
            this.Load += new System.EventHandler(this.PlantLifeForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PlantLifeForm_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.peoplePicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlantLifePicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox peoplePicture;
        private System.Windows.Forms.PictureBox PlantLifePicture;
    }
}