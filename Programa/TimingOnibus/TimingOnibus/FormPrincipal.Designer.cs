namespace TimingOnibus
{
    partial class FormPrincipal
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
            lbl_linha = new Label();
            cbx_linha = new ComboBox();
            btn_calcular = new Button();
            cbx_pontos = new ComboBox();
            lbl_pontos = new Label();
            ltx_log = new ListBox();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            SuspendLayout();
            // 
            // lbl_linha
            // 
            lbl_linha.AutoSize = true;
            lbl_linha.Location = new Point(1088, 9);
            lbl_linha.Name = "lbl_linha";
            lbl_linha.Size = new Size(79, 15);
            lbl_linha.TabIndex = 0;
            lbl_linha.Text = "Digite a linha:";
            // 
            // cbx_linha
            // 
            cbx_linha.FormattingEnabled = true;
            cbx_linha.Location = new Point(1177, 5);
            cbx_linha.Name = "cbx_linha";
            cbx_linha.Size = new Size(238, 23);
            cbx_linha.TabIndex = 1;
            // 
            // btn_calcular
            // 
            btn_calcular.Location = new Point(1091, 42);
            btn_calcular.Name = "btn_calcular";
            btn_calcular.Size = new Size(324, 35);
            btn_calcular.TabIndex = 2;
            btn_calcular.Text = "Calcular";
            btn_calcular.UseVisualStyleBackColor = true;
            btn_calcular.Click += btn_calcular_Click;
            // 
            // cbx_pontos
            // 
            cbx_pontos.FormattingEnabled = true;
            cbx_pontos.Location = new Point(1216, 91);
            cbx_pontos.Name = "cbx_pontos";
            cbx_pontos.Size = new Size(199, 23);
            cbx_pontos.TabIndex = 3;
            // 
            // lbl_pontos
            // 
            lbl_pontos.AutoSize = true;
            lbl_pontos.Location = new Point(1088, 95);
            lbl_pontos.Name = "lbl_pontos";
            lbl_pontos.Size = new Size(109, 15);
            lbl_pontos.TabIndex = 4;
            lbl_pontos.Text = "Pontos disponíveis:";
            // 
            // ltx_log
            // 
            ltx_log.Font = new Font("Microsoft Sans Serif", 12F);
            ltx_log.FormattingEnabled = true;
            ltx_log.ItemHeight = 20;
            ltx_log.Location = new Point(1091, 119);
            ltx_log.Name = "ltx_log";
            ltx_log.Size = new Size(324, 484);
            ltx_log.TabIndex = 5;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Location = new Point(12, 5);
            webView21.Name = "webView21";
            webView21.Size = new Size(1053, 598);
            webView21.TabIndex = 6;
            webView21.ZoomFactor = 1D;
            // 
            // FormPrincipal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1427, 608);
            Controls.Add(webView21);
            Controls.Add(ltx_log);
            Controls.Add(lbl_pontos);
            Controls.Add(cbx_pontos);
            Controls.Add(btn_calcular);
            Controls.Add(cbx_linha);
            Controls.Add(lbl_linha);
            Name = "FormPrincipal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FormPrincipal";
            Load += FormPrincipal_Load;
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_linha;
        private ComboBox cbx_linha;
        private Button btn_calcular;
        private ComboBox cbx_pontos;
        private Label lbl_pontos;
        private ListBox ltx_log;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
    }
}
