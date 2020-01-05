using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DragDrop
{
    public sealed class RenderContext
    {
        public RenderContext(Graphics g, Rectangle bound)
        {

            this.G = g;
            this.Bound = bound;
        }

        public Graphics G
        {
            get;
        }

        public Rectangle Bound
        {
            get;
        }
    }
}
