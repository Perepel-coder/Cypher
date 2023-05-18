using Cypher.Cypher_Tools;
using Mathematics;

namespace Cypher.Cypher_Algorithms
{
    public class AES: BaseAlg
    {
        private readonly int numBWord;
        public override string type { get; } = "symmetry";
        public override string name { get; } = "AES";
        public override int sizeBlock { get; }
        protected override int sizeKey { get; }
        protected override byte defaultChar { get; }
        protected override int countRound { get; }

        public AES(char defaultChar)
        {
            sizeKey = 4;
            sizeBlock = 4;
            countRound = 10;
            numBWord = Word.Count();
            this.defaultChar = (byte)defaultChar;
        }
        public AES(int NumberWordkey, int NumberWordBlock, char defaultChar)
        {
            sizeBlock = 4;
            sizeKey = 4;
            numBWord = Word.Count();
            this.defaultChar = (byte)defaultChar;
            switch (NumberWordkey)
            {
                case 4: countRound = 10; break;
                case 6: countRound = 12; break;
                case 8: countRound = 16; break;
                default: throw new Exception("Не возможно применить заданные параметры ключа (длина)");
            }
        }
        private static void AddRoundKey(List<Word> currentKey, Word[] block)
        {
            for (int i = 0; i < block.Length; i++)
            {
                block[i] = block[i].XOR(currentKey[i]);
            }
        }
        protected override void ExpandKey()
        {
            List<Word> NewKey = new();
            int counter = sizeBlock * (countRound + 1);
            while (key.Count % (sizeKey * numBWord) != 0)
            {
                key.Add(defaultChar);
            }
            for (int i = 0; i < key.Count; i += 4)
            {
                NewKey.Add(new Word(key, i));
            }
            for (int i = sizeKey; i < counter; i++)
            {
                Word temp = NewKey[i - 1];
                if (i % sizeKey == 0)
                {
                    Word Word1 = (temp.RotWordLeft()).SubWord();
                    int b1 = (int)System.Math.Pow(2, i / sizeKey);
                    if (b1 > 128) { b1 = MathematicsGF.DivOfPolyGF(b1, 283)[1]; }
                    Word Word2 = new() { Byte1 = (byte)b1 };
                    temp = Word1.XOR(Word2);
                }
                else if ((sizeKey > 6) && (i % sizeKey == 4)) { temp = temp.SubWord(); }
                NewKey.Add(NewKey[i - sizeKey].XOR(temp));
            }
            List<Word[]> result = new();
            int numbOfBlock = NewKey.Count / sizeBlock;
            for (int i = 0, x = 0; i < numbOfBlock; i++)
            {
                result.Add(new Word[sizeBlock]);
                for (int j = 0; j < sizeBlock; j++, x++) { result[i][j] = NewKey[x]; }
            }
            for (int i = countRound + 1; i < result.Count - 1; i++)
            {
                result[i] = Word.XorBlocks(result[i], result[i + 1]);
            }
            CurrentRoundKeys = (result.ToArray())[0..(countRound + 1)].ToList();
        }

        #region Шифрование
        private static void SubBytes(Word[] block)
        {
            for (int i = 0; i < block.Length; i++)
            {
                block[i] = block[i].SubWord();
            }
        }
        private void ShiftRows(Word[] block)
        {
            int num = this.sizeBlock - this.numBWord;
            Word[] newBlock = new Word[this.sizeBlock];
            for (int i = 0; i <= num; i++)
            {
                newBlock[i] = new(block[i].Byte1, block[i + 1].Byte2, block[i + 2].Byte3, block[i + 3].Byte4);
            }
            num++;
            newBlock[num] = new(block[num].Byte1, block[num + 1].Byte2, block[num + 2].Byte3, block[0].Byte4); num++;
            newBlock[num] = new(block[num].Byte1, block[num + 1].Byte2, block[0].Byte3, block[1].Byte4); num++;
            newBlock[num] = new(block[num].Byte1, block[0].Byte2, block[1].Byte3, block[2].Byte4);

            for (int i = 0; i < block.Length; i++) { block[i] = newBlock[i]; }
        }
        private static void MixColumns(Word[] block)
        {
            int[,] matrix = new int[,]
            {
                { 0x02, 0x03, 0x01, 0x01 },
                { 0x01, 0x02, 0x03, 0x01 },
                { 0x01, 0x01, 0x02, 0x03 },
                { 0x03, 0x01, 0x01, 0x02 }
            };
            foreach (Word word in block)
            {
                int[,] matrixWord = new int[,]
                {
                        {word.Byte1 }, {word.Byte2 }, {word.Byte3 }, {word.Byte4 }
                };
                matrixWord = MathematicsGF.MultOfMatrixGF(matrix, matrixWord, 283);
                word.Byte1 = (byte)matrixWord[0, 0];
                word.Byte2 = (byte)matrixWord[1, 0];
                word.Byte3 = (byte)matrixWord[2, 0];
                word.Byte4 = (byte)matrixWord[3, 0];
            }
        }
        #endregion

        #region Дешифрование
        private static void InvSubBytes(Word[] block)
        {
            for (int i = 0; i < block.Length; i++)
            {
                block[i] = block[i].InvSubWord();
            }
        }
        private void InvShiftRows(Word[] block)
        {
            int size = this.sizeBlock - 1;
            Word[] newBlock = new Word[this.sizeBlock];
            newBlock[0] = new(block[0].Byte1, block[size].Byte2, block[size - 1].Byte3, block[size - 2].Byte4);
            newBlock[1] = new(block[1].Byte1, block[0].Byte2, block[size].Byte3, block[size - 1].Byte4);
            newBlock[2] = new(block[2].Byte1, block[1].Byte2, block[0].Byte3, block[size].Byte4);
            for (int i = 3; i < block.Length; i++)
            {
                newBlock[i] = new(block[i].Byte1, block[i - 1].Byte2, block[i - 2].Byte3, block[i - 3].Byte4);
            }
            for (int i = 0; i < block.Length; i++) { block[i] = newBlock[i]; }
        }
        private static void InvMixColumns(Word[] block)
        {
            int[,] matrix = new int[,]
            {
                { 0x0e, 0x0b, 0x0d, 0x09 },
                { 0x09, 0x0e, 0x0b, 0x0d },
                { 0x0d, 0x09, 0x0e, 0x0b },
                { 0x0b, 0x0d, 0x09, 0x0e }
            };
            foreach (Word word in block)
            {
                int[,] matrixWord = new int[,]
                {
                    {word.Byte1 }, {word.Byte2 }, {word.Byte3 }, {word.Byte4 }
                };
                matrixWord = MathematicsGF.MultOfMatrixGF(matrix, matrixWord, 283);
                word.Byte1 = (byte)matrixWord[0, 0];
                word.Byte2 = (byte)matrixWord[1, 0];
                word.Byte3 = (byte)matrixWord[2, 0];
                word.Byte4 = (byte)matrixWord[3, 0];
            }
        }
        #endregion

        #region Blocks
        public override void EncodeBlock(Word[] block, IEnumerable<Word[]> currentKey)
        {
            var key = currentKey.ToList();
            for (int i = 0; i <= countRound - 2; i++)
            {
                AddRoundKey(key[i].ToList(), block);
                SubBytes(block);
                ShiftRows(block);
                MixColumns(block);
            }
            AddRoundKey(key[countRound - 1].ToList(), block);
            SubBytes(block);
            ShiftRows(block);
            AddRoundKey(key[countRound].ToList(), block);
        }
        public override void DecodeBlock(Word[] block, IEnumerable<Word[]> currentKey)
        {
            var key = currentKey.ToList();
            AddRoundKey(key[countRound].ToList(), block);
            InvShiftRows(block);
            InvSubBytes(block);
            AddRoundKey(key[countRound - 1].ToList(), block);
            for (int i = countRound - 2; i >= 0; i--)
            {
                InvMixColumns(block);
                InvShiftRows(block);
                InvSubBytes(block);
                AddRoundKey(key[i].ToList(), block);
            }
        }
        #endregion
    }
}