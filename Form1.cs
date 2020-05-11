﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Grade_Calculator_3
{
    public partial class Main : Form
    {
        private readonly int PAGE_LEN = 5;
        public int currentPage, pages;
        public DataRow[] DataRows;
        private SchoolClass CurrentClass;
        private AddPoints[] addPoints;
        public bool blank = true;


        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            XMLHandler.DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Grade Calculator/";
            comboBoxClasses.DropDownStyle = ComboBoxStyle.DropDownList;
            InitialSetup();
        }

        public void InitialSetup()
        {
            currentPage = 1;
            pages = 1;
            CurrentClass = null;
            addPoints = new AddPoints[0];
            LoadClassData("");
            ChangeInputMode(1);
            RefreshClassList();
        }

        private void addClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddClass addClassForm = new AddClass(this);
            addClassForm.Show();
            
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        public void RefreshClassList(bool refreshDependents=true)
        {
            XMLHandler.Data = XMLHandler.ReadSchoolClasses();
            comboBoxClasses.Items.Clear();
            if(XMLHandler.Data == null)
            {
                return;
            }
            foreach(SchoolClass schoolClass in XMLHandler.Data)
            {
                comboBoxClasses.Items.Add(schoolClass.className);
            }

            if (refreshDependents)
            {
                RefreshEditClass();
                RefreshRemoveClass();
            }
        }

        private void RefreshEditClass()
        {
            editClassToolStripMenuItem.DropDownItems.Clear();
            if (XMLHandler.Data == null)
            {
                editClassToolStripMenuItem.Enabled = false;
                return;
            }
            editClassToolStripMenuItem.Enabled = true;
            foreach (SchoolClass schoolClass in XMLHandler.Data)
            {
                ToolStripItem temp = editClassToolStripMenuItem.DropDownItems.Add(schoolClass.className);
                temp.Click += delegate
                {
                    EditClassHandler(temp.Text);
                };
            }
        }

        private void RefreshRemoveClass()
        {
            removeClassToolStripMenuItem.DropDownItems.Clear();
            if (XMLHandler.Data == null)
            {
                removeClassToolStripMenuItem.Enabled = false;
                return;
            }
            removeClassToolStripMenuItem.Enabled = true;
            foreach (SchoolClass schoolClass in XMLHandler.Data)
            {
                ToolStripItem temp = removeClassToolStripMenuItem.DropDownItems.Add(schoolClass.className);
                temp.Click += delegate
                {
                    DeleteClassHandler(temp.Text);
                };
            }
        }

        private void comboBoxClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            blank = false;
            CloseAllAddWindows();
            LoadClassData(comboBoxClasses.Text);
        }

        public void LoadClassData(string className)
        {
            TextBox[] CatTextBoxes = { TextBoxCat1, TextBoxCat2, TextBoxCat3, TextBoxCat4, TextBoxCat5 };

            //if the class is blank (initialization), add PAGE_LEN blank categories to DataRows
            if (className.Equals(""))
            {
                DataRows = new DataRow[PAGE_LEN];
                int c = 0;
                foreach(DataRow DataRow in DataRows)
                {
                    DataRow temp = new DataRow();
                    temp.SetAllToEmpty();
                    DataRows[c] = temp;
                    c++;
                }
                setVisibility(PAGE_LEN, 1);
            }
            else
            {
                bool found = false;
                int c = 0;
                SchoolClass workingClass = null;
                while (!found && c != XMLHandler.Data.Length)
                {
                    workingClass = XMLHandler.Data[c];
                    if (workingClass.className.Equals(className))
                    {
                        found = true;
                        CurrentClass = workingClass;
                    }
                    c++;
                }
                if (!found)
                {
                    throw new ArgumentException("The class selected does not exist. This should never happen.");
                }

                //fill DataRows with category data
                DataRows = new DataRow[CurrentClass.catNames.Length];

                c = 0;
                foreach(DataRow dataRow in DataRows)
                {
                    DataRows[c] = new DataRow();
                    DataRows[c].SetAllToEmpty();
                    DataRows[c].CatName = workingClass.catNames[c];
                    DataRows[c].Weight = workingClass.catWorths[c].ToString();
                    c++;
                }
            }
            currentPage = 1;
            pages = (int)Math.Ceiling(Convert.ToDouble(DataRows.Length) / Convert.ToDouble(PAGE_LEN));

            //DataRows is now populated with non-point data
            //Display the data in DataRows in the textboxes
            DisplayPage(currentPage);

        }

        public void DisplayPage(int page)
        {
            TextBox[] TextBoxes1 = { TextBoxCat1, TextBoxP1, TextBoxOutOf1, TextBoxW1, TextBoxPer1, TextBoxT1 };
            TextBox[] TextBoxes2 = { TextBoxCat2, TextBoxP2, TextBoxOutOf2, TextBoxW2, TextBoxPer2, TextBoxT2 };
            TextBox[] TextBoxes3 = { TextBoxCat3, TextBoxP3, TextBoxOutOf3, TextBoxW3, TextBoxPer3, TextBoxT3 };
            TextBox[] TextBoxes4 = { TextBoxCat4, TextBoxP4, TextBoxOutOf4, TextBoxW4, TextBoxPer4, TextBoxT4 };
            TextBox[] TextBoxes5 = { TextBoxCat5, TextBoxP5, TextBoxOutOf5, TextBoxW5, TextBoxPer5, TextBoxT5 };

            TextBox[][] TextBox2D = { TextBoxes1, TextBoxes2, TextBoxes3, TextBoxes4, TextBoxes5 };

            int start = (page - 1) * PAGE_LEN; //start INDEX
            int end = start + PAGE_LEN - 1;
            if(end > (DataRows.Length - 1))
            {
                end = DataRows.Length - 1;
            }
            setVisibility(DataRows.Length, page);
            int c = 0;
            int offset = (page - 1) * PAGE_LEN;
            while(start <= end)
            {
                TextBox2D[c][0].Text = DataRows[c + offset].CatName;
                TextBox2D[c][1].Text = DataRows[c + offset].Points;
                TextBox2D[c][2].Text = DataRows[c + offset].OutOf;
                TextBox2D[c][3].Text = DataRows[c + offset].Weight;
                TextBox2D[c][4].Text = DataRows[c + offset].Percent;
                TextBox2D[c][5].Text = DataRows[c + offset].Total;
                c++;
                start++;
            }
            currentPage = page;
        }

        public void SaveDataToMem(int page)
        {
            TextBox[] TextBoxes1 = { TextBoxCat1, TextBoxP1, TextBoxOutOf1, TextBoxW1, TextBoxPer1, TextBoxT1 };
            TextBox[] TextBoxes2 = { TextBoxCat2, TextBoxP2, TextBoxOutOf2, TextBoxW2, TextBoxPer2, TextBoxT2 };
            TextBox[] TextBoxes3 = { TextBoxCat3, TextBoxP3, TextBoxOutOf3, TextBoxW3, TextBoxPer3, TextBoxT3 };
            TextBox[] TextBoxes4 = { TextBoxCat4, TextBoxP4, TextBoxOutOf4, TextBoxW4, TextBoxPer4, TextBoxT4 };
            TextBox[] TextBoxes5 = { TextBoxCat5, TextBoxP5, TextBoxOutOf5, TextBoxW5, TextBoxPer5, TextBoxT5 };

            TextBox[][] TextBox2D = { TextBoxes1, TextBoxes2, TextBoxes3, TextBoxes4, TextBoxes5 };

            int start = (page - 1) * PAGE_LEN; //start INDEX
            int end = start + PAGE_LEN - 1;
            if (end > (DataRows.Length - 1))
            {
                end = DataRows.Length - 1;
            }
            int c = 0;
            int offset = (page - 1) * PAGE_LEN;
            while (start <= end)
            {
                DataRows[c + offset].CatName = TextBox2D[c][0].Text;
                DataRows[c + offset].Points = TextBox2D[c][1].Text;
                DataRows[c + offset].OutOf = TextBox2D[c][2].Text;
                DataRows[c + offset].Weight = TextBox2D[c][3].Text;
                DataRows[c + offset].Percent = TextBox2D[c][4].Text;
                DataRows[c + offset].Total = TextBox2D[c][5].Text;
                c++;
                start++;
            }
        }

        public bool SaveIndexToMem(int index, string points_i, string outOf_i, bool neg)
        {
            double coe = 1;
            if (neg)
            {
                coe = -1;
            }
            string points, outOf;
            if (points_i.Equals(""))
            {
                points = "0";
            }
            else
            {
                points = points_i;
            }

            if (outOf_i.Equals(""))
            {
                outOf = "0";
            }
            else
            {
                outOf = outOf_i;
            }
            SaveDataToMem(currentPage);
            if (ErrorChecking.textIsType("Double", points) && ErrorChecking.textIsType("Double", outOf))
            {
                if(ErrorChecking.textIsType("Double", DataRows[index].Points) && ErrorChecking.textIsType("Double", DataRows[index].OutOf))
                {
                    DataRows[index].Points = (Convert.ToDouble(DataRows[index].Points) + coe * Convert.ToDouble(points)).ToString();
                    DataRows[index].OutOf = (Convert.ToDouble(DataRows[index].OutOf) + coe * Convert.ToDouble(outOf)).ToString();
                }
                else
                {
                    DataRows[index].Points = (coe * Convert.ToDouble(points)).ToString();
                    DataRows[index].OutOf = (coe * Convert.ToDouble(outOf)).ToString();
                }
                return true;
            }
            return false;
        }

        private void ClearInput()
        {
            //clear the actual textboxes
            for(int c = 0; c < 5; c++)
            {
                ClearInputInRow(c + 1);
            }
            TextBoxTotalPer.Text = "";
            TextBoxGrade.Text = "";

            foreach(DataRow dataRow in DataRows)
            {
                dataRow.SetDataToEmpty();
            }
            //DisplayPage(currentPage);
        }
        private void ClearInputInRow(int row)
        {
            if (row == 1)
            {
                TextBox[] TextBoxes = { TextBoxP1, TextBoxOutOf1, TextBoxPer1, TextBoxT1 };
                ClearInputInRow(TextBoxes);
            }
            else if (row == 2)
            {
                TextBox[] TextBoxes = { TextBoxP2, TextBoxOutOf2, TextBoxPer2, TextBoxT2 };
                ClearInputInRow(TextBoxes);
            }
            else if (row == 3)
            {
                TextBox[] TextBoxes = { TextBoxP3, TextBoxOutOf3, TextBoxPer3, TextBoxT3 };
                ClearInputInRow(TextBoxes);
            }
            else if (row == 4)
            {
                TextBox[] TextBoxes = { TextBoxP4, TextBoxOutOf4,TextBoxPer4, TextBoxT4 };
                ClearInputInRow(TextBoxes);
            }
            else if (row == 5)
            {
                TextBox[] TextBoxes = { TextBoxP5, TextBoxOutOf5, TextBoxPer5, TextBoxT5 };
                ClearInputInRow(TextBoxes);
            }
            else
            {
                throw new Exception("Row does not exist");
            }
        }
        private void ClearInputInRow(TextBox[] TBs)
        {
            foreach (TextBox TB in TBs)
            {
                TB.Text = "";
            }
        }

        private void setVisibility(int elements, int currentPage)
        {
            int start = (currentPage - 1) * PAGE_LEN;
            int end = elements;
            int c = 1;
            while(c <= PAGE_LEN) //should set row's visibility
            {
                ChangeRowVisibility(c, start + c <= end);
                c++;
            }

            //button visibility
            if(currentPage == 1)
            {
                ButtonBack.Enabled = false;
            }
            else
            {
                ButtonBack.Enabled = true;
            }
            if(currentPage < pages)
            {
                ButtonForward.Enabled = true;
            }
            else
            {
                ButtonForward.Enabled = false;
            }
        }

        private void ChangeRowVisibility(int row, bool visible)
        {
            if (row == 1)
            {
                TextBox[] TextBoxes = { TextBoxCat1, TextBoxP1, TextBoxOutOf1, TextBoxW1, TextBoxPer1, TextBoxT1 };
                Button[] Buttons = { ButtonA1 };
                ChangeRowVisHelper(TextBoxes, Buttons, visible);
            }
            else if (row == 2)
            {
                TextBox[] TextBoxes = { TextBoxCat2, TextBoxP2, TextBoxOutOf2, TextBoxW2, TextBoxPer2, TextBoxT2 };
                Button[] Buttons = { ButtonA2 };
                ChangeRowVisHelper(TextBoxes, Buttons, visible);
            }
            else if (row == 3)
            {
                TextBox[] TextBoxes = { TextBoxCat3, TextBoxP3, TextBoxOutOf3, TextBoxW3, TextBoxPer3, TextBoxT3 };
                Button[] Buttons = { ButtonA3 };
                ChangeRowVisHelper(TextBoxes, Buttons, visible);
            }
            else if (row == 4)
            {
                TextBox[] TextBoxes = { TextBoxCat4, TextBoxP4, TextBoxOutOf4, TextBoxW4, TextBoxPer4, TextBoxT4 };
                Button[] Buttons = { ButtonA4 };
                ChangeRowVisHelper(TextBoxes, Buttons, visible);
            }
            else if (row == 5)
            {
                TextBox[] TextBoxes = { TextBoxCat5, TextBoxP5, TextBoxOutOf5, TextBoxW5, TextBoxPer5, TextBoxT5 };
                Button[] Buttons = { ButtonA5 };
                ChangeRowVisHelper(TextBoxes, Buttons, visible);
            }
            else
            {
                throw new Exception("Row does not exist");
            }
        }
        private void ChangeRowVisHelper(TextBox[] TBs, Button[] BTNs, bool vis)
        {
            foreach(TextBox TB in TBs)
            {
                TB.Visible = vis;
            }
            foreach(Button BTN in BTNs)
            {
                BTN.Visible = vis;
            }
        }

        private void CalculateGrade()
        {
            double[] points = new double[DataRows.Length];
            double[] outOf = new double[DataRows.Length];
            double[] weight = new double[DataRows.Length];
            double[] percent = new double[DataRows.Length];
            double[] total = new double[DataRows.Length];
            double finalPercentage = 0;
            string finalGrade = "";

            int c = 0;
            foreach(DataRow dataRow in DataRows)
            {
                //convert the strings in the text boxes to numerical values (if possible)
                if(dataRow.Points.Equals(""))
                {
                    points[c] = 0;
                }
                else
                {
                    try
                    {
                        points[c] = Convert.ToDouble(dataRow.Points);
                    }
                    catch
                    {
                        MessageBox.Show("At least one entry in points is invalid.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                if (dataRow.OutOf.Equals(""))
                {
                    outOf[c] = 0;
                }
                else
                {
                    try
                    {
                        outOf[c] = Convert.ToDouble(dataRow.OutOf);
                    }
                    catch
                    {
                        MessageBox.Show("At least one entry in out of is invalid.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                if (dataRow.Weight.Equals(""))
                {
                    weight[c] = 0;
                }
                else
                {
                    try
                    {
                        weight[c] = Convert.ToDouble(dataRow.Weight);
                    }
                    catch
                    {
                        MessageBox.Show("At least one entry in weight is invalid.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if(weight[c] != 0)
                {
                    if(outOf[c] == 0)
                    {
                        MessageBox.Show("Out of cannot be 0 if points is non-zero.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    percent[c] = 100 * points[c] / outOf[c];
                    total[c] = percent[c] * weight[c] / 100;
                    finalPercentage += total[c];
                }
                c++;
            }


            //get the grade scale
            double[] gradeScaleVals = new double[0];
            string[] gradeScaleGrade = new string[0];
            if(CurrentClass == null)
            {
                //will not find a letter grade for a non-class
            }
            else if(CurrentClass.gradeScaleFormat == 1)
            {
                c = 0;
                foreach(double val in CurrentClass.gradeScale)
                {
                    if(val != -1) //if the grade is enabled
                    {
                        Array.Resize(ref gradeScaleVals, gradeScaleVals.Length + 1);
                        Array.Resize(ref gradeScaleGrade, gradeScaleGrade.Length + 1);
                        gradeScaleVals[gradeScaleVals.Length - 1] = val;
                        gradeScaleGrade[gradeScaleGrade.Length - 1] = GetGradeFromIndexAF(c);
                    }
                    c++;
                }
                //for adding F at the end
                Array.Resize(ref gradeScaleVals, gradeScaleVals.Length + 1);
                Array.Resize(ref gradeScaleGrade, gradeScaleGrade.Length + 1);
                gradeScaleVals[gradeScaleVals.Length - 1] = 0;
                gradeScaleGrade[gradeScaleGrade.Length - 1] = GetGradeFromIndexAF(11);

                bool found = false;
                c = 0;
                while (!found && (c != gradeScaleVals.Length))
                {
                    if(gradeScaleVals[c] <= finalPercentage)
                    {
                        finalGrade = gradeScaleGrade[c];
                        found = true;
                    }
                    c++;
                }
            }
            else
            {
                throw new NotImplementedException("S/N grading is not yet supported");
            }
            TextBoxGrade.Text = finalGrade;
            TextBoxTotalPer.Text = finalPercentage.ToString();

            c = 0;
            foreach(DataRow dataRow in DataRows)
            {
                DataRows[c].Points = points[c].ToString();
                DataRows[c].OutOf = outOf[c].ToString();
                DataRows[c].Weight = weight[c].ToString();
                DataRows[c].Percent = percent[c].ToString();
                DataRows[c].Total = total[c].ToString();
                c++;
            }
            DisplayPage(currentPage);

        }

        private string GetGradeFromIndexAF(int index)
        {
            string[] grades = { "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-", "F" };
            return grades[index];
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            ClearInput();
        }

        private void ButtonForward_Click(object sender, EventArgs e)
        {
            SaveDataToMem(currentPage);
            DisplayPage(currentPage + 1);
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            SaveDataToMem(currentPage);
            DisplayPage(currentPage - 1);
        }

        private void ButtonCalculate_Click(object sender, EventArgs e)
        {
            SaveDataToMem(currentPage);
            CalculateGrade();
        }

        private void ButtonA1_Click(object sender, EventArgs e)
        {
            LaunchAddWindow(1);
        }

        private void ButtonA2_Click(object sender, EventArgs e)
        {
            LaunchAddWindow(2);
        }

        private void ButtonA3_Click(object sender, EventArgs e)
        {
            LaunchAddWindow(3);
        }

        private void ButtonA4_Click(object sender, EventArgs e)
        {
            LaunchAddWindow(4);
        }

        private void ButtonA5_Click(object sender, EventArgs e)
        {
            LaunchAddWindow(5);
        }

        private void LaunchAddWindow(int button)
        {
            int offset = button - 1;
            int index = (currentPage - 1) * PAGE_LEN + offset;
            Array.Resize(ref addPoints, addPoints.Length + 1);
            addPoints[addPoints.Length - 1] = new AddPoints(index, this);
            addPoints[addPoints.Length - 1].Show();
        }

        private void CloseAllAddWindows()
        {
            foreach(AddPoints form in addPoints)
            {
                form.Close();
            }
            addPoints = new AddPoints[0];
        }

        private void ButtonCurve_Click(object sender, EventArgs e)
        {
            CurveForm CF = new CurveForm();
            CF.Show();
        }

        private void refreshClassListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitialSetup();
        }

        public void EditClassHandler(string className)
        {
            bool found = false;
            int c = 0;
            SchoolClass workingClass = null;
            while (!found && c != XMLHandler.Data.Length)
            {
                workingClass = XMLHandler.Data[c];
                if (workingClass.className.Equals(className))
                {
                    found = true;
                }
                c++;
            }
            if (!found)
            {
                throw new ArgumentException("The class selected does not exist. This should never happen.");
            }

            AddClass edit = new AddClass(this, workingClass);
            edit.Show();
        }

        public void DeleteClassHandler(string className)
        {
            //if class was deleted
            if (XMLHandler.DeleteClass(className))
            {
                InitialSetup();
            }
        }

        private void ChangeInputMode(int mode)
        {
            //basic = 1, advanced = 2
            Button[] addButtons = { ButtonA1, ButtonA2, ButtonA3, ButtonA4, ButtonA5 };
            TextBox[] pointsTextBoxes = { TextBoxP1, TextBoxP2, TextBoxP3, TextBoxP4, TextBoxP5 };
            TextBox[] outOfTextBoxes = { TextBoxOutOf1, TextBoxOutOf2, TextBoxOutOf3, TextBoxOutOf4, TextBoxOutOf5 };

            if(mode == 1)
            {
                foreach(Button btn in addButtons)
                {
                    btn.Text = "Add";
                }
                foreach(TextBox tb in pointsTextBoxes)
                {
                    tb.ReadOnly = false;
                }
                foreach(TextBox tb in outOfTextBoxes)
                {
                    tb.ReadOnly = false;
                }
                return;
            }
            else if(mode == 2)
            {
                foreach (Button btn in addButtons)
                {
                    btn.Text = "Q Add";
                }
                foreach (TextBox tb in pointsTextBoxes)
                {
                    tb.ReadOnly = true;
                }
                foreach (TextBox tb in outOfTextBoxes)
                {
                    tb.ReadOnly = true;
                }
                return;
            }
            throw new NotImplementedException("Invalid mode passed to ChangeInputMode");
        }

        public class DataRow
        {
            public string CatName, Points, OutOf, Weight, Percent, Total;

            public void SetAllToEmpty()
            {
                CatName = "";
                Points = "";
                OutOf = "";
                Weight = "";
                Percent = "";
                Total = "";
            }
            public void SetDataToEmpty()
            {
                Points = "";
                OutOf = "";
                Percent = "";
                Total = "";
            }
        }
    }

    public static class ErrorChecking
    {
        public static bool textIsType(string type, Object value)
        {
            try
            {
                if (type == "Double" || type == "double")
                {
                    Double temp = Convert.ToDouble(value);
                    return true;
                }
                else if (type == "int")
                {
                    int temp = Convert.ToInt32(value);
                    return true;
                }
            }
            catch
            {
                return false;
            }
            throw new System.ArgumentException("Argument passed to textIsType has not been implemented");
        }
    }
}
