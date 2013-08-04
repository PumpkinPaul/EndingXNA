using System;
using System.Collections.Generic;
using System.Text;

namespace flash
{
    /// <summary>
    /// Implementation of a Flash style Array.
    /// </summary>
    /// <remarks>
    /// The Array class lets you access and manipulate arrays Flash style arrays. 
    /// Array indices are zero-based, which means that the first element in the array is [0], the second element is [1], and so on. 
    /// To create an Array object, you use the new Array{T}() constructor. In addition, you can use the array access indexer ([]) operator to initialize an array or access the elements of an array.
    /// You can store a wide variety of data types in an array element, including numbers, strings, objects, and even other arrays. 
    /// You can create a multidimensional array by creating an indexed array and assigning to each of its elements a different indexed array. Such an array is considered multidimensional because it can be used to represent data in a table.
    ///
    /// Arrays are contiguous (there might be an element at index 0 and another at index 5, but nothing in the index positions between those two elements. In such a case, the elements in positions 1 through 4 are of default(T)).
    ///
    /// Array assignment is by reference or value depending on typeparam. When you assign one array variable to another array variable, both refer to the same array:
    /// var oneArray:Array = new Array("a", "b", "c");
    /// var twoArray:Array = oneArray;  // Both array variables refer to the same array.
    /// twoArray[0] = "z";             
    /// language.trace(oneArray);       // Output: z,b,c.
    ///
    /// Do not use the Array class to create associative arrays (also called hashes), which are data structures that contain named elements instead of numbered elements. 
    /// To create associative arrays, use the Dictionary{TKey, TValue} class. 
    /// NOTE: ActionScript permits you to create associative arrays using the Array class, you cannot use any of the Array class methods or properties with associative arrays.
    /// </remarks>
    /// <typeparam name="T">The data type the array contains.</typeparam>
    public class Array<T> : List<T> { 
    
        private readonly StringBuilder _sb = new StringBuilder();

        public Array() { }
        public Array(IEnumerable<T> collection) : base(collection) { }
        public Array(int capacity) : base(capacity) { }

        public override string ToString() {
            _sb.Length = 0;
            bool isFirst = true;
            foreach(var item in this) {
                if (!isFirst)
                    _sb.Append(',');
                _sb.Append(item.ToString());

                isFirst = false;
            }

            return _sb.ToString();
        }

        /// <summary>
        /// Executes a test function on each item in the array and constructs a new array for all items that return true for the specified function. 
        /// If an item returns false, it is not included in the new array.
        /// </summary>
        /// <param name="callback">The function to run on each item in the array. This function can contain a simple comparison (for example, item == 20) 
        /// or a more complex operation, and is invoked with three arguments; the value of an item, the index of an item, and the Array object</param>
        /// <param name="thisObject">An object to use as this for the function.</param>
        /// <returns>	Array — A new array that contains all items from the original array that returned true.</returns>
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

        /// <summary>
        /// An integer specifying the number of elements in the array.
        /// </summary>
        public int length { 
            get { return Count; } 
            set { 
            
                if (value == 0) 
                    Clear(); //?
                else
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Removes the last element from an array and returns the value of that element.
        /// </summary>
        /// <returns>The value of the last element (of any data type) in the specified array.</returns>
        public T pop() {
            var last = this[Count - 1];
            RemoveAt(Count - 1);

            return last;
        }

        /// <summary>
        /// Adds one or more elements to the end of an array and returns the new length of the array.
        /// </summary>
        /// <param name="item">The value to add to the array.</param>
        /// <returns>An integer representing the length of the new array.</returns>
        public int push(T item) {
            Add(item);

            return Count;
        }

        /// <summary>
        /// Removes the first element from an array and returns that element. The remaining array elements are moved from their original position, i, to i-1.
        /// </summary>
        /// <returns>The first element (of any data type) in an array.</returns>
        public T shift() {
            var first = this[0];
            RemoveAt(0);

            return first;
        }

        /// <summary>
        /// Returns a new array that consists of a range of elements from the original array, without modifying the original array. The returned array includes the startIndex element and all elements up to, but not including, the endIndex element.
        ///
        /// If you don't pass any parameters, the new array is a duplicate (shallow clone) of the original array.
        /// </summary>
        /// <param name="startIndex"> A number specifying the index of the starting point for the slice. If startIndex is a negative number, the starting point begins at the end of the array, where -1 is the last element.</param>
        /// <param name="endIndex">A number specifying the index of the ending point for the slice. If you omit this parameter, the slice includes all elements from the starting point to the end of the array. If endIndex is a negative number, the ending point is specified from the end of the array, where -1 is the last element.</param>
        /// <returns>An array that consists of a range of elements from the original array.</returns>
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
