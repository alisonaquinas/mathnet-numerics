﻿using System;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{
    static class ReferenceDiscreteFourierTransform
    {
        /// <summary>
        /// Naive forward DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        public static Complex32[] NaiveForward(Complex32[] samples, FourierOptions options = FourierOptions.Default)
        {
            var frequencySpace = Naive(samples, SignByOptions(options));
            ForwardScaleByOptions(options, frequencySpace);
            return frequencySpace;
        }

        /// <summary>
        /// Naive forward DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        public static Complex[] NaiveForward(Complex[] samples, FourierOptions options = FourierOptions.Default)
        {
            var frequencySpace = Naive(samples, SignByOptions(options));
            ForwardScaleByOptions(options, frequencySpace);
            return frequencySpace;
        }

        /// <summary>
        /// Naive inverse DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="spectrum">Frequency-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding time-space vector.</returns>
        public static Complex32[] NaiveInverse(Complex32[] spectrum, FourierOptions options = FourierOptions.Default)
        {
            var timeSpace = Naive(spectrum, -SignByOptions(options));
            InverseScaleByOptions(options, timeSpace);
            return timeSpace;
        }

        /// <summary>
        /// Naive inverse DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="spectrum">Frequency-space sample vector.</param>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Corresponding time-space vector.</returns>
        public static Complex[] NaiveInverse(Complex[] spectrum, FourierOptions options = FourierOptions.Default)
        {
            var timeSpace = Naive(spectrum, -SignByOptions(options));
            InverseScaleByOptions(options, timeSpace);
            return timeSpace;
        }

        /// <summary>
        /// Naive generic DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        static Complex32[] Naive(Complex32[] samples, int exponentSign)
        {
            var w0 = exponentSign * Constants.Pi2 / samples.Length;
            var spectrum = new Complex32[samples.Length];

            CommonParallel.For(0, samples.Length, (u, v) =>
            {
                for (int i = u; i < v; i++)
                {
                    var wk = w0 * i;
                    var sum = Complex32.Zero;
                    for (var n = 0; n < samples.Length; n++)
                    {
                        var w = n * wk;
                        sum += samples[n] * new Complex32((float)Math.Cos(w), (float)Math.Sin(w));
                    }

                    spectrum[i] = sum;
                }
            });

            return spectrum;
        }

        /// <summary>
        /// Naive generic DFT, useful e.g. to verify faster algorithms.
        /// </summary>
        /// <param name="samples">Time-space sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <returns>Corresponding frequency-space vector.</returns>
        static Complex[] Naive(Complex[] samples, int exponentSign)
        {
            var w0 = exponentSign * Constants.Pi2 / samples.Length;
            var spectrum = new Complex[samples.Length];

            CommonParallel.For(0, samples.Length, (u, v) =>
            {
                for (int i = u; i < v; i++)
                {
                    var wk = w0 * i;
                    var sum = Complex.Zero;
                    for (var n = 0; n < samples.Length; n++)
                    {
                        var w = n * wk;
                        sum += samples[n] * new Complex(Math.Cos(w), Math.Sin(w));
                    }

                    spectrum[i] = sum;
                }
            });

            return spectrum;
        }

        /// <summary>
        /// Extract the exponent sign to be used in forward transforms according to the
        /// provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <returns>Fourier series exponent sign.</returns>
        static int SignByOptions(FourierOptions options)
        {
            return (options & FourierOptions.InverseExponent) == FourierOptions.InverseExponent ? 1 : -1;
        }

        /// <summary>
        /// Rescale FFT-the resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        static void ForwardScaleByOptions(FourierOptions options, Complex32[] samples)
        {
            if ((options & FourierOptions.NoScaling) == FourierOptions.NoScaling ||
                (options & FourierOptions.AsymmetricScaling) == FourierOptions.AsymmetricScaling)
            {
                return;
            }

            var scalingFactor = (float)Math.Sqrt(1.0 / samples.Length);
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Rescale FFT-the resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        static void ForwardScaleByOptions(FourierOptions options, Complex[] samples)
        {
            if ((options & FourierOptions.NoScaling) == FourierOptions.NoScaling ||
                (options & FourierOptions.AsymmetricScaling) == FourierOptions.AsymmetricScaling)
            {
                return;
            }

            var scalingFactor = Math.Sqrt(1.0 / samples.Length);
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Rescale the iFFT-resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        static void InverseScaleByOptions(FourierOptions options, Complex32[] samples)
        {
            if ((options & FourierOptions.NoScaling) == FourierOptions.NoScaling)
            {
                return;
            }

            var scalingFactor = (float)1.0 / samples.Length;
            if ((options & FourierOptions.AsymmetricScaling) != FourierOptions.AsymmetricScaling)
            {
                scalingFactor = (float)Math.Sqrt(scalingFactor);
            }

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Rescale the iFFT-resulting vector according to the provided convention options.
        /// </summary>
        /// <param name="options">Fourier Transform Convention Options.</param>
        /// <param name="samples">Sample Vector.</param>
        static void InverseScaleByOptions(FourierOptions options, Complex[] samples)
        {
            if ((options & FourierOptions.NoScaling) == FourierOptions.NoScaling)
            {
                return;
            }

            var scalingFactor = 1.0 / samples.Length;
            if ((options & FourierOptions.AsymmetricScaling) != FourierOptions.AsymmetricScaling)
            {
                scalingFactor = Math.Sqrt(scalingFactor);
            }

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }
    }
}