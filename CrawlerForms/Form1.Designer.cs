namespace CrawlerForm
{
	partial class Crawler
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Crawler));
			this.ToggleRunningButton = new System.Windows.Forms.Button();
			this.SelectorBox = new System.Windows.Forms.ComboBox();
			this.OutputBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// ToggleRunningButton
			// 
			this.ToggleRunningButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ToggleRunningButton.Location = new System.Drawing.Point(12, 40);
			this.ToggleRunningButton.Name = "ToggleRunningButton";
			this.ToggleRunningButton.Size = new System.Drawing.Size(230, 23);
			this.ToggleRunningButton.TabIndex = 1;
			this.ToggleRunningButton.Text = "Start";
			this.ToggleRunningButton.UseVisualStyleBackColor = true;
			this.ToggleRunningButton.Click += new System.EventHandler(this.ToggleRunningButton_Click);
			// 
			// SelectorBox
			// 
			this.SelectorBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectorBox.CausesValidation = false;
			this.SelectorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SelectorBox.FormattingEnabled = true;
			this.SelectorBox.Items.AddRange(new object[] {
            "Order by Latest",
            "Order by Downloads",
            "Order by Hottest"});
			this.SelectorBox.Location = new System.Drawing.Point(13, 13);
			this.SelectorBox.MaxDropDownItems = 3;
			this.SelectorBox.Name = "SelectorBox";
			this.SelectorBox.Size = new System.Drawing.Size(229, 21);
			this.SelectorBox.TabIndex = 2;
			this.SelectorBox.SelectedIndexChanged += new System.EventHandler(this.SelectorBox_SelectedIndexChanged);
			// 
			// OutputBox
			// 
			this.OutputBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputBox.HideSelection = false;
			this.OutputBox.Location = new System.Drawing.Point(12, 69);
			this.OutputBox.Name = "OutputBox";
			this.OutputBox.Size = new System.Drawing.Size(230, 80);
			this.OutputBox.TabIndex = 3;
			this.OutputBox.Text = "";
			// 
			// Crawler
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(254, 161);
			this.Controls.Add(this.OutputBox);
			this.Controls.Add(this.SelectorBox);
			this.Controls.Add(this.ToggleRunningButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(270, 200);
			this.Name = "Crawler";
			this.Text = "Crawler";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Crawler_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Crawler_FormClosed);
			this.Load += new System.EventHandler(this.Crawler_Load);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button ToggleRunningButton;
		private System.Windows.Forms.ComboBox SelectorBox;
		private System.Windows.Forms.RichTextBox OutputBox;
	}
}

