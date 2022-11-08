namespace FontView
{
    partial class MainWnd
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
            this.label3 = new System.Windows.Forms.Label();
            this.edt_ThknssY = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.edt_ThknssX = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_Thickness = new System.Windows.Forms.Button();
            this.FontInf = new System.Windows.Forms.Button();
            this.btnChngeCode = new System.Windows.Forms.Button();
            this.btnMKHanyi = new System.Windows.Forms.Button();
            this.btnMgrFnt = new System.Windows.Forms.Button();
            this.btnCodePrss = new System.Windows.Forms.Button();
            this.myPic2 = new FontView.GlpyhWnd();
            this.myPic1 = new FontView.GlpyhWnd();
            this.WOFF2TTF = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.myPic2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.myPic1)).BeginInit();
            this.SuspendLayout();
            // 
            // liulan
            // 
            this.liulan.Location = new System.Drawing.Point(1029, 12);
            this.liulan.Name = "liulan";
            this.liulan.Size = new System.Drawing.Size(75, 23);
            this.liulan.TabIndex = 1;
            this.liulan.Text = "浏览";
            this.liulan.UseVisualStyleBackColor = true;
            this.liulan.Click += new System.EventHandler(this.button1_Click);
            // 
            // Nextbtn
            // 
            this.Nextbtn.Location = new System.Drawing.Point(1112, 139);
            this.Nextbtn.Name = "Nextbtn";
            this.Nextbtn.Size = new System.Drawing.Size(75, 23);
            this.Nextbtn.TabIndex = 3;
            this.Nextbtn.Text = "下一字形";
            this.Nextbtn.UseVisualStyleBackColor = true;
            this.Nextbtn.Click += new System.EventHandler(this.Nextbtn_Click);
            // 
            // PREBTN
            // 
            this.PREBTN.Location = new System.Drawing.Point(1029, 139);
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
            this.richTextBox1.Location = new System.Drawing.Point(1028, 50);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(159, 39);
            this.richTextBox1.TabIndex = 6;
            this.richTextBox1.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1028, 103);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "上一点";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1112, 103);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "下一点";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // FindText
            // 
            this.FindText.Location = new System.Drawing.Point(1058, 194);
            this.FindText.Name = "FindText";
            this.FindText.Size = new System.Drawing.Size(77, 21);
            this.FindText.TabIndex = 9;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1143, 194);
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
            this.label1.Location = new System.Drawing.Point(1027, 199);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "Uni";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1027, 239);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "GID";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(1143, 234);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(44, 23);
            this.button4.TabIndex = 13;
            this.button4.Text = "查找";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // FindText1
            // 
            this.FindText1.Location = new System.Drawing.Point(1058, 236);
            this.FindText1.Name = "FindText1";
            this.FindText1.Size = new System.Drawing.Size(77, 21);
            this.FindText1.TabIndex = 12;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(10, 518);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(97, 37);
            this.button5.TabIndex = 15;
            this.button5.Text = "测试";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // SubSetFont
            // 
            this.SubSetFont.Location = new System.Drawing.Point(121, 518);
            this.SubSetFont.Name = "SubSetFont";
            this.SubSetFont.Size = new System.Drawing.Size(97, 37);
            this.SubSetFont.TabIndex = 16;
            this.SubSetFont.Text = "抽取子字库";
            this.SubSetFont.UseVisualStyleBackColor = true;
            this.SubSetFont.Click += new System.EventHandler(this.SubSetFont_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(232, 518);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(97, 37);
            this.button6.TabIndex = 17;
            this.button6.Text = "TTF To EOT";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(343, 518);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(97, 37);
            this.button7.TabIndex = 18;
            this.button7.Text = "TTF to Woff";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(583, 518);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(97, 37);
            this.button8.TabIndex = 19;
            this.button8.Text = "TTC To FONT";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(690, 518);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(97, 37);
            this.button9.TabIndex = 20;
            this.button9.Text = "FONTS TO TTC";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(801, 518);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(97, 37);
            this.button10.TabIndex = 21;
            this.button10.Text = "可变数据验证";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(83, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(11, 12);
            this.label3.TabIndex = 26;
            this.label3.Text = "Y";
            // 
            // edt_ThknssY
            // 
            this.edt_ThknssY.Location = new System.Drawing.Point(100, 20);
            this.edt_ThknssY.Name = "edt_ThknssY";
            this.edt_ThknssY.Size = new System.Drawing.Size(35, 21);
            this.edt_ThknssY.TabIndex = 25;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(11, 12);
            this.label4.TabIndex = 24;
            this.label4.Text = "X";
            // 
            // edt_ThknssX
            // 
            this.edt_ThknssX.Location = new System.Drawing.Point(34, 20);
            this.edt_ThknssX.Name = "edt_ThknssX";
            this.edt_ThknssX.Size = new System.Drawing.Size(35, 21);
            this.edt_ThknssX.TabIndex = 23;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_Thickness);
            this.groupBox1.Controls.Add(this.edt_ThknssX);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.edt_ThknssY);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(1027, 285);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(154, 85);
            this.groupBox1.TabIndex = 27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "变形";
            // 
            // btn_Thickness
            // 
            this.btn_Thickness.Location = new System.Drawing.Point(46, 54);
            this.btn_Thickness.Name = "btn_Thickness";
            this.btn_Thickness.Size = new System.Drawing.Size(75, 23);
            this.btn_Thickness.TabIndex = 28;
            this.btn_Thickness.Text = "执行";
            this.btn_Thickness.UseVisualStyleBackColor = true;
            this.btn_Thickness.Click += new System.EventHandler(this.btn_Thickness_Click);
            // 
            // FontInf
            // 
            this.FontInf.Location = new System.Drawing.Point(1031, 392);
            this.FontInf.Name = "FontInf";
            this.FontInf.Size = new System.Drawing.Size(75, 23);
            this.FontInf.TabIndex = 28;
            this.FontInf.Text = "字库信息";
            this.FontInf.UseVisualStyleBackColor = true;
            this.FontInf.Click += new System.EventHandler(this.FontInf_Click);
            // 
            // btnChngeCode
            // 
            this.btnChngeCode.Location = new System.Drawing.Point(914, 518);
            this.btnChngeCode.Name = "btnChngeCode";
            this.btnChngeCode.Size = new System.Drawing.Size(97, 37);
            this.btnChngeCode.TabIndex = 29;
            this.btnChngeCode.Text = "变更编码";
            this.btnChngeCode.UseVisualStyleBackColor = true;
            this.btnChngeCode.Click += new System.EventHandler(this.btnChngeCode_Click);
            // 
            // btnMKHanyi
            // 
            this.btnMKHanyi.Location = new System.Drawing.Point(232, 586);
            this.btnMKHanyi.Name = "btnMKHanyi";
            this.btnMKHanyi.Size = new System.Drawing.Size(97, 37);
            this.btnMKHanyi.TabIndex = 30;
            this.btnMKHanyi.Text = "汉仪字库";
            this.btnMKHanyi.UseVisualStyleBackColor = true;
            // 
            // btnMgrFnt
            // 
            this.btnMgrFnt.Location = new System.Drawing.Point(10, 586);
            this.btnMgrFnt.Name = "btnMgrFnt";
            this.btnMgrFnt.Size = new System.Drawing.Size(97, 37);
            this.btnMgrFnt.TabIndex = 31;
            this.btnMgrFnt.Text = "合并字库";
            this.btnMgrFnt.UseVisualStyleBackColor = true;
            this.btnMgrFnt.Click += new System.EventHandler(this.btnMgrFnt_Click);
            // 
            // btnCodePrss
            // 
            this.btnCodePrss.Location = new System.Drawing.Point(121, 586);
            this.btnCodePrss.Name = "btnCodePrss";
            this.btnCodePrss.Size = new System.Drawing.Size(97, 37);
            this.btnCodePrss.TabIndex = 32;
            this.btnCodePrss.Text = "码表处理";
            this.btnCodePrss.UseVisualStyleBackColor = true;
            this.btnCodePrss.Click += new System.EventHandler(this.btnCodeRmRepeat_Click);
            // 
            // myPic2
            // 
            this.myPic2.BackColor = System.Drawing.SystemColors.Window;
            this.myPic2.iMouseSel = -1;
            this.myPic2.Location = new System.Drawing.Point(510, 1);
            this.myPic2.Name = "myPic2";
            this.myPic2.Size = new System.Drawing.Size(500, 500);
            this.myPic2.TabIndex = 22;
            this.myPic2.TabStop = false;
            // 
            // myPic1
            // 
            this.myPic1.BackColor = System.Drawing.SystemColors.Window;
            this.myPic1.iMouseSel = -1;
            this.myPic1.Location = new System.Drawing.Point(-1, 1);
            this.myPic1.Name = "myPic1";
            this.myPic1.Size = new System.Drawing.Size(500, 500);
            this.myPic1.TabIndex = 5;
            this.myPic1.TabStop = false;
            this.myPic1.Click += new System.EventHandler(this.myPic1_Click_1);
            // 
            // WOFF2TTF
            // 
            this.WOFF2TTF.Location = new System.Drawing.Point(462, 518);
            this.WOFF2TTF.Name = "WOFF2TTF";
            this.WOFF2TTF.Size = new System.Drawing.Size(97, 37);
            this.WOFF2TTF.TabIndex = 33;
            this.WOFF2TTF.Text = "Woff to TTF";
            this.WOFF2TTF.UseVisualStyleBackColor = true;
            this.WOFF2TTF.Click += new System.EventHandler(this.WOFF2TTF_Click);
            // 
            // MainWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1202, 688);
            this.Controls.Add(this.WOFF2TTF);
            this.Controls.Add(this.btnCodePrss);
            this.Controls.Add(this.btnMgrFnt);
            this.Controls.Add(this.btnMKHanyi);
            this.Controls.Add(this.btnChngeCode);
            this.Controls.Add(this.FontInf);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.myPic2);
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
            this.Name = "MainWnd";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.myPic2)).EndInit();
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
        private GlpyhWnd myPic2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox edt_ThknssY;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox edt_ThknssX;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_Thickness;
        private System.Windows.Forms.Button FontInf;
        private System.Windows.Forms.Button btnChngeCode;
        private System.Windows.Forms.Button btnMKHanyi;
        private System.Windows.Forms.Button btnMgrFnt;
        private System.Windows.Forms.Button btnCodePrss;
        private System.Windows.Forms.Button WOFF2TTF;
    }
}

