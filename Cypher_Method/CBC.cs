using Cypher.Cypher_Algorithms;
using Cypher.Cypher_Tools;

namespace Cypher.Cypher_Method
{
    public class CBC: BaseMethod
    {
        public override string Name
        {
            get => "CBC";
        }
        public CBC(BaseAlg alg) : base(alg) { }
        public override IEnumerable<int> Encode()
        {
            for (int i = 0; i < alg.InputData.Count; i++)
            {
                if (i == 0) 
                {
                    foreach(var vec in alg.InitVector)
                    {
                        alg.InputData[0] = Word.XorBlocks(vec, alg.InputData[0]);
                    }
                }
                else 
                {
                    alg.InputData[i] = Word.XorBlocks(alg.InputData[i - 1], alg.InputData[i]); 
                }
                alg.EncodeBlock(alg.InputData[i], alg.CurrentRoundKeys);
            }
            return alg.GetResultData(alg.InputData);
        }
        public override IEnumerable<int> Decode()
        {
            List<Word[]> cipherData = new();
            foreach (var el in alg.InputData)
            {
                Word[] block = new Word[el.Length];
                Array.Copy(el, block, el.Length);
                cipherData.Add(block);
            }
            for (int i = 0; i < alg.InputData.Count; i++)
            {
                alg.DecodeBlock(alg.InputData[i], alg.CurrentRoundKeys);
                if (i == 0) 
                {
                    foreach (var vec in alg.InitVector)
                    {
                        alg.InputData[0] = Word.XorBlocks(vec, alg.InputData[0]);
                    }
                }
                else 
                {
                    alg.InputData[i] = Word.XorBlocks(cipherData[i - 1], alg.InputData[i]); 
                }
            }
            return alg.GetResultData(alg.InputData);
        }
    }
}