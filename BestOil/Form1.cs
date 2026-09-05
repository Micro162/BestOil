using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace BestOilApp
{
    public class Form1 : Form
    {
        private readonly Dictionary<string, decimal> fuelPrices = new Dictionary<string, decimal>
        {
            { "А-76", 6.40m },
            { "А-92", 7.20m },
            { "А-95", 7.80m },
            { "Дизель", 6.90m }
        };

        private readonly (string Name, decimal Price)[] cafeItems = new (string, decimal)[]
        {
            ("Хот-дог", 4.00m),
            ("Гамбургер", 5.40m),
            ("Картопля-фрі", 7.20m),
            ("Кока-кола", 4.40m)
        };

        private ComboBox cmbFuel;
        private TextBox txtFuelPrice;
        private RadioButton rbQuantity;
        private RadioButton rbSum;
        private TextBox txtQuantity;
        private TextBox txtSum;
        private Label lblFuelPayCaption;
        private Label lblFuelResult;
        private Label lblFuelUnit;

        private CheckBox[] cafeChecks;
        private TextBox[] cafeQuantities;
        private Label lblCafeResult;

        private Button btnCalculate;
        private Label lblTotalResult;
        private PictureBox picSmile;

        private System.Windows.Forms.Timer clearTimer;
        private decimal dailyRevenue = 0m;

        public Form1()
        {
            InitializeUI();
            UpdateFuelPrice();
            UpdateFuelMode();
        }

        
        private void InitializeUI()
        {
            this.Text = "BestOil";
            this.ClientSize = new Size(980, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += Form1_FormClosing;
            this.BackColor = Color.FromArgb(230, 226, 199); 
            this.Font = new Font("Segoe UI", 9F);
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            GroupBox grpFuel = new GroupBox
            {
                Text = "Автозаправка",
                Location = new Point(20, 20),
                Size = new Size(430, 460),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            Label lblFuel = new Label { Text = "Бензин", Location = new Point(20, 40), AutoSize = true, Font = this.Font };
            cmbFuel = new ComboBox
            {
                Location = new Point(120, 36),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = this.Font
            };
            foreach (var kv in fuelPrices) cmbFuel.Items.Add(kv.Key);
            cmbFuel.SelectedIndex = 0;
            cmbFuel.SelectedIndexChanged += (s, e) => { UpdateFuelPrice(); RecalcFuel(); };

            Label lblPrice = new Label { Text = "Ціна", Location = new Point(20, 90), AutoSize = true, Font = this.Font };
            txtFuelPrice = new TextBox { Location = new Point(120, 86), Width = 120, ReadOnly = true, TextAlign = HorizontalAlignment.Right, Font = this.Font, BackColor = Color.White };
            Label lblPriceUnit = new Label { Text = "грн.", Location = new Point(250, 90), AutoSize = true, Font = this.Font };

            rbQuantity = new RadioButton { Text = "Кількість", Location = new Point(20, 150), AutoSize = true, Checked = true, Font = this.Font };
            rbSum = new RadioButton { Text = "Сума", Location = new Point(20, 200), AutoSize = true, Font = this.Font };
            rbQuantity.CheckedChanged += (s, e) => UpdateFuelMode();
            rbSum.CheckedChanged += (s, e) => UpdateFuelMode();

            txtQuantity = new TextBox { Location = new Point(120, 146), Width = 120, Font = this.Font, Text = "0" };
            Label lblQuantityUnit = new Label { Text = "л.", Location = new Point(250, 150), AutoSize = true, Font = this.Font };
            txtSum = new TextBox { Location = new Point(120, 196), Width = 120, Font = this.Font, Text = "0" };
            Label lblSumUnit = new Label { Text = "грн.", Location = new Point(250, 200), AutoSize = true, Font = this.Font };

            txtQuantity.TextChanged += (s, e) => RecalcFuel();
            txtSum.TextChanged += (s, e) => RecalcFuel();

            GroupBox grpFuelPay = new GroupBox
            {
                Location = new Point(20, 260),
                Size = new Size(390, 170),
                Font = this.Font
            };
            lblFuelPayCaption = new Label { Text = "До оплати:", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            lblFuelResult = new Label { Text = "0,00", Location = new Point(60, 55), AutoSize = true, Font = new Font("Segoe UI", 20F, FontStyle.Bold) };
            lblFuelUnit = new Label { Text = "грн.", Location = new Point(230, 65), AutoSize = true, Font = new Font("Segoe UI", 12F) };
            grpFuelPay.Controls.AddRange(new Control[] { lblFuelPayCaption, lblFuelResult, lblFuelUnit });

            grpFuel.Controls.AddRange(new Control[] {
                lblFuel, cmbFuel, lblPrice, txtFuelPrice, lblPriceUnit,
                rbQuantity, rbSum, txtQuantity, lblQuantityUnit, txtSum, lblSumUnit,
                grpFuelPay
            });

            GroupBox grpCafe = new GroupBox
            {
                Text = "Міні-Кафе",
                Location = new Point(470, 20),
                Size = new Size(480, 460),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            Label lblColPrice = new Label { Text = "Ціна", Location = new Point(240, 35), AutoSize = true, Font = this.Font };
            Label lblColQty = new Label { Text = "Кількість", Location = new Point(350, 35), AutoSize = true, Font = this.Font };
            grpCafe.Controls.Add(lblColPrice);
            grpCafe.Controls.Add(lblColQty);

            int n = cafeItems.Length;
            cafeChecks = new CheckBox[n];
            cafeQuantities = new TextBox[n];

            int y0 = 65;
            for (int i = 0; i < n; i++)
            {
                int y = y0 + i * 45;
                var chk = new CheckBox { Text = cafeItems[i].Name, Location = new Point(20, y), AutoSize = true, Font = this.Font };
                var priceBox = new TextBox { Location = new Point(240, y - 3), Width = 90, ReadOnly = true, TextAlign = HorizontalAlignment.Right, Text = cafeItems[i].Price.ToString("0.00", CultureInfo.InvariantCulture), Font = this.Font, BackColor = Color.White };
                var qtyBox = new TextBox { Location = new Point(350, y - 3), Width = 90, Enabled = false, Text = "0", Font = this.Font };

                chk.CheckedChanged += (s, e) =>
                {
                    qtyBox.Enabled = chk.Checked;
                    if (!chk.Checked) qtyBox.Text = "0";
                    RecalcCafe();
                };
                qtyBox.TextChanged += (s, e) => RecalcCafe();

                cafeChecks[i] = chk;
                cafeQuantities[i] = qtyBox;

                grpCafe.Controls.Add(chk);
                grpCafe.Controls.Add(priceBox);
                grpCafe.Controls.Add(qtyBox);
            }

            GroupBox grpCafePay = new GroupBox
            {
                Location = new Point(20, y0 + n * 45 + 10),
                Size = new Size(440, 100),
                Font = this.Font
            };
            Label lblCafePayCaption = new Label { Text = "До оплати:", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10F) };
            lblCafeResult = new Label { Text = "0,00", Location = new Point(60, 45), AutoSize = true, Font = new Font("Segoe UI", 20F, FontStyle.Bold) };
            Label lblCafeUnit = new Label { Text = "грн.", Location = new Point(230, 55), AutoSize = true, Font = new Font("Segoe UI", 12F) };
            grpCafePay.Controls.AddRange(new Control[] { lblCafePayCaption, lblCafeResult, lblCafeUnit });
            grpCafe.Controls.Add(grpCafePay);

            GroupBox grpTotal = new GroupBox
            {
                Text = "ВСЬОГО до сплати",
                Location = new Point(20, 500),
                Size = new Size(930, 150),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            picSmile = new PictureBox
            {
                Location = new Point(20, 40),
                Size = new Size(70, 70),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Gold
            };

            btnCalculate = new Button
            {
                Text = "Прорахувати",
                Location = new Point(130, 55),
                Size = new Size(200, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.WhiteSmoke
            };
            btnCalculate.Click += BtnCalculate_Click;

            lblTotalResult = new Label
            {
                Text = "0,00 грн.",
                Location = new Point(600, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold)
            };

            grpTotal.Controls.AddRange(new Control[] { picSmile, btnCalculate, lblTotalResult });

            this.Controls.AddRange(new Control[] { grpFuel, grpCafe, grpTotal });

            // Таймер: через 10 секунд після розрахунку запитуємо про очищення форми
            clearTimer = new System.Windows.Forms.Timer { Interval = 10000 };
            clearTimer.Tick += ClearTimer_Tick;
        }

        private decimal ParseDecimal(string text)
        {
            if (decimal.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;
            return 0m;
        }

        private void UpdateFuelPrice()
        {
            string fuel = cmbFuel.SelectedItem?.ToString() ?? fuelPrices.Keys.First();
            decimal price = fuelPrices[fuel];
            txtFuelPrice.Text = price.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void UpdateFuelMode()
        {
            txtQuantity.Enabled = rbQuantity.Checked;
            txtSum.Enabled = rbSum.Checked;
            RecalcFuel();
        }

        private void RecalcFuel()
        {
            string fuel = cmbFuel.SelectedItem?.ToString() ?? fuelPrices.Keys.First();
            decimal price = fuelPrices[fuel];

            if (rbQuantity.Checked)
            {
                decimal qty = ParseDecimal(txtQuantity.Text);
                decimal sum = qty * price;
                lblFuelPayCaption.Text = "До оплати:";
                lblFuelResult.Text = sum.ToString("0.00", CultureInfo.InvariantCulture);
                lblFuelUnit.Text = "грн.";
            }
            else
            {
                decimal sum = ParseDecimal(txtSum.Text);
                decimal liters = price > 0 ? sum / price : 0;
                lblFuelPayCaption.Text = "До видачі:";
                lblFuelResult.Text = liters.ToString("0.00", CultureInfo.InvariantCulture);
                lblFuelUnit.Text = "л.";
            }
        }

        private decimal GetFuelMoneyAmount()
        {
            string fuel = cmbFuel.SelectedItem?.ToString() ?? fuelPrices.Keys.First();
            decimal price = fuelPrices[fuel];

            if (rbQuantity.Checked)
            {
                decimal qty = ParseDecimal(txtQuantity.Text);
                return qty * price;
            }
            else
            {
                return ParseDecimal(txtSum.Text);
            }
        }

        private void RecalcCafe()
        {
            lblCafeResult.Text = GetCafeAmount().ToString("0.00", CultureInfo.InvariantCulture);
        }

        private decimal GetCafeAmount()
        {
            decimal total = 0m;
            for (int i = 0; i < cafeItems.Length; i++)
            {
                if (cafeChecks[i].Checked)
                {
                    decimal qty = ParseDecimal(cafeQuantities[i].Text);
                    total += qty * cafeItems[i].Price;
                }
            }
            return total;
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            decimal fuelMoney = GetFuelMoneyAmount();
            decimal cafeMoney = GetCafeAmount();
            decimal total = fuelMoney + cafeMoney;

            lblTotalResult.Text = total.ToString("0.00", CultureInfo.InvariantCulture) + " грн.";
            dailyRevenue += total;

            clearTimer.Stop();
            clearTimer.Start();
        }

        private void ClearTimer_Tick(object sender, EventArgs e)
        {
            clearTimer.Stop();
            DialogResult result = MessageBox.Show(
                "Наступний клієнт. Очистити форму?",
                "BestOil",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ResetForm();
            }
            else
            {
                clearTimer.Start();
            }
        }

        private void ResetForm()
        {
            cmbFuel.SelectedIndex = 0;
            rbQuantity.Checked = true;
            txtQuantity.Text = "0";
            txtSum.Text = "0";

            for (int i = 0; i < cafeItems.Length; i++)
            {
                cafeChecks[i].Checked = false;
                cafeQuantities[i].Text = "0";
            }

            lblTotalResult.Text = "0,00 грн.";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show(
                $"Робочий день завершено.\nЗагальна виручка за день: {dailyRevenue.ToString("0.00", CultureInfo.InvariantCulture)} грн.",
                "BestOil",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}