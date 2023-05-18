using Cypher.Cypher_Tools;
using Mathematics;

namespace Cypher.Cypher_Algorithms
{
    public class GOST89: BaseAlg
    {
        private readonly int numBWord;
        private readonly int countKeys;

        public override string type { get; } = "symmetry";
        public override string name { get; } = "GOST89";
        public override int sizeBlock { get; }
        protected override int sizeKey { get; }
        protected override byte defaultChar { get; }
        protected override int countRound { get; }

        public GOST89(char defaultChar)
        {
            this.defaultChar = (byte)defaultChar;
            sizeKey = 1;
            sizeBlock = 2;
            countKeys = 8;
            countRound = 4;
            numBWord = Word.Count();
        }
        protected override void ExpandKey()
        {
            List<Word> NewKey = new();
            while (key.Count % (sizeKey* numBWord) != 0) 
            { 
                key.Add(defaultChar); 
            }
            for (int i = 0, j = 0; i < key.Count; i += 4)
            {
                if (NewKey.Count != (countKeys * sizeKey))
                {
                    NewKey.Add(new Word(key, i));
                }
                else
                {
                    NewKey[j] = NewKey[j].XOR(new Word(key, i));
                    if (j < countKeys * sizeKey - 1) { j++; }
                    else { j = 0; }
                }
            }
            int counter = sizeKey * countKeys;
            int start = NewKey.Count;
            for (int i = start; i < counter; i++)
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
            CurrentRoundKeys = new();
            foreach (var el in NewKey)
            {
                CurrentRoundKeys.Add(new Word[1] { el });
            }
        }

        #region CriptoFunction
        private static string RotLeft(char[] myStr)
        {
            char cup = myStr[0];
            string result = string.Empty;
            for (int i = 1; i < myStr.Length; i++)
            {
                result += myStr[i];
            }
            result += cup;

            return result;
        }
        private static string RotRight(char[] myStr)
        {
            string result = myStr[^1].ToString();
            for (int i = 0; i < myStr.Length - 1; i++)
            {
                result += myStr[i];
            }
            return result;
        }
        private static byte SubBit(byte myByte, int index)
        {
            string resultBit = Convert.ToString(myByte, 2);
            while (resultBit.Length < 4) { resultBit = '0' + resultBit; }
            for (int i = 0; i <= index; i++)
            {
                resultBit = GOST89.RotLeft(resultBit.ToArray());
            }
            byte res = Convert.ToByte(resultBit, 2);
            res ^= (byte)(index + 1);
            return res;
        }
        private static byte InvSubBit(byte myByte, int index)
        {
            myByte ^= (byte)(index + 1);
            string resultBit = Convert.ToString(myByte, 2);
            while (resultBit.Length < 4) { resultBit = '0' + resultBit; }
            for (int i = 0; i <= index; i++)
            {
                resultBit = GOST89.RotRight(resultBit.ToArray());
            }
            byte res = Convert.ToByte(resultBit, 2);
            return res;
        }
        #endregion

        #region Blocks
        public override void EncodeBlock(Word[] block, IEnumerable<Word[]> currentKey)
        {
            var key = currentKey.ToList();
            void Step(Word key)
            {
                Word olderPart = block[0];
                Word youngerPart = block[1];
                youngerPart = youngerPart.MultGF(key, 4299161607);

                #region SubBytes
                string stringView = youngerPart.ToStringView16();
                string arrayS = string.Empty;
                for (int i = 0; i < stringView.Length; i++)
                {
                    byte res = SubBit(Convert.ToByte(stringView[i].ToString(), 16), i);
                    arrayS += Convert.ToString(res, 16);
                }
                youngerPart = new Word
                {
                    Byte1 = Convert.ToByte(arrayS[0..2], 16),
                    Byte2 = Convert.ToByte(arrayS[2..4], 16),
                    Byte3 = Convert.ToByte(arrayS[4..6], 16),
                    Byte4 = Convert.ToByte(arrayS[6..8], 16)
                };
                #endregion

                #region RotLeft() << 11
                stringView = youngerPart.ToStringView2();
                for (int i = 0; i < 11; i++)
                {
                    stringView = RotLeft(stringView.ToArray());
                }
                stringView = Convert.ToString((long)Convert.ToUInt64(stringView, 2), 16);
                while (stringView.Length < 8) 
                { 
                    stringView = Convert.ToString(0) + stringView; 
                }
                youngerPart = new Word
                {
                    Byte1 = Convert.ToByte(stringView[0..2], 16),
                    Byte2 = Convert.ToByte(stringView[2..4], 16),
                    Byte3 = Convert.ToByte(stringView[4..6], 16),
                    Byte4 = Convert.ToByte(stringView[6..8], 16)
                };
                #endregion

                olderPart = youngerPart.XOR(olderPart);
                block[0] = youngerPart; 
                block[1] = olderPart;
            }
                
            for (int i = 0; i < countRound; i++)
            {
                if (i != 3)
                {
                    for (int j = 0; j < countKeys; j++) 
                    { 
                        Step(key[j][0]); 
                    }
                }
                else
                {
                    for (int j = countKeys - 1; j >= 0; j--) 
                    { 
                        Step(key[j][0]); 
                    }
                }
            }
            currentKey = key;
        }
        public override void DecodeBlock(Word[] block, IEnumerable<Word[]> currentKey)
        {
            var key = currentKey.ToList();
            void Step(Word key)
            {
                Word youngerPart = block[0];
                Word olderPart = block[1].XOR(youngerPart);

                #region RotRight() << 11
                var t = youngerPart.ToStringView16();
                string stringView = youngerPart.ToStringView2();
                for (int i = 0; i < 11; i++)
                {
                    stringView = RotRight(stringView.ToArray());
                }
                stringView = Convert.ToString((long)Convert.ToUInt64(stringView, 2), 16);
                while (stringView.Length < 8) 
                {
                    stringView = Convert.ToString(0) + stringView;
                }
                youngerPart = new Word
                {
                    Byte1 = Convert.ToByte(stringView[0..2], 16),
                    Byte2 = Convert.ToByte(stringView[2..4], 16),
                    Byte3 = Convert.ToByte(stringView[4..6], 16),
                    Byte4 = Convert.ToByte(stringView[6..8], 16)
                };
                #endregion

                #region SubBytes
                stringView = youngerPart.ToStringView16();
                string arrayS = string.Empty;
                for (int i = 0; i < stringView.Length; i++)
                {
                    byte res = InvSubBit(Convert.ToByte(stringView[i].ToString(), 16), i);
                    arrayS += Convert.ToString(res, 16);
                }
                youngerPart = new Word
                {
                    Byte1 = Convert.ToByte(arrayS[0..2], 16),
                    Byte2 = Convert.ToByte(arrayS[2..4], 16),
                    Byte3 = Convert.ToByte(arrayS[4..6], 16),
                    Byte4 = Convert.ToByte(arrayS[6..8], 16)
                };
                #endregion

                long insertKey = MathematicsGF.InversionPolynomialGF(key.ToLongView32(), 4299161607);
                youngerPart = youngerPart.MultGF(insertKey, 4299161607);
                block[0] = olderPart;
                block[1] = youngerPart;
            }
            for (int i = 0; i < countRound; i++)
            {
                if (i == 0)
                {
                    for (int j = 0; j < countKeys; j++) 
                    { 
                        Step(key[j][0]); 
                    }
                }
                else
                {
                    for (int j = countKeys - 1; j >= 0; j--) 
                    { 
                        Step(key[j][0]); 
                    }
                }
            }
            currentKey = key;
        }
        #endregion
    }
}