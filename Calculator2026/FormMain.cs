using System;
using System.Drawing;
using System.Windows.Forms;

namespace Calculator2026
{
    public partial class FormMain : Form
    {
        private Label resultLabel;

        private static readonly Color OPERATOR_BG = Color.LightGray;
        private static readonly Color NUMBER_BG = Color.WhiteSmoke;
        private static readonly Color EQUAL_BG = Color.LightSeaGreen;

        public enum SymbolType {
            Number,
            Operator,
            EqualSign,
            DecimalPoint,
            PlusMinusSign,
            Backspace,
            Undefined
        }

        public struct BtnStruct
        {
            public char Content;
            public SymbolType Type;
            public BtnStruct(char content, SymbolType type = SymbolType.Undefined)
            {
                this.Content = content;
                this.Type = type;
            }
            public override string ToString()
            {
                return Content.ToString();
            }
        }

        private readonly BtnStruct[,] buttons =
        {
            { new BtnStruct('%'), new BtnStruct('\u0152'), new BtnStruct('C'), new BtnStruct('\u232B', SymbolType.Backspace) },
            { new BtnStruct('\u215F'), new BtnStruct('\u00B2'), new BtnStruct('\u221A'), new BtnStruct('\u00F7', SymbolType.Operator) },
            { new BtnStruct('7', SymbolType.Number), new BtnStruct('8', SymbolType.Number), new BtnStruct('9', SymbolType.Number), new BtnStruct('x', SymbolType.Operator) },
            { new BtnStruct('4', SymbolType.Number), new BtnStruct('5', SymbolType.Number), new BtnStruct('6', SymbolType.Number), new BtnStruct('-', SymbolType.Operator) },
            { new BtnStruct('1', SymbolType.Number), new BtnStruct('2', SymbolType.Number), new BtnStruct('3', SymbolType.Number), new BtnStruct('+', SymbolType.Operator) },
            { new BtnStruct('\u00B1', SymbolType.PlusMinusSign), new BtnStruct('0', SymbolType.Number), new BtnStruct(',', SymbolType.DecimalPoint), new BtnStruct('=', SymbolType.EqualSign) }
        };


        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            MakeResultLabel();
            MakeButtons();
        }

        private void MakeResultLabel()
        {
            resultLabel = new Label()
            {
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Location = new Point(-20, 0),
                Size = new Size(this.Width, 100),
                BackColor = Color.Beige,
                Text = "0"
            };
            Controls.Add(resultLabel);
        }

        private void MakeButtons()
        {
            int btnWidth = 80, btnHeight = 60;
            int posY = 114;
            for (int i = 0; i < buttons.GetLength(0); i++)
            {
                int posX = 0;
                for (int j = 0; j < buttons.GetLength(1); j++)
                {
                    Button btn = new Button();
                    btn.Width = btnWidth;
                    btn.Height = btnHeight;
                    btn.Top = posY;
                    btn.Left = posX;
                    btn.Font = new Font("Segoe UI", 16);
                    btn.Text = buttons[i,j].ToString();
                    switch (buttons[i, j].Type)
                    {
                        case SymbolType.Number:
                        case SymbolType.DecimalPoint:
                        case SymbolType.PlusMinusSign:
                            btn.BackColor = NUMBER_BG;
                            break;
                        case SymbolType.Operator:
                        case SymbolType.Backspace:
                        case SymbolType.Undefined:
                            btn.BackColor = OPERATOR_BG;
                            break;
                        case SymbolType.EqualSign:
                            btn.BackColor = EQUAL_BG;
                            break;
                    }
                    btn.Tag = buttons[i, j];
                    btn.Click += Btn_Click;
                    Controls.Add(btn);
                    posX += btnWidth;
                }
                posY += btnHeight;
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            BtnStruct clickedButtonStruct = (BtnStruct)clickedButton.Tag;
            switch (clickedButtonStruct.Type)
            {
                case SymbolType.Number:
                    if (resultLabel.Text == "0") resultLabel.Text = "";
                    resultLabel.Text += clickedButtonStruct.Content;
                    break;
                case SymbolType.Operator:
                    break;
                case SymbolType.EqualSign:
                    break;
                case SymbolType.DecimalPoint:
                    if (!resultLabel.Text.Contains(","))
                        resultLabel.Text += clickedButtonStruct.Content;
                    break;
                case SymbolType.PlusMinusSign:
                    if (!resultLabel.Text.Contains("-"))
                        resultLabel.Text = "-" + resultLabel.Text;
                    break;
                case SymbolType.Backspace:
                    break;
                case SymbolType.Undefined:
                    break;
                default:
                    break;
            }
        }
    }
}
