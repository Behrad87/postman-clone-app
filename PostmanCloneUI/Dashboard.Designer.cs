namespace PostmanCloneUI
{
    partial class Dashboard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Dashboard));
            apiLabel = new Label();
            urlTextBox = new TextBox();
            responseTextBox = new TextBox();
            callApi = new Button();
            formHeader = new Label();
            statusStrip = new StatusStrip();
            systemStatus = new ToolStripStatusLabel();
            resultLabel = new Label();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // apiLabel
            // 
            apiLabel.AutoSize = true;
            apiLabel.Font = new Font("Segoe UI", 16F);
            apiLabel.Location = new Point(45, 86);
            apiLabel.Name = "apiLabel";
            apiLabel.Size = new Size(50, 30);
            apiLabel.TabIndex = 0;
            apiLabel.Text = "API:";
            // 
            // urlTextBox
            // 
            urlTextBox.Location = new Point(143, 74);
            urlTextBox.Name = "urlTextBox";
            urlTextBox.Size = new Size(724, 39);
            urlTextBox.TabIndex = 2;
            // 
            // responseTextBox
            // 
            responseTextBox.BackColor = Color.White;
            responseTextBox.BorderStyle = BorderStyle.FixedSingle;
            responseTextBox.Location = new Point(45, 186);
            responseTextBox.Multiline = true;
            responseTextBox.Name = "responseTextBox";
            responseTextBox.ReadOnly = true;
            responseTextBox.ScrollBars = ScrollBars.Both;
            responseTextBox.Size = new Size(922, 384);
            responseTextBox.TabIndex = 3;
            // 
            // callApi
            // 
            callApi.Location = new Point(873, 74);
            callApi.Name = "callApi";
            callApi.Size = new Size(94, 49);
            callApi.TabIndex = 4;
            callApi.Text = "Go";
            callApi.UseVisualStyleBackColor = true;
            callApi.Click += callApi_Click;
            // 
            // formHeader
            // 
            formHeader.AutoSize = true;
            formHeader.Location = new Point(35, 19);
            formHeader.Name = "formHeader";
            formHeader.Size = new Size(174, 32);
            formHeader.TabIndex = 5;
            formHeader.Text = "Postman Clone";
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.White;
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { systemStatus });
            statusStrip.Location = new Point(0, 589);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(987, 30);
            statusStrip.TabIndex = 6;
            statusStrip.Text = "statusStrip";
            // 
            // systemStatus
            // 
            systemStatus.Font = new Font("Segoe UI", 14F);
            systemStatus.Name = "systemStatus";
            systemStatus.Size = new Size(62, 25);
            systemStatus.Text = "Ready";
            // 
            // resultLabel
            // 
            resultLabel.AutoSize = true;
            resultLabel.Font = new Font("Segoe UI", 16F);
            resultLabel.Location = new Point(35, 142);
            resultLabel.Name = "resultLabel";
            resultLabel.Size = new Size(79, 30);
            resultLabel.TabIndex = 7;
            resultLabel.Text = "Results";
            // 
            // Dashboard
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(987, 619);
            Controls.Add(resultLabel);
            Controls.Add(statusStrip);
            Controls.Add(formHeader);
            Controls.Add(callApi);
            Controls.Add(responseTextBox);
            Controls.Add(urlTextBox);
            Controls.Add(apiLabel);
            Font = new Font("Segoe UI", 18F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(6);
            Name = "Dashboard";
            Text = "Postman Clone by Behrad Zarei";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label apiLabel;
        private ComboBox requestTypeComboBox;
        private TextBox urlTextBox;
        private TextBox responseTextBox;
        private Button callApi;
        private Label formHeader;
        private StatusStrip statusStrip;
        private Label resultLabel;
        private ToolStripStatusLabel systemStatus;
    }
}
