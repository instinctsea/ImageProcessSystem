using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DragDrop
{
    public class Map
    {
        enum Status
        {
            None=0,
            Selected=1,
            Pan=2,
            Resize=4,
            Rotate=8
        }
        class ImageItem
        {
            private Image _img = null;
            internal Image Img
            {
                get { return _img; }
                set
                {
                    _img = value;
                    if (_img != null)
                    {
                        _width = _img.Width;
                        _height = _img.Height;
                    }
                    UpdateTransformArray();
                }
            }

            private int _width = 0;
            private int _height = 0;
            public Size Size
            {
                get { return new Size(_width, _height); }
                set
                {
                    _width = value.Width;
                    _height = value.Height;
                }
            }

            public Point Location
            {
                get;set;
            }

            internal Rectangle Bound
            {
                get
                {
                    return new Rectangle(Location, Size);
                }
            }

            internal Status Status { get; set; }

            private Matrix _matrix=new Matrix();

            internal void Render(RenderContext renderContext)
            {
                renderContext.G.Transform = _matrix;
                renderContext.G.DrawImage(this._img, Bound);
                if (this.Status != Status.None)
                {
                    using (Pen border = new Pen(Color.Red, 2))
                    {
                        renderContext.G.DrawRectangle(border, this.Bound);
                    }
                }
                renderContext.G.ResetTransform();
            }

            public void Pan(Point start, Point end)
            {
                Matrix matrix = _matrix.Clone();
                matrix.Invert();
                Point[] points = new Point[2] { start,end };
                matrix.TransformPoints(points);

                var vector = ImageEditor.MeasureVector(points[0], points[1]);
                //Matrix matrix = new Matrix();
                //matrix.Translate(vector.X, vector.Y);
                _matrix.Translate(vector.X,vector.Y);
                ////_matrix.Translate(vector.X, vector.Y);
                UpdateTransformArray();
            }

            public void ScaleTo(Point start, Point end)
            {
                if (this._width == 0 || this._height == 0)
                    return;
                Matrix matrix = _matrix.Clone();
                matrix.Invert();
                Point[] points = new Point[2] { start, end };
                matrix.TransformPoints(points);

                var vector = ImageEditor.MeasureVector(points[0], points[1]);

                /////matrix.Invert();
                float centerx = this.Bound.Left + this.Bound.Width / 2;
                float centery = this.Bound.Top + this.Bound.Height / 2;

                
                if (start.X > centerx)
                {
                    _width += vector.X;
                }
                else
                {
                    _width -= vector.X;
                    Location = new Point(Location.X + vector.X, Location.Y);
                }

                if (start.Y > centery)
                {
                    _height += vector.Y;
                }
                else
                {
                    _height -= vector.Y;
                    Location = new Point(Location.X, Location.Y+vector.Y);
                }
                ///////_matrix.Reset();
                //float xScale = vector.X*1.0f / this._width+1;
                //float yScale = vector.Y*1.0f / this._height+1;
                //_matrix.Scale(xScale,yScale,MatrixOrder.Prepend);

                
                ////_matrix.Translate(-1*vector.X,-1* vector.Y);
                //matrix.Invert();
                /////_matrix.Multiply(matrix);
                ///_matrix.Multiply(matrix);
                UpdateTransformArray();
            }

            float[] _transformPoints = null;
            public void Rotate(float angle,Point pt)
            {
                Matrix matrix = _matrix.Clone();
                matrix.Invert();
                Point[] points = new Point[1] { pt };
                matrix.TransformPoints(points);
                //Matrix matrix = new Matrix();
                //matrix.RotateAt(angle, points[]);
                _matrix.RotateAt(angle,points[0]);
                UpdateTransformArray();
            }

            Point[] GetPolygon()
            {
                Point[] polygon = new Point[4];
                polygon[0] = new Point(Bound.Left, Bound.Top);
                polygon[1] = new Point(Bound.Left, Bound.Bottom);
                polygon[2] = new Point(Bound.Right, Bound.Bottom);
                polygon[3] = new Point(Bound.Right, Bound.Top);
                _matrix.TransformPoints(polygon);

                return polygon;
            }
            void UpdateTransformArray()
            {
                Point[] polygon = GetPolygon();

                List<float> result = new List<float>();
                foreach (var pt in polygon)
                {
                    result.Add(pt.X);
                    result.Add(pt.Y);
                }

                _transformPoints= result.ToArray();
            }
            public bool Contains(Point location)
            {
                if (_transformPoints == null)
                    return false;
                return IsPointInPolygon(location.X,location.Y,_transformPoints);
            }

            public static bool IsPointInPolygon(float x, float y, float[] data)
            {
                bool result = false;
                int j = data.Length / 2 - 1;
                for (int i = 0; i < data.Length / 2; i++)
                {
                    double curx = data[i * 2];
                    double cury = data[i * 2 + 1];

                    double indexjx = data[j * 2];
                    double indexjy = data[j * 2 + 1];
                    if (cury < y && indexjy >= y || indexjy < y && cury >= y)
                    {
                        if (curx + (y - cury) / (indexjy - cury) * (indexjx - curx) < x)
                        {
                            result = !result;
                        }
                    }
                    j = i;
                }
                return result;
            }
            const int Tolerance = 6;
            public bool IsAroundJiao(Point location)
            {
                if (_transformPoints == null)
                    return false;

                for (int i = 0; i < _transformPoints.Length / 2;i++)
                {
                    if (Math.Sqrt(Math.Pow(location.X - _transformPoints[i * 2], 2) + Math.Pow(location.Y - _transformPoints[i * 2 + 1], 2)) < Tolerance)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool IsOnLineCenter(Point location)
            {
                var temp = new Point(586, 253);
                //var newMaxtrix = _matrix.Clone();
                //newMaxtrix.Invert();
                //Point[] points = new Point[1] { location };
                //newMaxtrix.TransformPoints(points);
                for (int i = 0; i < _transformPoints.Length / 2; i++)
                {
                    int next = (i + 1) * 2;
                    if (next > _transformPoints.Length - 1)
                    next = 0;
                    float centerx = (_transformPoints[i * 2] + _transformPoints[next])/2;
                    float centery = (_transformPoints[i * 2+1] + _transformPoints[next+1])/2;
                    if (Math.Sqrt(Math.Pow(location.X -centerx , 2) + Math.Pow(location.Y - centery, 2)) < Tolerance)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool IsOnCenter(Point location)
            {
                var newMaxtrix = _matrix.Clone();
                newMaxtrix.Invert();
                Point[] points = new Point[1] { location };
                newMaxtrix.TransformPoints(points);

                float centerx = this.Bound.Left + this.Bound.Width / 2;
                float centery = this.Bound.Top + this.Bound.Height / 2;
                if (Math.Sqrt(Math.Pow(centerx - points[0].X, 2) + Math.Pow(centery-points[0].Y, 2)) < Tolerance)
                {
                    return true;
                }

                return false;
            }
        }
        public Rectangle Bound
        {
            get;internal set;
        }
        List<ImageItem> _items = new List<ImageItem>();
        public void Add(Image img, Point location)
        {
            _items.Add(new ImageItem() { Location = location, Img = img });
        }
        public virtual void Render(RenderContext rc)
        {
            using (Pen black = new Pen(Color.Black, 1))
            {
                rc.G.DrawRectangle(black, rc.Bound);
            }
                
            foreach (ImageItem item in _items)
            {
                item.Render(rc);
            }
        }

        bool _isMouseDown = false;
        public virtual void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _lastMousePoint = e.Location;
                _isMouseDown = true;
                foreach (var item in _items)
                {
                    if (item.Status == Status.None)
                    {
                        if (item.Contains(e.Location))
                        {
                            item.Status = Status.Selected;

                            foreach (var item2 in _items)
                            {
                                if (item != item2)
                                {
                                    item2.Status = Status.None;
                                }
                            }
                            break;
                        }
                        else
                        {
                            item.Status = Status.None;
                        }
                    }
                    else
                    {
                        if (item.Status== Status.Rotate)
                        {
                            item.Rotate(1, e.Location);
                        }
                        //else if (item.IsOnCenter(e.Location))
                        //{
                        //    item.Status = Status.Pan;
                        //}
                        //else if (item.IsOnLineCenter(e.Location))
                        //{
                        //    item.Status = Status.Rotate;
                        //}
                        //else if(item.IsAroundJiao(e.Location))
                        //else
                        //{
                        //    if (!item.Contains(e.Location))
                        //    {
                        //        item.Status = Status.None;
                        //    }
                        //    else
                        //        item.Status = Status.Selected;
                        //}
                    }
                    
                }

                //sort
                _items.Sort((l, r) =>
                {
                    if (l.Status < r.Status)
                        return -1;
                    else if (l.Status == r.Status)
                        return 0;
                    else
                        return 1;
                });
            }
        }

        public event EventHandler<CursorChangedEvenrArgs> CursorChanged;

        Point _lastMousePoint;
        public virtual void OnMouseMove(MouseEventArgs e)
        {
            
            Cursor cursor = Cursors.Arrow;
            
            foreach (var item in _items)
            {
                if (item.Status == Status.None)
                    continue;
                if (item.Status != Status.None)
                {
                    if (_isMouseDown)
                    {
                        ////Console.WriteLine($"{e.Location.X},{e.Location.Y};{item.Bound}");
                        if (item.Status== Status.Resize)
                        {
                            //set mouse
                            /////item.Status = Status.Resize;
                           cursor = Cursors.SizeNWSE;

                            //resize
                            if (_isMouseDown)
                            {
                                var vector = ImageEditor.MeasureVector(_lastMousePoint, e.Location);
                                item.ScaleTo(_lastMousePoint, e.Location);
                                //resize
                            }

                        }
                        else if (item.Status== Status.Pan)
                        {
                            ///item.Status = Status.Pan;
                            cursor = Cursors.SizeAll;

                            if (_isMouseDown)
                            {
                                var vector = ImageEditor.MeasureVector(_lastMousePoint, e.Location);
                                item.Pan(_lastMousePoint,e.Location);
                                //resize
                            }
                        }
                        else if (item.Status== Status.Rotate)
                        {
                            /////item.Status = Status.Rotate;
                            cursor = Cursors.AppStarting;
                        }
                        else
                        {
                            item.Status = Status.Selected;
                        }
                    }
                    else
                    {
                        ////Console.WriteLine($"{e.Location.X},{e.Location.Y};{item.Bound}");
                        if (item.IsAroundJiao(e.Location))
                        {
                            //set mouse
                            item.Status = Status.Resize;
                            cursor = Cursors.SizeNWSE;

                            //resize
                            if (_isMouseDown)
                            {
                                var vector = ImageEditor.MeasureVector(_lastMousePoint, e.Location);

                                //resize
                            }

                        }
                        else if (item.IsOnCenter(e.Location))
                        {
                            item.Status = Status.Pan;
                            cursor = Cursors.SizeAll;

                            if (_isMouseDown)
                            {
                                var vector = ImageEditor.MeasureVector(_lastMousePoint, e.Location);
                                item.Pan(_lastMousePoint,e.Location);
                                //resize
                            }
                        }
                        else if (item.IsOnLineCenter(e.Location))
                        {
                            item.Status = Status.Rotate;
                            cursor = Cursors.AppStarting;
                        }
                        else
                        {
                            ////item.Status = Status.Selected;
                        }
                    }
                    


                }
            }
            _lastMousePoint = e.Location;
            CursorChanged?.Invoke(this, new CursorChangedEvenrArgs(cursor));
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            _isMouseDown = false;
        }

        public class CursorChangedEvenrArgs : EventArgs
        {
            public CursorChangedEvenrArgs(Cursor cursor)
            {
                this.Cursor = cursor;
            }

            public Cursor Cursor
            {
                get;
            }
        }
    }
}
