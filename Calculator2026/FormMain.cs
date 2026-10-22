using System;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculator2026
{
    public partial class FormMain : Form
    {
        private Label infoLabel;
        private Label resultLabel;

        private static readonly Color OPERATOR_BG = Color.LightGray;
        private static readonly Color NUMBER_BG = Color.WhiteSmoke;
        private static readonly Color EQUAL_BG = Color.LightSeaGreen;

        decimal operand1, operand2, result;
        char lastOperator = ' ';
        BtnStruct lastBtnStruct;

        public enum SymbolType
        {
            Number,
            Operator,
            SpecialOperator,
            EqualSign,
            DecimalPoint,
            PlusMinusSign,
            Backspace,
            ClearEntry,
            ClearAll,
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
            { new BtnStruct('%', SymbolType.SpecialOperator), new BtnStruct('\u0152', SymbolType.ClearEntry), new BtnStruct('C', SymbolType.ClearAll), new BtnStruct('\u232B', SymbolType.Backspace) },
            { new BtnStruct('\u215F', SymbolType.SpecialOperator), new BtnStruct('\u00B2', SymbolType.SpecialOperator), new BtnStruct('\u221A', SymbolType.SpecialOperator), new BtnStruct('\u00F7', SymbolType.Operator) },
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
            MakeInfoLabel();
            MakeResultLabel();
            MakeButtons();
        }

        private void MakeInfoLabel()
        {
            infoLabel = new Label()
            {
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Location = new Point(0, 0),
                Size = new Size(this.ClientSize.Width, 34),
                BackColor = Color.Beige,
                Text = ""
            };
            Controls.Add(infoLabel);
        }

        private void MakeResultLabel()
        {
            resultLabel = new Label()
            {
                Font = new Font("Segoe UI", 34, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Location = new Point(0, 34),
                Size = new Size(this.ClientSize.Width, 66),
                BackColor = Color.Beige,
                Text = "0"
            };
            resultLabel.TextChanged += ResultLabel_TextChanged;
            Controls.Add(resultLabel);
        }

        private void ResultLabel_TextChanged(object sender, EventArgs e)
        {
            if (resultLabel.Text.Length > 0)
            {
                decimal num = decimal.Parse(resultLabel.Text);
                NumberFormatInfo nfi = new CultureInfo("it-IT", false).NumberFormat;
                int decimalSeparatorPosition = resultLabel.Text.IndexOf(',');
                if (decimalSeparatorPosition == -1)
                    nfi.NumberDecimalDigits = 0;
                else
                    nfi.NumberDecimalDigits = resultLabel.Text.Length - decimalSeparatorPosition - 1;
                string stOut = num.ToString("N", nfi);
                if (decimalSeparatorPosition == resultLabel.Text.Length - 1)
                    stOut += ",";
                resultLabel.Text = stOut;
            }

            float fontSize = 34;
            if (resultLabel.Text.Length > 11)
            {
                float delta = (resultLabel.Text.Length - 11) * (float)2.8;
                fontSize = 34 - delta;
                if (fontSize < 10)
                    fontSize = 10;
            }
            if (resultLabel.Font.Size != fontSize)
                resultLabel.Font = new Font("Segoe UI", fontSize, FontStyle.Bold);
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
                    btn.Text = buttons[i, j].ToString();
                    switch (buttons[i, j].Type)
                    {
                        case SymbolType.Number:
                        case SymbolType.DecimalPoint:
                        case SymbolType.PlusMinusSign:
                            btn.BackColor = NUMBER_BG;
                            break;
                        case SymbolType.Operator:
                        case SymbolType.SpecialOperator:
                        case SymbolType.Backspace:
                        case SymbolType.ClearEntry:
                        case SymbolType.ClearAll:
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
                    if (lastBtnStruct.Type == SymbolType.EqualSign)
                        ClearAll();
                    if (resultLabel.Text == "0" || lastBtnStruct.Type == SymbolType.Operator
                        || lastBtnStruct.Type == SymbolType.SpecialOperator)
                        resultLabel.Text = "";
                    if (resultLabel.Text.Replace(".", "").Replace(",", "").Replace("-", "").Length < 16)
                        resultLabel.Text += clickedButtonStruct.Content;
                    break;
                case SymbolType.Operator:
                case SymbolType.EqualSign:
                    _ = ManageOperator(clickedButtonStruct);
                    break;
                case SymbolType.SpecialOperator:
                    _ = ManageSpecialOperator(clickedButtonStruct);
                    break;
                case SymbolType.DecimalPoint:
                    if (lastBtnStruct.Type == SymbolType.EqualSign)
                        ClearAll();
                    if (lastBtnStruct.Type == SymbolType.Operator || lastBtnStruct.Type == SymbolType.SpecialOperator)
                        resultLabel.Text = "0";
                    if (!resultLabel.Text.Contains(","))
                        resultLabel.Text += clickedButtonStruct.Content;
                    break;
                case SymbolType.PlusMinusSign:
                    if (resultLabel.Text == "0")
                        return;   // lo zero non ha segno
                    if (!resultLabel.Text.Contains("-"))
                        resultLabel.Text = "-" + resultLabel.Text;
                    else
                        resultLabel.Text = resultLabel.Text.Substring(1);
                    if (lastBtnStruct.Type == SymbolType.EqualSign)
                    {
                        // cambio segno al risultato: l'ultimo tasto resta "="
                        operand1 = decimal.Parse(resultLabel.Text);
                        result = operand1;
                        return;
                    }
                    if (lastBtnStruct.Type == SymbolType.Operator || lastBtnStruct.Type == SymbolType.SpecialOperator)
                    {
                        // il numero col segno cambiato si comporta come il risultato di un'operazione speciale
                        lastBtnStruct = new BtnStruct(clickedButtonStruct.Content, SymbolType.SpecialOperator);
                        return;
                    }
                    break;
                case SymbolType.Backspace:
                    if (lastBtnStruct.Type == SymbolType.Operator || lastBtnStruct.Type == SymbolType.EqualSign
                        || lastBtnStruct.Type == SymbolType.SpecialOperator)
                        return;   // ignora il tasto: lastBtnStruct resta invariato
                    string newText = resultLabel.Text.Substring(0, resultLabel.Text.Length - 1);
                    if (newText == "" || newText == "-" || newText == "-0")
                        newText = "0";
                    resultLabel.Text = newText;
                    break;
                case SymbolType.ClearEntry:
                    if (lastBtnStruct.Type == SymbolType.EqualSign)
                        ClearAll();
                    else
                        resultLabel.Text = "0";
                    break;
                case SymbolType.ClearAll:
                    ClearAll();
                    break;
                case SymbolType.Undefined:
                    break;
                default:
                    break;
            }
            lastBtnStruct = clickedButtonStruct;
        }

        private void ClearAll()
        {
            operand1 = 0;
            operand2 = 0;
            result = 0;
            lastOperator = ' ';
            infoLabel.Text = "";
            resultLabel.Text = "0";
        }

        private string FormatNumber(decimal number)
        {
            // arrotonda a 10 decimali ed elimina gli zeri finali
            return Math.Round(number, 10).ToString("0.##########");
        }

        private async Task ManageSpecialOperator(BtnStruct clickedButtonStruct)
        {
            if (lastOperator == ' ' || lastBtnStruct.Type == SymbolType.EqualSign)
            {
                infoLabel.Text = "";
            }
            operand2 = Decimal.Parse(resultLabel.Text);
            switch (clickedButtonStruct.Content)
            {
                case '%':
                    switch (lastOperator)
                    {
                        case '+':
                        case '-':
                            operand2 = (operand1 * operand2) / 100;
                            break;
                        case 'x':
                        case '\u00F7':
                            operand2 = operand2 / 100;
                            break;
                        default:
                            operand2 = 0;
                            break;
                    }
                    infoLabel.Text += $" {FormatNumber(operand2)}";
                    break;
                case '\u215F':
                    // 1 diviso x
                    if (operand2 == 0)
                    {
                        string st = infoLabel.Text;
                        infoLabel.Text = "Impossibile dividere per zero";
                        await Task.Delay(2000); // attesa di 2 secondi
                        infoLabel.Text = st;
                        return;
                    }
                    else
                    {
                        infoLabel.Text += $" 1/{FormatNumber(operand2)}";
                        operand2 = 1 / operand2;
                    }
                    break;
                case '\u00B2':
                    // x quadro
                    try
                    {
                        decimal square = operand2 * operand2;
                        infoLabel.Text += $" sqr({FormatNumber(operand2)})";
                        operand2 = square;
                    }
                    catch (OverflowException)
                    {
                        string st = infoLabel.Text;
                        infoLabel.Text = "Overflow";
                        await Task.Delay(2000); // attesa di 2 secondi
                        infoLabel.Text = st;
                        return;
                    }
                    break;
                case '\u221A':
                    // radice di x
                    if (operand2 < 0)
                    {
                        string st = infoLabel.Text;
                        infoLabel.Text = "Input non valido";
                        await Task.Delay(2000); // attesa di 2 secondi
                        infoLabel.Text = st;
                        return;
                    }
                    else
                    {
                        infoLabel.Text += $" \u221A({FormatNumber(operand2)})";
                        operand2 = (decimal)Math.Sqrt((double)operand2);
                    }
                    break;
            }
            resultLabel.Text = FormatNumber(operand2);
        }

        private async Task ManageOperator(BtnStruct clickedButtonStruct)
        {
            decimal lastOperand1 = operand1;
            if (lastBtnStruct.Type != SymbolType.Operator || clickedButtonStruct.Type == SymbolType.EqualSign)
            {
                if (lastOperator == ' ')
                {
                    // non ho in coda alcuna operazione
                    operand1 = decimal.Parse(resultLabel.Text);
                }
                else
                {
                    if (clickedButtonStruct.Type != SymbolType.EqualSign || lastBtnStruct.Type != SymbolType.EqualSign)
                        operand2 = decimal.Parse(resultLabel.Text);
                    if (!(clickedButtonStruct.Type == SymbolType.Operator && lastBtnStruct.Type == SymbolType.EqualSign))
                    {
                        // non schiaccio operator subito dopo aver schiacciato uguale
                        // devo svolgere l'operazione che ho in coda
                        try
                        {
                            switch (lastOperator)
                            {
                                case '+':
                                    result = operand1 + operand2;
                                    break;
                                case '-':
                                    result = operand1 - operand2;
                                    break;
                                case 'x':
                                    result = operand1 * operand2;
                                    break;
                                            case '\u00F7':
                                                if (operand2 == 0)
                                                {
                                                    string st = infoLabel.Text;
                                                    infoLabel.Text = "Impossibile dividere per 0";
                                                    await Task.Delay(2000); // attesa di 2 secondi
                                                    infoLabel.Text = st;
                                                    return;
                                                }
                                                result = operand1 / operand2;
                                                break;
                                        }
                                    }
                                    catch (OverflowException)
                                    {
                                        string st = infoLabel.Text;
                                        infoLabel.Text = "Overflow";
                                        await Task.Delay(2000); // attesa di 2 secondi
                                        infoLabel.Text = st;
                                        return;
                                    }
                                }
                                operand1 = result;
                                resultLabel.Text = FormatNumber(result);
                            }
            }
            if (clickedButtonStruct.Type == SymbolType.EqualSign)
            {
                if (lastOperator == ' ')
                    infoLabel.Text = $"{FormatNumber(operand1)} =";
                else
                    infoLabel.Text = $"{FormatNumber(lastOperand1)} {lastOperator} {FormatNumber(operand2)} =";
            }
            else
            {
                lastOperator = clickedButtonStruct.Content;
                infoLabel.Text = $"{FormatNumber(operand1)} {lastOperator}";
            }
        }
    }
}
