using Cypher.Cypher_Tools;

namespace Cypher.Cypher_Algorithms
{
    public abstract class BaseAlg
    {
        public abstract string type { get; }
        public abstract string name { get; }
        public abstract int sizeBlock { get;}
        protected abstract int sizeKey { get; }
        protected abstract byte defaultChar { get; }
        protected abstract int countRound { get; }

        protected List<int> key = null!;

        public List<Word[]> OpenKeys { get; protected set; } = null!;
        public List<Word[]> CloseKeys { get; protected set; } = null!;
        public List<Word[]> CurrentRoundKeys { get; protected set; } = null!;
        public List<Word[]>? InitVector { get; protected set; }
        public List<Word[]> InputData { get; protected set; } = null!;

        public virtual void Registration(IEnumerable<int> inputData, IEnumerable<int> key, int? initVec = null)
        {
            this.key = new(key);
            ExpandKey();
            InputData = GetWordsViewFromByte(new(inputData));
            if (initVec != null)
            {
                var temp = CreatInitVector((int)initVec, InputData.Count);
                InitVector = GetWordsViewFromByte(temp);
            }
        }
        public virtual void Registration(IEnumerable<int> inputData, bool openKey, int? initVec = null)
        {
            CurrentRoundKeys = openKey ? OpenKeys : CloseKeys;
            InputData = GetWordsViewFromByte(new(inputData));
            if (initVec != null)
            {
                var temp = CreatInitVector((int)initVec, InputData.Count);
                InitVector = GetWordsViewFromByte(temp);
            }
        }

        protected abstract void ExpandKey();
        public abstract void EncodeBlock(Word[] block, IEnumerable<Word[]> currentKey);
        public abstract void DecodeBlock(Word[] block, IEnumerable<Word[]> currentKey);


        protected virtual List<int> CreatInitVector(int vector, int numBlocks)
        {
            List<int> initVec = new();
            for (int j = 1; j <= numBlocks; j++)
            {
                for (int i = 1; i <= sizeBlock; i++)
                {
                    Random random = new(vector);
                    vector = random.Next() ^ i ^ j;
                    string result = Convert.ToString(vector, 16);
                    while (result.Length < 8)
                    { 
                        result = Convert.ToString(0) + result; 
                    }
                    initVec.Add(Convert.ToByte(result[0..2], 16));
                    initVec.Add(Convert.ToByte(result[2..4], 16));
                    initVec.Add(Convert.ToByte(result[4..6], 16));
                    initVec.Add(Convert.ToByte(result[6..8], 16));
                }
            }
            return initVec;
        }
        protected virtual int CountOfBlocks(IEnumerable<int> inputData)
        {
            int size = inputData.Count();
            while (size % (sizeBlock * 4) != 0)
            {
                size++;
            }
            return size / (sizeBlock * 4);
        }
        protected virtual List<Word[]> GetWordsViewFromByte(List<int> inputData)
        {
            List<Word[]> currentData = new();
            int size = CountOfBlocks(inputData);
            while (inputData.Count / (sizeBlock * 4) != size)
            {
                inputData.Add(defaultChar);
            }
            int j = 0;
            for (int i = 0; i < size; i++)
            {
                currentData.Add(new Word[sizeBlock]);

                for (int x = 0; x < currentData[i].Length; x++, j += 4)
                {
                    currentData[i][x] = new(inputData, j);
                }
            }
            return currentData;
        }

        public virtual IEnumerable<int> GetResultData(IEnumerable<int> currentData)
        {
            var code = currentData.ToList();
            int id = code.FindIndex(x => x == defaultChar);
            for(int i = id; i < code.Count; i++)
            {
                if(i < 0 || code[i] != defaultChar)
                {
                    return code;
                }
            }
            return code.GetRange(0, id);
        }
        public virtual IEnumerable<int> GetResultData(IEnumerable<Word[]> currentData)
        {
            List<int> code = new();
            foreach (var block in currentData)
            {
                foreach (Word word in block)
                {
                    code.AddRange(word.ToArray());
                }
            }
            return GetResultData(code);
        }
    }
}
