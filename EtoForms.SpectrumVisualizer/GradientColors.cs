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

namespace EtoForms.SpectrumVisualizer;

/// <summary>
/// A start and end color class for a simple gradient.
/// </summary>
public class GradientColors
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GradientColors"/> class.
    /// </summary>
    /// <param name="startColor">The gradient start color.</param>
    /// <param name="endColor">The gradient end color.</param>
    public GradientColors(Color startColor, Color endColor)
    {
        StartColor = startColor;
        EndColor = endColor;
    }

    /// <summary>
    /// Gets the gradient start color.
    /// </summary>
    /// <value>The gradient start color.</value>
    public Color StartColor { get; init; }

    /// <summary>
    /// Gets the gradient end color.
    /// </summary>
    /// <value>The gradient end color.</value>
    public Color EndColor { get; init; }
}