using PMS_CS.src.Models;
using PMS_CS.src.Repositories;
using PMS_CS.src.Services;

namespace PMS_CS.Views;

    public class OrderView : UserControl
    {
        private Order currentOrder = new Order();

        public OrderView(MainForm main)
        {
            if (Session.CurrentPatient == null)
            {
                MessageBox.Show("Session expired. Please log in again.");
                main.LoadPage(new EntryView(main));
                return;
            }

            var lblTitle = new Label { Text = "Place Order", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(50, 20) };
            var btnBack = new Button { Text = "Back", Location = new Point(700, 20), Size = new Size(100, 30), BackColor = Color.Gray, ForeColor = Color.White };

            var gridAvailable = new DataGridView { Location = new Point(50, 80), Size = new Size(350, 300), ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            var gridCart = new DataGridView { Location = new Point(450, 80), Size = new Size(350, 300), ReadOnly = true };
            
            var invService = new InventoryService();
            gridAvailable.DataSource = invService.GetAllMedicines().Select(m => new { m.MedicineId, m.Name, m.Price, Stock = m.StockQuantity }).ToList();

            var btnAdd = new Button { Text = "Add to Order ->", Location = new Point(50, 400), Size = new Size(150, 40), BackColor = Color.DodgerBlue, ForeColor = Color.White };
            var btnPlace = new Button { Text = "Submit Order", Location = new Point(450, 400), Size = new Size(150, 40), BackColor = Color.SeaGreen, ForeColor = Color.White };
            var lblTotal = new Label { Text = "Total: $0.00", Font = new Font("Arial", 14, FontStyle.Bold), AutoSize = true, Location = new Point(650, 410) };

            Action RefreshCart = () => {
                gridCart.DataSource = null;
                gridCart.DataSource = currentOrder.OrderItems.Select(i => new { i.MedicineId, i.Quantity, i.UnitPrice, Total = i.LineTotal }).ToList();
                currentOrder.RecalculateTotal();
                lblTotal.Text = $"Total: {currentOrder.TotalPrice:C}";
            };

            btnAdd.Click += (s, e) => {
                if(gridAvailable.SelectedRows.Count > 0) {
                    int medId = (int)gridAvailable.SelectedRows[0].Cells["MedicineId"].Value;
                    double price = (double)gridAvailable.SelectedRows[0].Cells["Price"].Value;
                    string qtyStr = Prompt.ShowDialog("Enter quantity:", "Quantity");
                    if(int.TryParse(qtyStr, out int qty) && qty > 0) {
                        currentOrder.AddItem(new OrderItem { MedicineId = medId, Quantity = qty, UnitPrice = price });
                        RefreshCart();
                    }
                }
            };

            btnPlace.Click += (s, e) => {
                currentOrder.PatientId = Session.CurrentPatient.UserId;
                var employeeRepo = new EmployeeRepository();
                var assignedCashier =
                    employeeRepo.GetByJobType("Cashier").FirstOrDefault() ??
                    employeeRepo.GetByJobType("Admin").FirstOrDefault();

                if (assignedCashier == null)
                {
                    MessageBox.Show("No cashier/admin account exists in the database to attach this order.");
                    return;
                }

                currentOrder.CashierId = assignedCashier.UserId;
                var (id, err) = new OrderService().PlaceOrder(currentOrder);
                if(id > 0) { MessageBox.Show("Order Placed!"); main.LoadPage(new PatientProfileView(main)); }
                else MessageBox.Show(err, "Error");
            };

            btnBack.Click += (s, e) => main.LoadPage(new PatientProfileView(main));

            Controls.AddRange(new Control[] { lblTitle, btnBack, new Label{Text="Available Medicines", Location=new Point(50,60), AutoSize=true}, gridAvailable, new Label{Text="Your Cart", Location=new Point(450,60), AutoSize=true}, gridCart, btnAdd, btnPlace, lblTotal });
        }
    }
