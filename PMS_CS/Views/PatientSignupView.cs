using PMS_CS.src.Services;

namespace PMS_CS.Views;

    public class PatientSignupView : UserControl
    {
        public PatientSignupView(MainForm main)
        {
            var lblTitle = new Label { Text = "Patient Sign Up", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(350, 20) };
            var txtUser = new TextBox { Location = new Point(350, 80), Width = 200 };
            var txtPass = new TextBox { Location = new Point(350, 120), Width = 200, PasswordChar='*' };
            var txtEmail = new TextBox { Location = new Point(350, 160), Width = 200 };
            var txtPhone = new TextBox { Location = new Point(350, 200), Width = 200 };
            var txtAge = new TextBox { Location = new Point(350, 240), Width = 200 };
            var txtAddress = new TextBox { Location = new Point(350, 280), Width = 200 };

            var btnSignup = new Button { Text = "Sign Up", Location = new Point(350, 330), Size = new Size(90, 40), BackColor = Color.SteelBlue, ForeColor = Color.White };
            var btnBack = new Button { Text = "Back", Location = new Point(460, 330), Size = new Size(90, 40), BackColor = Color.SteelBlue, ForeColor = Color.White };

            btnSignup.Click += (s, e) => {
                if(float.TryParse(txtAge.Text, out float age)) {
                    var (id, err) = new UserService().RegisterPatient(txtUser.Text, txtPass.Text, txtEmail.Text, txtPhone.Text, age, txtAddress.Text);
                    if(id > 0) {
                        var (user, loginErr) = new UserService().Login(txtUser.Text, txtPass.Text);
                        if (user != null)
                        {
                            var patient = new PatientService().GetPatientById(user.UserId);
                            if (patient != null)
                            {
                                Session.CurrentUser = user;
                                Session.CurrentPatient = patient;
                                MessageBox.Show("Sign up successful.");
                                main.LoadPage(new PatientProfileView(main));
                            }
                            else
                            {
                                MessageBox.Show("Sign up successful. Please log in.");
                                main.LoadPage(new PatientLoginView(main));
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Sign up successful, but auto-login failed: {loginErr}");
                            main.LoadPage(new PatientLoginView(main));
                        }
                    } else MessageBox.Show(err, "Error");
                } else MessageBox.Show("Invalid age.");
            };

            btnBack.Click += (s, e) => main.LoadPage(new EntryView(main));

            Controls.AddRange(new Control[] { lblTitle,
                new Label{Text="Username:", Location=new Point(250,80)}, txtUser,
                new Label{Text="Password:", Location=new Point(250,120)}, txtPass,
                new Label{Text="Email:", Location=new Point(250,160)}, txtEmail,
                new Label{Text="Phone:", Location=new Point(250,200)}, txtPhone,
                new Label{Text="Age:", Location=new Point(250,240)}, txtAge,
                new Label{Text="Address:", Location=new Point(250,280)}, txtAddress,
                btnSignup, btnBack
            });
        }
    }
