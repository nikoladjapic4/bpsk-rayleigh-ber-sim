namespace BpskRayleighBerSim
{
    public static class FrameBuilder
    {
        public static int[] InsertPreamble(int[] dataBits, int blockLen, int preambleLen)
        {
            if (preambleLen == 0) return dataBits;

            int numBlocks = dataBits.Length / blockLen;
            int blockWithPreamble = blockLen + preambleLen;
            var frameBits = new int[numBlocks * blockWithPreamble];

            for (int b = 0; b < numBlocks; b++)
            {
                // Preambula = 1
                for (int p = 0; p < preambleLen; p++)
                    frameBits[b * blockWithPreamble + p] = 1;

                // Payload
                Array.Copy(dataBits, b * blockLen, frameBits, b * blockWithPreamble + preambleLen, blockLen);
            }

            return frameBits;
        }
    }
}
