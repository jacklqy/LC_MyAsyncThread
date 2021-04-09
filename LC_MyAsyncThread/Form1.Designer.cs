
namespace LC_MyAsyncThread
{
    partial class Form1
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
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSync = new System.Windows.Forms.Button();
            this.btnAsync = new System.Windows.Forms.Button();
            this.btnAsyncAdvanced = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSync
            // 
            this.btnSync.Location = new System.Drawing.Point(98, 55);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(75, 23);
            this.btnSync.TabIndex = 0;
            this.btnSync.Text = "同步方法";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            // 
            // btnAsync
            // 
            this.btnAsync.Location = new System.Drawing.Point(98, 103);
            this.btnAsync.Name = "btnAsync";
            this.btnAsync.Size = new System.Drawing.Size(75, 23);
            this.btnAsync.TabIndex = 1;
            this.btnAsync.Text = "异步方法";
            this.btnAsync.UseVisualStyleBackColor = true;
            this.btnAsync.Click += new System.EventHandler(this.btnAsync_Click);
            // 
            // btnAsyncAdvanced
            // 
            this.btnAsyncAdvanced.Location = new System.Drawing.Point(98, 150);
            this.btnAsyncAdvanced.Name = "btnAsyncAdvanced";
            this.btnAsyncAdvanced.Size = new System.Drawing.Size(118, 23);
            this.btnAsyncAdvanced.TabIndex = 2;
            this.btnAsyncAdvanced.Text = "AsyncAdvanced";
            this.btnAsyncAdvanced.UseVisualStyleBackColor = true;
            this.btnAsyncAdvanced.Click += new System.EventHandler(this.btnAsyncAdvanced_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 289);
            this.Controls.Add(this.btnAsyncAdvanced);
            this.Controls.Add(this.btnAsync);
            this.Controls.Add(this.btnSync);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.Button btnAsync;
        private System.Windows.Forms.Button btnAsyncAdvanced;
    }
}

