using System.Numerics;

namespace BpskRayleighBerSim
{
    public static class BpskModulator
    {
        public static Complex[] Modulate(int[] bits)
        {
            var symbols = new Complex[bits.Length];
            for (int i = 0; i < bits.Length; i++)
                symbols[i] = new Complex(2 * bits[i] - 1, 0);
            return symbols;
        }
    }
}
