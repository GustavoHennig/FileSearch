using System.Drawing;
using Modern.Forms;

namespace ControlGallery
{
    public class DialogForm : Form
    {
        public DialogForm (string message)
        {
            Text = "Dialog Form";
            Size = new Size (500, 250);

            
            var label = Controls.Add(new Label
            {
                Text = message,
                Left = 10,
                Top = 10,
                Width = 250
            });

            var button1 = Controls.Add (new Button {
                Text = "Set Form's DialogResult to Retry",
                Left = 10,
                Top = 44,
                Width = 250
            });

            button1.Click += (o, e) => DialogResult = DialogResult.Retry;

            var button2 = Controls.Add (new Button {
                Text = "Set Form's DialogResult to Ignore",
                Left = 10,
                Top = 84,
                Width = 250
            });


            button2.Click += (o, e) => DialogResult = DialogResult.Ignore;

        }
    }
}
