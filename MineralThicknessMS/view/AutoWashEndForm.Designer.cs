namespace MineralThicknessMS.view
{
    partial class AutoWashEndForm
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
            btnContinueMeasuring = new Button();
            btnKeepFolding = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(60, 40);
            label1.Name = "label1";
            label1.Size = new Size(657, 30);
            label1.TabIndex = 0;
            label1.Text = "提示：自动冲洗已结束，请确定是否继续测量或者保持折叠状态";
            // 
            // btnContinueMeasuring
            // 
            btnContinueMeasuring.Font = new Font("Microsoft YaHei UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point);
            btnContinueMeasuring.Location = new Point(206, 98);
            btnContinueMeasuring.Name = "btnContinueMeasuring";
            btnContinueMeasuring.Size = new Size(140, 50);
            btnContinueMeasuring.TabIndex = 1;
            btnContinueMeasuring.Text = "继续进行测量";
            btnContinueMeasuring.UseVisualStyleBackColor = true;
            btnContinueMeasuring.Click += btnContinueMeasuring_Click;
            // 
            // btnKeepFolding
            // 
            btnKeepFolding.Font = new Font("Microsoft YaHei UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point);
            btnKeepFolding.Location = new Point(426, 98);
            btnKeepFolding.Name = "btnKeepFolding";
            btnKeepFolding.Size = new Size(140, 50);
            btnKeepFolding.TabIndex = 2;
            btnKeepFolding.Text = "保持折叠状态";
            btnKeepFolding.UseVisualStyleBackColor = true;
            btnKeepFolding.Click += btnKeepFolding_Click;
            // 
            // AutoWashEndForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(787, 162);
            Controls.Add(btnKeepFolding);
            Controls.Add(btnContinueMeasuring);
            Controls.Add(label1);
            Name = "AutoWashEndForm";
            Text = "自动冲洗";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button btnContinueMeasuring;
        private Button btnKeepFolding;
    }
}