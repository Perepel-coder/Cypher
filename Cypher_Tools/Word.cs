using Mathematics;

namespace Cypher.Cypher_Tools
{
    public class Word
    {
        public int Byte1 { get; set; }
        public int Byte2 { get; set; }
        public int Byte3 { get; set; }
        public int Byte4 { get; set; }

        #region Конструкторы
        public Word()
        {
            Byte1 = 0x00;
            Byte2 = 0x00;
            Byte3 = 0x00;
            Byte4 = 0x00;
        }
        public Word(int b)
        {
            Byte1 = b;
            Byte2 = b;
            Byte3 = b;
            Byte4 = b;
        }
        public Word(int b1, int b2, int b3, int b4)
        {
            Byte1 = b1;
            Byte2 = b2;
            Byte3 = b3;
            Byte4 = b4;
        }
        public Word(IEnumerable<int> array, int i)
        {
            List<int> arr = new(array);
            Byte1 = arr[i];
            Byte2 = arr[i+1];
            Byte3 = arr[i+2];
            Byte4 = arr[i+3];
        }
        public Word(string str, int i)
        {
            Byte1 = (byte)str[i];
            Byte2 = (byte)str[i+1];
            Byte3 = (byte)str[i+2];
            Byte4 = (byte)str[i+3];
        }
        public Word(Word word)
        {
            Byte1 = word.Byte1;
            Byte2 = word.Byte2;
            Byte3 = word.Byte3;
            Byte4 = word.Byte4;
        }
        #endregion

        #region Замена байтов в слове
        public static byte SubBytes(byte myByte)
        {
            int[,] matrix = new int[,]
            {
                {1, 0, 0, 0, 1, 1, 1, 1 },
                {1, 1, 0, 0, 0, 1, 1, 1 },
                {1, 1, 1, 0, 0, 0, 1, 1 },
                {1, 1, 1, 1, 0, 0, 0, 1 },
                {1, 1, 1, 1, 1, 0, 0, 0 },
                {0, 1, 1, 1, 1, 1, 0, 0 },
                {0, 0, 1, 1, 1, 1, 1, 0 },
                {0, 0, 0, 1, 1, 1, 1, 1 }
            };
            int b = MathematicsGF.InversionPolynomialGF(myByte, 283);
            int[,] c = MathematicsGF.MultOfMatrixGF(matrix, MathematicsGF.CreatBITMatrixFromPoly(b, 8), 7);
            int[,] d = MathematicsGF.XorMatrix(c, MathematicsGF.CreatBITMatrixFromPoly(0x63, 8));
            byte result = (byte)MathematicsGF.CreatPolyFromBITMatrix(d);
            return result;
        }
        public static byte InvSubBytes(byte myByte)
        {
            int[,] matrix = new int[,]
            {
                {0, 0, 1, 0, 0, 1, 0, 1 },
                {1, 0, 0, 1, 0, 0, 1, 0 },
                {0, 1, 0, 0, 1, 0, 0, 1 },
                {1, 0, 1, 0, 0, 1, 0, 0 },
                {0, 1, 0, 1, 0, 0, 1, 0 },
                {0, 0, 1, 0, 1, 0, 0, 1 },
                {1, 0, 0, 1, 0, 1, 0, 0 },
                {0, 1, 0, 0, 1, 0, 1, 0 }
            };
            int[,] c = MathematicsGF.XorMatrix(MathematicsGF.CreatBITMatrixFromPoly(myByte, 8), MathematicsGF.CreatBITMatrixFromPoly(0x63, 8));
            int[,] b = MathematicsGF.MultOfMatrixGF(matrix, c, 7);
            byte result = (byte)MathematicsGF.InversionPolynomialGF(MathematicsGF.CreatPolyFromBITMatrix(b), 283);
            return result;
        }

        public Word SubWord()
        {
            return new()
            {
                Byte1 = SubBytes((byte)Byte1),
                Byte2 = SubBytes((byte)Byte2),
                Byte3 = SubBytes((byte)Byte3),
                Byte4 = SubBytes((byte)Byte4)
            };
        }
        public Word InvSubWord()
        {
            return new()
            {
                Byte1 = InvSubBytes((byte)Byte1),
                Byte2 = InvSubBytes((byte)Byte2),
                Byte3 = InvSubBytes((byte)Byte3),
                Byte4 = InvSubBytes((byte)Byte4)
            };
        }

        public void ChangeByte(int id, int value)
        {
            switch (id)
            {
                case 1: Byte1 = value; break;
                case 2: Byte2 = value; break;
                case 3: Byte3 = value; break;
                case 4: Byte4 = value; break;
            }
        }
        #endregion

        #region Сдвиг байтов в слове
        public Word RotWordLeft()
        {
            return new()
            {
                Byte1 = Byte2,
                Byte2 = Byte3,
                Byte3 = Byte4,
                Byte4 = Byte1
            };
        }
        public Word RotWordRight()
        {
            return new()
            {
                Byte1 = this.Byte4,
                Byte2 = this.Byte1,
                Byte3 = this.Byte2,
                Byte4 = this.Byte3
            };
        }
        #endregion

        public static int Count() { return 4; }

        public Word XOR(Word word2)
        {
            return new Word
            {
                Byte1 = Byte1 ^ word2.Byte1,
                Byte2 = Byte2 ^ word2.Byte2,
                Byte3 = Byte3 ^ word2.Byte3,
                Byte4 = Byte4 ^ word2.Byte4
            };
        }
        public Word XOR(IEnumerable<int> word)
        {
            int[] myWord = word.ToArray();
            return new Word
            {
                Byte1 = (Byte1 ^ myWord[0]),
                Byte2 = (Byte2 ^ myWord[1]),
                Byte3 = (Byte3 ^ myWord[2]),
                Byte4 = (Byte4 ^ myWord[3])
            };
        }
        public Word MultGF(Word word, long modPoly)
        {
            long word1 = Convert.ToUInt32(this.ToStringView16(), 16);
            long word2 = Convert.ToUInt32(word.ToStringView16(), 16);

            long multOfPolyGF = MathematicsGF.MultOfPolyGF(word1, word2, modPoly);

            string result = Convert.ToString(multOfPolyGF, 16);
            while (result.Length < 8) { result = Convert.ToString(0) + result; }
            return new()
            {
                Byte1 = Convert.ToByte(result[0..2], 16),
                Byte2 = Convert.ToByte(result[2..4], 16),
                Byte3 = Convert.ToByte(result[4..6], 16),
                Byte4 = Convert.ToByte(result[6..8], 16)
            };
        }
        public Word MultGF(long myByte, long modPoly)
        {
            long word1 = Convert.ToUInt32(this.ToStringView16(), 16);

            long multOfPolyGF = MathematicsGF.MultOfPolyGF(word1, myByte, modPoly);

            string result = Convert.ToString(multOfPolyGF, 16);
            while (result.Length < 8) { result = Convert.ToString(0) + result; }
            return new()
            {
                Byte1 = Convert.ToByte(result[0..2], 16),
                Byte2 = Convert.ToByte(result[2..4], 16),
                Byte3 = Convert.ToByte(result[4..6], 16),
                Byte4 = Convert.ToByte(result[6..8], 16)
            };
        }

        public int[] ToArray()
        {
            return new int[4]
            {
                Byte1,
                Byte2,
                Byte3,
                Byte4
            };
        }

        public ulong[] ToArrayLong()
        {
            return new ulong[4]
            {
                (ulong)Byte1,
                (ulong)Byte2,
                (ulong)Byte3,
                (ulong)Byte4
            };
        }

        public string ToStringView16()
        {
            static string Step(int myByte) 
            {
                string resultByte = Convert.ToString(myByte, 16);
                while (resultByte.Length < 2) { resultByte = "0" + resultByte; }
                return resultByte;
            }
            string result = Step(Byte1) + Step(Byte2) + Step(Byte3) + Step(Byte4);
            return result;
        }
        public long ToLongView32()
        {
            return Convert.ToUInt32(this.ToStringView16(), 16);
        }
        public string ToStringView2()
        {
            static string Step(int myByte)
            {
                string resultByte = Convert.ToString(myByte, 2);
                while (resultByte.Length < 8) { resultByte = "0" + resultByte; }
                return resultByte;
            }
            string result = Step(Byte1) + Step(Byte2) + Step(Byte3) + Step(Byte4);
            return result;
        }
        public override bool Equals(object? obj)
        {
            Word word = (Word)obj;
            if (word != null && this.Byte1 == word.Byte1 &&
                this.Byte2 == word.Byte2 &&
                this.Byte3 == word.Byte3 &&
                this.Byte4 == word.Byte4)
            {
                return true;
            }
            return false;
        }

        public static Word[] XorBlocks(Word[] block1, Word[] block2)
        {
            if(block1.Length != block2.Length)
            {
                throw new Exception("Различные величины блоков");
            }
            int size = block1.Length;
            Word[] result = new Word[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = block1[i].XOR(block2[i]);
            }
            return result;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
