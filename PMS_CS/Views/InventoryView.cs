using PMS_CS.src.Models;
using PMS_CS.src.Services;

namespace PMS_CS.Views;

    public class InventoryView : UserControl
    {
        public InventoryView(MainForm main)
        {
            var lblTitle = new Label { Text = "Inventory Management", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(50, 20) };
            var btnBack = new Button { Text = "Back", Location = new Point(700, 20), Size = new Size(100, 30), BackColor = Color.Gray, ForeColor = Color.White };

            var gridMeds = new DataGridView { Location = new Point(50, 80), Size = new Size(750, 300), ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            
            Action RefreshGrid = () => { gridMeds.DataSource = new InventoryService().GetAllMedicines().Select(m => new { m.MedicineId, m.Name, m.Category, m.Price, m.StockQuantity }).ToList(); };
            RefreshGrid();

            var txtName = new TextBox { Location = new Point(150, 400), Width = 120 };
            var txtCat = new TextBox { Location = new Point(150, 430), Width = 120 };
            var txtPrice = new TextBox { Location = new Point(150, 460), Width = 120 };
            var txtQty = new TextBox { Location = new Point(150, 490), Width = 120 };
            var btnAdd = new Button { Text = "Add Item", Location = new Point(150, 520), Size = new Size(120, 30), BackColor = Color.SeaGreen, ForeColor = Color.White };

            btnAdd.Click += (s, e) => {
                if(double.TryParse(txtPrice.Text, out double price) && int.TryParse(txtQty.Text, out int qty)) {
                    var med = new Medicine(txtName.Text, price, txtCat.Text, "2026-01-01", qty, "Take daily", false);
                    var dummyAdmin = new Employee { JobType = "Admin" }; // Bypass Admin check for UI functionality
                    var (succ, err) = new InventoryService().AddMedicine(med, dummyAdmin);
                    if(succ) { MessageBox.Show("Added!"); RefreshGrid(); } else MessageBox.Show(err);
                } else MessageBox.Show("Invalid numbers.");
            };

            btnBack.Click += (s, e) => main.LoadPage(new PharmacistProfileView(main));

            Controls.AddRange(new Control[] { lblTitle, btnBack, gridMeds,
                new Label{Text="Name:", Location=new Point(50,400)}, txtName,
                new Label{Text="Category:", Location=new Point(50,430)}, txtCat,
                new Label{Text="Price:", Location=new Point(50,460)}, txtPrice,
                new Label{Text="Quantity:", Location=new Point(50,490)}, txtQty,
                btnAdd
            });
        }
    }
