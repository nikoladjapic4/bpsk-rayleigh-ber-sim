using ScottPlot;
using ScottPlot.Colormaps;

namespace BpskRayleighBerSim
{
    public static class PlotHelper
    {
        public static void PlotBer(
            SortedDictionary<string, SortedDictionary<int, double>> curves,
            string outputFile)
        {
            var plt = new ScottPlot.Plot();


            // SNR opseg
            int snrMin = curves.Values.Min(c => c.Keys.Min());
            int snrMax = curves.Values.Max(c => c.Keys.Max());

            double[] snrRange = Enumerable
                .Range(snrMin, snrMax - snrMin + 1)
                .Select(x => (double)x)
                .ToArray();


            // Teorijske krive (kombinovani graf)
            AddTheoreticalRayleigh(plt, snrRange);
            AddTheoreticalSc(plt, snrRange, 2);
            AddTheoreticalMrc(plt, snrRange, 2);

            // Simulacije (kombinovani graf)
            var palette = new ScottPlot.Palettes.Category10();
            int colorIndex = 0;

            foreach (var (label, data) in curves)
            {
                var ordered = data.OrderBy(p => p.Key).ToArray();

                double[] xs = ordered.Select(p => (double)p.Key).ToArray();
                double[] ys = ordered
                    .Select(p => Math.Log10(Math.Max(p.Value, 1e-12)))
                    .ToArray();

                var sc = plt.Add.Scatter(xs, ys);
                sc.LegendText = label;
                sc.LineWidth = 2;
                sc.MarkerSize = 7;
                sc.Color = palette.GetColor(colorIndex++);
            }

            // Stil (kombinovani graf)

            plt.Title("BER vs SNR (Monte Carlo simulacija)");
            plt.Axes.Bottom.Label.Text = "SNR [dB]";
            plt.Axes.Left.Label.Text = "log10(BER)";

            plt.Axes.SetLimits(
                snrMin - 1,
                snrMax + 1,
                -6,
                Math.Log10(5)
            );


            plt.ShowLegend(Alignment.LowerLeft);
            plt.Grid.IsVisible = true;



            plt.SavePng(outputFile, 1200, 800);

            Console.WriteLine($"Grafik sacuvan: {outputFile}");

            // Plotovanje grafika za svaku krivu posebno
            foreach (var (label, data) in curves)
            {
                var pltSingle = new ScottPlot.Plot();

     
                bool isDiversity = label.IndexOf("Diversity", StringComparison.OrdinalIgnoreCase) >= 0
                                   || label.IndexOf("MRC", StringComparison.OrdinalIgnoreCase) >= 0
                                   || label.IndexOf("SC", StringComparison.OrdinalIgnoreCase) >= 0 && !label.StartsWith("Scenario");

                if (isDiversity)
                {
                    AddTheoreticalSc(pltSingle, snrRange, 2);
                    AddTheoreticalMrc(pltSingle, snrRange, 2);
                }
                else
                {
                    AddTheoreticalRayleigh(pltSingle, snrRange);
                }

                // Add the simulated curve
                var ordered = data.OrderBy(p => p.Key).ToArray();
                double[] xs = ordered.Select(p => (double)p.Key).ToArray();
                double[] ys = ordered
                    .Select(p => Math.Log10(Math.Max(p.Value, 1e-12)))
                    .ToArray();

                var sc = pltSingle.Add.Scatter(xs, ys);
                sc.LegendText = label;
                sc.LineWidth = 2;
                sc.MarkerSize = 7;
                sc.Color = palette.GetColor(colorIndex++);

                pltSingle.Title($"BER vs SNR - {label}");
                pltSingle.Axes.Bottom.Label.Text = "SNR [dB]";
                pltSingle.Axes.Left.Label.Text = "log10(BER)";

                pltSingle.Axes.SetLimits(
                    snrMin - 1,
                    snrMax + 1,
                    -6,
                    Math.Log10(5)
                );

                pltSingle.ShowLegend(Alignment.LowerLeft);
                pltSingle.Grid.IsVisible = true;

                string singleFile = Path.GetFileNameWithoutExtension(outputFile) + $"_{label}.png";
                // sanitize filename
                foreach (var c in Path.GetInvalidFileNameChars())
                    singleFile = singleFile.Replace(c, '_');

                pltSingle.SavePng(singleFile, 1200, 800);
                Console.WriteLine($"Grafik sacuvan: {singleFile}");
            }
        }

        // TEORIJA

        private static void AddTheoreticalRayleigh(Plot plt, double[] snrDb)
        {
            double[] y = snrDb.Select(snr =>
            {
                double g = Math.Pow(10, snr / 10);
                double ber = 0.5 * (1 - Math.Sqrt(g / (1 + g)));
                return Math.Log10(Math.Max(ber, 1e-12));
            }).ToArray();

            var sc = plt.Add.Scatter(snrDb, y);
            sc.LegendText = "Teorija Rayleigh (SISO)";
            sc.LinePattern = LinePattern.Dashed;
            sc.MarkerSize = 0;
        }

        private static void AddTheoreticalSc(Plot plt, double[] snrDb, int M)
        {
            double[] y = snrDb.Select(snr =>
            {
                double g = Math.Pow(10, snr / 10);
                double sum = 0;

                for (int k = 1; k <= M; k++)
                    sum += Math.Pow(-1, k - 1) * Binomial(M, k) *
                           (1 - Math.Sqrt(g / (g + k)));

                return Math.Log10(Math.Max(0.5 * sum, 1e-12));
            }).ToArray();

            var sc = plt.Add.Scatter(snrDb, y);
            sc.LegendText = $"Teorija SC (M={M})";
            sc.LinePattern = LinePattern.Dashed;
            sc.MarkerSize = 0;
        }

        private static void AddTheoreticalMrc(Plot plt, double[] snrDb, int M)
        {
            double[] y = snrDb.Select(snr =>
            {
                double g = Math.Pow(10, snr / 10);
                double mu = Math.Sqrt(g / (1 + g));

                double pref = Math.Pow((1 - mu) / 2, M);
                double sum = 0;

                for (int k = 0; k < M; k++)
                    sum += Binomial(M - 1 + k, k) *
                           Math.Pow((1 + mu) / 2, k);

                return Math.Log10(Math.Max(pref * sum, 1e-12));
            }).ToArray();

            var sc = plt.Add.Scatter(snrDb, y);
            sc.LegendText = $"Teorija MRC (M={M})";
            sc.LinePattern = LinePattern.Dashed;
            sc.MarkerSize = 0;
        }

        // BINOMNI KOEFICIJENT
        private static double Binomial(int n, int k)
        {
            if (k < 0 || k > n) return 0;
            if (k == 0 || k == n) return 1;

            double res = 1;
            for (int i = 1; i <= k; i++)
            {
                res *= (n - k + i);
                res /= i;
            }
            return res;
        }
    }
}
