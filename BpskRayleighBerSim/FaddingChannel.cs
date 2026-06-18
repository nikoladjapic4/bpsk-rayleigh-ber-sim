using System.Numerics;

namespace BpskRayleighBerSim
{
    public static class FadingChannel
    {
        private static readonly Random rnd = new Random();

        // Blok-Rayleigh fading
        // jedan koeficijent po bloku i anteni
        public static Complex[,] GenerateBlockRayleighFading(
            int numberOfBlocks,
            int blockSize,
            int M)
        {
            int K = numberOfBlocks * blockSize;
            Complex[,] fading = new Complex[M, K];

            for (int b = 0; b < numberOfBlocks; b++)
            {
                for (int m = 0; m < M; m++)
                {
                    // Rayleigh h 
                    Complex h = GenerateRayleighScalar();

                    for (int k = 0; k < blockSize; k++)
                    {
                        int idx = b * blockSize + k;
                        fading[m, idx] = h;
                    }
                }
            }

            return fading;
        }


        // Jedan Rayleigh koeficijent
        public static Complex GenerateRayleighScalar()
        {
            double x = Gaussian();
            double y = Gaussian();
            return new Complex(x, y) / Math.Sqrt(2.0);
        }


        // AWGN
        public static Complex[,] AddAwgn(Complex[,] signal, double snrDb)
        {
            int M = signal.GetLength(0);
            int K = signal.GetLength(1);

            Complex[,] noisy = new Complex[M, K];

            double snrLinear = Math.Pow(10.0, snrDb / 10.0);

            // prosečna snaga signala
            double power = 0.0;
            for (int m = 0; m < M; m++)
                for (int k = 0; k < K; k++)
                    power += signal[m, k].Magnitude * signal[m, k].Magnitude;

            power /= (M * K);

            double sigma = Math.Sqrt(power / (2.0 * snrLinear));

            for (int m = 0; m < M; m++)
            {
                for (int k = 0; k < K; k++)
                {
                    double ni = Gaussian() * sigma;
                    double nq = Gaussian() * sigma;
                    noisy[m, k] = signal[m, k] + new Complex(ni, nq);
                }
            }

            return noisy;
        }


        // Gaussian generator (Box–Muller)
        private static double Gaussian()
        {
            double u1 = 1.0 - rnd.NextDouble();
            double u2 = 1.0 - rnd.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        // Vremenski korelisan rayleigh fading
        public static Complex[,] GenerateTimeCorrelatedRayleigh(
            int K,
            int M,
            double f0,
            double vKmh,
            double vb)
        {
            double c = 3e8;
            double v = vKmh / 3.6;          // m/s
            double fm = v / c * f0;         // Doppler
            double Ts = 1.0 / vb;           // trajanje bita

            double rho = BesselJ0(2 * Math.PI * fm * Ts);

            Complex[,] fading = new Complex[M, K];

            for (int m = 0; m < M; m++)
            {
                fading[m, 0] = GenerateRayleighScalar();

                for (int k = 1; k < K; k++)
                {
                    Complex w = GenerateRayleighScalar();
                    fading[m, k] = rho * fading[m, k - 1] + Math.Sqrt(1 - rho * rho) * w;
                }
            }

            return fading;
        }

        // Aproksimacija Bessel J0
        private static double BesselJ0(double x)
        {
            if (x < 3.0)
            {
                double x2 = x * x;
                return 1.0 - x2 / 4.0 + x2 * x2 / 64.0 - x2 * x2 * x2 / 2304.0;
            }
            else
            {
                return Math.Sqrt(2.0 / (Math.PI * x)) * Math.Cos(x - Math.PI / 4.0);
            }
        }

    }
}
