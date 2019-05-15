﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.TailTracking
{

    [Description("Detects tail beat frequency from tail curvature.")]
    [WorkflowElementCategory(ElementCategory.Transform)]

    public class DetectBout : Transform<double, int>
    {

        [Description("Delta is used to determine how much of a threshold is necessary to determine a peak in the tail angle.")]
        public double Delta { get; set; }

        [Description("FrameWindow is used to determine the window in terms of number of frames for calculating frequency.")]
        public int FrameWindow { get; set; }

        private int i = 0;
        private double[] peaks = new double[2];
        private bool findMax = true;
        private double minVal = double.PositiveInfinity;
        private double maxVal = double.NegativeInfinity;
        private int pos = 0;
        private int frequency = 0;

        public override IObservable<int> Process(IObservable<double> source)
        {
            return source.Select(value =>
            {

                double delta = Delta;
                int frameWindow = FrameWindow;

                i++;

                if ((peaks.Length == 1 && (i - peaks[0]) > frameWindow) || (peaks.Length == 2 && (i - peaks[1]) > frameWindow))
                {
                    Array.Clear(peaks, 0, peaks.Length);
                    findMax = true;
                    minVal = double.PositiveInfinity;
                    maxVal = double.NegativeInfinity;
                    frequency = 0;
                    pos = 0;
                    i = 0;
                }

                if (value > maxVal)
                {
                    maxVal = value;
                    pos = i;
                }

                if (value < minVal)
                {
                    minVal = value;
                }

                if (findMax)
                {
                    if (value < (maxVal - delta))
                    {
                        if (peaks.Length == 0)
                        {
                            peaks[0] = pos;
                        }
                        else if (peaks.Length == 1)
                        {
                            peaks[1] = pos;
                            frequency = 1;
                        }
                        else
                        {
                            peaks[0] = peaks[1];
                            peaks[1] = pos;
                            frequency = 1;
                        }
                        minVal = value;
                        findMax = false;
                    }
                }
                else
                {
                    if (value > (minVal + delta))
                    {
                        maxVal = value;
                        findMax = true;
                    }
                }

                if (frequency == double.PositiveInfinity || frequency == double.NegativeInfinity)
                {
                    frequency = 0;
                }

                return frequency;

            });

        }

    }

}
