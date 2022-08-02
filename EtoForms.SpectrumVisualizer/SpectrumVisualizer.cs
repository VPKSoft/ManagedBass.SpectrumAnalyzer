#region License
/*
MIT License

Copyright(c) 2022 Petteri Kautonen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using Eto.Drawing;
using Eto.Forms;
using ManagedBass.FftSignalProvider;

namespace EtoForms.SpectrumVisualizer;

/// <summary>
/// A control to visualize <see cref="ManagedBass"/> channel data into spectrum graph using
/// <see cref="EtoForms.SpectrumVisualizer.SpectrumType.Bar"/> or
/// <see cref="EtoForms.SpectrumVisualizer.SpectrumType.Line"/> formats.
/// Implements the <see cref="Drawable" />
/// </summary>
/// <seealso cref="Drawable" />
public class SpectrumVisualizer : Drawable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpectrumVisualizer"/> class.
    /// </summary>
    public SpectrumVisualizer()
    {
        multiChannel = false;
        Paint += SpectrumAnalyzer_Paint;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectrumVisualizer"/> class.
    /// </summary>
    /// <param name="multiChannel">if set to <c>true</c> the amount of graphs is the amount of the channel data / channel provided by the <see cref="ManagedBass"/>.</param>
    public SpectrumVisualizer(bool multiChannel) : this()
    {
        this.multiChannel = multiChannel;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectrumVisualizer"/> class.
    /// </summary>
    /// <param name="signalProvider">The signal provider for the audio data.</param>
    /// <param name="multiChannel">if set to <c>true</c> the amount of graphs is the amount of the channel data / channel provided by the <see cref="ManagedBass"/>.</param>
    public SpectrumVisualizer(SignalProvider? signalProvider, bool multiChannel) : this(multiChannel)
    {
        SignalProvider = signalProvider;
        Paint += SpectrumAnalyzer_Paint;
    }

    private List<ChannelData<double>> channelData = new();

    private void SpectrumAnalyzer_Paint(object? sender, PaintEventArgs e)
    {
        if (spectrumType == SpectrumType.Bar)
        {
            if (!multiChannel && channelData.Count > 0)
            {

                var gradientColors = visualizeColors[0];
                using Brush brush1 = useGradientColorsOnBars
                    ? new LinearGradientBrush(gradientColors.StartColor, gradientColors.EndColor,
                        new PointF(0, e.ClipRectangle.Top), new PointF(0, e.ClipRectangle.Bottom))
                    : new SolidBrush(gradientColors.StartColor);

                DrawBarGraph(e.Graphics, brush1, e.ClipRectangle, channelData[0]);
            }
            if (multiChannel)
            {
                var i = 0;
                var size = e.ClipRectangle.Height / channelData.Count;
                foreach (var data in channelData)
                {
                    var gradientColors = visualizeColors[i % channelData.Count];
                    using Brush brush1 = useGradientColorsOnBars
                        ? new LinearGradientBrush(gradientColors.StartColor, gradientColors.EndColor,
                            new PointF(0, size * i), new PointF(0, (i + 1) * size))
                        : new SolidBrush(gradientColors.StartColor);

                    var rect = new RectangleF(new PointF(0, size * i),
                        new PointF(e.ClipRectangle.Width, (i + 1) * size));
                    i++;
                    DrawBarGraph(e.Graphics, brush1, rect, data);
                }
            }
        }
        else if (spectrumType == SpectrumType.Line)
        {
            if (!multiChannel && channelData.Count > 0)
            {

                var gradientColors = visualizeColors[0];
                using var brush1 = new LinearGradientBrush(gradientColors.StartColor, gradientColors.EndColor,
                    new PointF(0, e.ClipRectangle.Top), new PointF(0, e.ClipRectangle.Bottom));

                DrawLineGraph(e.Graphics, gradientColors.StartColor, e.ClipRectangle, channelData[0]);
            }
            if (multiChannel)
            {
                var i = 0;
                var size = e.ClipRectangle.Height / channelData.Count;
                foreach (var data in channelData)
                {
                    var gradientColors = visualizeColors[i % channelData.Count];

                    var rect = new RectangleF(new PointF(0, size * i),
                        new PointF(e.ClipRectangle.Width, (i + 1) * size));
                    i++;
                    DrawLineGraph(e.Graphics, gradientColors.StartColor, rect, data);
                }
            }
        }
    }

    private void DrawLineGraph(Graphics graphics, Color lineColor, RectangleF positionRectangle, ChannelData<double> channelDataItem)
    {
        graphics.FillRectangle(BackgroundColor, positionRectangle);

        var data = channelDataItem.AdjustToScale(0, positionRectangle.Height, true, out _);

        var pointWidth = positionRectangle.Width / data.Data.Length;
        var height = positionRectangle.Height;

        var points = data.Data.Select((value, index) =>
            new PointF(index * pointWidth, positionRectangle.Top + height - (float)value)).ToArray();

        graphics.DrawLines(lineColor, points);
    }

    private void DrawBarGraph(Graphics graphics, Brush brush, RectangleF positionRectangle, ChannelData<double> channelDataItem)
    {
        graphics.FillRectangle(BackgroundColor, positionRectangle);
        var data = channelDataItem.DivideToParts(barCount);
        data = data.AdjustToScale(0, positionRectangle.Height, true, out _);

        var barIndex = 0;
        var barWidth = (positionRectangle.Width - barCount * barSpan) / barCount;
        var barStep = positionRectangle.Width / barCount;
        var width = positionRectangle.Width;

        for (float i = 0; i < width; i += barStep)
        {
            if (barIndex + 1 >= barCount)
            {
                // This should never happen, but better to be safe than sorry.
                continue;
            }

            var point1 = new PointF(i, positionRectangle.Bottom - (float)data.Data[barIndex++]);
            var point2 = new PointF(i + barWidth, positionRectangle.Bottom);
            var barRectangle = new RectangleF(point1, point2);
            graphics.FillRectangle(brush, barRectangle);
        }
    }

    private readonly bool multiChannel;
    private int barCount = 64;
    private SpectrumType spectrumType;
    private List<GradientColors> visualizeColors = new(new[]
    {
        new GradientColors(Colors.MidnightBlue, Colors.LightSteelBlue),
        new GradientColors(Colors.Magenta, Colors.MediumPurple),
        new GradientColors(Colors.LimeGreen, Colors.Lime),
        new GradientColors(Colors.DeepSkyBlue, Colors.LightSkyBlue),
    });


    private SignalProvider? signalProvider;
    private Thread? signalUpdateThread;
    private volatile bool signalThreadStopped;
    private int barSpan = 2;

    /// <summary>
    /// Gets or sets the signal provider providing the audio data to visualize.
    /// </summary>
    /// <value>The signal provider for the audio data.</value>
    public SignalProvider? SignalProvider
    {
        get => signalProvider;

        set
        {
            if (signalUpdateThread != null)
            {
                Stop();
            }

            signalProvider = value;
            if (value != null)
            {
                Start();
            }
        }
    }

    /// <summary>
    /// The signal read thread function.
    /// </summary>
    private async void SignalThreadFunc()
    {
        while (!signalThreadStopped)
        {
            if (signalProvider != null)
            {
                channelData = signalProvider.DataSampleWindowed;
                await Application.Instance.InvokeAsync(Invalidate);
            }

            Thread.Sleep(UpdateFrequency);
        }
    }

    /// <summary>
    /// Gets or sets the type of the spectrum to visualize.
    /// </summary>
    /// <value>The type of the spectrum to visualize.</value>
    public SpectrumType SpectrumType
    {
        get => spectrumType;

        set
        {
            if (spectrumType != value)
            {
                spectrumType = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the bar count in case this is a bar graph.
    /// </summary>
    /// <value>The bar count.</value>
    public int BarCount
    {
        get => barCount;

        set
        {
            if (barCount != value)
            {
                barCount = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the span in pixels between bars in case the <see cref="SpectrumType"/> is set to <see cref="EtoForms.SpectrumVisualizer.SpectrumType.Bar"/>.
    /// </summary>
    /// <value>The bar span in pixels.</value>
    public int BarSpan
    {
        get => barSpan;

        set
        {
            if (value != barSpan)
            {
                barSpan = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the update frequency of the visualization in milliseconds. Smaller if more often.
    /// </summary>
    /// <value>The update frequency of the visualization.</value>
    /// <remarks>Too small value might generate a high CPU/GPU usage.</remarks>
    public int UpdateFrequency { get; set; } = 20;

    /// <summary>
    /// Gets or sets the colors used for audio visualization.
    /// </summary>
    /// <value>The visualization colors.</value>
    public IReadOnlyList<GradientColors> VisualizeColors
    {
        get => visualizeColors;

        set
        {
            if (!value.SequenceEqual(visualizeColors))
            {
                visualizeColors = new List<GradientColors>(value);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Stops the signal updating for this instance.
    /// </summary>
    public void Stop()
    {
        var startTime = DateTime.Now;
        signalThreadStopped = true;
        while (!signalUpdateThread?.Join(UpdateFrequency) == false)
        {
            if ((DateTime.Now - startTime).TotalSeconds > WaitForThreadSeconds)
            {
                return;
            }
        }

        signalUpdateThread = null;
    }

    /// <summary>
    /// Gets or sets the amount in seconds to wait the spectrum data thread to join.
    /// </summary>
    /// <value>The wait time for data thread in seconds.</value>
    public double WaitForThreadSeconds { get; set; } = 2;

    /// <summary>
    /// Gets or sets the color of the background.
    /// </summary>
    /// <value>The color of the background.</value>
    /// <remarks>Note that on some platforms (e.g. Mac), setting the background color of a control can change the performance
    /// characteristics of the control and its children, since it must enable layers to do so.</remarks>
    public new Color BackgroundColor
    {
        get => base.BackgroundColor;

        set
        {
            if (base.BackgroundColor != value)
            {
                base.BackgroundColor = value;
                Invalidate();
            }
        }
    }

    private bool useGradientColorsOnBars = true;

    /// <summary>
    /// Gets or sets a value indicating whether use gradient colors when <see cref="SpectrumType"/> = <see cref="EtoForms.SpectrumVisualizer.SpectrumType.Bar"/>.
    /// </summary>
    /// <value><c>true</c> if to use gradient colors on <see cref="EtoForms.SpectrumVisualizer.SpectrumType.Bar"/> visualization; otherwise, <c>false</c>.</value>
    public bool UseGradientColorsOnBars
    {
        get => useGradientColorsOnBars;

        set
        {
            if (useGradientColorsOnBars != value)
            {
                useGradientColorsOnBars = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Starts the signal updating for this instance.
    /// </summary>
    public void Start()
    {
        if (signalUpdateThread == null)
        {
            signalUpdateThread = new Thread(SignalThreadFunc);
            signalUpdateThread.Start();
        }
    }
}