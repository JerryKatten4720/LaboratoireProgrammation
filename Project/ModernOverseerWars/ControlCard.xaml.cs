using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LaboratoireProgrammation.Project.ModernOverseerWars;

public partial class ControlCard : UserControl {
    public static readonly DependencyProperty CardImageProperty =
        DependencyProperty.Register("CardImage", typeof(string), typeof(ControlCard), new PropertyMetadata("Default"));

    public static readonly DependencyProperty PowerProperty =
        DependencyProperty.Register("Power", typeof(int), typeof(ControlCard), new PropertyMetadata(0));

    private static readonly List<ControlCard> _instances = new();

    private Canvas _parentCanvas;
    private Point _relativeMousePos;

    public ControlCard() {
        InitializeComponent();

        _instances.Add(this);

        Unloaded += (s, e) => _instances.Remove(this);
    }

    public string CardImage {
        get => (string)GetValue(CardImageProperty);
        set => SetValue(CardImageProperty, value);
    }

    public int Power {
        get => (int)GetValue(PowerProperty);
        set => SetValue(PowerProperty, value);
    }

    public static ControlCard GetClosestCardTo(ControlCard target) {
        ControlCard closest = null;
        var minDistance = double.MaxValue;
        var targetPos = target.GetCurrentPosition();

        foreach (var card in _instances) {
            if (card == target) continue;

            var distance = GetDistanceSquared(targetPos, card.GetCurrentPosition());
            if (distance < minDistance) {
                minDistance = distance;
                closest = card;
            }
        }

        return closest;
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
        base.OnMouseEnter(e);
        ApplyScaleAnimation(1.05, 0.2);
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        base.OnMouseLeave(e);
        ApplyScaleAnimation(1.0, 0.2);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
        base.OnMouseLeftButtonDown(e);

        _parentCanvas ??= VisualTreeHelper.GetParent(this) as Canvas;
        if (_parentCanvas == null) return;

        _relativeMousePos = e.GetPosition(this);
        CaptureMouse();

        BringToFront();

        OverseerWarsWindow.heldCard = this;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);

        if (IsMouseCaptured) {
            var currentMousePos = e.GetPosition(_parentCanvas);
            MoveTo(currentMousePos.X - _relativeMousePos.X, currentMousePos.Y - _relativeMousePos.Y);
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
        base.OnMouseLeftButtonUp(e);

        ReleaseMouseCapture();
        OverseerWarsWindow.heldCard = null;

        HandleSnapToClosestAnim();
    }

    public void BringToFront() {
        var maxZ = _instances.Max(c => Panel.GetZIndex(c));
        Panel.SetZIndex(this, maxZ + 1);
    }

    public void PutAbove(ControlCard other) {
        Panel.SetZIndex(this, Panel.GetZIndex(other) + 1);
    }

    public void PutBelow(ControlCard other) {
        var targetZ = Panel.GetZIndex(other);
        Panel.SetZIndex(this, Math.Max(0, targetZ - 1));
    }

    public void SwapZIndex(ControlCard other) {
        var thisZ = Panel.GetZIndex(this);
        var otherZ = Panel.GetZIndex(other);

        Panel.SetZIndex(this, otherZ);
        Panel.SetZIndex(other, thisZ);
    }

    private void HandleSnapToClosestAnim() {
        var closest = GetClosestCardTo(this);
        if (closest == null) return;

        var currentPos = GetCurrentPosition();
        var targetPos = closest.GetCurrentPosition();

        if (GetDistanceSquared(currentPos, targetPos) < 2400) AnimateToPosition(targetPos, 0.4);
    }

    private void AnimateToPosition(Point target, double durationSeconds) {
        var easing = new QuarticEase { EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromSeconds(durationSeconds);

        var animX = CreateDoubleAnimation(GetCurrentPosition().X, target.X, duration, easing);
        var animY = CreateDoubleAnimation(GetCurrentPosition().Y, target.Y, duration, easing);

        animX.Completed += (s, e) => {
            BeginAnimation(Canvas.LeftProperty, null);
            Canvas.SetLeft(this, target.X);
        };
        animY.Completed += (s, e) => {
            BeginAnimation(Canvas.TopProperty, null);
            Canvas.SetTop(this, target.Y);
        };

        BeginAnimation(Canvas.LeftProperty, animX);
        BeginAnimation(Canvas.TopProperty, animY);
    }

    private DoubleAnimation CreateDoubleAnimation(double from, double to, TimeSpan duration, IEasingFunction easing) {
        return new DoubleAnimation(from, to, duration) { EasingFunction = easing };
    }

    private void ApplyScaleAnimation(double targetScale, double durationSeconds) {
        if (LayoutTransform is not ScaleTransform scaleTransform) {
            scaleTransform = new ScaleTransform(1.0, 1.0);
            LayoutTransform = scaleTransform;
        }

        var animation = new DoubleAnimation {
            To = targetScale,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }

    private void MoveTo(double x, double y) {
        Canvas.SetLeft(this, x);
        Canvas.SetTop(this, y);
    }

    public Point GetCurrentPosition() {
        var x = Canvas.GetLeft(this);
        var y = Canvas.GetTop(this);

        return new Point(double.IsNaN(x) ? 0 : x, double.IsNaN(y) ? 0 : y);
    }

    private static double GetDistanceSquared(Point p1, Point p2) {
        return Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2);
    }
}