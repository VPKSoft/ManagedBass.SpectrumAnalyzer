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

using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using EtoForms.SpectrumVisualizer;
using ManagedBass;
using ManagedBass.FftSignalProvider;

namespace TestBassSpectrum;
internal class FormMain : Form
{
    public FormMain()
    {
        Title = "Audio spectrum visualizer test";

        Bass.Init();

        Shown += FormMain_Shown;

        spectrumAnalyzer.AudioLevelsChanged += SpectrumAnalyzer_AudioLevelsChanged;
        spectrumAnalyzer.RaiseAudioLevelsChanged = true;

        Content = new TableLayout
        {
            Rows =
            {
                new TableRow { Cells =
                {
                    new TableCell(spectrumAnalyzer, true),
                },
                ScaleHeight = true, },
                new TableRow(new TableCell(levelLeftBar)) { ScaleHeight = false,},
                new TableRow(new TableCell(levelRightBar)) { ScaleHeight = false,},
            },
        };
    }

    private async void SpectrumAnalyzer_AudioLevelsChanged(object? sender, AudioLevelsChangeEventArgs e)
    {
        await Application.Instance.InvokeAsync(() =>
        {
            levelLeftBar.Value = (int)e.LevelLeftChannel;
            levelRightBar.Value = (int)e.LevelRightChannel;
        });
        Debug.WriteLine($"Level left: {e.LevelLeftChannel:F1}, Level right: {e.LevelRightChannel:F1}");
    }

    private int playBackHandle;

    private readonly SpectrumVisualizer spectrumAnalyzer = new(true)
    { Width = 300, Height = 150, SpectrumType = SpectrumType.Bar, BackgroundColor = Colors.Black, };

    private readonly ProgressBar levelLeftBar = new() { Height = 20, Value = 100, };
    private readonly ProgressBar levelRightBar = new() { Height = 20, Value = 100, };

    private void FormMain_Shown(object? sender, EventArgs e)
    {
        playBackHandle = Bass.CreateStream(@"your audio file goes here.mp3");
        Bass.ChannelPlay(playBackHandle);

        spectrumAnalyzer.SignalProvider = new SignalProvider(DataFlags.FFT1024, true, true) { WindowType = WindowType.Hanning, };
        spectrumAnalyzer.SetChannel(playBackHandle);
    }
}