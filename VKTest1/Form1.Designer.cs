namespace VKTest1
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label1 = new Label();
            label2 = new Label();
            rtbTest1 = new RichTextBox();
            rtbTest2 = new RichTextBox();
            rtbSystem = new RichTextBox();
            label3 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 8.36F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(415, 67);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(6);
            label1.Size = new Size(131, 27);
            label1.TabIndex = 0;
            label1.Text = "Первый тест (CPU)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 8.36F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label2.ForeColor = SystemColors.ControlText;
            label2.Location = new Point(676, 67);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Padding = new Padding(6);
            label2.Size = new Size(129, 27);
            label2.TabIndex = 1;
            label2.Text = "Второй тест (GPU)";
            // 
            // rtbTest1
            // 
            rtbTest1.Location = new Point(374, 98);
            rtbTest1.Margin = new Padding(4, 3, 4, 3);
            rtbTest1.Name = "rtbTest1";
            rtbTest1.ReadOnly = true;
            rtbTest1.Size = new Size(230, 346);
            rtbTest1.TabIndex = 2;
            rtbTest1.Text = "";
            // 
            // rtbTest2
            // 
            rtbTest2.Location = new Point(630, 98);
            rtbTest2.Margin = new Padding(4, 3, 4, 3);
            rtbTest2.Name = "rtbTest2";
            rtbTest2.ReadOnly = true;
            rtbTest2.Size = new Size(230, 346);
            rtbTest2.TabIndex = 3;
            rtbTest2.Text = "";
            // 
            // rtbSystem
            // 
            rtbSystem.Location = new Point(35, 98);
            rtbSystem.Margin = new Padding(4, 3, 4, 3);
            rtbSystem.Name = "rtbSystem";
            rtbSystem.ReadOnly = true;
            rtbSystem.Size = new Size(230, 346);
            rtbSystem.TabIndex = 5;
            rtbSystem.Text = "";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 8.36F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label3.ForeColor = SystemColors.ControlText;
            label3.Location = new Point(75, 67);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Padding = new Padding(6);
            label3.Size = new Size(159, 27);
            label3.TabIndex = 4;
            label3.Text = "Информация о системе";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 224, 192);
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new Size(901, 513);
            Controls.Add(rtbSystem);
            Controls.Add(label3);
            Controls.Add(rtbTest2);
            Controls.Add(rtbTest1);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "Бенчмарк автомайзер";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtbTest1;
        private System.Windows.Forms.RichTextBox rtbTest2;
        private System.Windows.Forms.RichTextBox rtbSystem;
        private System.Windows.Forms.Label label3;
    }
}

