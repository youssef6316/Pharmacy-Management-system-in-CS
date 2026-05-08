// ─── ENTRY VIEW ────────────────────────────────────────────────────────

namespace PMS_CS.Views;
public class EntryView : UserControl
{
    public EntryView(MainForm main)
    {
        var lblTitle = new Label { Text = "Pharmacy Management System", Font = new Font("Arial", 24, FontStyle.Bold), AutoSize = true, Location = new Point(250, 150) };
        var btnPatient = new Button { Text = "Patient Login", Font = new Font("Arial", 14), Size = new Size(200, 50), Location = new Point(200, 250), BackColor = Color.SeaGreen, ForeColor = Color.White };
        var btnPharmacist = new Button { Text = "Employee Login", Font = new Font("Arial", 14), Size = new Size(200, 50), Location = new Point(450, 250), BackColor = Color.DodgerBlue, ForeColor = Color.White };

        btnPatient.Click += (s, e) => main.LoadPage(new PatientLoginView(main));
        btnPharmacist.Click += (s, e) => main.LoadPage(new PharmacistLoginView(main));

        Controls.Add(lblTitle);
        Controls.Add(btnPatient);
        Controls.Add(btnPharmacist);
    }
}