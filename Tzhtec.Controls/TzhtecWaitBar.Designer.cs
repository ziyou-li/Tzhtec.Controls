namespace Tzhtec.Controls
{
    partial class TzhtecWaitBar
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (nowThread != null && nowThread.ThreadState != System.Threading.ThreadState.Stopped)
            {
                try { nowThread.Abort(); }
                catch { }
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblStop = new System.Windows.Forms.LinkLabel();
            this.lblInfo = new System.Windows.Forms.Label();
            this.picWait = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picWait)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblStop);
            this.panel1.Controls.Add(this.lblInfo);
            this.panel1.Controls.Add(this.picWait);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(277, 92);
            this.panel1.TabIndex = 0;
            // 
            // lblStop
            // 
            this.lblStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStop.AutoSize = true;
            this.lblStop.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStop.Location = new System.Drawing.Point(237, 72);
            this.lblStop.Name = "lblStop";
            this.lblStop.Size = new System.Drawing.Size(35, 14);
            this.lblStop.TabIndex = 2;
            this.lblStop.TabStop = true;
            this.lblStop.Text = "取消";
            this.lblStop.VisitedLinkColor = System.Drawing.Color.Blue;
            this.lblStop.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblCancel_LinkClicked);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.Location = new System.Drawing.Point(68, 34);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(176, 16);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "数据准备中，请稍后...";
            // 
            // picWait
            // 
            this.picWait.Image = global::Tzhtec.Controls.Resource1.loader;
            this.picWait.Location = new System.Drawing.Point(20, 21);
            this.picWait.Name = "picWait";
            this.picWait.Size = new System.Drawing.Size(42, 42);
            this.picWait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picWait.TabIndex = 0;
            this.picWait.TabStop = false;
            // 
            // TzhtecWaitBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "TzhtecWaitBar";
            this.Size = new System.Drawing.Size(277, 92);
            this.Resize += new System.EventHandler(this.TzhtecWaitBar_Resize);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picWait)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox picWait;
        private System.Windows.Forms.LinkLabel lblStop;
        private System.Windows.Forms.Label lblInfo;
    }
}
