using Cypher.Cypher_Algorithms;

namespace Cypher.Cypher_Method
{
    public class ECB : BaseMethod
    {
        public override string Name
        {
            get => "ECB";
        }
        public ECB(BaseAlg alg) : base(alg) { }
        public override IEnumerable<int> Encode()
        {
            for (int i = 0; i < alg.InputData.Count; i++)
            {
                alg.EncodeBlock(alg.InputData[i], alg.CurrentRoundKeys);
            }
            return alg.GetResultData(alg.InputData);
        }
        public override IEnumerable<int> Decode()
        {
            for (int i = 0; i < alg.InputData.Count; i++)
            {
                alg.DecodeBlock(alg.InputData[i], alg.CurrentRoundKeys);
            }
            return alg.GetResultData(alg.InputData);
        }
    }
}