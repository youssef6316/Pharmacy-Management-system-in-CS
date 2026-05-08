namespace PMS_CS.Views;
public class MainForm : Form
{
    public Panel ContentPanel { get; private set; }

    public MainForm()
    {
        this.Text = "Pharmacy Management System";
        this.Size = new Size(900, 700);
        this.StartPosition = FormStartPosition.CenterScreen;

        ContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.WhiteSmoke
        };
        this.Controls.Add(ContentPanel);

        LoadPage(new EntryView(this));
    }

    public void LoadPage(UserControl view)
    {
        ContentPanel.Controls.Clear();
        view.Dock = DockStyle.Fill;
        ContentPanel.Controls.Add(view);
    }
}