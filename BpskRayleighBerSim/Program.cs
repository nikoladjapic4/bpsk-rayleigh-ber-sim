namespace BpskRayleighBerSim
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenarios = new (string Name, SimulationConfig Config)[]
            {
                ("ScenarioA", SimulationConfig.ScenarioA()),
                ("ScenarioB", SimulationConfig.ScenarioB()),
                ("ScenarioC", SimulationConfig.ScenarioC()),
                ("ScenarioD", SimulationConfig.ScenarioD()),
                ("Diversity_SC", SimulationConfig.DiversityM2_SC()),
                ("Diversity_MRC", SimulationConfig.DiversityM2_MRC())
            };

            var allCurves = new SortedDictionary<string, SortedDictionary<int, double>>();

            foreach (var (name, config) in scenarios)
            {
                Console.WriteLine($"Pokrecem simulaciju: {name}");

                double[] berArr = Simulator.Run(config);

                Console.WriteLine("SNR(dB)\tBER");

                var curve = new SortedDictionary<int, double>();
                int snr = config.SnrStartDb;

                foreach (var ber in berArr)
                {
                    Console.WriteLine($"{snr}\t{ber:E}");
                    curve[snr] = ber;
                    snr += config.SnrStepDb;
                }

                allCurves[name] = curve;

                using var writer = new StreamWriter($"{name}_BER.csv");
                writer.WriteLine("SNR_dB,BER");
                foreach (var kv in curve)
                    writer.WriteLine($"{kv.Key},{kv.Value}");

                Console.WriteLine("----------------------------------------");
            }

            PlotHelper.PlotBer(allCurves, "BER.png");

            Console.WriteLine("Simulacija zavrsena.");
        }
    }
}
