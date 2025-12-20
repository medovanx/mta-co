namespace System.Collections.Generic
{
    public class SafeDictionary<T, T2> : Dictionary<T, T2>
    {
        public SafeDictionary()
        {

        }
        public SafeDictionary(int nulledNumber)
        {
        }

        public void Add(T key, T2 value)
        {
            base[key] = value;
        }

        public T2 this[T key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return base[key];
                }
                return default(T2);
            }
            set
            {
                base[key] = value;
            }
        }
    }
}

