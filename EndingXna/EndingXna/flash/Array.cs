using System;
using System.Collections.Generic;
using System.Text;

namespace flash
{
    public class Array<T> : List<T> { 
    
        private readonly StringBuilder _sb = new StringBuilder();

        public Array() { }
        public Array(IEnumerable<T> collection) : base(collection) { }
        public Array(int capacity) : base(capacity) { }

        public T this[string index] 
        { 
            get { return default(T); }
            set {}
        }

        public override string ToString() {
            _sb.Clear();
            bool isFirst = true;
            foreach(var item in this) {
                if (!isFirst)
                    _sb.Append(',');
                _sb.Append(item.ToString());

                isFirst = false;
            }

            return _sb.ToString();
        }

        public Array<T> filter(Func<T, int, Array<T>, Boolean> callback, Array<T> thisObject = null) {
            var a = new Array<T>();

            var arrayToIterate = thisObject ?? this;

            int index = 0;
            foreach(var item in arrayToIterate) {
                if (callback(item, index, arrayToIterate))
                    a.Add(item);

                index++;
            }

            return a;
        }

        public int length { 
            get { return Count; } 
            set { 
            
                if (value == 0) 
                    Clear(); //?
                else
                    throw new NotImplementedException();
            }
        }

        public T pop() {
            var last = this[Count - 1];
            RemoveAt(Count - 1);

            return last;
        }

        public void push(T item) {
            Add(item);
        }

        public T shift() {
            var first = this[0];
            RemoveAt(0);

            return first;
        }

        public Array<T> slice(int startIndex = 0, int endIndex = 16777215) {
            int start = startIndex >= 0 ? startIndex : Count + startIndex;
            int end = endIndex == 16777215 ? Count : endIndex >= 0 ? endIndex : Count + endIndex;

            var array = new Array<T>(end - start);

            for(int i = start; i < end; i++) {
                array.Add(this[i]);
            }

            return array;
        }

        /// <summary>
        /// Adds elements to and removes elements from an array. This method modifies the array without making a copy.
        /// </summary>
        /// <param name="startIndex">An integer that specifies the index of the element in the array where the insertion or deletion begins. You can use a negative integer to specify a position relative to the end of the array (for example, -1 is the last element of the array).</param>
        /// <param name="deleteCount">An integer that specifies the number of elements to be deleted. This number includes the element specified in the startIndex parameter. If you do not specify a value for the deleteCount parameter, the method deletes all of the values from the startIndex element to the last element in the array. If the value is 0, no elements are deleted</param>
        /// <param name="p">An optional list of one or more comma-separated values to insert into the array at the position specified in the startIndex parameter. If an inserted value is of type Array, the array is kept intact and inserted as a single element. For example, if you splice an existing array of length three with another array of length three, the resulting array will have only four elements. One of the elements, however, will be an array of length three</param>
        /// <returns>An array containing the elements that were removed from the original array.</returns>
        public Array<T> splice(int startIndex, uint deleteCount, params T[] p) {
            var array = new Array<T>();
            for(int i = 0; i < deleteCount; i++) {
                array.push(this[startIndex + i]);
            }

            this.RemoveRange(startIndex, (int)deleteCount);

            if (p != null) {
                InsertRange(startIndex, p);    
            }

            return array;
        }

        /// <summary>
        /// Adds one or more elements to the beginning of an array and returns the new length of the array.
        /// </summary>
        /// <param name="args">One or more numbers, elements, or variables to be inserted at the beginning of the array.</param>
        /// <returns>An integer representing the new length of the array.</returns>
        public uint unshift(params T[] args) {
            InsertRange(0, args);

            return (uint)Count;
        }
    }

    public class Array : Array<object> {}
}
