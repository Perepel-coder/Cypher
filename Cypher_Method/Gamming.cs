using Cypher.Cypher_Algorithms;
using Cypher.Cypher_Tools;

namespace Cypher.Cypher_Method
{
    public class Gamming: BaseMethod
    {
        public override string Name
        {
            get => "GAMMING";
        }
        public Gamming(BaseAlg alg) : base(alg) { }
        public override IEnumerable<int> Encode()
        {
            foreach(var vec in alg.InitVector)
            {
                alg.EncodeBlock(vec, alg.CurrentRoundKeys);
            }
            var gamma = alg.GetResultData(alg.InitVector).ToList();
            int i = 0;
            foreach(var block in alg.InputData)
            {
                foreach(var word in block)
                {
                    var gammaWord = word.XOR(gamma.GetRange(i, 4));
                    gamma[i] = gammaWord.Byte1;
                    gamma[i+1] = gammaWord.Byte2;
                    gamma[i+2] = gammaWord.Byte3;
                    gamma[i+3] = gammaWord.Byte4;
                    i += 4;
                }
            }
            return alg.GetResultData(gamma);
        }
        public override IEnumerable<int> Decode()
        {
            return Encode();
        }
    }
}
