namespace MineralThicknessMS.view
{
    partial class RadarDataInputForm
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
            label1 = new Label();
            txtDepth = new TextBox();
            btnSetDepth = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(49, 41);
            label1.Name = "label1";
            label1.Size = new Size(262, 41);
            label1.TabIndex = 0;
            label1.Text = "盐池底板高度(m)";
            // 
            // txtDepth
            // 
            txtDepth.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Regular, GraphicsUnit.Point);
            txtDepth.Location = new Point(317, 38);
            txtDepth.Name = "txtDepth";
            txtDepth.Size = new Size(150, 48);
            txtDepth.TabIndex = 1;
            // 
            // btnSetDepth
            // 
            btnSetDepth.Location = new Point(208, 102);
            btnSetDepth.Name = "btnSetDepth";
            btnSetDepth.Size = new Size(119, 46);
            btnSetDepth.TabIndex = 2;
            btnSetDepth.Text = "确定";
            btnSetDepth.UseVisualStyleBackColor = true;
            btnSetDepth.Click += btnSetDepth_Click;
            // 
            // RadarDataInputForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(541, 174);
            Controls.Add(btnSetDepth);
            Controls.Add(txtDepth);
            Controls.Add(label1);
            Name = "RadarDataInputForm";
            Text = "请设置当前盐池数据";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtDepth;
        private Button btnSetDepth;
    }
}