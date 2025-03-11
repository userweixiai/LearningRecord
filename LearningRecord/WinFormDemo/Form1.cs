using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormDemo
{
    public partial class Form1 : Form
    {
        private TextBox txtDisplay;
        private Button[] buttons;
        private string currentInput = "";
        private string expression = "";
        private bool isNewInput = false;

        public Form1()
        {
            InitializeComponent();

            InitializeCalculator();
        }

        private void InitializeCalculator()
        {
            this.Text = "计算器";
            this.Width = 350;
            this.Height = 500;
            this.BackColor = Color.LightGray;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Resize += Form1_Resize;

            txtDisplay = new TextBox
            {
                Left = 10,
                Top = 10,
                Width = this.ClientSize.Width - 20,
                ReadOnly = true,
                Font = new Font("Arial", 18, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            this.Controls.Add(txtDisplay);

            string[] buttonTexts = { "7", "8", "9", "/", "4", "5", "6", "*", "1", "2", "3", "-", "0", ".", "C", "=", "+" };
            buttons = new Button[17];

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = new Button
                {
                    Text = buttonTexts[i],
                    Font = new Font("Arial", 14, FontStyle.Bold),
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat
                };
                buttons[i].FlatAppearance.BorderSize = 1;
                buttons[i].Click += Button_Click;
                this.Controls.Add(buttons[i]);
            }
            ArrangeButtons();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ArrangeButtons();
        }

        private void ArrangeButtons()
        {
            int buttonWidth = (this.ClientSize.Width - 50) / 4;
            int buttonHeight = (this.ClientSize.Height - 150) / 5;

            txtDisplay.Width = this.ClientSize.Width - 20;
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Width = buttonWidth;
                buttons[i].Height = buttonHeight;
                buttons[i].Left = 10 + (i % 4) * (buttonWidth + 5);
                buttons[i].Top = 50 + (i / 4) * (buttonHeight + 5);
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            string btnText = btn.Text;

            if (char.IsDigit(btnText[0]) || btnText == ".")
            {
                if (isNewInput)
                {
                    currentInput = "";
                    isNewInput = false;
                }
                currentInput += btnText;
                txtDisplay.Text = expression + currentInput;
            }
            else if (btnText == "C")
            {
                currentInput = "";
                expression = "";
                txtDisplay.Text = "";
            }
            else if (btnText == "=")
            {
                try
                {
                    expression += currentInput;
                    var result = new DataTable().Compute(expression, null);
                    txtDisplay.Text = result.ToString();
                    expression = result.ToString();
                    currentInput = "";
                    isNewInput = true;
                }
                catch
                {
                    txtDisplay.Text = "错误";
                    currentInput = "";
                    expression = "";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentInput))
                {
                    expression += currentInput;
                }
                if (expression.Length > 0 && "+-*/".Contains(expression[expression.Length - 1].ToString()))
                {
                    expression = expression.Remove(expression.Length - 1);
                }
                expression += btnText;
                txtDisplay.Text = expression;
                isNewInput = true;
            }
        }
    }
}
