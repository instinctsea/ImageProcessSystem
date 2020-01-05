using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DragDrop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ImageEditor imageEditor = new ImageEditor();
            imageEditor.Dock = DockStyle.Fill;
            this.Controls.Add(imageEditor);

            imageEditor.Add(Image.FromFile("./1.png"));
            imageEditor.Add(Image.FromFile("./2.png"));
        }
    }
}
