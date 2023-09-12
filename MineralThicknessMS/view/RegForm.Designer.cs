namespace MineralThicknessMS.view
{
    partial class RegForm
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
            GetMachineCode = new Button();
            txtMachineCode = new TextBox();
            label2 = new Label();
            label1 = new Label();
            txtActivate = new TextBox();
            activate = new Button();
            SuspendLayout();
            // 
            // GetMachineCode
            // 
            GetMachineCode.Location = new Point(631, 86);
            GetMachineCode.Name = "GetMachineCode";
            GetMachineCode.Size = new Size(112, 34);
            GetMachineCode.TabIndex = 11;
            GetMachineCode.Text = "获取";
            GetMachineCode.UseVisualStyleBackColor = true;
            GetMachineCode.Click += GetMachineCode_Click;
            // 
            // txtMachineCode
            // 
            txtMachineCode.Font = new Font("新宋体", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtMachineCode.Location = new Point(247, 88);
            txtMachineCode.Name = "txtMachineCode";
            txtMachineCode.Size = new Size(353, 28);
            txtMachineCode.TabIndex = 10;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(126, 91);
            label2.Name = "label2";
            label2.Size = new Size(100, 24);
            label2.TabIndex = 9;
            label2.Text = "获取机器码";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(126, 138);
            label1.Name = "label1";
            label1.Size = new Size(100, 24);
            label1.TabIndex = 8;
            label1.Text = "输入激活码";
            // 
            // txtActivate
            // 
            txtActivate.Location = new Point(247, 138);
            txtActivate.Name = "txtActivate";
            txtActivate.Size = new Size(353, 30);
            txtActivate.TabIndex = 7;
            // 
            // activate
            // 
            activate.Location = new Point(631, 136);
            activate.Name = "activate";
            activate.Size = new Size(112, 34);
            activate.TabIndex = 6;
            activate.Text = "激活";
            activate.UseVisualStyleBackColor = true;
            activate.Click += activate_Click;
            // 
            // RegForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(880, 253);
            Controls.Add(GetMachineCode);
            Controls.Add(txtMachineCode);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtActivate);
            Controls.Add(activate);
            Name = "RegForm";
            Text = "订阅";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button GetMachineCode;
        private TextBox txtMachineCode;
        private Label label2;
        private Label label1;
        private TextBox txtActivate;
        private Button activate;
    }
}