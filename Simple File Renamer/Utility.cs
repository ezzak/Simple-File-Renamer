using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace File_Renamer {

  public static class Utility {
    public static string RemovePath(string fileName) {
      return fileName.Substring(fileName.LastIndexOf("\\") + 1);
    }

    public static string GetPath(string fileName) {
      return fileName.Substring(0, fileName.LastIndexOf("\\") + 1);
    }

    public static string RemoveExtension(string s) {
      if (s.LastIndexOf(".") > 0)
        return s.Substring(0, s.LastIndexOf('.'));
      else
        return null;
    }

    public static string GetExtension(string s) {
      if (s.LastIndexOf(".") > 0)
        return s.Substring(s.LastIndexOf('.'));
      else
        return null;
    }

    public static bool ValidFileName(string fileName) {
      if (fileName.Contains("/") ||
          fileName.Contains("\\") ||
          fileName.Contains("?") ||
          fileName.Contains("<") ||
          fileName.Contains(">") ||
          fileName.Contains("*") ||
          fileName.Contains(":") ||
          fileName.Contains("|") ||
          fileName.Contains("\"")
        ) {
        return false;
      } else return true;
    }

    public static bool ValidExtension(string s) {

      if (s[0] == '.') {
        s = s.Substring(1);
      }
      if (s.Contains('.'))
        return false;
      else
        return true;
    }

    public static bool NonNullOrEmptyString(string s) {
      if (s != null && s != "" && s != "\n" && s != "\t")
        return true;
      return false;
    }

    public static bool RenameAFile(string path, string oldFileName, string newFileName) {
      //if new doesn't exist and old exists
      if (File.Exists(path + oldFileName) && !File.Exists(path + newFileName)) {
        //if new filename valid
        if (Utility.ValidFileName(newFileName)) {
          File.Move(path + oldFileName, path + newFileName);
          return true;
        } else {
          MessageBox.Show("File name \"" + newFileName + "\" is invalid (contains special character(s)).\n" + "File will be skipped.", "Warning");
        }
      }
      return false;
    }

    public static bool CheckNumber(ref TextBox tb, ref int val, ref Label label) {

      if (Utility.NonNullOrEmptyString(tb.Text)) {
        try {
          val = int.Parse(tb.Text);
          if (val < 0) throw new Exception("Invalid value for " + label.Text);
        } catch (Exception e) {

          MessageBox.Show(e.Message);
          tb.Clear();
          tb.Focus();
          return false;
        }

      } else {
        MessageBox.Show("No value entered for" + label.Text, "Warning");
        return false;
      }

      return true;
    }

    public static int CheckLength(ref TextBox tb, int boxHeight) {
      if (tb.Lines.Length > boxHeight) {

        int newHeight = boxHeight;
        for (int i = 0; i < tb.Lines.Length - boxHeight; i++) {
          tb.Parent.Parent.Parent.Parent.Height += 10;
          newHeight += 1;
        }

        tb.SelectionStart = 0;
        tb.ScrollToCaret();

        return newHeight;
      }

      return 4;
    }

    public static DialogResult InputBox(string title, string promptText, ref string value, Icon icon) {
      Form form = new Form();
      Label label = new Label();
      TextBox textBox = new TextBox();
      Button buttonOk = new Button();
      Button buttonCancel = new Button();

      form.Text = title;
      label.Text = promptText;
      textBox.Text = value;

      buttonOk.Text = "OK";
      buttonCancel.Text = "Cancel";
      buttonOk.DialogResult = DialogResult.OK;
      buttonCancel.DialogResult = DialogResult.Cancel;

      label.SetBounds(9, 5, 372, 13);
      textBox.SetBounds(12, 21, 372, 45);
      buttonOk.SetBounds(228, 72, 75, 23);
      buttonCancel.SetBounds(309, 72, 75, 23);

      //textBox.Multiline = true;
      textBox.ScrollBars = ScrollBars.Vertical;
      textBox.AcceptsReturn = true;


      label.AutoSize = true;
      textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
      buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

      form.ClientSize = new Size(396, 107);
      form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
      form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
      form.FormBorderStyle = FormBorderStyle.FixedDialog;
      form.StartPosition = FormStartPosition.CenterScreen;
      form.MinimizeBox = false;
      form.MaximizeBox = false;
      form.AcceptButton = buttonOk;
      form.CancelButton = buttonCancel;
      form.Icon = icon;

      DialogResult dialogResult = form.ShowDialog();
      value = textBox.Text;
      return dialogResult;
    }

    public static Stream GenerateStreamFromString(string s) {
      MemoryStream stream = new MemoryStream();
      StreamWriter writer = new StreamWriter(stream);
      writer.Write(s);
      writer.Flush();
      stream.Position = 0;
      return stream;
    }

    public static void ActivateVerticalScrollBars(ref TextBox tb) {
      //set scroll bars
      if (tb.ScrollBars == ScrollBars.None)
        tb.ScrollBars = ScrollBars.Vertical;
    }

    public static void DisableVerticalScrollBars(ref TextBox tb) {
      //disable scroll bars
      if (tb.ScrollBars == ScrollBars.Vertical)
        tb.ScrollBars = ScrollBars.None;
    }

    public static void HidePages(ref List<TableLayoutPanel> mainPages) {
      foreach (TableLayoutPanel panel in mainPages) {
        panel.Hide();
      }
    }

    public static IEnumerable<Control> GetAll(Control control, Type type) {
      var ctrls = control.Controls.Cast<Control>();

      return ctrls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(ctrls).Where(c => c.GetType() == type);
    }

    public static T ParseEnum<T>(string value) {
      return (T)Enum.Parse(typeof(T), value, true);
    }
  }
}
