﻿//The MIT License(MIT)

//copyright(c) 2016 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveChartsCore;
using Size = System.Windows.Size;

namespace Desktop
{
    public abstract class Chart : UserControl, IChartView
    {
        private readonly LiveChartsCore.Chart _model;

        protected Chart()
        {
            _model = new LiveChartsCore.Chart(this);

            UpdateLayout();
            Measure(new Size(double.MaxValue, double.MaxValue));
            Canvas.SetTop(Canvas, 0d);
            Canvas.SetLeft(Canvas, 0d);
            Canvas.Width = DesiredSize.Width;
            Canvas.Height = DesiredSize.Height;
            Canvas = new Canvas();
            Content = Canvas;

            DrawMargin = new Canvas {ClipToBounds = true};

            SetValue(MinHeightProperty, 125d);
            SetValue(MinWidthProperty, 125d);

            SetValue(AxisXProperty, new List<Axis>());
            SetValue(AxisYProperty, new List<Axis>());

            SetValue(ChartLegendProperty, new ChartLegend());

            CursorX = new ChartCursor(this, AxisTags.X);
            CursorY = new ChartCursor(this, AxisTags.Y);

            if (RandomizeStartingColor) 
                CurrentColorIndex = Randomizer.Next(0, Colors.Count - 1);

            SetCurrentValue(SeriesProperty,
                new SeriesCollection()
                    .Setup(new SeriesConfiguration<double>()
                        .Y(v => v)
                        .X((v, i) => i)));


        }

        static Chart()
        {
            Colors = new List<Color>
            {
                Color.FromRgb(33, 149, 242),
                Color.FromRgb(243, 67, 54),
                Color.FromRgb(254, 192, 7),
                Color.FromRgb(96, 125, 138),
                Color.FromRgb(155, 39, 175),
                Color.FromRgb(0, 149, 135),
                Color.FromRgb(76, 174, 80),
                Color.FromRgb(121, 85, 72),
                Color.FromRgb(157, 157, 157),
                Color.FromRgb(232, 30, 99),
                Color.FromRgb(63, 81, 180),
                Color.FromRgb(0, 187, 211),
                Color.FromRgb(254, 234, 59),
                Color.FromRgb(254, 87, 34)
            };
            Randomizer = new Random();
        }

        private Canvas Canvas { get; set; }
        internal Canvas DrawMargin { get; set; }

        private static Random Randomizer { get; set; }
        private static bool RandomizeStartingColor { get; set; }

        public static List<Color> Colors { get; set; }
        public int CurrentColorIndex { get; set; }

        #region Dependency Properties

        public static readonly DependencyProperty AxisYProperty = DependencyProperty.Register(
            "AxisY", typeof (List<Axis>), typeof (Chart),
            new PropertyMetadata(null, OnPropertyChanged((v, m) => m.AxisY = v.AxisY)));
        /// <summary>
        /// Gets or sets vertical axis
        /// </summary>
        public List<Axis> AxisY
        {
            get { return (List<Axis>)GetValue(AxisYProperty); }
            set { SetValue(AxisYProperty, value); }
        }

        public static readonly DependencyProperty AxisXProperty = DependencyProperty.Register(
            "AxisX", typeof (List<Axis>), typeof (Chart),
            new PropertyMetadata(null, OnPropertyChanged((v, m) => m.AxisX = v.AxisX)));
        /// <summary>
        /// Gets or sets horizontal axis
        /// </summary>
        public List<Axis> AxisX
        {
            get { return (List<Axis>)GetValue(AxisXProperty); }
            set { SetValue(AxisXProperty, value); }
        }

        public static readonly DependencyProperty ChartLegendProperty = DependencyProperty.Register(
            "ChartLegend", typeof (ChartLegend), typeof (Chart), new PropertyMetadata(default(ChartLegend)));
        /// <summary>
        /// Gets or sets the control to use as chart legend fot this chart.
        /// </summary>
        public ChartLegend ChartLegend
        {
            get { return (ChartLegend) GetValue(ChartLegendProperty); }
            set { SetValue(ChartLegendProperty, value); }
        }

        public static readonly DependencyProperty CursorXProperty = DependencyProperty.Register(
            "CursorX", typeof (ChartCursor), typeof (Chart), new PropertyMetadata(default(ChartCursor)));

        public ChartCursor CursorX
        {
            get { return (ChartCursor) GetValue(CursorXProperty); }
            set { SetValue(CursorXProperty, value); }
        }

        public static readonly DependencyProperty CursorYProperty = DependencyProperty.Register(
            "CursorY", typeof (ChartCursor), typeof (Chart), new PropertyMetadata(default(ChartCursor)));

        public ChartCursor CursorY
        {
            get { return (ChartCursor) GetValue(CursorYProperty); }
            set { SetValue(CursorYProperty, value); }
        }

        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
            "Series", typeof (SeriesCollection), typeof (Chart),
            new PropertyMetadata(default(SeriesCollection), OnPropertyChanged((v, m) => m.Series = v.Series)));

        public SeriesCollection Series
        {
            get { return (SeriesCollection) GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        public static readonly DependencyProperty AnimationsSpeedProperty = DependencyProperty.Register(
            "AnimationsSpeed", typeof (TimeSpan), typeof (Chart), 
            new PropertyMetadata(default(TimeSpan), OnPropertyChanged(true)));

        public TimeSpan AnimationsSpeed
        {
            get { return (TimeSpan) GetValue(AnimationsSpeedProperty); }
            set { SetValue(AnimationsSpeedProperty, value); }
        }

        public static readonly DependencyProperty DisableAnimationsProperty = DependencyProperty.Register(
            "DisableAnimations", typeof (bool), typeof (Chart), new PropertyMetadata(default(bool)));

        public bool DisableAnimations
        {
            get { return (bool) GetValue(DisableAnimationsProperty); }
            set { SetValue(DisableAnimationsProperty, value); }
        }
        #endregion

        public IChartModel Model
        {
            get { return _model; }
        }

        public void InitializeSeries(ISeriesView series)
        {
            var index = CurrentColorIndex++;
            var defColor = Colors[(int)(index - Colors.Count * Math.Truncate(index / (decimal)Colors.Count))];
            var seriesView = series as Desktop.Series;
            if (seriesView == null) return;
            seriesView.Stroke = seriesView.Stroke ?? new SolidColorBrush(defColor);
            seriesView.Fill = seriesView.Fill ?? new SolidColorBrush(defColor) { Opacity = seriesView.DefaultFillOpacity };
        }

        public void Update(bool restartAnimations = true)
        {
            _model.Update(restartAnimations);
        }

        public void Erase()
        {
            throw new NotImplementedException();
            //foreach (var yi in AxisY) yi.Reset();
            //foreach (var xi in AxisX) xi.Reset();
            //DrawMargin.Children.Clear();
            //Canvas.Children.Clear();
            //Shapes.Clear();
            //ShapesMapper.Clear();
        }

        public void AddToView(object element)
        {
            var wpfElement = element as FrameworkElement;
            if (wpfElement == null) return;
            Canvas.Children.Add(wpfElement);
        }

        public void RemoveFromView(object element)
        {
            var wpfElement = element as FrameworkElement;
            if (wpfElement == null) return;
            Canvas.Children.Remove(wpfElement);
        }

        #region Callbacks
        private static PropertyChangedCallback OnPropertyChanged(bool animate = false)
        {
            return (o, args) =>
            {
                var wpfSeries = o as Series;
                if (wpfSeries == null) return;
                wpfSeries.Model.Chart.Update(animate);
            };
        }

        private static PropertyChangedCallback OnPropertyChanged(Action<Chart, IChartModel> map, bool animate = false)
        {
            return (o, args) =>
            {
                var wpfChart = o as Chart;
                if (wpfChart == null) return;

                map(wpfChart, wpfChart.Model);

                wpfChart.Update(animate);
            };
        }
        #endregion
    }
}
