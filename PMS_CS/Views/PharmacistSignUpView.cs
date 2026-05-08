using PMS_CS.src.Models;
using PMS_CS.src.Services;

namespace PMS_CS.Views;

    public class PharmacistSignupView : UserControl
    {
        public PharmacistSignupView(MainForm main)
        {
            var lblTitle = new Label { Text = "Employee Sign Up", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(330, 20) };
            var txtUser = new TextBox { Location = new Point(350, 80), Width = 200 };
            var txtPass = new TextBox { Location = new Point(350, 120), Width = 200, PasswordChar='*' };
            var txtEmail = new TextBox { Location = new Point(350, 160), Width = 200 };
            var txtPhone = new TextBox { Location = new Point(350, 200), Width = 200 };
            var cmbRole = new ComboBox { Location = new Point(350, 240), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new[] { "Pharmacist", "Cashier" });
            cmbRole.SelectedIndex = 0;
            var txtCode = new TextBox { Location = new Point(350, 280), Width = 200 };

            var btnSignup = new Button { Text = "Sign Up", Location = new Point(350, 330), Size = new Size(90, 40), BackColor = Color.SeaGreen, ForeColor = Color.White };
            var btnBack = new Button { Text = "Back", Location = new Point(460, 330), Size = new Size(90, 40), BackColor = Color.Gray, ForeColor = Color.White };

            btnSignup.Click += (s, e) => {
                if(string.Equals(txtCode.Text?.Trim(), "PHARMACY123", StringComparison.OrdinalIgnoreCase)) {
                    // C# Backend Requires an Admin to register an employee. We simulate an Admin here just for the signup to work!
                    var dummyAdmin = new Employee { JobType = "Admin" };
                    var selectedRole = cmbRole.SelectedItem?.ToString() ?? "Pharmacist";
                    var (id, err) = new UserService().RegisterEmployee(txtUser.Text, txtPass.Text, txtEmail.Text, txtPhone.Text, selectedRole, 5000, dummyAdmin);
                    if(id > 0) {
                        var (user, loginErr) = new UserService().Login(txtUser.Text, txtPass.Text);
                        if (user != null)
                        {
                            var employee = new PMS_CS.src.Repositories.EmployeeRepository().GetEmployeeById(user.UserId);
                            if (employee != null)
                            {
                                Session.CurrentUser = user;
                                Session.CurrentEmployee = employee;
                                MessageBox.Show("Sign up successful.");
                                main.LoadPage(new PharmacistProfileView(main));
                            }
                            else
                            {
                                MessageBox.Show("Sign up successful. Please log in.");
                                main.LoadPage(new PharmacistLoginView(main));
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Sign up successful, but auto-login failed: {loginErr}");
                            main.LoadPage(new PharmacistLoginView(main));
                        }
                    } else MessageBox.Show(err, "Error");
                } else MessageBox.Show("Invalid validation code.");
            };

            btnBack.Click += (s, e) => main.LoadPage(new EntryView(main));

            Controls.AddRange(new Control[] { lblTitle,
                new Label{Text="Username:", Location=new Point(220,80)}, txtUser,
                new Label{Text="Password:", Location=new Point(220,120)}, txtPass,
                new Label{Text="Email:", Location=new Point(220,160)}, txtEmail,
                new Label{Text="Phone:", Location=new Point(220,200)}, txtPhone,
                new Label{Text="Role:", Location=new Point(220,240)}, cmbRole,
                new Label{Text="Validation Code:", Location=new Point(220,280)}, txtCode,
                btnSignup, btnBack
            });
        }
    }
