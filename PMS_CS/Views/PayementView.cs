using PMS_CS.src.Services;

namespace PMS_CS.Views;

    public class PaymentView : UserControl
    {
        public PaymentView(MainForm main)
        {
            if (Session.CurrentPatient == null)
            {
                MessageBox.Show("Session expired. Please log in again.");
                main.LoadPage(new EntryView(main));
                return;
            }

            var lblTitle = new Label { Text = "Make Payment", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(50, 20) };
            var btnBack = new Button { Text = "Back", Location = new Point(700, 20), Size = new Size(100, 30), BackColor = Color.Gray, ForeColor = Color.White };

            var orders = new OrderService().GetOrdersByPatient(Session.CurrentPatient.UserId).Where(o => o.Status == "Pending").ToList();
            var gridPending = new DataGridView { Location = new Point(50, 80), Size = new Size(750, 250), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            gridPending.DataSource = orders.Select(o => new { o.OrderId, o.OrderDate, Total = o.TotalPrice }).ToList();

            var cmbMethod = new ComboBox { Location = new Point(50, 360), Width = 150 };
            cmbMethod.Items.AddRange(new[] { "Credit Card", "Cash", "Fawry" });
            cmbMethod.SelectedIndex = 0;

            var btnPay = new Button { Text = "Pay Selected Order", Location = new Point(220, 350), Size = new Size(150, 40), BackColor = Color.SeaGreen, ForeColor = Color.White };

            btnPay.Click += (s, e) => {
                if(gridPending.SelectedRows.Count > 0) {
                    int orderId = (int)gridPending.SelectedRows[0].Cells["OrderId"].Value;
                    var method = cmbMethod.SelectedItem?.ToString();
                    if (string.IsNullOrWhiteSpace(method))
                    {
                        MessageBox.Show("Please select a payment method.");
                        return;
                    }

                    var (pid, err) = new PaymentService().ProcessPayment(orderId, method);
                    if(pid > 0) { 
                        MessageBox.Show("Payment Successful!"); 
                        // Refresh Patient Balance
                        Session.CurrentPatient = new PatientService().GetPatientById(Session.CurrentPatient.UserId);
                        main.LoadPage(new PatientProfileView(main)); 
                    } else MessageBox.Show(err, "Error");
                }
            };

            btnBack.Click += (s, e) => main.LoadPage(new PatientProfileView(main));

            Controls.AddRange(new Control[] { lblTitle, btnBack, new Label{Text="Pending Orders", Location=new Point(50,60), AutoSize=true}, gridPending, new Label{Text="Payment Method:", Location=new Point(50,340)}, cmbMethod, btnPay });
        }
    }
