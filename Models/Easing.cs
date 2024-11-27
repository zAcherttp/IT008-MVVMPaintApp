using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
namespace MVVMPaintApp.Models
{
    public class Easing : DependencyObject, IDisposable
    {
        public enum EasingType
        {
            Linear,
            EaseOutCubic,
            EaseInOutCubic,
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(Easing),
                new PropertyMetadata(0.0)
            );

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private Storyboard? storyBoard;

        public Easing(double initialPropertyValue)
        {
            Value = initialPropertyValue;
        }

        public async Task<bool> EaseToAsync(double targetPropertyValue, EasingType easingType, int duration = 1000)
        {
            storyBoard?.Stop();

            var animation = new DoubleAnimation
            {
                From = Value,
                To = targetPropertyValue,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = CreateEasingFunction(easingType),
            };

            storyBoard = new Storyboard();
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(ValueProperty));

            storyBoard.Children.Add(animation);

            var tcs = new TaskCompletionSource<bool>();

            storyBoard.Completed += (s, e) =>
            {
                tcs.TrySetResult(true);
            };

            storyBoard.Begin();

            return await tcs.Task;
        }

        public async Task<bool> EaseDeltaAsync(double targetPropertyDelta, EasingType easingType, int duration = 1000)
        {
            return await EaseToAsync(Value + targetPropertyDelta, easingType, duration);
        }

        private static IEasingFunction CreateEasingFunction(EasingType easingType)
        {
            return easingType switch
            {
                EasingType.Linear => new LinearEase(),
                EasingType.EaseOutCubic => new CubicEase { EasingMode = EasingMode.EaseOut },
                EasingType.EaseInOutCubic => new CubicEase { EasingMode = EasingMode.EaseInOut },
                _ => new LinearEase()
            };
        }

        public void Cancel()
        {
            storyBoard?.Stop();
        }

        public void Dispose()
        {
            Cancel();
            storyBoard = null;
            GC.SuppressFinalize(this);
        }
    }

    public class LinearEase : EasingFunctionBase
    {
        protected override Freezable CreateInstanceCore()
        {
            return new LinearEase();
        }

        protected override double EaseInCore(double normalizedTime)
        {
            return normalizedTime;
        }
    }
}