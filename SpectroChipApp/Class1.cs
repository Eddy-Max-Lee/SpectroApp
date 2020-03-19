using System.Drawing;
using System.Windows.Forms;

namespace SpectroChipApp
{
    internal class frame : PictureBox
    {
        public frame(int x, int y, int w, int h)
        {
            this.BackColor = Color.Transparent;
            this.Location = new Point(x, y);
            this.Size = new Size(w, h);
        }
    }
}