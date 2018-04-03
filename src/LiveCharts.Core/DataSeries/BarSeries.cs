#region License
// The MIT License (MIT)
// 
// Copyright (c) 2016 Alberto Rodr�guez Orozco & LiveCharts contributors
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
// of the Software, and to permit persons to whom the Software is furnished to 
// do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#region

using System;
using System.Drawing;
using System.Linq;
using LiveCharts.Core.Abstractions;
using LiveCharts.Core.Abstractions.DataSeries;
using LiveCharts.Core.Charts;
using LiveCharts.Core.DataSeries.Data;
using LiveCharts.Core.Dimensions;
using LiveCharts.Core.Drawing;
using LiveCharts.Core.Interaction;
using LiveCharts.Core.ViewModels;
using Point = LiveCharts.Core.Coordinates.Point;

#endregion

namespace LiveCharts.Core.DataSeries
{
    /// <summary>
    /// The column series class.
    /// </summary>The column series class.
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public class BarSeries<TModel>
        : CartesianSeries<TModel, Point, BarViewModel, Point<TModel, Point, BarViewModel>>, IBarSeries
    {
        private static ISeriesViewProvider<TModel, Point, BarViewModel> _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarSeries{TModel}"/> class.
        /// </summary>
        public BarSeries()
        {
            MaxColumnWidth = 45f;
            ColumnPadding = 6f;
            Charting.BuildFromSettings<IBarSeries>(this);
        }

        /// <inheritdoc />
        public float MaxColumnWidth { get; set; }

        /// <inheritdoc />
        public float ColumnPadding { get; set; }

        /// <inheritdoc />
        public override Type ResourceKey => typeof(IBarSeries);

        /// <inheritdoc />
        public override float[] DefaultPointWidth => new[] {1f, 0f};

        /// <inheritdoc />
        public override float[] PointMargin => new[] {0f, 0f};

        /// <inheritdoc />
        protected override ISeriesViewProvider<TModel, Point, BarViewModel>
            DefaultViewProvider => _provider ?? (_provider = Charting.Current.UiProvider.BarViewProvider<TModel>());

        /// <inheritdoc />
        public override void UpdateView(ChartModel chart)
        {
            int wi = 0, hi = 1, inverted = 1;
            var orientation = Orientation.Horizontal;

            var directionAxis = chart.Dimensions[0][ScalesAt[0]];
            var scaleAxis = chart.Dimensions[1][ScalesAt[1]];

            var uw = chart.Get2DUiUnitWidth(directionAxis, scaleAxis);

            var columnSeries = chart.Series
                .Where(series => series.ScalesAt[1] == ScalesAt[1] &&
                                 series is IBarSeries)
                .ToList();

            var cw = (uw[0] - ColumnPadding * columnSeries.Count) / columnSeries.Count;
            var position = columnSeries.IndexOf(this);

            if (cw > MaxColumnWidth)
            {
                cw = MaxColumnWidth;
            }

            var offsetX = -cw * .5f + uw[0] * .5f;
            var offsetY = 0f;

            var positionOffset = new float[2];

            if (chart.InvertXy)
            {
                wi = 1;
                hi = 0;
                inverted = 0;
                orientation = Orientation.Vertical;
                offsetX = 0;
                offsetY = -cw * .5f - uw[0] * .5f;
            }

            positionOffset[wi] =
                (ColumnPadding + cw) * position - (ColumnPadding + cw) * ((columnSeries.Count - 1) * .5f);

            var columnStart = GetColumnStart(chart, scaleAxis, directionAxis);

            Point<TModel, Coordinates.Point, BarViewModel> previous = null;

            foreach (var current in Points)
            {
                var offset = chart.ScaleToUi(current.Coordinate[0][0], directionAxis);

                var columnCorner1 = new[]
                {
                    offset,
                    chart.ScaleToUi(current.Coordinate[1][0], scaleAxis)
                };

                var columnCorner2 = new[]
                {
                    offset + cw,
                    columnStart
                };

                var difference = Perform.SubstractEach2D(columnCorner1, columnCorner2);

                if (current.View == null)
                {
                    current.View = ViewProvider.Getter();
                }

                var location = new[]
                {
                    offset,
                    columnStart - Math.Abs(difference[1]) * inverted
                };

                if (current.View.VisualElement == null)
                {
                    var initialRectangle = chart.InvertXy
                        ? new RectangleF(
                            columnStart,
                            location[hi] + offsetY + positionOffset[1],
                            0f,
                            Math.Abs(difference[hi]))
                        : new RectangleF(
                            location[wi] + offsetX + positionOffset[0],
                            columnStart,
                            Math.Abs(difference[wi]),
                            0f);
                    current.ViewModel = new BarViewModel(RectangleF.Empty, initialRectangle, orientation);
                }

                var vm = new BarViewModel(
                    current.ViewModel.To,
                    new RectangleF(
                        location[wi] + offsetX + positionOffset[0],
                        location[hi] + offsetY + positionOffset[1],
                        Math.Abs(difference[wi]),
                        Math.Abs(difference[hi])),
                    orientation);

                current.InteractionArea = new RectangleInteractionArea(vm.To);

                current.ViewModel = vm;
                current.View.DrawShape(current, previous);

                Mapper.EvaluateModelDependentActions(current.Model, current.View.VisualElement, current);

                previous = current;
            }
        }

        private static float GetColumnStart(ChartModel chart, Plane target, Plane complementary)
        {
            var value = target.ActualMinValue >= 0 && complementary.ActualMaxValue > 0
                ? target.ActualMinValue
                : (target.ActualMinValue < 0 && complementary.ActualMaxValue <= 0
                    ? target.ActualMaxValue
                    : 0);
            return chart.ScaleToUi(value, target);
        }
    }
}