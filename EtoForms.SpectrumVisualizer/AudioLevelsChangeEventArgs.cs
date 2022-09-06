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

namespace EtoForms.SpectrumVisualizer;

/// <summary>
/// Event arguments to indicate audio level (peak amplitude) change for the audio channels.
/// Implements the <see cref="EventArgs" />
/// </summary>
/// <seealso cref="EventArgs" />
public class AudioLevelsChangeEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioLevelsChangeEventArgs"/> class.
    /// </summary>
    /// <param name="levelLeftChannel">The audio level (peak amplitude) on the left channel.</param>
    /// <param name="levelRightChannel">The audio level (peak amplitude) on the right channel.</param>
    public AudioLevelsChangeEventArgs(double levelLeftChannel, double levelRightChannel)
    {
        LevelLeftChannel = levelLeftChannel;
        LevelRightChannel = levelRightChannel;
    }

    /// <summary>
    /// Gets or sets the audio level (peak amplitude) on the right channel.
    /// </summary>
    /// <value>The peak amplitude of the right channel.</value>
    public double LevelRightChannel { get; }

    /// <summary>
    /// Gets or sets the audio level (peak amplitude) on the left channel.
    /// </summary>
    /// <value>The peak amplitude of the left channel.</value>
    public double LevelLeftChannel { get; }
}