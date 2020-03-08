using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SpectroChipApp
{
    class frame:PictureBox
    {

        public frame(int x, int y, int w, int h)
        {
            this.BackColor = Color.Transparent;
            this.Location = new Point(x , y );
            this.Size = new Size(w, h);
        }



    }
}
