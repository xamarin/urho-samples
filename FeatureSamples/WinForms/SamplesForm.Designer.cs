namespace Urho.Samples.WinForms
{
	partial class SamplesForm
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
			this.samplesListbox = new System.Windows.Forms.ListBox();
			this.urhoSurfacePlaceholder = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// samplesListbox
			// 
			this.samplesListbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.samplesListbox.FormattingEnabled = true;
			this.samplesListbox.Location = new System.Drawing.Point(8, 8);
			this.samplesListbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.samplesListbox.Name = "samplesListbox";
			this.samplesListbox.Size = new System.Drawing.Size(124, 498);
			this.samplesListbox.TabIndex = 0;
			this.samplesListbox.SelectedIndexChanged += new System.EventHandler(this.samplesListbox_SelectedIndexChanged);
			// 
			// urhoSurfacePlaceholder
			// 
			this.urhoSurfacePlaceholder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.urhoSurfacePlaceholder.BackColor = System.Drawing.Color.OldLace;
			this.urhoSurfacePlaceholder.Location = new System.Drawing.Point(140, 8);
			this.urhoSurfacePlaceholder.Margin = new System.Windows.Forms.Padding(2);
			this.urhoSurfacePlaceholder.Name = "urhoSurfacePlaceholder";
			this.urhoSurfacePlaceholder.Size = new System.Drawing.Size(792, 499);
			this.urhoSurfacePlaceholder.TabIndex = 1;
			// 
			// SamplesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(940, 513);
			this.Controls.Add(this.urhoSurfacePlaceholder);
			this.Controls.Add(this.samplesListbox);
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.Name = "SamplesForm";
			this.Text = "UrhoSharp Samples";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox samplesListbox;
		private System.Windows.Forms.Panel urhoSurfacePlaceholder;
	}
}

