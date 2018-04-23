namespace SharpMonoInjector
{
    partial class Main
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
            this.cbProcesses = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtAssembly = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.txtClass = new System.Windows.Forms.TextBox();
            this.txtMethod = new System.Windows.Forms.TextBox();
            this.lstInjected = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnEject = new System.Windows.Forms.Button();
            this.btnInject = new System.Windows.Forms.Button();
            this.txtUnload = new System.Windows.Forms.TextBox();
            this.lblRefresh = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // cbProcesses
            // 
            this.cbProcesses.FormattingEnabled = true;
            this.cbProcesses.Location = new System.Drawing.Point(12, 29);
            this.cbProcesses.Name = "cbProcesses";
            this.cbProcesses.Size = new System.Drawing.Size(195, 21);
            this.cbProcesses.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Target process";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Assembly to inject";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(182, 68);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(25, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtAssembly
            // 
            this.txtAssembly.Location = new System.Drawing.Point(12, 69);
            this.txtAssembly.Name = "txtAssembly";
            this.txtAssembly.Size = new System.Drawing.Size(164, 22);
            this.txtAssembly.TabIndex = 4;
            this.txtAssembly.TextChanged += new System.EventHandler(this.txtAssembly_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Namespace";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 135);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Class";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 176);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Method";
            // 
            // txtNamespace
            // 
            this.txtNamespace.Location = new System.Drawing.Point(12, 110);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(195, 22);
            this.txtNamespace.TabIndex = 8;
            // 
            // txtClass
            // 
            this.txtClass.Location = new System.Drawing.Point(12, 151);
            this.txtClass.Name = "txtClass";
            this.txtClass.Size = new System.Drawing.Size(195, 22);
            this.txtClass.TabIndex = 9;
            // 
            // txtMethod
            // 
            this.txtMethod.Location = new System.Drawing.Point(12, 192);
            this.txtMethod.Name = "txtMethod";
            this.txtMethod.Size = new System.Drawing.Size(195, 22);
            this.txtMethod.TabIndex = 10;
            // 
            // lstInjected
            // 
            this.lstInjected.FormattingEnabled = true;
            this.lstInjected.Location = new System.Drawing.Point(246, 29);
            this.lstInjected.Name = "lstInjected";
            this.lstInjected.Size = new System.Drawing.Size(196, 160);
            this.lstInjected.TabIndex = 11;
            this.lstInjected.SelectedIndexChanged += new System.EventHandler(this.lstInjected_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(245, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(106, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Injected assemblies";
            // 
            // btnEject
            // 
            this.btnEject.Enabled = false;
            this.btnEject.Location = new System.Drawing.Point(367, 220);
            this.btnEject.Name = "btnEject";
            this.btnEject.Size = new System.Drawing.Size(75, 23);
            this.btnEject.TabIndex = 13;
            this.btnEject.Text = "Eject";
            this.btnEject.UseVisualStyleBackColor = true;
            this.btnEject.Click += new System.EventHandler(this.btnEject_Click);
            // 
            // btnInject
            // 
            this.btnInject.Enabled = false;
            this.btnInject.Location = new System.Drawing.Point(132, 220);
            this.btnInject.Name = "btnInject";
            this.btnInject.Size = new System.Drawing.Size(75, 23);
            this.btnInject.TabIndex = 14;
            this.btnInject.Text = "Inject";
            this.btnInject.UseVisualStyleBackColor = true;
            this.btnInject.Click += new System.EventHandler(this.btnInject_Click);
            // 
            // txtUnload
            // 
            this.txtUnload.Enabled = false;
            this.txtUnload.Location = new System.Drawing.Point(246, 192);
            this.txtUnload.Name = "txtUnload";
            this.txtUnload.Size = new System.Drawing.Size(196, 22);
            this.txtUnload.TabIndex = 17;
            this.txtUnload.Text = "Unload method name";
            this.txtUnload.Enter += new System.EventHandler(this.txtUnload_Enter);
            this.txtUnload.Leave += new System.EventHandler(this.txtUnload_Leave);
            // 
            // lblRefresh
            // 
            this.lblRefresh.AutoSize = true;
            this.lblRefresh.Location = new System.Drawing.Point(161, 13);
            this.lblRefresh.Name = "lblRefresh";
            this.lblRefresh.Size = new System.Drawing.Size(46, 13);
            this.lblRefresh.TabIndex = 19;
            this.lblRefresh.TabStop = true;
            this.lblRefresh.Text = "Refresh";
            this.lblRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblRefresh_LinkClicked);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 254);
            this.Controls.Add(this.lblRefresh);
            this.Controls.Add(this.txtUnload);
            this.Controls.Add(this.btnInject);
            this.Controls.Add(this.btnEject);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lstInjected);
            this.Controls.Add(this.txtMethod);
            this.Controls.Add(this.txtClass);
            this.Controls.Add(this.txtNamespace);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAssembly);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbProcesses);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Main";
            this.Text = "Sharp Mono Injector";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbProcesses;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtAssembly;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtNamespace;
        private System.Windows.Forms.TextBox txtClass;
        private System.Windows.Forms.TextBox txtMethod;
        private System.Windows.Forms.ListBox lstInjected;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnEject;
        private System.Windows.Forms.Button btnInject;
        private System.Windows.Forms.TextBox txtUnload;
        private System.Windows.Forms.LinkLabel lblRefresh;
    }
}

