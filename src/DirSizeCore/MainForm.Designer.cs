namespace DirSize
{
    partial class MainForm
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.diveLetterStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.freeSpaceStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.totalBytesCapacityStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.folderListView = new System.Windows.Forms.ListView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.InfoItemStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(5, 60);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(26, 629);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.diveLetterStatusLabel,
            this.freeSpaceStatusLabel,
            this.totalBytesCapacityStatusLabel,
            this.InfoItemStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(31, 659);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(959, 30);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // diveLetterStatusLabel
            // 
            this.diveLetterStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
            this.diveLetterStatusLabel.Font = new System.Drawing.Font("Segoe UI Black", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.diveLetterStatusLabel.Name = "diveLetterStatusLabel";
            this.diveLetterStatusLabel.Size = new System.Drawing.Size(29, 25);
            this.diveLetterStatusLabel.Text = "C:";
            // 
            // freeSpaceStatusLabel
            // 
            this.freeSpaceStatusLabel.Name = "freeSpaceStatusLabel";
            this.freeSpaceStatusLabel.Size = new System.Drawing.Size(147, 25);
            this.freeSpaceStatusLabel.Text = "-,---,--- Byte free.";
            // 
            // totalBytesCapacityStatusLabel
            // 
            this.totalBytesCapacityStatusLabel.Name = "totalBytesCapacityStatusLabel";
            this.totalBytesCapacityStatusLabel.Size = new System.Drawing.Size(230, 25);
            this.totalBytesCapacityStatusLabel.Text = "-,---,--- total Bytes capacity.";
            // 
            // folderListView
            // 
            this.folderListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.folderListView.HideSelection = false;
            this.folderListView.Location = new System.Drawing.Point(31, 60);
            this.folderListView.Name = "folderListView";
            this.folderListView.Size = new System.Drawing.Size(959, 629);
            this.folderListView.TabIndex = 2;
            this.folderListView.UseCompatibleStateImageBehavior = false;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(5, 5);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(985, 55);
            this.panel1.TabIndex = 3;
            // 
            // InfoItemStatusLabel
            // 
            this.InfoItemStatusLabel.Name = "InfoItemStatusLabel";
            this.InfoItemStatusLabel.Size = new System.Drawing.Size(179, 25);
            this.InfoItemStatusLabel.Text = "toolStripStatusLabel1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(995, 694);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.folderListView);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "MainForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ListView folderListView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripStatusLabel diveLetterStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel freeSpaceStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel totalBytesCapacityStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel InfoItemStatusLabel;
    }
}