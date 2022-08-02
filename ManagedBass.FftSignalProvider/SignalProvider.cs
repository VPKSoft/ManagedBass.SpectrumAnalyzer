using System;
using System.Collections.Generic;
using System.Linq;
using FftSharp;

namespace ManagedBass.FftSignalProvider;

/// <summary>
/// A class to provide FFT signal data from a Bass channel.
/// </summary>
public class SignalProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignalProvider"/> class.
    /// </summary>
    /// <param name="signalFlags">The signal flags.</param>
    /// <param name="noHanningWindow">if set to <c>true</c> no Hanning window function is applied to the signal.</param>
    /// <param name="individualChannels">if set to <c>true</c> retrieve separate data for individual channels.</param>
    public SignalProvider(DataFlags signalFlags, bool noHanningWindow, bool individualChannels)
    {
        this.signalFlags = signalFlags;
        this.individualChannels = individualChannels;

        if (noHanningWindow)
        {
            this.signalFlags |= DataFlags.FFTNoWindow;
        }

        if (individualChannels)
        {
            this.signalFlags |= DataFlags.FFTIndividual;
        }

        CreateBuffer();
    }

    private int resolution;

    /// <summary>
    /// Sets the channel to generate the read the FFT data from.
    /// </summary>
    /// <param name="bassChannelHandle">The Bass channel or stream handle.</param>
    public void SetChannel(int bassChannelHandle)
    {
        channelHandle = bassChannelHandle;
        var info = Bass.ChannelGetInfo(channelHandle);
        CreateBuffer();
        ChannelInfo = info;
        resolution = info.Frequency / info.Channels;
    }

    /// <summary>
    /// Gets the channel information for the Bass channel or stream.
    /// </summary>
    /// <value>The channel information.</value>
    public ChannelInfo ChannelInfo { get; private set; }

    private void CreateBuffer()
    {
        if (signalFlags.HasFlag(DataFlags.FFT256))
        {
            bufferSize = 256;
        }

        if (signalFlags.HasFlag(DataFlags.FFT512))
        {
            bufferSize = 512;
        }

        if (signalFlags.HasFlag(DataFlags.FFT1024))
        {
            bufferSize = 1024;
        }

        if (signalFlags.HasFlag(DataFlags.FFT2048))
        {
            bufferSize = 2048;
        }

        if (signalFlags.HasFlag(DataFlags.FFT4096))
        {
            bufferSize = 4096;
        }

        if (signalFlags.HasFlag(DataFlags.FFT8192))
        {
            bufferSize = 8192;
        }

        if (signalFlags.HasFlag(DataFlags.FFT16384))
        {
            bufferSize = 16384;
        }

        if (signalFlags.HasFlag(DataFlags.FFT32768))
        {
            bufferSize = 32768;
        }

        buffer = new float[bufferSize];
        calculatedBufferSize = bufferSize / 2;
    }

    /// <summary>
    /// Gets the data sample for the Bass data channel or stream.
    /// </summary>
    /// <value>The data sample.</value>
    public List<ChannelData<float>> DataSample
    {
        get
        {
            var result = new List<ChannelData<float>>();

            var channels = individualChannels ? ChannelInfo.Channels : 1;

            for (var i = 0; i < channels; i++)
            {
                result.Add(new ChannelData<float>(calculatedBufferSize / channels)
                { ChannelNumber = i, Resolution = resolution, });
            }

            Bass.ChannelGetData(channelHandle, buffer, (int)signalFlags);

            if (individualChannels && ChannelInfo.Channels > 1)
            {
                for (int i = 0; i < channels; i++)
                {
                    result[i].Data = buffer.Select((value, index) => (value, index))
                        .Where(f => f.index < calculatedBufferSize && (f.index % (i + 1)) == 0).Select(f => f.value).ToArray();
                }
            }
            else
            {
                if (result.Count > 0)
                {
                    result[0].Data = buffer.Take(calculatedBufferSize).ToArray();
                }
            }

            return result;
        }
    }

    private WindowType windowType = WindowType.Hanning;
    private Window window = new FftSharp.Windows.Hanning();

    /// <summary>
    /// Gets or sets the type of the FFT window for the <see cref="DataSampleWindowed"/> property.
    /// </summary>
    /// <value>The type of the FFT window.</value>
    public WindowType WindowType
    {
        get => windowType;

        set
        {
            if (windowType != value)
            {
                windowType = value;
                window = windowType switch
                {
                    WindowType.Bartlett => new FftSharp.Windows.Bartlett(),
                    WindowType.Blackman => new FftSharp.Windows.Blackman(),
                    WindowType.Cosine => new FftSharp.Windows.Cosine(),
                    WindowType.FlatTop => new FftSharp.Windows.FlatTop(),
                    WindowType.Hamming => new FftSharp.Windows.Hamming(),
                    WindowType.Hanning => new FftSharp.Windows.Hanning(),
                    WindowType.Kaiser => new FftSharp.Windows.Kaiser(),
                    WindowType.Rectangular => new FftSharp.Windows.Rectangular(),
                    WindowType.Tukey => new FftSharp.Windows.Tukey(),
                    WindowType.Welch => new FftSharp.Windows.Welch(),
                    _ => new FftSharp.Windows.Hanning(),
                };
            }
        }
    }

    /// <summary>
    /// Gets the FFT-windowed data sample for the Bass data channel or stream.
    /// </summary>
    /// <value>The FFT-windowed data sample.</value>
    public List<ChannelData<double>> DataSampleWindowed
    {
        get
        {
            var sample = DataSample;

            var result = new List<ChannelData<double>>();

            var channels = sample.Count;

            for (var i = 0; i < channels; i++)
            {
                var addData = new ChannelData<double>(calculatedBufferSize / channels);
                result.Add(addData);
                addData.Data = sample[i].Data.Select(f => (double)f).ToArray();
                addData.ChannelNumber = sample[i].ChannelNumber;
                addData.Resolution = resolution;
            }

            foreach (var channelData in result)
            {
                if (WindowType == WindowType.Custom)
                {
                    CustomWindowAction?.Invoke(channelData.Data);
                }
                else if (WindowType != WindowType.None)
                {
                    window?.ApplyInPlace(channelData.Data);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Gets or sets the custom window action to use with the <see cref="WindowType"/> set to <see cref="ManagedBass.FftSignalProvider.WindowType.Custom"/>.
    /// </summary>
    /// <value>The custom window action.</value>
    public Action<double[]>? CustomWindowAction { get; set; }

    private int bufferSize;
    private int calculatedBufferSize;
    private float[] buffer = Array.Empty<float>();
    private int channelHandle;
    private readonly DataFlags signalFlags;
    private readonly bool individualChannels;
}
