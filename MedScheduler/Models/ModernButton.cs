using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MedScheduler.Controls
{
    #region Modern Button
    /// <summary>
    /// A modern styled button with rounded corners and hover effects
    /// </summary>
    public class ModernButton : Button
    {
        // Fields
        private int borderSize = 0;
        private int borderRadius = 20;
        private Color borderColor = Color.PaleVioletRed;
        private Color hoverBackColor;
        private Color pressedBackColor;
        private bool isHovering = false;
        private bool isPressed = false;

        // Properties
        [Category("Modern Appearance")]
        public int BorderSize
        {
            get => borderSize;
            set
            {
                borderSize = value;
                this.Invalidate();
            }
        }

        [Category("Modern Appearance")]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                if (value <= this.Height)
                    borderRadius = value;
                else
                    borderRadius = this.Height;
                this.Invalidate();
            }
        }

        [Category("Modern Appearance")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                this.Invalidate();
            }
        }

        [Category("Modern Appearance")]
        public Color BackgroundColor
        {
            get => this.BackColor;
            set => this.BackColor = value;
        }

        [Category("Modern Appearance")]
        public Color TextColor
        {
            get => this.ForeColor;
            set => this.ForeColor = value;
        }

        [Category("Modern Appearance")]
        public Color HoverBackColor
        {
            get => hoverBackColor;
            set => hoverBackColor = value;
        }

        [Category("Modern Appearance")]
        public Color PressedBackColor
        {
            get => pressedBackColor;
            set => pressedBackColor = value;
        }

        // Constructor
        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(150, 40);
            this.BackColor = ColorTranslator.FromHtml("#3498DB");
            this.hoverBackColor = ControlPaint.Light(this.BackColor);
            this.pressedBackColor = ControlPaint.Dark(this.BackColor);
            this.ForeColor = Color.White;
            this.Resize += new EventHandler(Button_Resize);
            this.MouseEnter += new EventHandler(Button_MouseEnter);
            this.MouseLeave += new EventHandler(Button_MouseLeave);
            this.MouseDown += new MouseEventHandler(Button_MouseDown);
            this.MouseUp += new MouseEventHandler(Button_MouseUp);
        }

        private void Button_Resize(object sender, EventArgs e)
        {
            if (borderRadius > this.Height)
                borderRadius = this.Height;
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            isHovering = true;
            this.Invalidate();
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            isHovering = false;
            isPressed = false;
            this.Invalidate();
        }

        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPressed = true;
                this.Invalidate();
            }
        }

        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            isPressed = false;
            this.Invalidate();
        }

        // Methods
        private GraphicsPath GetFigurePath(Rectangle rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            Rectangle rectSurface = this.ClientRectangle;
            Rectangle rectBorder = Rectangle.Inflate(rectSurface, -borderSize, -borderSize);
            int smoothSize = 2;

            if (borderSize > 0)
                smoothSize = borderSize;

            if (borderRadius > 2) // Rounded button
            {
                using (GraphicsPath pathSurface = GetFigurePath(rectSurface, borderRadius))
                using (GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - borderSize))
                using (Pen penSurface = new Pen(this.Parent.BackColor, smoothSize))
                using (Pen penBorder = new Pen(borderColor, borderSize))
                {
                    pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Button surface
                    this.Region = new Region(pathSurface);

                    // Determine the button's background color based on state
                    Color buttonColor = this.BackColor;
                    if (isPressed && pressedBackColor != Color.Empty)
                        buttonColor = pressedBackColor;
                    else if (isHovering && hoverBackColor != Color.Empty)
                        buttonColor = hoverBackColor;

                    // Draw button surface
                    using (SolidBrush brushSurface = new SolidBrush(buttonColor))
                    {
                        pevent.Graphics.FillPath(brushSurface, pathSurface);
                    }

                    // Draw surface border for HD result
                    pevent.Graphics.DrawPath(penSurface, pathSurface);

                    // Button border                    
                    if (borderSize >= 1)
                        pevent.Graphics.DrawPath(penBorder, pathBorder);
                }
            }
            else // Normal button
            {
                pevent.Graphics.SmoothingMode = SmoothingMode.None;

                // Button surface
                this.Region = new Region(rectSurface);

                // Determine the button's background color based on state
                Color buttonColor = this.BackColor;
                if (isPressed && pressedBackColor != Color.Empty)
                    buttonColor = pressedBackColor;
                else if (isHovering && hoverBackColor != Color.Empty)
                    buttonColor = hoverBackColor;

                // Draw button surface
                using (SolidBrush brushSurface = new SolidBrush(buttonColor))
                {
                    pevent.Graphics.FillRectangle(brushSurface, rectSurface);
                }

                // Button border
                if (borderSize >= 1)
                {
                    using (Pen penBorder = new Pen(borderColor, borderSize))
                    {
                        penBorder.Alignment = PenAlignment.Inset;
                        pevent.Graphics.DrawRectangle(penBorder, 0, 0, this.Width - 1, this.Height - 1);
                    }
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.Parent.BackColorChanged += new EventHandler(Container_BackColorChanged);
        }

        private void Container_BackColorChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }
    }
    #endregion

    #region Modern Panel
    /// <summary>
    /// A panel with rounded corners and shadow effect
    /// </summary>
    public class ModernPanel : Panel
    {
        // Fields
        private int borderRadius = 10;
        private float shadowOpacity = 0.35F;
        private int shadowDepth = 5;
        private int shadowThickness = 3;
        private Color shadowColor = Color.Black;
        private bool showShadow = true;

        // Properties
        [Category("Modern Appearance")]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                borderRadius = value;
                this.Invalidate();
            }
        }

        [Category("Modern Appearance")]
        public bool ShowShadow
        {
            get => showShadow;
            set
            {
                showShadow = value;
                this.Invalidate();
            }
        }

        [Category("Modern Appearance")]
        public float ShadowOpacity
        {
            get => shadowOpacity;
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                shadowOpacity = value;
                this.Invalidate();
            }
        }

        [Category("Modern Appearance")]
        public int ShadowDepth
        {
            get => shadowDepth;
            set
            {
                shadowDepth = value;
                this.Invalidate();
            }
        }

        [Category("Modern Appearance")]
        public int ShadowThickness
        {
            get => shadowThickness;
            set
            {
                shadowThickness = value;
                this.Invalidate();
            }
        }

        [Category("Modern Appearance")]
        public Color ShadowColor
        {
            get => shadowColor;
            set
            {
                shadowColor = value;
                this.Invalidate();
            }
        }

        // Constructor
        public ModernPanel()
        {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.White;
            this.Padding = new Padding(shadowDepth);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw shadow (if enabled)
            if (showShadow)
            {
                // Create shadow rectangle
                Rectangle shadowRect = new Rectangle(
                    shadowDepth,
                    shadowDepth,
                    this.Width - shadowDepth * 2,
                    this.Height - shadowDepth * 2);

                // Draw shadow
                if (borderRadius > 0)
                {
                    using (GraphicsPath shadowPath = CreateRoundRectPath(shadowRect, borderRadius))
                    {
                        DrawShadow(g, shadowPath);
                    }
                }
                else
                {
                    using (GraphicsPath shadowPath = new GraphicsPath())
                    {
                        shadowPath.AddRectangle(shadowRect);
                        DrawShadow(g, shadowPath);
                    }
                }
            }

            // Draw panel background
            Rectangle panelRect = new Rectangle(
                0,
                0,
                this.Width - (showShadow ? shadowDepth : 0),
                this.Height - (showShadow ? shadowDepth : 0));

            if (borderRadius > 0)
            {
                using (GraphicsPath path = CreateRoundRectPath(panelRect, borderRadius))
                {
                    using (SolidBrush brush = new SolidBrush(this.BackColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    g.FillRectangle(brush, panelRect);
                }
            }
        }

        private void DrawShadow(Graphics g, GraphicsPath path)
        {
            for (int i = 1; i <= shadowThickness; i++)
            {
                float opacity = shadowOpacity / shadowThickness * i;

                using (Pen shadowPen = new Pen(Color.FromArgb((int)(opacity * 255), shadowColor), i * 2))
                {
                    shadowPen.LineJoin = LineJoin.Round;
                    g.DrawPath(shadowPen, path);
                }
            }
        }

        private GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int diameter = radius * 2;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top-left corner
            path.AddArc(arcRect, 180, 90);

            // Top-right corner
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // Bottom-right corner
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // Bottom-left corner
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
    #endregion

    #region Toggle Switch
    /// <summary>
    /// A modern toggle switch control
    /// </summary>
    public class ToggleSwitch : Control
    {
        // Fields
        private bool isOn = false;
        private Color onBackColor = ColorTranslator.FromHtml("#2ECC71");
        private Color offBackColor = Color.Gray;
        private Color onToggleColor = Color.White;
        private Color offToggleColor = Color.White;

        // Properties
        [Category("Toggle Switch")]
        public bool IsOn
        {
            get => isOn;
            set
            {
                isOn = value;
                this.Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        [Category("Toggle Switch")]
        public Color OnBackColor
        {
            get => onBackColor;
            set
            {
                onBackColor = value;
                this.Invalidate();
            }
        }

        [Category("Toggle Switch")]
        public Color OffBackColor
        {
            get => offBackColor;
            set
            {
                offBackColor = value;
                this.Invalidate();
            }
        }

        [Category("Toggle Switch")]
        public Color OnToggleColor
        {
            get => onToggleColor;
            set
            {
                onToggleColor = value;
                this.Invalidate();
            }
        }

        [Category("Toggle Switch")]
        public Color OffToggleColor
        {
            get => offToggleColor;
            set
            {
                offToggleColor = value;
                this.Invalidate();
            }
        }

        // Custom events
        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        // Constructor
        public ToggleSwitch()
        {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            this.Size = new Size(50, 25);
            this.Cursor = Cursors.Hand;
            this.BackColor = Color.Transparent;

            this.Click += (sender, e) => {
                this.IsOn = !this.IsOn;
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int toggleSize = this.Height - 4;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            // Draw the background
            using (GraphicsPath path = CreateRoundRectPath(rect, this.Height / 2))
            {
                using (SolidBrush brush = new SolidBrush(isOn ? onBackColor : offBackColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw the toggle
            int togglePosition = isOn ? this.Width - toggleSize - 2 : 2;
            Rectangle toggleRect = new Rectangle(togglePosition, 2, toggleSize, toggleSize);

            using (GraphicsPath togglePath = CreateRoundRectPath(toggleRect, toggleSize / 2))
            {
                using (SolidBrush toggleBrush = new SolidBrush(isOn ? onToggleColor : offToggleColor))
                {
                    g.FillPath(toggleBrush, togglePath);
                }
            }
        }

        private GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int diameter = radius * 2;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top-left corner
            path.AddArc(arcRect, 180, 90);

            // Top-right corner
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // Bottom-right corner
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // Bottom-left corner
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
    #endregion

    #region Card View
    /// <summary>
    /// A card view control for displaying information
    /// </summary>
    public class CardView : Control
    {
        // Fields
        private string title = "Card Title";
        private string description = "Card Description";
        private string value = "Value";
        private Image icon;
        private Color cardColor = Color.White;
        private Color titleColor = Color.Black;
        private Color descriptionColor = Color.Gray;
        private Color valueColor = ColorTranslator.FromHtml("#3498DB");
        private int borderRadius = 10;
        private bool showShadow = true;

        // Properties
        [Category("Card")]
        public string Title
        {
            get => title;
            set
            {
                title = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public string Description
        {
            get => description;
            set
            {
                description = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public Image Icon
        {
            get => icon;
            set
            {
                icon = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public Color CardColor
        {
            get => cardColor;
            set
            {
                cardColor = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public Color TitleColor
        {
            get => titleColor;
            set
            {
                titleColor = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public Color DescriptionColor
        {
            get => descriptionColor;
            set
            {
                descriptionColor = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public Color ValueColor
        {
            get => valueColor;
            set
            {
                valueColor = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                borderRadius = value;
                this.Invalidate();
            }
        }

        [Category("Card")]
        public bool ShowShadow
        {
            get => showShadow;
            set
            {
                showShadow = value;
                this.Invalidate();
            }
        }

        // Constructor
        public CardView()
        {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            this.Size = new Size(200, 100);
            this.BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            // Draw shadow (if enabled)
            if (showShadow)
            {
                // Create shadow path
                Rectangle shadowRect = new Rectangle(3, 3, this.Width - 6, this.Height - 6);

                using (GraphicsPath shadowPath = CreateRoundRectPath(shadowRect, borderRadius))
                {
                    // Draw shadow
                    for (int i = 1; i <= 5; i++)
                    {
                        float opacity = 0.1f / 5 * i;
                        using (Pen shadowPen = new Pen(Color.FromArgb((int)(opacity * 255), Color.Black), i))
                        {
                            g.DrawPath(shadowPen, shadowPath);
                        }
                    }
                }
            }

            // Draw card background
            using (GraphicsPath path = CreateRoundRectPath(rect, borderRadius))
            {
                using (SolidBrush brush = new SolidBrush(cardColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Calculate content areas
            int padding = 15;
            Rectangle iconRect = new Rectangle(padding, padding, 40, 40);
            Rectangle contentRect = new Rectangle(
                icon != null ? iconRect.Right + padding : padding,
                padding,
                this.Width - (icon != null ? iconRect.Right + padding * 2 : padding * 2),
                this.Height - padding * 2);

            // Draw icon (if available)
            if (icon != null)
            {
                g.DrawImage(icon, iconRect);
            }

            // Draw title
            using (Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                using (SolidBrush titleBrush = new SolidBrush(titleColor))
                {
                    g.DrawString(title, titleFont, titleBrush, contentRect.Location);
                }
            }

            // Draw description
            using (Font descFont = new Font("Segoe UI", 8))
            {
                using (SolidBrush descBrush = new SolidBrush(descriptionColor))
                {
                    g.DrawString(description, descFont, descBrush,
                        new Point(contentRect.X, contentRect.Y + 20));
                }
            }

            // Draw value
            using (Font valueFont = new Font("Segoe UI", 14, FontStyle.Bold))
            {
                using (SolidBrush valueBrush = new SolidBrush(valueColor))
                {
                    // Calculate text size to align to the right
                    SizeF valueSize = g.MeasureString(value, valueFont);
                    PointF valuePoint = new PointF(
                        contentRect.Right - valueSize.Width,
                        contentRect.Y + contentRect.Height - valueSize.Height);

                    g.DrawString(value, valueFont, valueBrush, valuePoint);
                }
            }
        }

        private GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int diameter = radius * 2;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top-left corner
            path.AddArc(arcRect, 180, 90);

            // Top-right corner
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // Bottom-right corner
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // Bottom-left corner
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
    #endregion

    #region Loading Spinner
    /// <summary>
    /// A loading spinner control
    /// </summary>
    public class LoadingSpinner : Control
    {
        // Fields
        private Timer animationTimer;
        private int numberOfCircles = 12;
        private int currentCircle = 0;
        private int circleSize = 4;
        private int radius = 15;
        private Color spinnerColor = ColorTranslator.FromHtml("#3498DB");

        // Properties
        [Category("Spinner")]
        public int NumberOfCircles
        {
            get => numberOfCircles;
            set
            {
                numberOfCircles = value;
                this.Invalidate();
            }
        }

        [Category("Spinner")]
        public int CircleSize
        {
            get => circleSize;
            set
            {
                circleSize = value;
                this.Invalidate();
            }
        }

        [Category("Spinner")]
        public int Radius
        {
            get => radius;
            set
            {
                radius = value;
                this.Invalidate();
            }
        }

        [Category("Spinner")]
        public Color SpinnerColor
        {
            get => spinnerColor;
            set
            {
                spinnerColor = value;
                this.Invalidate();
            }
        }

        [Category("Spinner")]
        public bool IsSpinning
        {
            get => animationTimer.Enabled;
            set
            {
                animationTimer.Enabled = value;
                if (!value)
                {
                    currentCircle = 0;
                    this.Invalidate();
                }
            }
        }

        // Constructor
        public LoadingSpinner()
        {
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            this.Size = new Size(40, 40);
            this.BackColor = Color.Transparent;

            // Initialize animation timer
            animationTimer = new Timer();
            animationTimer.Interval = 100;
            animationTimer.Tick += (sender, e) => {
                currentCircle = (currentCircle + 1) % numberOfCircles;
                this.Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Calculate center point
            Point center = new Point(this.Width / 2, this.Height / 2);

            // Draw circles
            for (int i = 0; i < numberOfCircles; i++)
            {
                // Calculate position
                double angle = Math.PI * 2 * i / numberOfCircles;
                int x = (int)(center.X + radius * Math.Cos(angle) - circleSize / 2);
                int y = (int)(center.Y + radius * Math.Sin(angle) - circleSize / 2);

                // Calculate opacity based on position
                int diff = (i - currentCircle + numberOfCircles) % numberOfCircles;
                float opacity = 1f - (float)diff / numberOfCircles;

                // Draw circle
                using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(opacity * 255), spinnerColor)))
                {
                    g.FillEllipse(brush, x, y, circleSize, circleSize);
                }
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // Start or stop the animation when visibility changes
            animationTimer.Enabled = this.Visible;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
    #endregion
}