using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.Runtime.InteropServices;

namespace PoorMansTwitch
{
    public partial class Form1 : Form
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        public ChromiumWebBrowser chromeBrowser;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd,
  int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        public Form1()
        {
            InitializeComponent();
            InitializeChromium();

           

            // Alt = 1, Ctrl = 2, Shift = 4, Win = 8
            RegisterHotKey(this.Handle,
              this.GetType().GetHashCode(), 2, (int)'T');
        }

        protected override void WndProc(ref Message m)
        {
            
            if (m.Msg == 0x0312)
            {
                if (this.FormBorderStyle == FormBorderStyle.None)
                {
                    this.FormBorderStyle = FormBorderStyle.Fixed3D;

                    SetWindowLong(this.Handle, GWL_EXSTYLE,
(IntPtr)(GetWindowLong(this.Handle, GWL_EXSTYLE)));

                    this.TransparencyKey = BackColor;
                    SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
                    this.Opacity = 1.0f;
                }
                else
                {

                    this.FormBorderStyle = FormBorderStyle.None;

                    SetWindowLong(this.Handle, GWL_EXSTYLE,
(IntPtr)(GetWindowLong(this.Handle, GWL_EXSTYLE) | WS_EX_LAYERED | WS_EX_TRANSPARENT));
                    // set transparency to 50% (128)
                    SetLayeredWindowAttributes(this.Handle, 0, 128, LWA_ALPHA);
                    this.TopMost = true;

                    this.TransparencyKey = BackColor;
                    this.Opacity = .50f;
                    SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);


                }

            }
            
              
            base.WndProc(ref m);
        }

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            // Initialize cef with the provided settings
            Cef.Initialize(settings);
            // Create a browser component
            //about:blank
            chromeBrowser = new ChromiumWebBrowser("https://www.twitch.tv/");
            // Add it to the form and fill it to the form window.
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

            
        }


        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Control == true && e.KeyCode == Keys.V)
            {
                //PasteFromClipboard(); //where you handle the actual pasting from the clipboard
                textBox1.Clear();
                textBox1.Text += Clipboard.GetText(TextDataFormat.Text);
            }

        }

        //Selects everything in TextBox1.
        private void textBox1_Click(object sender, System.EventArgs e)
        {
            textBox1.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Visible = false;
            button1.Visible = false;
            label1.Visible = false;

            IFrame mainbrowserwindow = chromeBrowser.GetMainFrame();
            mainbrowserwindow.LoadUrl(textBox1.Text);
        }
    }
}
