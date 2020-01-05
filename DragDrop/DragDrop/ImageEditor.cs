using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DragDrop
{
    public partial class ImageEditor : UserControl
    {
        List<Image> _images = new List<Image>();
        List<ImageBound> imageBounds = new List<ImageBound>();
        Map _map = new Map();
        public ImageEditor()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _map.CursorChanged += (s, e) =>
              {
                  this.Cursor = e.Cursor;
              };
        }
        class ImageBound
        {
            public Image Img { get; set; }

            public Point Location { get; set; }
            public Rectangle Bound
            {
                get
                {
                    if (Img == null)
                        return new Rectangle(0, 0, 0, 0);

                    return new Rectangle(Location, Img.Size);
                }
            }
        }
        public void Add(Image img)
        {
            if (img == null)
                throw new ArgumentNullException();

            _images.Add(img);
        }

        public void AddRange(IEnumerable<Image> imgs)
        {
            if (imgs == null)
                throw new ArgumentNullException();

            foreach (var img in imgs)
                this.Add(img);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int x = 0,y=0;

            int leftWidth = this.Width / 2;
            int maxHeight = 0;
            imageBounds.Clear();
            //render images;
            for (int i = 0; i < _images.Count; i++)
            {
                if (x + _images[i].Width > leftWidth)
                {
                    x = 0;
                    y += maxHeight;
                    maxHeight = 0;
                }
                e.Graphics.DrawImage(_images[i],x,y);

                imageBounds.Add(new ImageBound() { Img = _images[i], Location = new Point(x, y) });
                x += _images[i].Width;
                if (maxHeight < _images[i].Height)
                    maxHeight = _images[i].Height;
               
            }
            _map.Bound = new Rectangle(this.Width / 2, 0, this.Width / 2, this.Height);
            if (cur != null)
                e.Graphics.DrawImage(cur.Img, cur.Bound);
            _map.Render(new RenderContext(e.Graphics, _map.Bound));
        }

        bool _isMouseDown = false;
        ImageBound cur = null;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            cur = null;
            _isMouseDown = true;
            foreach (var img in imageBounds)
            {
                if (img.Bound.Contains(e.Location))
                {
                    cur = new ImageBound() { Img = img.Img,Location = img.Location };
                    break;
                }
            }
            _lastMouseLocation = e.Location;
            base.OnMouseDown(e);

            _map.OnMouseDown(e);
            this.Invalidate(false);
        }
        bool Intersect(Rectangle r1, Rectangle r2)
        {
            return r1.IntersectsWith(r2);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (cur != null)
            {
                if (cur.Bound.IntersectsWith(_map.Bound))
                {
                    //add 
                    _map.Add(cur.Img, cur.Bound.Location);
                }
            }
            _isMouseDown = false;
            cur = null;
            base.OnMouseUp(e);
            _map.OnMouseUp(e);
            this.Invalidate(false);
        }

        Point _lastMouseLocation;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isMouseDown && cur != null)
            {
                Point start = cur.Location;
                Pan(ref start, MeasureVector(_lastMouseLocation,e.Location));
                cur.Location = start;
            }
            _lastMouseLocation = e.Location;
            base.OnMouseMove(e);
            _map.OnMouseMove(e);
            this.Invalidate(false);
        }

        public static void Pan(ref Point location, Point vector)
        {
            location = new Point(location.X + vector.X, location.Y + vector.Y);
        }

        public static Point MeasureVector(Point start, Point end)
        {
            return new Point(end.X - start.X, end.Y - start.Y);
         }
    }
}
