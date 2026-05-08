using PMS_CS.src.Services;

namespace PMS_CS.Views;

    // ─── PATIENT LOGIN VIEW ────────────────────────────────────────────────
    public class PatientLoginView : UserControl
    {
        public PatientLoginView(MainForm main)
        {
            var lblTitle = new Label { Text = "Patient Login", Font = new Font("Arial", 20, FontStyle.Bold), AutoSize = true, Location = new Point(350, 50) };
            var txtUser = new TextBox { Location = new Point(350, 150), Width = 200 };
            var txtPass = new TextBox { Location = new Point(350, 200), Width = 200, PasswordChar = '*' };
            
            var btnLogin = new Button { Text = "Login", Location = new Point(350, 250), Size = new Size(90, 40), BackColor = Color.DodgerBlue, ForeColor = Color.White };
            var btnSignup = new Button { Text = "Sign Up", Location = new Point(460, 250), Size = new Size(90, 40), BackColor = Color.SeaGreen, ForeColor = Color.White };
            var btnBack = new Button { Text = "Back", Location = new Point(405, 310), Size = new Size(90, 40), BackColor = Color.Gray, ForeColor = Color.White };

            btnLogin.Click += (s, e) => {
                var (user, error) = new UserService().Login(txtUser.Text, txtPass.Text);
                if (user != null)
                {
                    var patient = new PatientService().GetPatientById(user.UserId);
                    if (patient != null)
                    {
                        Session.CurrentUser = user;
                        Session.CurrentPatient = patient;
                        main.LoadPage(new PatientProfileView(main));
                    }
                    else MessageBox.Show("User is not a patient.");
                }
                else MessageBox.Show(error, "Error");
            };

            btnSignup.Click += (s, e) => main.LoadPage(new PatientSignupView(main));
            btnBack.Click += (s, e) => main.LoadPage(new EntryView(main));

            Controls.AddRange(new Control[] { lblTitle, new Label{Text="Username:", Location=new Point(250,150)}, txtUser, new Label{Text="Password:", Location=new Point(250,200)}, txtPass, btnLogin, btnSignup, btnBack });
        }
    }