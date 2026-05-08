using PMS_CS.src.Services;

namespace PMS_CS.Views;

// ─── RECEIVED ORDERS VIEW ──────────────────────────────────────────────
public class ReceivedOrdersView : UserControl
{
    public ReceivedOrdersView(MainForm main)
    {
        var currentEmployee = Session.CurrentEmployee;
        if (currentEmployee == null)
        {
            MessageBox.Show("Session expired. Please log in again.");
            main.LoadPage(new EntryView(main));
            return;
        }

        var lblTitle = new Label { Text = "Received Orders", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(50, 20) };
        var btnBack = new Button { Text = "Back", Location = new Point(700, 20), Size = new Size(100, 30), BackColor = Color.Gray, ForeColor = Color.White };
        var btnRefresh = new Button { Text = "Refresh", Location = new Point(580, 20), Size = new Size(100, 30), BackColor = Color.DodgerBlue, ForeColor = Color.White };
        var btnComplete = new Button { Text = "Complete Selected", Location = new Point(50, 500), Size = new Size(170, 36), BackColor = Color.SeaGreen, ForeColor = Color.White };
        var btnCancel = new Button { Text = "Cancel Selected", Location = new Point(240, 500), Size = new Size(170, 36), BackColor = Color.IndianRed, ForeColor = Color.White };

        // A grid to actually show the orders
        var gridOrders = new DataGridView
        {
            Location = new Point(50, 80),
            Size = new Size(750, 400),
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        var orderService = new OrderService();
        Action refreshGrid = () =>
        {
            var orders = orderService.GetReceivedOrdersForEmployee(currentEmployee);
            gridOrders.DataSource = orders.Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                o.Status,
                Total = o.TotalPrice.ToString("C"),
                o.PatientId,
                o.CashierId
            }).ToList();
        };
        refreshGrid();

        btnRefresh.Click += (s, e) => refreshGrid();

        btnComplete.Click += (s, e) =>
        {
            if (gridOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order first.");
                return;
            }

            int orderId = (int)gridOrders.SelectedRows[0].Cells["OrderId"].Value;
            var (success, err) = orderService.CompleteOrder(orderId, currentEmployee);
            if (!success)
            {
                MessageBox.Show(err, "Error");
                return;
            }

            MessageBox.Show("Order marked as completed.");
            refreshGrid();
        };

        btnCancel.Click += (s, e) =>
        {
            if (gridOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an order first.");
                return;
            }

            int orderId = (int)gridOrders.SelectedRows[0].Cells["OrderId"].Value;
            var (success, err) = orderService.CancelOrder(orderId);
            if (!success)
            {
                MessageBox.Show(err, "Error");
                return;
            }

            MessageBox.Show("Order cancelled.");
            refreshGrid();
        };

        btnBack.Click += (s, e) => main.LoadPage(new PharmacistProfileView(main));

        Controls.AddRange(new Control[] { lblTitle, btnBack, btnRefresh, gridOrders, btnComplete, btnCancel });
    }
}