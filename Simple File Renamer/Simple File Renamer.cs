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

  public partial class FileRenamer : Form {

    //page 1 Variables
    private string oldFileListPath, newFileListName;

    ///COMMON VARIABLES
    private const int pageNb = 6;
    string[] currDirectory = new string[pageNb];

    private string desktopDir = ////"C:\\Users\\user\\Desktop\\File Renamer\\File Renamer Releases\\Test Files";
        System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    //used to resize form
    private int tbHeight;

    //contains list of the main pages
    private List<TableLayoutPanel> mainPages = new List<TableLayoutPanel>();

    //themes
    private enum Themes { JustRed, JustBlue, JustGreen, JustOrange, Coral, Beach, Default };

    //constructor
    public FileRenamer() {
      InitializeComponent();
      Initialize();
    }

    private void Initialize() {

      //set initial textbox height
      tbHeight = 4;

      //set initial directories for file dialogs
      openNewFileListDialogPg1.InitialDirectory = openOldFileListDialogPg1.InitialDirectory = desktopDir;
      openOldFileDialogPg2.InitialDirectory = openOldFileDialogPg3.InitialDirectory = desktopDir;
      openOldFileDialogPg4.InitialDirectory = openOldFileDialogPg5.InitialDirectory = desktopDir;
      openOldFileDialogPg6.InitialDirectory = desktopDir;

      for (int i = 0; i < currDirectory.Length; i++)
        currDirectory[i] = "";

      mainPages.Add(tableLayoutPanelMainPage1);
      mainPages.Add(tableLayoutPanelMainPage2);
      mainPages.Add(tableLayoutPanelMainPage3);
      mainPages.Add(tableLayoutPanelMainPage4);
      mainPages.Add(tableLayoutPanelMainPage5);
      mainPages.Add(tableLayoutPanelMainPage6);
      mainPages.Add(tableLayoutPanelMainPageSettings);


      if (!File.Exists(Application.StartupPath + "theme.txt")) {
        using (StreamWriter sw = File.CreateText(Application.StartupPath + "theme.txt")) {
          sw.WriteLine("Default");
          setTheme(Themes.Default);
        }
      } else {
        using (StreamReader sr = File.OpenText(Application.StartupPath + "theme.txt")) {
          string s = sr.ReadLine();
          setTheme(Utility.ParseEnum<Themes>(s));
        }
      }

      if (!File.Exists(Application.StartupPath + "options.txt")) {
        using (StreamWriter sw = File.CreateText(Application.StartupPath + "options.txt")) {
          sw.WriteLine("KeepListChecked");
          checkBoxOptionsKLA.Checked = true;

        }
      } else {
        using (StreamReader sr = File.OpenText(Application.StartupPath + "options.txt")) {
          string s = sr.ReadLine();
          if (s == "KeepListChecked")
            checkBoxOptionsKLA.Checked = true;
          else if (s == "KeepListUnchecked")
            checkBoxOptionsKLA.Checked = false;
        }
      }

      Utility.HidePages(ref mainPages);
      tableLayoutPanelMainPage2.Show();
    }

    ///PAGE 1 AUTOMATIC MODE
    //Old names list select
    private void oldNames_Click(object sender, EventArgs e) {

      if (openOldFileListDialogPg1.ShowDialog() == DialogResult.OK) {
        //save name of old file list
        oldFileListPath = openOldFileListDialogPg1.FileName;
        //change text of button based on list name
        buttonOldNamesDialogPg1.Text = Utility.RemovePath(oldFileListPath);
        //set next initial directory for file dialog
        openOldFileListDialogPg1.InitialDirectory = Utility.GetPath(oldFileListPath);
      }
    }
    //New names list select
    private void newNames_Click(object sender, EventArgs e) {

      if (openNewFileListDialogPg1.ShowDialog() == DialogResult.OK) {
        //save name od new file list
        newFileListName = openNewFileListDialogPg1.FileName;
        //change text of button based on list name
        buttonNewNamesDialogPg1.Text = Utility.RemovePath(newFileListName);
        //set next initial directory for fie dialog
        openNewFileListDialogPg1.InitialDirectory = Utility.GetPath(newFileListName);
      }
    }
    //Start Renaming Automatic (Lists Provided)
    private void StartAutomatic_Click(object sender, EventArgs e) {
      //if both file list names are non-empy and non-null
      if (Utility.NonNullOrEmptyString(newFileListName) && Utility.NonNullOrEmptyString(oldFileListPath)) {
        //convert both files to streamReaders
        StreamReader oldSR = new StreamReader(oldFileListPath);
        StreamReader newSR = new StreamReader(newFileListName);
        //set currDirectory
        currDirectory[0] = Utility.GetPath(oldFileListPath);
        //atempt rename
        AttemptRenameAutomatic(oldSR, newSR, currDirectory[0]);
        //succes of operation
        MessageBox.Show("Renaming Complete", "Success");
      }
    }

    ///PAGE 2 MANUAL MODE
    //Add to Old List
    private void buttonOldFileDialog_Click(object sender, EventArgs e) {
      //call populate text box function
      PopulateOldFileTextBoxButtonClick(ref textBoxOldPg2, ref e, ref buttonOldFileDialogPg2,
          ref openOldFileDialogPg2, ref currDirectory[1], ref tbHeight);
    }
    //Add to New List
    private void buttonNewFileDialog_Click(object sender, EventArgs e) {
      //set scroll bars
      Utility.ActivateVerticalScrollBars(ref textBoxOldPg2);

      //create string which will receive user input
      string fileNameInput = "";

      //call pop up text box
      if (Utility.InputBox("File Renamer", "Enter New File Name", ref fileNameInput, this.Icon) == DialogResult.OK)
        //check that result is non Null
        if (Utility.NonNullOrEmptyString(fileNameInput))
          //append to text box
          textBoxNewPg2.AppendText(fileNameInput + "\n");
      //adjust box length
      tbHeight = Utility.CheckLength(ref textBoxNewPg2, tbHeight);
    }
    //start MANUAL MODE
    private void StartManual_Click(object sender, EventArgs e) {
      if (textBoxOldPg2.Text != "" && textBoxNewPg2.Text != "") {

        //generate stream readers 
        StreamReader oldSR = new StreamReader(Utility.GenerateStreamFromString(textBoxOldPg2.Text));
        StreamReader newSR = new StreamReader(Utility.GenerateStreamFromString(textBoxNewPg2.Text));

        AttemptRenameManual(oldSR, newSR, currDirectory[1], ref textBoxOldPg2, ref buttonOldFileDialogPg2);

        //clear text boxes
        textBoxNewPg2.Clear();
        //enable oldTextbot
        //buttonOldFileDialogPg2.Enabled = true;

      }
    }

    ///PAGE 3 INSERT MODE
    //Add to Old List
    private void buttonOldFileDialogPg3_Click(object sender, EventArgs e) {
      //call populate text box function
      PopulateOldFileTextBoxButtonClick(ref textBoxOldPg3, ref e, ref buttonOldFileDialogPg3,
          ref openOldFileDialogPg3, ref currDirectory[2], ref tbHeight);
    }
    //start INSERT MODE
    private void buttonStartInsert_Click(object sender, EventArgs e) {

      if (textBoxOldPg3.Lines.Count() == 0)
        return;
      string insString;
      int index = -1;

      if (Utility.NonNullOrEmptyString(textBoxTextToInsertPg3.Text)) {

        if (Utility.ValidFileName(textBoxTextToInsertPg3.Text)) {
          insString = textBoxTextToInsertPg3.Text;
        } else {
          MessageBox.Show("Invalid text to insert", "Warning");
          textBoxTextToInsertPg3.Clear();
          textBoxTextToInsertPg3.Focus();
          return;
        }

      } else {
        MessageBox.Show("Nothing to insert", "Warning");
        return;
      }

      if (Utility.CheckNumber(ref textBoxInsertionIndexPg3, ref index, ref labelInstructionsLeftPg3) == false)
        return;

      string newName;

      string[] newBox = new string[textBoxOldPg3.Lines.Count()];
      int count = 0;

      foreach (string name in textBoxOldPg3.Lines) {

        if (Utility.NonNullOrEmptyString(name)) {

          int nameLength = name.Length - Utility.GetExtension(name).Length;
          if (index <= nameLength)
            if (checkBoxInsertBack.Checked == false)
              newName = name.Insert(index, insString);
            else
              newName = name.Insert(nameLength - index, insString);
          else {
            MessageBox.Show("Invalid Position for \"" + name + "\" file.\n Skipping File.");
            newBox[count] = name;
            continue;
          }
          newBox[count] = Utility.RenameAFile(currDirectory[2], name, newName) ? newName : name;
          count++;
        }
      }

      finalizeBox(ref textBoxOldPg3, ref buttonOldFileDialogPg3, ref newBox);

    }

    ///PAGE 4 DELETE MODE
    //Add to Old List
    private void buttonOldFileDialogPg4_Click(object sender, EventArgs e) {
      PopulateOldFileTextBoxButtonClick(ref textBoxOldPg4, ref e, ref buttonOldFileDialogPg4,
          ref openOldFileDialogPg4, ref currDirectory[3], ref tbHeight);
    }
    //start DELETE MODE
    private void buttonStartDelete_Click(object sender, EventArgs e) {
      if (0 == textBoxOldPg4.Lines.Count())
        return;
      if (tabControlPg4.SelectedTab == tabPageDeleteFromPos)
        tabDeleteFromPos(ref sender, ref e);
      else
        tabDeleteExact(ref sender, ref e);
    }
    //delete certain size from certain posistion
    private void tabDeleteFromPos(ref object sender, ref EventArgs e) {

      int startingInd = -1;
      int sizeToDel = -1;

      if (Utility.CheckNumber(ref textBoxStartingPosPg4, ref startingInd, ref labelInstructionStartingPositionPg4) == false)
        return;
      if (Utility.CheckNumber(ref textBoxNbCharPg4, ref  sizeToDel, ref labelInstructionNbCharPg4) == false)
        return;

      string newName;
      string tempName;

      string[] newBox = new string[textBoxOldPg4.Lines.Count()];
      int count = 0;

      foreach (string name in textBoxOldPg4.Lines) {

        if (Utility.NonNullOrEmptyString(name)) {
          string extension = Utility.GetExtension(name);
          int nameLength = name.Length - extension.Length;
          tempName = Utility.RemoveExtension(name);
          if (startingInd <= nameLength)
            if (checkBoxDeleteBack.Checked == false) {
              if (nameLength - startingInd < sizeToDel)
                sizeToDel = nameLength - startingInd;
              newName = tempName.Substring(0, startingInd) + tempName.Substring(startingInd + sizeToDel) + extension;
            } else {
              if (startingInd < sizeToDel)
                sizeToDel = startingInd;
              newName = tempName.Substring(0, nameLength - startingInd);
              newName += tempName.Substring(nameLength - startingInd + sizeToDel) + extension;
            } else {
            newBox[count] = name;
            MessageBox.Show("Invalid Position for \"" + name + "\" file.\n Skipping File.");
            continue;
          }
          newBox[count] = Utility.RenameAFile(currDirectory[3], name, newName) ? newName : name;
          count++;
        }
      }

      finalizeBox(ref textBoxOldPg4, ref buttonOldFileDialogPg4, ref newBox);
    }
    //delete exact word
    private void tabDeleteExact(ref object sender, ref EventArgs e) {
      string newFileName;
      string[] newBox = new string[textBoxOldPg4.Lines.Count()];
      int count = 0;
      foreach (string fileName in textBoxOldPg4.Lines) {
        if (Utility.NonNullOrEmptyString(fileName)) {
          newFileName = fileName;
          while (newFileName.Contains(textBoxWordToDeletePg4.Text)) {
            newFileName = newFileName.Replace(textBoxWordToDeletePg4.Text, "");
          }
          newBox[count] = Utility.RenameAFile(currDirectory[3], fileName, newFileName) ? newFileName : fileName;
          count++;
        }
      }
      finalizeBox(ref textBoxOldPg4, ref buttonOldFileDialogPg4, ref newBox);
    }

    ///PAGE 5 CASECONVERSION MODE
    //Add to Old List
    private void buttonOldFileDialogPg5_Click(object sender, EventArgs e) {
      PopulateOldFileTextBoxButtonClick(ref textBoxOldPg5, ref e, ref buttonOldFileDialogPg5,
         ref openOldFileDialogPg5, ref currDirectory[4], ref tbHeight);
    }
    //start CASECONVERSION MODE
    private void buttonStartCaseconversion_Click(object sender, EventArgs e) {

      if (textBoxOldPg5.Lines.Count() == 0)
        return;
      if (!checkBoxLowercase.Checked && !checkBoxUppercase.Checked && !checkBoxPropercase.Checked) {
        return;
      }

      checkBoxLowercase.Hide();
      checkBoxPropercase.Hide();
      checkBoxUppercase.Hide();
      string temp = "temp.temp";
      string[] newBox = new string[textBoxOldPg5.Lines.Count()];
      int count = 0;
      foreach (string name in textBoxOldPg5.Lines) {
        if (Utility.NonNullOrEmptyString(name)) {
          string newName = "";

          if (checkBoxUppercase.Checked) {
            newName = Utility.RemoveExtension(name).ToUpper() + Utility.GetExtension(name);
          } else if (checkBoxLowercase.Checked) {
            newName = Utility.RemoveExtension(name).ToLower() + Utility.GetExtension(name);
          } else {
            newName = name.ToLower();
            if (newName[0] >= 97 && newName[0] <= 122)
              newName = (char)((int)newName[0] - 32) + newName.Remove(0, 1);
          }

          Random random = new Random();

          temp = "temp.temp";
          while (File.Exists(temp)) {
            temp = ((char)random.Next(97, 122)) + temp;
          }
          Utility.RenameAFile(currDirectory[4], name, temp);
          Utility.RenameAFile(currDirectory[4], temp, newName);
          newBox[count] = newName;
          count++;
        }
      }

      if (File.Exists(temp))
        File.Delete(temp);

      checkBoxLowercase.Show();
      checkBoxPropercase.Show();
      checkBoxUppercase.Show();

      finalizeBox(ref textBoxOldPg5, ref buttonOldFileDialogPg5, ref newBox);
    }

    ///PAGE 6 EXTENSION MODE
    //Add to old list
    private void buttonOldFileDialogPg6_Click(object sender, EventArgs e) {
      PopulateOldFileTextBoxButtonClick(ref textBoxOldPg6, ref e, ref buttonOldFileDialogPg6,
          ref openOldFileDialogPg6, ref currDirectory[5], ref tbHeight);
    }
    //Set options - new extensions
    private void buttonModePg6_Click(object sender, EventArgs e) {
      Button button = sender as Button;

      if (button.Text != "Same Extension Mode") {
        button.Text = "Same Extension Mode";
        textBoxNewPg6.Multiline = false;
        labelExtensionMode.Text = "Enter single extension for all files";
      } else {
        button.Text = "One to One Extension Mode";
        textBoxNewPg6.Multiline = true;
        labelExtensionMode.Text = "Enter extension for each file";
      }
    }
    //start EXTENSION MODE
    private void buttonStartExtension_Click(object sender, EventArgs e) {
      if (textBoxOldPg6.Lines.Count() == 0)
        return;

      string newName;
      string[] newBox = new string[textBoxOldPg6.Lines.Count()];
      int count = 0;
      string[] newExtensions = textBoxNewPg6.Lines;
      int index = 0;
      bool type = textBoxNewPg6.Multiline;

      foreach (string name in textBoxOldPg6.Lines) {
        if (Utility.NonNullOrEmptyString(name)) {
          newName = "";
          if (index < newExtensions.Length) {
            if (Utility.ValidExtension(newExtensions[index])) {
              newName = Utility.RemoveExtension(name);
              if (newExtensions[index][0] == '.')
                newName += newExtensions[index];
              else
                newName += '.' + newExtensions[index];

              Utility.RenameAFile(currDirectory[5], name, newName);
              newBox[count] = newName;
              count++;
            } else {
              newBox[count] = name;
              count++;
            }
            if (type) index++;
          } else {
            newBox[count] = name;
            count++;
          }
        }
      }
      textBoxNewPg6.Clear();
      finalizeBox(ref textBoxOldPg6, ref buttonOldFileDialogPg6, ref newBox);
    }

    ///Tool Strip Menus functions  
    //show page 1
    private void automaticToolStripMenuItem_Click(object sender, EventArgs e) {
      Utility.HidePages(ref mainPages);
      tableLayoutPanelMainPage1.Show();
    }
    //show page 2
    private void manualToolStripMenuItem_Click(object sender, EventArgs e) {
      Utility.HidePages(ref mainPages);
      tableLayoutPanelMainPage2.Show();
    }
    //show page 3
    private void insertToolStripMenuItem_Click(object sender, EventArgs e) {
      Utility.HidePages(ref mainPages);
      tableLayoutPanelMainPage3.Show();
    }
    //show page 4
    private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
      Utility.HidePages(ref mainPages);
      tableLayoutPanelMainPage4.Show();
    }
    //show page 5
    private void caseConversionToolStripMenuItem_Click(object sender, EventArgs e) {
      Utility.HidePages(ref mainPages);
      tableLayoutPanelMainPage5.Show();
    }
    //show page 6
    private void extensionToolStripMenuItem_Click(object sender, EventArgs e) {
      Utility.HidePages(ref mainPages);
      tableLayoutPanelMainPage6.Show();
    }
    //Show settings
    private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
      Utility.HidePages(ref mainPages);
      tableLayoutPanelMainPageSettings.Show();
    }
    ///Extra Functions
    private void buttonHoverToolStrip(object sender, EventArgs e) {
      this.Cursor = Cursors.Arrow;
    }

    //populate an old text box
    //@args
    //ref TextBot
    //ref EventArgs
    //ref Button
    //ref FileDialog
    //ref string CurrDirectory
    //ref int BoxHeight
    private void PopulateOldFileTextBoxButtonClick(ref TextBox tb, ref EventArgs e, ref Button tbButton, ref OpenFileDialog openFileDialog, ref string currDirectory, ref int boxHeight) {

      //if box is to be cleared
      if (tbButton.Text == "Clear List") {
        tb.Clear();
        tbButton.Text = "Add Old File(s)";
        Utility.DisableVerticalScrollBars(ref tb);
        return;
      }

      //enable multiselect
      openFileDialog.Multiselect = true;

      if (openFileDialog.ShowDialog() == DialogResult.OK) {
        Utility.ActivateVerticalScrollBars(ref tb);

        foreach (string fileName in openFileDialog.FileNames) {
          //remove path from filename
          string name = Utility.RemovePath(fileName);
          //append to textbox if valid
          if (Utility.NonNullOrEmptyString(name))
            tb.AppendText(name + "\n");

        }

        //set current directory based on curr position of files
        currDirectory = Utility.GetPath(openFileDialog.FileName);
        //readjust box height
        boxHeight = Utility.CheckLength(ref tb, boxHeight);
        //reset text value of textbox
        tbButton.Text = "Clear List";
      }
      openFileDialog.InitialDirectory = currDirectory;
    }

    //populate text box based on dragged items
    private void PopulateOldFileTextBoxDrag(ref TextBox tb, ref DragEventArgs e, ref Button tbButton, ref string currDir, ref int boxHeight) {

      string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);

      if (fileNames.Length == 0)
        return;

      Utility.ActivateVerticalScrollBars(ref tb);

      tb.Clear();
      foreach (string fileName in fileNames) {
        //set current directory based on curr position of files
        currDir = Utility.GetPath(fileName);
        //remove path from file name
        string name = Utility.RemovePath(fileName);
        if (Utility.NonNullOrEmptyString(name))
          tb.AppendText(name + "\n");
      }

      //readjust box height
      boxHeight = Utility.CheckLength(ref tb, boxHeight);
      //reset text value of textbox
      tbButton.Text = "Clear List";

    }
    //display release version
    private void versionToolStripMenuItem_Click(object sender, EventArgs e) {
      MessageBox.Show("Version 1.0.0.7 Alpha Build", "Version");
    }
    ///EVENTS
    private void tb_DragDrop(object sender, DragEventArgs e) {
      TextBox tb = sender as TextBox;

      if (tb == textBoxOldPg2)
        PopulateOldFileTextBoxDrag(ref tb, ref e, ref buttonOldFileDialogPg2, ref currDirectory[1], ref tbHeight);
      else if (tb == textBoxOldPg3)
        PopulateOldFileTextBoxDrag(ref tb, ref e, ref buttonOldFileDialogPg3, ref currDirectory[2], ref tbHeight);
      else if (tb == textBoxOldPg4)
        PopulateOldFileTextBoxDrag(ref tb, ref e, ref buttonOldFileDialogPg4, ref currDirectory[3], ref tbHeight);
      else if (tb == textBoxOldPg5)
        PopulateOldFileTextBoxDrag(ref tb, ref e, ref buttonOldFileDialogPg5, ref currDirectory[4], ref tbHeight);
      else if (tb == textBoxOldPg6)
        PopulateOldFileTextBoxDrag(ref tb, ref e, ref buttonOldFileDialogPg6, ref currDirectory[5], ref tbHeight);
      else return;

    }

    private void tb_DragOver(object sender, DragEventArgs e) {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.Copy;
      else
        e.Effect = DragDropEffects.None;
    }

    private void KLA_Click(object sender, EventArgs e) {
      if (checkBoxOptionsKLA.Checked) {
        System.IO.File.WriteAllText(Application.StartupPath + "options.txt", "KeepListChecked");
      } else {
        System.IO.File.WriteAllText(Application.StartupPath + "options.txt", "KeepListUnchecked");
      }
    }


    ///THEMES
    Color colorOffwhite = Color.FromArgb(240, 240, 240);
    //Just Red Theme
    private void justRedToolStripMenuItem_Click(object sender, EventArgs e) {
      setTheme(Themes.JustRed);
      System.IO.File.WriteAllText(Application.StartupPath + "theme.txt", Themes.JustRed.ToString());
    }

    //Just Blue Theme
    private void justBlueToolStripMenuItem_Click(object sender, EventArgs e) {
      setTheme(Themes.JustBlue);
      System.IO.File.WriteAllText(Application.StartupPath + "theme.txt", Themes.JustBlue.ToString());
    }

    //Just Green Theme
    private void justGreenToolStripMenuItem_Click(object sender, EventArgs e) {
      setTheme(Themes.JustGreen);
      System.IO.File.WriteAllText(Application.StartupPath + "theme.txt", Themes.JustGreen.ToString());
    }

    //Just Orange Theme
    private void justOrangeToolStripMenuItem_Click(object sender, EventArgs e) {
      setTheme(Themes.JustOrange);
      System.IO.File.WriteAllText(Application.StartupPath + "theme.txt", Themes.JustOrange.ToString());
    }

    //Coral Theme
    private void coralToolStripMenuItem_Click(object sender, EventArgs e) {
      setTheme(Themes.Coral);
      System.IO.File.WriteAllText(Application.StartupPath + "theme.txt", Themes.Coral.ToString());
    }

    //Beach Theme
    private void beachToolStripMenuItem_Click(object sender, EventArgs e) {
      setTheme(Themes.Beach);
      System.IO.File.WriteAllText(Application.StartupPath + "theme.txt", Themes.Beach.ToString());
    }

    //Default Theme
    private void resetDefaultToolStripMenuItem_Click(object sender, EventArgs e) {
      setTheme(Themes.Default);
      System.IO.File.WriteAllText(Application.StartupPath + "theme.txt", Themes.Default.ToString());
    }

    private void setTheme(Themes theme) {

      Color frontC, backC, restC;
      switch ((int)theme) {

        case 0:
          frontC = Color.FromArgb(220, 0, 0);
          backC = Color.Black;
          restC = Color.FromArgb(255, 64, 64);
          break;

        case 1:
          frontC = Color.FromArgb(0, 66, 255);
          backC = Color.Black;
          restC = Color.FromArgb(126, 192, 238);
          break;

        case 2:
          frontC = Color.FromArgb(30, 245, 30);
          backC = Color.Black;
          restC = Color.FromArgb(0, 238, 118);
          break;

        case 3:
          frontC = Color.FromArgb(255, 127, 0);
          backC = Color.Black;
          restC = Color.FromArgb(255, 165, 79);
          break;

        case 4:
          frontC = Color.DarkTurquoise;
          backC = Color.DarkSlateBlue;
          restC = colorOffwhite;
          break;

        case 5:
          frontC = Color.LimeGreen;
          backC = Color.FromArgb(150, 113, 23);
          restC = Color.DarkGreen;
          break;

        default:
          frontC = Color.Gray;
          backC = Color.Black;
          restC = colorOffwhite;
          break;
      }

      menuStrip.BackColor = backC;
      menuStrip.ForeColor = frontC;

      foreach (TabPage tP in Utility.GetAll(this, typeof(TabPage))) {
        tP.BackColor = frontC;
      }

      foreach (TableLayoutPanel tlp in Utility.GetAll(this, typeof(TableLayoutPanel))) {
        tlp.BackColor = backC;
      }

      foreach (Button button in Utility.GetAll(this, typeof(Button))) {
        button.BackColor = frontC;
        button.ForeColor = backC;
      }

      foreach (GroupBox groupBox in Utility.GetAll(this, typeof(GroupBox))) {
        groupBox.BackColor = frontC;
        groupBox.ForeColor = backC;
      }

      foreach (Label label in Utility.GetAll(this, typeof(Label))) {
        label.BackColor = backC;
        label.ForeColor = restC;
      }

      foreach (CheckBox checkBox in Utility.GetAll(this, typeof(CheckBox))) {
        checkBox.BackColor = backC;
        checkBox.ForeColor = restC;
      }

      foreach (TextBox tB in Utility.GetAll(this, typeof(TextBox))) {
        if (tB.Multiline == false) {
          tB.BackColor = frontC;
          tB.ForeColor = backC;
        } else {
          tB.BackColor = restC;
          tB.ForeColor = backC;
        }
      }

      textBoxInstructionBoxPg1.BackColor = frontC;
      textBoxInstructionBoxPg1.ForeColor = colorOffwhite;
    }

    //Ctrl+A
    private void KeysUpTb(object sender, KeyEventArgs e) {
      if (e.Control && e.KeyCode == Keys.A) {
        (sender as TextBox).SelectAll();
      }
    }

    //Edit Menu
    private void cutToolStripMenuItem_Click(object sender, EventArgs e) {
      foreach (TextBox tB in Utility.GetAll(this, typeof(TextBox))) {
        if (tB.ContainsFocus) {
          System.Windows.Forms.Clipboard.SetText(tB.SelectedText);
          tB.SelectedText = "";
        }
      }
    }

    private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
      foreach (TextBox tB in Utility.GetAll(this, typeof(TextBox))) {
        if (tB.ContainsFocus) {
          System.Windows.Forms.Clipboard.SetText(tB.SelectedText);
        }
      }
    }

    private void pasteToolStripMenuItem_Click(object sender, EventArgs e) {
      foreach (TextBox tB in Utility.GetAll(this, typeof(TextBox))) {
        if (tB.ContainsFocus && !tB.ReadOnly) {
          tB.Paste(System.Windows.Forms.Clipboard.GetText());
        }
      }
    }

    ///HELP SECTION
    private void automaticToolStripMenuItem_Click_1(object sender, EventArgs e) {
      MessageBox.Show("Provide lists for old filenames list and list of corresponding new file names. \nOne to one direct rename.");
    }

    private void manualToolStripMenuItem_Click_1(object sender, EventArgs e) {
      MessageBox.Show("Choose/Drag old file names. Type in corresponding new file names.");
    }

    private void insertToolStripMenuItem_Click_1(object sender, EventArgs e) {
      MessageBox.Show("Choose/Drag old file names. Choose insertion text and options.");
    }

    private void deleteToolStripMenuItem_Click_1(object sender, EventArgs e) {
      MessageBox.Show("Choose/Drag old file names. Enter text to find and delete.");
    }

    private void caseconversionToolStripMenuItem1_Click(object sender, EventArgs e) {
      MessageBox.Show("Choose/Drag old file names. Choose caseconversion mode.");
    }

    private void extensionModeToolStripMenuItem_Click(object sender, EventArgs e) {
      MessageBox.Show("Choose/Drag old file names. \nClick right button to choose between single ext. mode or multiple ext. mode.");
    }
    private void feedbackToolStripMenuItem_Click(object sender, EventArgs e) {
      MessageBox.Show("For feedback and suggestions: ezz1994@gmail.com", "Feedback");
    }



    ///PAGE 5 checkboxes
    private void checkBoxLowercase_CheckedChanged(object sender, EventArgs e) {
      if (checkBoxLowercase.Checked) {
        checkBoxPropercase.Checked = false;
        checkBoxUppercase.Checked = false;
      }
    }

    private void checkBoxPropercase_CheckedChanged(object sender, EventArgs e) {
      if (checkBoxPropercase.Checked) {
        checkBoxLowercase.Checked = false;
        checkBoxUppercase.Checked = false;
      }
    }

    private void checkBoxUppercase_CheckedChanged(object sender, EventArgs e) {
      if (checkBoxUppercase.Checked) {
        checkBoxLowercase.Checked = false;
        checkBoxPropercase.Checked = false;
      }
    }

    ///Extra functions
    public void finalizeBox(ref TextBox tB, ref Button button, ref string[] list) {

      MessageBox.Show("Renaming Complete", "Success");

      if (!checkBoxOptionsKLA.Checked) {
        tB.Clear();
        button.Text = "Add Old File(s)";
      } else {
        tB.Lines = list;
        button.Text = "Clear List";
      }
    }

    public void AttemptRenameAutomatic(StreamReader oldSR, StreamReader newSR, string path) {
      //mode = true for automatic //mode = false for manual
      string oldFileName = oldSR.ReadLine();
      string newFileName = newSR.ReadLine();

      while (Utility.NonNullOrEmptyString(oldFileName) && Utility.NonNullOrEmptyString(newFileName)) {

        string extension = Utility.GetExtension(newFileName);
        if (null == extension) {
          string message = "New File Name List contains a name without an extension.\n";
          message += "Click Yes to use the corresponding extension from Old File Name List.\n";
          message += "Click No to rename without extension.\n";
          message += "Click Cancel to cancel renaming operation.";

          DialogResult result = MessageBox.Show(message, "Alert", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
          if (result == DialogResult.Cancel)
            break;
          else if (result == DialogResult.Yes) {
            newFileName = newFileName + Utility.GetExtension(oldFileName);
          }

        }

        Utility.RenameAFile(path, oldFileName, newFileName);
        oldFileName = oldSR.ReadLine();
        newFileName = newSR.ReadLine();

      }

    }

    public void AttemptRenameManual(StreamReader oldSR, StreamReader newSR, string path, ref TextBox tB, ref Button button) {

      string oldFileName = oldSR.ReadLine();
      string newFileName = newSR.ReadLine();
      string[] newBox = new string[tB.Lines.Count()];
      int count = 0;

      while (Utility.NonNullOrEmptyString(oldFileName) && Utility.NonNullOrEmptyString(newFileName)) {

        string extension = Utility.GetExtension(oldFileName);
        if (null == Utility.GetExtension(newFileName) || Utility.GetExtension(newFileName) != Utility.GetExtension(oldFileName))
          newFileName = newFileName + extension;

        newBox[count] = Utility.RenameAFile(path, oldFileName, newFileName) ? newFileName : oldFileName;
        oldFileName = oldSR.ReadLine();
        newFileName = newSR.ReadLine();
        count++;
      }
      finalizeBox(ref tB, ref button, ref newBox);
    }
  }
}
