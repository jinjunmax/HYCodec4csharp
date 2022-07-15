
namespace FontView
{
    partial class ChangCodeWnd
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
            this.btnFnt = new System.Windows.Forms.Button();
            this.btnSrcCode = new System.Windows.Forms.Button();
            this.btnCvtCode = new System.Windows.Forms.Button();
            this.tbxFnt = new System.Windows.Forms.TextBox();
            this.tbxSrcCode = new System.Windows.Forms.TextBox();
            this.tbxCvtCode = new System.Windows.Forms.TextBox();
            this.btnConvter = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnFnt
            // 
            this.btnFnt.Location = new System.Drawing.Point(402, 24);
            this.btnFnt.Name = "btnFnt";
            this.btnFnt.Size = new System.Drawing.Size(75, 23);
            this.btnFnt.TabIndex = 0;
            this.btnFnt.Text = "字库";
            this.btnFnt.UseVisualStyleBackColor = true;
            this.btnFnt.Click += new System.EventHandler(this.btnFnt_Click);
            // 
            // btnSrcCode
            // 
            this.btnSrcCode.Location = new System.Drawing.Point(402, 72);
            this.btnSrcCode.Name = "btnSrcCode";
            this.btnSrcCode.Size = new System.Drawing.Size(75, 23);
            this.btnSrcCode.TabIndex = 1;
            this.btnSrcCode.Text = "源码表";
            this.btnSrcCode.UseVisualStyleBackColor = true;
            this.btnSrcCode.Click += new System.EventHandler(this.btnSrcCode_Click);
            // 
            // btnCvtCode
            // 
            this.btnCvtCode.Location = new System.Drawing.Point(402, 118);
            this.btnCvtCode.Name = "btnCvtCode";
            this.btnCvtCode.Size = new System.Drawing.Size(75, 23);
            this.btnCvtCode.TabIndex = 2;
            this.btnCvtCode.Text = "转换码表";
            this.btnCvtCode.UseVisualStyleBackColor = true;
            this.btnCvtCode.Click += new System.EventHandler(this.btnCvtCode_Click);
            // 
            // tbxFnt
            // 
            this.tbxFnt.Location = new System.Drawing.Point(37, 25);
            this.tbxFnt.Name = "tbxFnt";
            this.tbxFnt.Size = new System.Drawing.Size(351, 21);
            this.tbxFnt.TabIndex = 3;
            // 
            // tbxSrcCode
            // 
            this.tbxSrcCode.Location = new System.Drawing.Point(37, 74);
            this.tbxSrcCode.Name = "tbxSrcCode";
            this.tbxSrcCode.Size = new System.Drawing.Size(351, 21);
            this.tbxSrcCode.TabIndex = 4;
            // 
            // tbxCvtCode
            // 
            this.tbxCvtCode.Location = new System.Drawing.Point(37, 118);
            this.tbxCvtCode.Name = "tbxCvtCode";
            this.tbxCvtCode.Size = new System.Drawing.Size(351, 21);
            this.tbxCvtCode.TabIndex = 5;
            // 
            // btnConvter
            // 
            this.btnConvter.Location = new System.Drawing.Point(402, 156);
            this.btnConvter.Name = "btnConvter";
            this.btnConvter.Size = new System.Drawing.Size(75, 23);
            this.btnConvter.TabIndex = 6;
            this.btnConvter.Text = "转换";
            this.btnConvter.UseVisualStyleBackColor = true;
            this.btnConvter.Click += new System.EventHandler(this.btnConvter_Click);
            // 
            // ChangCodeWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 200);
            this.Controls.Add(this.btnConvter);
            this.Controls.Add(this.tbxCvtCode);
            this.Controls.Add(this.tbxSrcCode);
            this.Controls.Add(this.tbxFnt);
            this.Controls.Add(this.btnCvtCode);
            this.Controls.Add(this.btnSrcCode);
            this.Controls.Add(this.btnFnt);
            this.Name = "ChangCodeWnd";
            this.Text = "ChangCodeWnd";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnFnt;
        private System.Windows.Forms.Button btnSrcCode;
        private System.Windows.Forms.Button btnCvtCode;
        private System.Windows.Forms.TextBox tbxFnt;
        private System.Windows.Forms.TextBox tbxSrcCode;
        private System.Windows.Forms.TextBox tbxCvtCode;
        private System.Windows.Forms.Button btnConvter;
    }
}