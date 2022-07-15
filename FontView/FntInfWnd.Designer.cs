
namespace FontView
{
    partial class FntInfWnd
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
            this.rTBFntInf = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rTBFntInf
            // 
            this.rTBFntInf.Location = new System.Drawing.Point(0, 1);
            this.rTBFntInf.Name = "rTBFntInf";
            this.rTBFntInf.Size = new System.Drawing.Size(457, 341);
            this.rTBFntInf.TabIndex = 0;
            this.rTBFntInf.Text = "";
            // 
            // FntInfWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 344);
            this.Controls.Add(this.rTBFntInf);
            this.Name = "FntInfWnd";
            this.Text = "字库信息";
            this.Load += new System.EventHandler(this.FntInfWnd_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rTBFntInf;
    }
}