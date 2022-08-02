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

using System;
using System.Linq;

namespace ManagedBass.FftSignalProvider;

/// <summary>
/// A channel signal data class for the <see cref="SignalProvider"/> class.
/// </summary>
/// <typeparam name="T">The type of the signal data.</typeparam>
public class ChannelData<T> where T : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelData{T}"/> class.
    /// </summary>
    /// <param name="size">The size of the signal data buffer.</param>
    public ChannelData(int size)
    {
        if (typeof(T) != typeof(double) && typeof(T) != typeof(float))
        {
            throw new InvalidOperationException("Only double and float data types are supported.");
        }

        Data = new T[size];
    }

    /// <summary>
    /// Gets the signal data buffer.
    /// </summary>
    /// <value>The signal data.</value>
    public T[] Data { get; internal set; }

    /// <summary>
    /// Gets the channel number. E.g. for stereo, 0, 1.
    /// </summary>
    /// <value>The channel number.</value>
    public int ChannelNumber { get; internal set; }

    /// <summary>
    /// Gets the resolution of the channel. E.g. 22050 Hz.
    /// </summary>
    /// <value>The resolution of the channel.</value>
    public int Resolution { get; internal set; }

    /// <summary>
    /// Adjusts the data to a specified scaling.
    /// </summary>
    /// <param name="minimum">The minimum value of the new scale.</param>
    /// <param name="maximum">The maximum value of the new scale.</param>
    /// <param name="adjustToPositive">if set to <c>true</c> all the data points are adjusted to positive values.</param>
    /// <param name="multiplierUsed">The multiplier used to adjust the data to fit the <paramref name="minimum"/> - <paramref name="maximum"/> scale.</param>
    /// <param name="overrideMultiplier">A multiplier to actually use for scaling. This can provide a slowly adjusting multiplier using a feed back loop with the <paramref name="multiplierUsed"/> parameter.</param>
    /// <returns>A new instance of the <see cref="ChannelData{T}"/> class with the parameterized adjustments.</returns>
    public ChannelData<double> AdjustToScale(double minimum, double maximum, bool adjustToPositive, out double multiplierUsed, double overrideMultiplier = 0)
    {
        var result = new ChannelData<double>(Data.Length);

        Array.Copy(Data, result.Data, Data.Length);
        result.ChannelNumber = ChannelNumber;
        result.Resolution = Resolution;

        var min = result.Data.Min();
        var max = result.Data.Max();

        if (adjustToPositive && min < 0)
        {
            var adjust = Math.Abs(min);
            for (var i = 0; i < result.Data.Length; i++)
            {
                result.Data[i] += adjust;
            }

            min = result.Data.Min();
        }


        var multiplier = (maximum - minimum) / (max - min);
        multiplierUsed = multiplier;
        multiplier = overrideMultiplier != 0 ? overrideMultiplier : multiplier;


        for (var i = 0; i < result.Data.Length; i++)
        {
            result.Data[i] *= multiplier;
        }

        return result;
    }

    /// <summary>
    /// Divides the channel data into specified amount averaged data points.
    /// </summary>
    /// <param name="partCount">The part count.</param>
    /// <returns>A new instance of the <see cref="ChannelData{T}"/> class divided into <paramref name="partCount"/> pieces.</returns>
    public ChannelData<double> DivideToParts(int partCount)
    {
        var result = new ChannelData<double>(partCount);
        var dataIndex = 0.0;
        var increment = Data.Length / (double)partCount;

        var data = new double[Data.Length];
        Array.Copy(Data, data, Data.Length);

        var takeAmount = (int)Math.Ceiling(increment);
        var index = 0;
        while (dataIndex < Data.Length)
        {
            var take = takeAmount;
            if (dataIndex + takeAmount > Data.Length)
            {
                take = Data.Length - (int)dataIndex;
                if (take < 1)
                {
                    continue;
                }
            }

            var dataPoint = data.Skip((int)dataIndex).Take(take).Average();
            result.Data[index++] = dataPoint;
            dataIndex += increment;
        }

        result.ChannelNumber = ChannelNumber;
        result.Resolution = Resolution;
        return result;
    }
}