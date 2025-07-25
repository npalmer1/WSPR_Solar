namespace WSPR_Solar
{
    partial class MessageForm
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
            components = new System.ComponentModel.Container();
            label1 = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            OKbutton = new System.Windows.Forms.Button();
            Yesbutton = new System.Windows.Forms.Button();
            Nobutton = new System.Windows.Forms.Button();
            Addbutton = new System.Windows.Forms.Button();
            Editbutton = new System.Windows.Forms.Button();
            Delbutton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 30);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(307, 15);
            label1.TabIndex = 0;
            label1.Text = "12345678901234567890123456789012345678901234567890";
            // 
            // timer1
            // 
            timer1.Interval = 3000;
            timer1.Tick += timer1_Tick;
            // 
            // OKbutton
            // 
            OKbutton.Location = new System.Drawing.Point(269, 63);
            OKbutton.Name = "OKbutton";
            OKbutton.Size = new System.Drawing.Size(42, 23);
            OKbutton.TabIndex = 1;
            OKbutton.Text = "OK";
            OKbutton.UseVisualStyleBackColor = true;
            OKbutton.Click += OKbutton_Click;
            // 
            // Yesbutton
            // 
            Yesbutton.Location = new System.Drawing.Point(195, 63);
            Yesbutton.Name = "Yesbutton";
            Yesbutton.Size = new System.Drawing.Size(37, 23);
            Yesbutton.TabIndex = 2;
            Yesbutton.Text = "Yes";
            Yesbutton.UseVisualStyleBackColor = true;
            Yesbutton.Click += Yesbutton_Click;
            // 
            // Nobutton
            // 
            Nobutton.Location = new System.Drawing.Point(247, 63);
            Nobutton.Name = "Nobutton";
            Nobutton.Size = new System.Drawing.Size(38, 23);
            Nobutton.TabIndex = 3;
            Nobutton.Text = "No";
            Nobutton.UseVisualStyleBackColor = true;
            Nobutton.Click += Nobutton_Click;
            // 
            // Addbutton
            // 
            Addbutton.Location = new System.Drawing.Point(118, 73);
            Addbutton.Name = "Addbutton";
            Addbutton.Size = new System.Drawing.Size(42, 23);
            Addbutton.TabIndex = 4;
            Addbutton.Text = "Add";
            Addbutton.UseVisualStyleBackColor = true;
            Addbutton.Visible = false;
            Addbutton.Click += Addbutton_Click;
            // 
            // Editbutton
            // 
            Editbutton.Location = new System.Drawing.Point(175, 73);
            Editbutton.Name = "Editbutton";
            Editbutton.Size = new System.Drawing.Size(41, 23);
            Editbutton.TabIndex = 5;
            Editbutton.Text = "Edit";
            Editbutton.UseVisualStyleBackColor = true;
            Editbutton.Visible = false;
            Editbutton.Click += Editbutton_Click;
            // 
            // Delbutton
            // 
            Delbutton.Location = new System.Drawing.Point(233, 73);
            Delbutton.Name = "Delbutton";
            Delbutton.Size = new System.Drawing.Size(41, 23);
            Delbutton.TabIndex = 6;
            Delbutton.Text = "Del";
            Delbutton.UseVisualStyleBackColor = true;
            Delbutton.Visible = false;
            Delbutton.Click += Delbutton_Click;
            // 
            // MessageForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.OldLace;
            CancelButton = OKbutton;
            ClientSize = new System.Drawing.Size(329, 108);
            Controls.Add(Delbutton);
            Controls.Add(Editbutton);
            Controls.Add(Addbutton);
            Controls.Add(Nobutton);
            Controls.Add(Yesbutton);
            Controls.Add(OKbutton);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MessageForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "MessageForm";
            FormClosing += MessageForm_FormClosing;
            Load += MessageForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button OKbutton;
        private System.Windows.Forms.Button Yesbutton;
        private System.Windows.Forms.Button Nobutton;
        private System.Windows.Forms.Button Addbutton;
        private System.Windows.Forms.Button Editbutton;
        private System.Windows.Forms.Button Delbutton;
    }
}