
namespace FontView
{
    partial class MergeFontWnd
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
            this.gbxLCMerge = new System.Windows.Forms.GroupBox();
            this.btnLCMerge = new System.Windows.Forms.Button();
            this.btnHansBtn = new System.Windows.Forms.Button();
            this.tbxHans = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxLatin = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLTBtn = new System.Windows.Forms.Button();
            this.gbxMMerge = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbxCover = new System.Windows.Forms.CheckBox();
            this.gbxLCMerge.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbxLCMerge
            // 
            this.gbxLCMerge.Controls.Add(this.btnLCMerge);
            this.gbxLCMerge.Controls.Add(this.btnHansBtn);
            this.gbxLCMerge.Controls.Add(this.tbxHans);
            this.gbxLCMerge.Controls.Add(this.label2);
            this.gbxLCMerge.Controls.Add(this.tbxLatin);
            this.gbxLCMerge.Controls.Add(this.label1);
            this.gbxLCMerge.Controls.Add(this.btnLTBtn);
            this.gbxLCMerge.Location = new System.Drawing.Point(12, 81);
            this.gbxLCMerge.Name = "gbxLCMerge";
            this.gbxLCMerge.Size = new System.Drawing.Size(582, 139);
            this.gbxLCMerge.TabIndex = 0;
            this.gbxLCMerge.TabStop = false;
            this.gbxLCMerge.Text = "中西文合库";
            // 
            // btnLCMerge
            // 
            this.btnLCMerge.Location = new System.Drawing.Point(501, 105);
            this.btnLCMerge.Name = "btnLCMerge";
            this.btnLCMerge.Size = new System.Drawing.Size(75, 23);
            this.btnLCMerge.TabIndex = 6;
            this.btnLCMerge.Text = "合成";
            this.btnLCMerge.UseVisualStyleBackColor = true;
            this.btnLCMerge.Click += new System.EventHandler(this.btnLCMerge_Click);
            // 
            // btnHansBtn
            // 
            this.btnHansBtn.Location = new System.Drawing.Point(501, 65);
            this.btnHansBtn.Name = "btnHansBtn";
            this.btnHansBtn.Size = new System.Drawing.Size(75, 23);
            this.btnHansBtn.TabIndex = 5;
            this.btnHansBtn.Text = "打开";
            this.btnHansBtn.UseVisualStyleBackColor = true;
            this.btnHansBtn.Click += new System.EventHandler(this.btnHansBtn_Click);
            // 
            // tbxHans
            // 
            this.tbxHans.Location = new System.Drawing.Point(105, 62);
            this.tbxHans.Name = "tbxHans";
            this.tbxHans.Size = new System.Drawing.Size(376, 21);
            this.tbxHans.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "中文字库";
            // 
            // tbxLatin
            // 
            this.tbxLatin.Location = new System.Drawing.Point(105, 19);
            this.tbxLatin.Name = "tbxLatin";
            this.tbxLatin.Size = new System.Drawing.Size(376, 21);
            this.tbxLatin.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "西文字库";
            // 
            // btnLTBtn
            // 
            this.btnLTBtn.Location = new System.Drawing.Point(501, 17);
            this.btnLTBtn.Name = "btnLTBtn";
            this.btnLTBtn.Size = new System.Drawing.Size(75, 23);
            this.btnLTBtn.TabIndex = 0;
            this.btnLTBtn.Text = "打开";
            this.btnLTBtn.UseVisualStyleBackColor = true;
            this.btnLTBtn.Click += new System.EventHandler(this.btnLTBtn_Click);
            // 
            // gbxMMerge
            // 
            this.gbxMMerge.Location = new System.Drawing.Point(12, 226);
            this.gbxMMerge.Name = "gbxMMerge";
            this.gbxMMerge.Size = new System.Drawing.Size(582, 280);
            this.gbxMMerge.TabIndex = 1;
            this.gbxMMerge.TabStop = false;
            this.gbxMMerge.Text = "多字库合库";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbxCover);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(575, 62);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "合库设置";
            // 
            // cbxCover
            // 
            this.cbxCover.AutoSize = true;
            this.cbxCover.Location = new System.Drawing.Point(23, 24);
            this.cbxCover.Name = "cbxCover";
            this.cbxCover.Size = new System.Drawing.Size(72, 16);
            this.cbxCover.TabIndex = 0;
            this.cbxCover.Text = "是否覆盖";
            this.cbxCover.UseVisualStyleBackColor = true;
            // 
            // MergeFontWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 518);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbxMMerge);
            this.Controls.Add(this.gbxLCMerge);
            this.Name = "MergeFontWnd";
            this.Text = "MergeFontWnd";
            this.gbxLCMerge.ResumeLayout(false);
            this.gbxLCMerge.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxLCMerge;
        private System.Windows.Forms.GroupBox gbxMMerge;
        private System.Windows.Forms.Button btnHansBtn;
        private System.Windows.Forms.TextBox tbxHans;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbxLatin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLTBtn;
        private System.Windows.Forms.Button btnLCMerge;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbxCover;
    }
}