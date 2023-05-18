using Cypher.Cypher_Tools;

namespace Cypher.Cypher_Algorithms
{
    public class RSA : BaseAlg
    {
        private const int powerEncode = 5;
        private const int powerDecode = 2;

        public override string type { get; } = "asymmetry";
        public override string name { get; } = "RSA";
        public override int sizeBlock { get; }
        protected override int sizeKey { get; }
        protected override byte defaultChar { get; }
        protected override int countRound { get; }

        public RSA(char defaultChar)
        {
            sizeKey = 2;
            sizeBlock = 1;
            countRound = 1;
            this.defaultChar = (byte)defaultChar;
            ExpandKey();
        }
        protected override void ExpandKey()
        {
            OpenKeys = new();
            CloseKeys = new();
            (int e, int d, int n)[] CreatKeys()
            {
                (int e, int d, int n)[] block = new (int e, int d, int n)[4];
                for (int i = 0; i < 4; i++)
                {
                    Random random = new(DateTime.Now.Millisecond);
                    int p = Mathematics.Math.NextSimple(random, 10, 1000);
                    int q = Mathematics.Math.NextSimple(random, 10, 1000);
                    int n = p * q;
                    int fn = (p - 1) * (q - 1);
                    int e = Mathematics.Math.GetMutuallySimple(fn);
                    int d, d2;
                    Mathematics.Math.GCD(e, fn, out d, out d2);
                    d += d < 0 ? fn : 0;
                    block[i] = (e, d, n);
                }
                return block;
            }
            for (int i = 0; i < countRound; i++)
            {
                (int e, int d, int n)[] block = CreatKeys();
                OpenKeys.Add(new Word[2]
                {
                    new Word(block[0].e, block[1].e, block[2].e, block[3].e),
                    new Word(block[0].n, block[1].n, block[3].n, block[3].n)
                });

                CloseKeys.Add(new Word[2]
                {
                    new Word(block[0].d, block[1].d, block[2].d, block[3].d),
                    new Word(block[0].n, block[1].n, block[2].n, block[3].n)
                });
            }
        }
        private void Code(Word[] block, IEnumerable<Word[]> keys, ulong power)
        {
            int[] data = block[0].ToArray();
            foreach (var key in keys)
            {
                ulong[] currentKey = key[0].ToArrayLong();
                ulong[] n = key[1].ToArrayLong();
                for (int i = 0; i < data.Length; i++)
                {
                    ulong full = currentKey[i] / power;
                    ulong rem = currentKey[i] % power;
                    ulong num_P = 1;
                    ulong full_num = (ulong)(Math.Pow(data[i], power) % n[i]);
                    ulong rem_num = (ulong)(Math.Pow(data[i], rem) % n[i]);
                    for (ulong j = 0; j <= full; j++)
                    {
                        if (j != full)
                        {
                            num_P = num_P * full_num;
                            if (num_P > n[i])
                            {
                                num_P %= n[i];
                            }
                        }
                        else
                        {
                            num_P *= rem_num;
                            num_P %= n[i];
                        }
                    }
                    block[0].ChangeByte(i + 1, (int)num_P);
                }
            }
        }
        public override void EncodeBlock(Word[] block, IEnumerable<Word[]> keys)
        {
            Code(block, keys, powerEncode);
        }
        public override void DecodeBlock(Word[] block, IEnumerable<Word[]> keys)
        {
            Code(block, keys, powerDecode);
        }
    }
}
