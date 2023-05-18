using Cypher.Cypher_Algorithms;

namespace Cypher.Cypher_Method
{
    public abstract class BaseMethod
    {
        public virtual string Name
        {
            get => "No realization";
        }
        protected BaseAlg alg;
        public BaseMethod(BaseAlg alg)
        {
            this.alg = alg;
        }
        public abstract IEnumerable<int> Encode();
        public abstract IEnumerable<int> Decode();
    }
}