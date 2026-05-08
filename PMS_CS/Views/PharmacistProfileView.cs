namespace PMS_CS.Views;

public class PharmacistProfileView : UserControl
{
    public PharmacistProfileView(MainForm main)
    {
        var emp = Session.CurrentEmployee;
        var u = Session.CurrentUser;
        if (emp == null || u == null)
        {
            MessageBox.Show("Session expired. Please log in again.");
            main.LoadPage(new EntryView(main));
            return;
        }

        var lblTitle = new Label { Text = $"Employee Profile: {u.Username}", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(50, 20) };
        var lblInfo = new Label { Text = $"Email: {u.Email} | Phone: {u.Phone} | Salary: {emp.Salary:C}", Location = new Point(50, 70), AutoSize = true, Font = new Font("Arial", 12) };
        var lblRole = new Label { Text = $"Role: {emp.JobType}", Location = new Point(50, 95), AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold) };

        var btnInventory = new Button { Text = "Manage Inventory", Location = new Point(50, 130), Size = new Size(150, 40), BackColor = Color.DodgerBlue, ForeColor = Color.White };
        var btnOrders = new Button { Text = "Received Orders", Location = new Point(220, 130), Size = new Size(150, 40), BackColor = Color.SeaGreen, ForeColor = Color.White };
        var btnLogout = new Button { Text = "Logout", Location = new Point(700, 20), Size = new Size(100, 30), BackColor = Color.Crimson, ForeColor = Color.White };

        btnInventory.Enabled = emp.IsPharmacist() || emp.IsAdmin();
        btnOrders.Enabled = emp.IsCashier() || emp.IsAdmin();

        btnInventory.Click += (s, e) => main.LoadPage(new InventoryView(main));
        btnOrders.Click += (s, e) => main.LoadPage(new ReceivedOrdersView(main));
        btnLogout.Click += (s, e) => { Session.Clear(); main.LoadPage(new EntryView(main)); };

        Controls.AddRange(new Control[] { lblTitle, lblInfo, lblRole, btnInventory, btnOrders, btnLogout });
    }
}
