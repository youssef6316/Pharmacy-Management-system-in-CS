using PMS_CS.src.Services;

namespace PMS_CS.Views;

    public class PatientProfileView : UserControl
    {
        public PatientProfileView(MainForm main)
        {
            var p = Session.CurrentPatient;
            var u = Session.CurrentUser;
            if (p == null || u == null)
            {
                MessageBox.Show("Session expired. Please log in again.");
                main.LoadPage(new EntryView(main));
                return;
            }

            var lblTitle = new Label { Text = $"Welcome, {u.Username}", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(50, 20) };
            
            var lblInfo = new Label { Text = $"Age: {p.Age} | Address: {p.Address} | Balance: {p.PatientBalance:C}", Location = new Point(50, 70), AutoSize=true, Font=new Font("Arial", 12) };

            var btnOrder = new Button { Text = "Place Order", Location = new Point(50, 110), Size = new Size(150, 40), BackColor = Color.SeaGreen, ForeColor = Color.White };
            var btnPay = new Button { Text = "Proceed to Payment", Location = new Point(220, 110), Size = new Size(150, 40), BackColor = Color.DodgerBlue, ForeColor = Color.White };
            var btnTopUp = new Button { Text = "Top Up Balance", Location = new Point(390, 110), Size = new Size(150, 40), BackColor = Color.Purple, ForeColor = Color.White };
            var btnLogout = new Button { Text = "Logout", Location = new Point(700, 20), Size = new Size(100, 30), BackColor = Color.Crimson, ForeColor = Color.White };

            var gridOrders = new DataGridView { Location = new Point(50, 180), Size = new Size(750, 400), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            
            // Load Orders
            gridOrders.DataSource = new OrderService().GetOrdersByPatient(p.UserId).Select(o => new { o.OrderId, o.OrderDate, o.Status, Total = o.TotalPrice }).ToList();

            btnOrder.Click += (s, e) => main.LoadPage(new OrderView(main));
            btnPay.Click += (s, e) => main.LoadPage(new PaymentView(main));
            btnLogout.Click += (s, e) => { Session.Clear(); main.LoadPage(new EntryView(main)); };
            btnTopUp.Click += (s, e) => {
                string amtStr = Prompt.ShowDialog("Enter amount to top up:", "Top Up");
                if (double.TryParse(amtStr, out double amt)) {
                    new PatientService().TopUpBalance(p.UserId, amt);
                    Session.CurrentPatient = new PatientService().GetPatientById(p.UserId); // Refresh
                    main.LoadPage(new PatientProfileView(main));
                }
            };

            Controls.AddRange(new Control[] { lblTitle, lblInfo, btnOrder, btnPay, btnTopUp, btnLogout, new Label{Text="Your Orders:", Location=new Point(50,150)}, gridOrders });
        }
    }
