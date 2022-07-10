using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tzhtec.Controls
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public static string Show(string Prompt, string Title, string Default, IWin32Window owner)
        {
            InputBox input = new InputBox();
            input.Text = string.IsNullOrEmpty(Title) ? "输入" : Title;
            input.txtValue.Text = Default;
            input.lblInfo.Text = Prompt;
            if (input.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
                return input.txtValue.Text;
            return string.Empty;
        }

        public static string Show(string Prompt, string Title, string Default)
        {
            return Show(Prompt, Title, Default, null);
        }

        public static string Show(string Prompt, string Title)
        {
            return Show(Prompt, Title, "", null);
        }

        public static string Show(string Prompt)
        {
            return Show(Prompt, "输入", "", null);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
