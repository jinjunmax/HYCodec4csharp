namespace FontView
{
    partial class Form1
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
            this.liulan = new System.Windows.Forms.Button();
            this.Nextbtn = new System.Windows.Forms.Button();
            this.PREBTN = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.myPic1 = new FontView.GlpyhWnd();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.FindText = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.FindText1 = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.SubSetFont = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.myPic1)).BeginInit();
            this.SuspendLayout();
            // 
            // liulan
            // 
            this.liulan.Location = new System.Drawing.Point(1005, 12);
            this.liulan.Name = "liulan";
            this.liulan.Size = new System.Drawing.Size(75, 23);
            this.liulan.TabIndex = 1;
            this.liulan.Text = "浏览";
            this.liulan.UseVisualStyleBackColor = true;
            this.liulan.Click += new System.EventHandler(this.button1_Click);
            // 
            // Nextbtn
            // 
            this.Nextbtn.Location = new System.Drawing.Point(1111, 148);
            this.Nextbtn.Name = "Nextbtn";
            this.Nextbtn.Size = new System.Drawing.Size(75, 23);
            this.Nextbtn.TabIndex = 3;
            this.Nextbtn.Text = "下一字形";
            this.Nextbtn.UseVisualStyleBackColor = true;
            this.Nextbtn.Click += new System.EventHandler(this.Nextbtn_Click);
            // 
            // PREBTN
            // 
            this.PREBTN.Location = new System.Drawing.Point(1005, 148);
            this.PREBTN.Name = "PREBTN";
            this.PREBTN.Size = new System.Drawing.Size(75, 23);
            this.PREBTN.TabIndex = 4;
            this.PREBTN.Text = "上一字形";
            this.PREBTN.UseVisualStyleBackColor = true;
            this.PREBTN.Click += new System.EventHandler(this.PREBTN_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.richTextBox1.Location = new System.Drawing.Point(1005, 50);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(201, 39);
            this.richTextBox1.TabIndex = 6;
            this.richTextBox1.Text = "";
            // 
            // myPic1
            // 
            this.myPic1.BackColor = System.Drawing.SystemColors.Window;
            this.myPic1.iMouseSel = -1;
            this.myPic1.Location = new System.Drawing.Point(-1, 1);
            this.myPic1.Name = "myPic1";
            this.myPic1.Size = new System.Drawing.Size(1000, 1000);
            this.myPic1.TabIndex = 5;
            this.myPic1.TabStop = false;
            this.myPic1.Click += new System.EventHandler(this.myPic1_Click_1);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1005, 95);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "上一点";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1111, 95);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "下一点";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // FindText
            // 
            this.FindText.Location = new System.Drawing.Point(1052, 195);
            this.FindText.Name = "FindText";
            this.FindText.Size = new System.Drawing.Size(95, 21);
            this.FindText.TabIndex = 9;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1153, 194);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(44, 23);
            this.button3.TabIndex = 10;
            this.button3.Text = "查找";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1005, 200);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "UniCode";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1005, 239);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "GID";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(1153, 233);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(44, 23);
            this.button4.TabIndex = 13;
            this.button4.Text = "查找";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // FindText1
            // 
            this.FindText1.Location = new System.Drawing.Point(1052, 234);
            this.FindText1.Name = "FindText1";
            this.FindText1.Size = new System.Drawing.Size(95, 21);
            this.FindText1.TabIndex = 12;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(1022, 287);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(154, 76);
            this.button5.TabIndex = 15;
            this.button5.Text = "测试";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // SubSetFont
            // 
            this.SubSetFont.Location = new System.Drawing.Point(1022, 375);
            this.SubSetFont.Name = "SubSetFont";
            this.SubSetFont.Size = new System.Drawing.Size(154, 76);
            this.SubSetFont.TabIndex = 16;
            this.SubSetFont.Text = "抽取子字库";
            this.SubSetFont.UseVisualStyleBackColor = true;
            this.SubSetFont.Click += new System.EventHandler(this.SubSetFont_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(1022, 462);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(154, 76);
            this.button6.TabIndex = 17;
            this.button6.Text = "TTF To EOT";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(1022, 548);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(154, 76);
            this.button7.TabIndex = 18;
            this.button7.Text = "TTF To Woff";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(1022, 631);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(154, 76);
            this.button8.TabIndex = 19;
            this.button8.Text = "TTC To FONT";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(1022, 712);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(154, 76);
            this.button9.TabIndex = 20;
            this.button9.Text = "FONTS TO TTC";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(1022, 794);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(154, 76);
            this.button10.TabIndex = 21;
            this.button10.Text = "To Execl";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1209, 1031);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.SubSetFont);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.FindText1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.FindText);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.myPic1);
            this.Controls.Add(this.PREBTN);
            this.Controls.Add(this.Nextbtn);
            this.Controls.Add(this.liulan);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.myPic1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button liulan;
        private System.Windows.Forms.Button Nextbtn;
        private System.Windows.Forms.Button PREBTN;
        private GlpyhWnd myPic1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox FindText;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox FindText1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button SubSetFont;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
    }
}

