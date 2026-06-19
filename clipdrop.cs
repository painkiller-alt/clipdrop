using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace clipdrop
{
    public partial class clipdrop : Form
    {
        // Win32 API
        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Константы сообщений
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 1;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_C = 0x43;

        private DateTime _lastSaveTime = DateTime.MinValue;
        private static int _fileCounter = 1;
        private readonly string _appFolder = AppDomain.CurrentDomain.BaseDirectory;

        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu;

        // Флаг видимости (для синхронизации)
        private bool _isVisible = false;

        public clipdrop()
        {
            InitializeComponent();

            // Настройка PictureBox
            image_box.AllowDrop = true;
            image_box.MouseDown += ImageBox_MouseDown;
            image_box.DragEnter += ImageBox_DragEnter;
            image_box.DragDrop += ImageBox_DragDrop;

            InitializeTray();

            this.TopMost = SettingsManager.GetTopMost();


            // ---- Скрываем при запуске ----
            // Эти свойства нужно установить ДО создания дескриптора окна,
            // чтобы избежать его лишнего пересоздания.
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            notifyIcon.Visible = false;
            _isVisible = false;
        }

        // Этот метод вызывается при создании (или пересоздании) дескриптора окна.
        // Это идеальное место для регистрации горячих клавиш, так как Handle уже точно существует.
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Регистрируем слушатель буфера обмена
            AddClipboardFormatListener(this.Handle);

            // Регистрируем горячую клавишу Alt+Shift+C
            bool registered = RegisterHotKey(this.Handle, HOTKEY_ID, MOD_ALT | MOD_SHIFT, VK_C);
            if (!registered)
            {
                // Если регистрация не удалась, покажем сообщение, но не критично
                // MessageBox.Show("Не удалось зарегистрировать горячую клавишу Alt+Shift+C", "Ошибка");
                System.Diagnostics.Debug.WriteLine("Ошибка регистрации горячей клавиши");
            }
        }

        private void InitializeTray()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Показать", null, (s, e) => ShowApp());
            trayMenu.Items.Add("Выход", null, (s, e) => Application.Exit());

            notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.icon,   // или new Icon(new MemoryStream(...))
                ContextMenuStrip = trayMenu,
                Visible = false,
                Text = "clipdrop – мониторинг буфера"
            };
            notifyIcon.DoubleClick += (s, e) => ToggleVisibility();
        }

        // ---------- Управление видимостью ----------
        private void ShowApp()
        {
            if (_isVisible) return;

            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.BringToFront();
            notifyIcon.Visible = true;
            _isVisible = true;
        }

        private void HideApp()
        {
            if (!_isVisible) return;

            this.Visible = false;
            this.ShowInTaskbar = false;
            notifyIcon.Visible = false;
            _isVisible = false;
        }

        private void ToggleVisibility()
        {
            if (_isVisible)
                HideApp();
            else
                ShowApp();
        }

        // ---------- Обработка сообщений ----------
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                // Защита от дублирования (200 мс)
                if ((DateTime.Now - _lastSaveTime).TotalMilliseconds < 200)
                {
                    base.WndProc(ref m);
                    return;
                }

                try
                {
                    if (Clipboard.ContainsImage())
                    {
                        Image clipboardImage = Clipboard.GetImage();
                        if (clipboardImage != null)
                        {
                            using (Bitmap bmp = new Bitmap(clipboardImage))
                            {
                                image_box.Image?.Dispose();
                                image_box.Image = new Bitmap(bmp);
                                image_box.SizeMode = PictureBoxSizeMode.Zoom;
                                SaveBitmapToFile(bmp);
                                _lastSaveTime = DateTime.Now;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Ошибка буфера: " + ex.Message);
                }
            }
            else if (m.Msg == WM_HOTKEY && (int)m.WParam == HOTKEY_ID)
            {
                // Нажата Alt+Shift+C – переключаем видимость
                ToggleVisibility();
            }

            base.WndProc(ref m);
        }

        // ---------- Сохранение в файл ----------
        private void SaveBitmapToFile(Bitmap bmp)
        {
            string fileName = $"clip_{_fileCounter++}.png";
            string fullPath = Path.Combine(_appFolder, fileName);
            bmp.Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);
            System.Diagnostics.Debug.WriteLine($"Сохранено: {fullPath}");
        }

        // ---------- Drag & Drop ----------
        private void ImageBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (image_box.Image == null) return;
            if (e.Button == MouseButtons.Left && _fileCounter > 1)
            {
                string lastFile = Path.Combine(_appFolder, $"clip_{_fileCounter - 1}.png");
                if (File.Exists(lastFile))
                {
                    DataObject data = new DataObject(DataFormats.FileDrop, new string[] { lastFile });
                    image_box.DoDragDrop(data, DragDropEffects.Copy);
                }
            }
        }

        private void ImageBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string ext = Path.GetExtension(files[0]).ToLower();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif")
                    {
                        e.Effect = DragDropEffects.Copy;
                        return;
                    }
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void ImageBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    try
                    {
                        Image loadedImage = Image.FromFile(files[0]);
                        image_box.Image?.Dispose();
                        image_box.Image = new Bitmap(loadedImage);
                        image_box.SizeMode = PictureBoxSizeMode.Zoom;
                        using (Bitmap bmp = new Bitmap(loadedImage))
                        {
                            SaveBitmapToFile(bmp);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Не удалось загрузить изображение: " + ex.Message);
                    }
                }
            }
        }

        // ---------- Закрытие ----------
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Если закрывают не через Application.Exit() (крестик или Alt+F4)
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                e.Cancel = true;
                HideApp(); // прячем в трей
                return;
            }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            RemoveClipboardFormatListener(this.Handle);
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            notifyIcon?.Dispose();
            base.OnFormClosed(e);
        }

        private void settings_button_Click(object sender, EventArgs e)
        {
            using (settings settingsForm = new settings(this))
            {
                settingsForm.ShowDialog(this);
            }
        }
    }
}