namespace AktieAnalyzer
{
    partial class StockAnalyzer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listViewResults = new ListView();
            ButtomLoadData = new Button();
            textBoxStartAmount = new TextBox();
            textBoxEndAmount = new TextBox();
            textBoxPL = new TextBox();
            textBoxCommission = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            checkBoxCommission = new CheckBox();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            textBoxXXX = new TextBox();
            textBoxCommissionProcent = new TextBox();
            textBoxCommissionPrTrans = new TextBox();
            SuspendLayout();
            // 
            // listViewResults
            // 
            listViewResults.FullRowSelect = true;
            listViewResults.GridLines = true;
            listViewResults.Location = new Point(-2, 155);
            listViewResults.Name = "listViewResults";
            listViewResults.Size = new Size(1000, 447);
            listViewResults.TabIndex = 0;
            listViewResults.UseCompatibleStateImageBehavior = false;
            listViewResults.View = View.Details;
            listViewResults.Click += listViewResults_ItemActivate;
            // 
            // ButtomLoadData
            // 
            ButtomLoadData.Location = new Point(18, 19);
            ButtomLoadData.Name = "ButtomLoadData";
            ButtomLoadData.Size = new Size(75, 23);
            ButtomLoadData.TabIndex = 1;
            ButtomLoadData.Text = "Load Data";
            ButtomLoadData.UseVisualStyleBackColor = true;
            ButtomLoadData.Click += ButtomLoadData_Click;
            // 
            // textBoxStartAmount
            // 
            textBoxStartAmount.Location = new Point(877, 12);
            textBoxStartAmount.Name = "textBoxStartAmount";
            textBoxStartAmount.Size = new Size(100, 23);
            textBoxStartAmount.TabIndex = 2;
            // 
            // textBoxEndAmount
            // 
            textBoxEndAmount.Location = new Point(877, 41);
            textBoxEndAmount.Name = "textBoxEndAmount";
            textBoxEndAmount.Size = new Size(100, 23);
            textBoxEndAmount.TabIndex = 3;
            // 
            // textBoxPL
            // 
            textBoxPL.Location = new Point(877, 70);
            textBoxPL.Name = "textBoxPL";
            textBoxPL.Size = new Size(100, 23);
            textBoxPL.TabIndex = 4;
            // 
            // textBoxCommission
            // 
            textBoxCommission.Location = new Point(877, 99);
            textBoxCommission.Name = "textBoxCommission";
            textBoxCommission.Size = new Size(100, 23);
            textBoxCommission.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(791, 20);
            label1.Name = "label1";
            label1.Size = new Size(76, 15);
            label1.TabIndex = 6;
            label1.Text = "Start amount";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(791, 49);
            label2.Name = "label2";
            label2.Size = new Size(74, 15);
            label2.TabIndex = 7;
            label2.Text = "End Amount";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(791, 78);
            label3.Name = "label3";
            label3.Size = new Size(25, 15);
            label3.TabIndex = 8;
            label3.Text = "P/L";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(791, 107);
            label4.Name = "label4";
            label4.Size = new Size(74, 15);
            label4.TabIndex = 9;
            label4.Text = "Commission";
            // 
            // checkBoxCommission
            // 
            checkBoxCommission.AutoSize = true;
            checkBoxCommission.Location = new Point(579, 12);
            checkBoxCommission.Name = "checkBoxCommission";
            checkBoxCommission.Size = new Size(93, 19);
            checkBoxCommission.TabIndex = 11;
            checkBoxCommission.Text = "Commission";
            checkBoxCommission.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(577, 102);
            label5.Name = "label5";
            label5.Size = new Size(28, 15);
            label5.TabIndex = 17;
            label5.Text = "XXX";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(577, 73);
            label6.Name = "label6";
            label6.Size = new Size(82, 15);
            label6.TabIndex = 16;
            label6.Text = "Commision %";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(577, 44);
            label7.Name = "label7";
            label7.Size = new Size(84, 15);
            label7.TabIndex = 15;
            label7.Text = "Commission #";
            // 
            // textBoxXXX
            // 
            textBoxXXX.Location = new Point(665, 99);
            textBoxXXX.Name = "textBoxXXX";
            textBoxXXX.Size = new Size(100, 23);
            textBoxXXX.TabIndex = 14;
            // 
            // textBoxCommissionProcent
            // 
            textBoxCommissionProcent.Location = new Point(665, 70);
            textBoxCommissionProcent.Name = "textBoxCommissionProcent";
            textBoxCommissionProcent.Size = new Size(100, 23);
            textBoxCommissionProcent.TabIndex = 13;
            textBoxCommissionProcent.Text = "0,3%";
            // 
            // textBoxCommissionPrTrans
            // 
            textBoxCommissionPrTrans.Location = new Point(665, 41);
            textBoxCommissionPrTrans.Name = "textBoxCommissionPrTrans";
            textBoxCommissionPrTrans.Size = new Size(100, 23);
            textBoxCommissionPrTrans.TabIndex = 12;
            textBoxCommissionPrTrans.Text = "25";
            // 
            // StockAnalyzer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(999, 596);
            Controls.Add(label5);
            Controls.Add(label6);
            Controls.Add(label7);
            Controls.Add(textBoxXXX);
            Controls.Add(textBoxCommissionProcent);
            Controls.Add(textBoxCommissionPrTrans);
            Controls.Add(checkBoxCommission);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBoxCommission);
            Controls.Add(textBoxPL);
            Controls.Add(textBoxEndAmount);
            Controls.Add(textBoxStartAmount);
            Controls.Add(ButtomLoadData);
            Controls.Add(listViewResults);
            Name = "StockAnalyzer";
            Text = "Stock Analyzer";
            ResumeLayout(false);
            PerformLayout();
        }

        private void ButtomLoadData_Click(object sender, EventArgs e)
        {
            StockAnalyzer_Load(sender, e);
        }

        #endregion

        private ListView listViewResults;
        private Button ButtomLoadData;
        private TextBox textBoxStartAmount;
        private TextBox textBoxEndAmount;
        private TextBox textBoxPL;
        private TextBox textBoxCommission;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private CheckBox checkBoxCommission;
        private Label label5;
        private Label label6;
        private Label label7;
        private TextBox textBoxXXX;
        private TextBox textBoxCommissionProcent;
        private TextBox textBoxCommissionPrTrans;
    }
}
