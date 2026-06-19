using System;
using System.Windows.Forms;

namespace clipdrop
{
    public partial class settings : Form
    {
        private Button toggleButton;
        private CheckBox topMostCheckBox;
        private readonly Form _mainForm;

        public settings(Form mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;

            this.Text = "Настройки";
            this.Size = new System.Drawing.Size(280, 150);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;

            topMostCheckBox = new CheckBox
            {
                Text = "Поверх всех окон",
                Size = new System.Drawing.Size(180, 25),
                Location = new System.Drawing.Point(50, 20)
            };
            topMostCheckBox.CheckedChanged += TopMostCheckBox_CheckedChanged;
            this.Controls.Add(topMostCheckBox);

            toggleButton = new Button
            {
                Size = new System.Drawing.Size(180, 35),
                Location = new System.Drawing.Point(50, 60)
            };
            toggleButton.Click += ToggleButton_Click;
            this.Controls.Add(toggleButton);
        }

        private void settings_Load(object sender, EventArgs e)
        {
            topMostCheckBox.Checked = SettingsManager.GetTopMost();
            UpdateButtonText();
        }

        private void TopMostCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = topMostCheckBox.Checked;
            _mainForm.TopMost = isChecked;
            SettingsManager.SetTopMost(isChecked);
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            bool current = SettingsManager.GetAutoStart();
            SettingsManager.SetAutoStart(!current);
            UpdateButtonText();
        }

        private void UpdateButtonText()
        {
            bool isEnabled = SettingsManager.GetAutoStart();
            toggleButton.Text = isEnabled ? "Отключить автозагрузку" : "Включить автозагрузку";
        }
    }
}