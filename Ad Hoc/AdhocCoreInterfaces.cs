using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prelude
{
    public sealed class AdHocComparable<T> : IComparable<T>
    {
        readonly Func<T, int> compare_to;

        public AdHocComparable(Func<T, int> compare_to)
        {
            this.compare_to = compare_to;
        }

        public int CompareTo(T other)
        {
            return compare_to(other);
        }
    }

    public sealed class AdHocComparer<T> : IComparer<T>
    {
        readonly Func<T, T, int> comparer;

        public AdHocComparer(Func<T, T, int> comparer)
        {
            this.comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return comparer(x, y);
        }
    }

    public sealed class AdHocEquatable<T> : IEquatable<T>
    {
        readonly Func<T, bool> equals;

        public AdHocEquatable(Func<T, bool> equals)
        {
            this.equals = equals;
        }

        public bool Equals(T other)
        {
            return equals(other);
        }
    }

    public abstract class AdHocIEnumerable<T> : IEnumerable<T>
    {
        public abstract IEnumerator<T> GetEnumerator();
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static IEnumerable<T> Create(Func<IEnumerator<T>> getEnumerator)
        {
            return new Default_AdHocEnumerable(getEnumerator);
        }

        public static IEnumerable<T> Create(IEnumerator<T> enumerator)
        {
            return new Default_AdHocEnumerable(() => enumerator);
        }

        sealed class Default_AdHocEnumerable : AdHocIEnumerable<T>
        {
            readonly Func<IEnumerator<T>> getEnumerator;
            
            public Default_AdHocEnumerable(Func<IEnumerator<T>> getEnumerator)
            {
                this.getEnumerator = getEnumerator;
            }

            public override IEnumerator<T> GetEnumerator()
            {
                return getEnumerator();
            }
        }
    }
}
