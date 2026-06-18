using System.Numerics;

namespace BpskRayleighBerSim
{
    public class Simulator
    {
        public static double[] Run(SimulationConfig config)
        {
            int blockSize = config.I + config.P;
            int numberOfBlocks = config.N / config.I;
            int K = numberOfBlocks * blockSize;

            double[] berArr =
                new double[(config.SnrEndDb - config.SnrStartDb) / config.SnrStepDb + 1];

            for (int snrIdx = 0; snrIdx < berArr.Length; snrIdx++)
            {
                double snrDb = config.SnrStartDb + snrIdx * config.SnrStepDb;
                double berSum = 0.0;

                for (int run = 0; run < config.MonteCarloRuns; run++)
                {
                    // 1. Generisanje podataka
                    int[] data = new int[config.N];
                    Random rnd = new Random();
                    for (int i = 0; i < data.Length; i++)
                        data[i] = rnd.Next(2);

                    int[] txBits =
                        FrameBuilder.InsertPreamble(data, config.I, config.P);

                    // 2. BPSK

                    Complex[] txSymbols =
                        BpskModulator.Modulate(txBits);

                    Complex[,] x = new Complex[config.Antennas, K];
                    for (int m = 0; m < config.Antennas; m++)
                        for (int k = 0; k < K; k++)
                            x[m, k] = txSymbols[k];


                    // 4. BLOK Rayleigh fading
                    Complex[,] fading;

                    if (config.UseTimeCorrelatedFading)
                    {
                        fading = FadingChannel.GenerateTimeCorrelatedRayleigh(
                            K,
                            config.Antennas,
                            config.f0,
                            config.v,
                            config.vb);
                    }
                    else
                    {
                        fading = FadingChannel.GenerateBlockRayleighFading(
                            numberOfBlocks,
                            blockSize,
                            config.Antennas);
                    }


                    Complex[,] y =
                        MatrixMultiply(x, fading);

                    y = FadingChannel.AddAwgn(y, snrDb);


                    // 5. Detekcija
                    double ber;

                    if (config.Antennas == 1)
                        ber = DemodulatorSiso(y, data, config);
                    else if (config.Diversity == DiversityType.SC)
                        ber = DemodulatorSC(y, data, config);
                    else
                        ber = DemodulatorMRC(y, data, config);

                    berSum += ber;
                }

                berArr[snrIdx] = berSum / config.MonteCarloRuns;
            }

            return berArr;
        }


        // SISO
        private static double DemodulatorSiso(
            Complex[,] rx,
            int[] payload,
            SimulationConfig cfg)
        {
            int[] decoded = new int[payload.Length];
            int blockSize = cfg.I + cfg.P;

            for (int b = 0; b < payload.Length / cfg.I; b++)
            {
                Complex h = Complex.Zero;
                for (int i = 0; i < cfg.P; i++)
                    h += rx[0, b * blockSize + i];
                h /= cfg.P;

                for (int i = 0; i < cfg.I; i++)
                {
                    Complex r = rx[0, b * blockSize + cfg.P + i];

                    if (cfg.UseTimeCorrelatedFading)
                    {
                        decoded[b * cfg.I + i] = r.Real > 0 ? 1 : 0;
                    }
                    else
                    {
                        Complex z = r / h;
                        decoded[b * cfg.I + i] = z.Real > 0 ? 1 : 0;
                    }
                }

            }

            return ComputeBer(decoded, payload);
        }


        // SC
        private static double DemodulatorSC(
            Complex[,] rx,
            int[] payload,
            SimulationConfig cfg)
        {
            int[] decoded = new int[payload.Length];
            int blockSize = cfg.I + cfg.P;

            for (int b = 0; b < payload.Length / cfg.I; b++)
            {
                Complex[] h = new Complex[cfg.Antennas];

                for (int m = 0; m < cfg.Antennas; m++)
                {
                    for (int i = 0; i < cfg.P; i++)
                        h[m] += rx[m, b * blockSize + i];
                    h[m] /= cfg.P;
                }

                int best = 0;
                double max = 0;
                for (int m = 0; m < cfg.Antennas; m++)
                {
                    if (h[m].Magnitude > max)
                    {
                        max = h[m].Magnitude;
                        best = m;
                    }
                }

                for (int i = 0; i < cfg.I; i++)
                {
                    Complex z =
                        rx[best, b * blockSize + cfg.P + i] / h[best];
                    decoded[b * cfg.I + i] = z.Real > 0 ? 1 : 0;
                }
            }

            return ComputeBer(decoded, payload);
        }

        // MRC (normalizovan)
        private static double DemodulatorMRC(
            Complex[,] rx,
            int[] payload,
            SimulationConfig cfg)
        {
            int[] decoded = new int[payload.Length];
            int blockSize = cfg.I + cfg.P;

            for (int b = 0; b < payload.Length / cfg.I; b++)
            {
                Complex[] h = new Complex[cfg.Antennas];

                for (int m = 0; m < cfg.Antennas; m++)
                {
                    for (int i = 0; i < cfg.P; i++)
                        h[m] += rx[m, b * blockSize + i];
                    h[m] /= cfg.P;
                }

                for (int i = 0; i < cfg.I; i++)
                {
                    Complex sum = Complex.Zero;
                    double norm = 0;

                    for (int m = 0; m < cfg.Antennas; m++)
                    {
                        sum += rx[m, b * blockSize + cfg.P + i] *
                               Complex.Conjugate(h[m]);
                        norm += h[m].Magnitude * h[m].Magnitude;
                    }

                    Complex z = sum / norm;
                    decoded[b * cfg.I + i] = z.Real > 0 ? 1 : 0;
                }
            }

            return ComputeBer(decoded, payload);
        }


        // Pomocne
        private static double ComputeBer(int[] dec, int[] refBits)
        {
            int err = 0;
            for (int i = 0; i < refBits.Length; i++)
                if (dec[i] != refBits[i]) err++;
            return (double)err / refBits.Length;
        }

        private static Complex[,] MatrixMultiply(
            Complex[,] a,
            Complex[,] b)
        {
            int M = a.GetLength(0);
            int K = a.GetLength(1);
            Complex[,] y = new Complex[M, K];

            for (int m = 0; m < M; m++)
                for (int k = 0; k < K; k++)
                    y[m, k] = a[m, k] * b[m, k];

            return y;
        }
    }
}
