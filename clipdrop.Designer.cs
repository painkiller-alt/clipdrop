namespace clipdrop
{
    partial class clipdrop
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
            image_box = new PictureBox();
            settings_button = new Button();
            ((System.ComponentModel.ISupportInitialize)image_box).BeginInit();
            SuspendLayout();
            // 
            // image_box
            // 
            image_box.Image = Properties.Resources.image_placeholder;
            image_box.Location = new Point(0, 22);
            image_box.Name = "image_box";
            image_box.Size = new Size(231, 152);
            image_box.SizeMode = PictureBoxSizeMode.Zoom;
            image_box.TabIndex = 0;
            image_box.TabStop = false;
            // 
            // settings_button
            // 
            settings_button.Location = new Point(0, -1);
            settings_button.Name = "settings_button";
            settings_button.Size = new Size(231, 23);
            settings_button.TabIndex = 1;
            settings_button.Text = "Настройки";
            settings_button.UseVisualStyleBackColor = true;
            settings_button.Click += settings_button_Click;
            // 
            // clipdrop
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(232, 173);
            Controls.Add(settings_button);
            Controls.Add(image_box);
            Name = "clipdrop";
            Text = "clipdrop";
            this.Icon = Properties.Resources.icon;
            ((System.ComponentModel.ISupportInitialize)image_box).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox image_box;
        private Button settings_button;
    }
}
