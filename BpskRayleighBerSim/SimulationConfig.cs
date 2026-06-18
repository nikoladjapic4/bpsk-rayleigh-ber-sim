namespace BpskRayleighBerSim
{
    public enum DiversityType
    {
        None,
        SC,
        MRC
    }

    public class SimulationConfig
    {
        public int N { get; set; } = 1000000;  // Duzina binarne sekvence
        public int I { get; set; } = 200;      // Blokovi
        public int P { get; set; } = 20;       // Preambula

        public double vb { get; set; } = 100000; // Bitrate [bit/s]
        public double f0 { get; set; } = 2e9;    // Nosilac
        public double v { get; set; } = 30;      // brzina km/h
        public int Antennas { get; set; } = 1;
        public DiversityType Diversity { get; set; } = DiversityType.None;
        public bool UseTimeCorrelatedFading { get; set; } = false;

        public int SnrStartDb { get; set; } = 0;
        public int SnrEndDb { get; set; } = 20;
        public int SnrStepDb { get; set; } = 1;

        public int MonteCarloRuns { get; set; } = 1;

        public double Vms => v / 3.6;
        public double BitDuration => 1.0 / vb;

        // Predefinisani scenariji
        public static SimulationConfig ScenarioA() => new SimulationConfig { v = 30, P = 0, Antennas = 1, Diversity = DiversityType.None };
        public static SimulationConfig ScenarioB() => new SimulationConfig { v = 30, P = 1, Antennas = 1, Diversity = DiversityType.None };
        public static SimulationConfig ScenarioC() => new SimulationConfig { v = 30, P = 20, Antennas = 1, Diversity = DiversityType.None };
        public static SimulationConfig ScenarioD() =>
            new SimulationConfig { v = 130, P = 20, Antennas = 1, Diversity = DiversityType.None, UseTimeCorrelatedFading = true };

        public static SimulationConfig DiversityM2_SC() => new SimulationConfig { v = 30, P = 20, Antennas = 2, Diversity = DiversityType.SC };
        public static SimulationConfig DiversityM2_MRC() => new SimulationConfig { v = 30, P = 20, Antennas = 2, Diversity = DiversityType.MRC };
    }
}
