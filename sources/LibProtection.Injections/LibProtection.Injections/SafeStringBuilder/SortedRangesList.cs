﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LibProtection.Injections
{
    internal partial class SortedRangesList : IEnumerable<Range>
    {
        internal class Item
        {
            public Range Range;
            public Item Prev;
            public Item Next;

            public Item(Range range)
            {
                Range = range;
                Prev = null;
                Next = null;
            }

            public Item(Range range, Item prev, Item next)
            {
                Range = range;
                Prev = prev;
                Next = next;
            }

            public void Offset(int value)
            {
                Range = Range.Offset(value);
            }
        }

        internal Item Head;
        internal Item Tail;

        private void AddBefore(Item item, Range newRange)
        {
            if (item != null)
            {
                if (item.Prev != null)
                {
                    AddAfter(item.Prev, newRange);
                }
                else
                {
                    var newItem = new Item(newRange, null, item);
                    item.Prev = newItem;
                    Head = newItem;
                }
            }
            else
            {
                var newItem = new Item(newRange, null, null);
                Head = newItem;
                Tail = newItem;
            }
        }

        private void AddAfter(Item item, Range newRange)
        {
            if (item != null)
            {
                if (item.Range.UpperBound == newRange.LowerBound)
                {
                    item.Range = new Range(item.Range.LowerBound, newRange.UpperBound);
                    return;
                }

                var next = item.Next;
                var newItem = new Item(newRange, item, next);
                item.Next = newItem;
                if (next != null)
                {
                    next.Prev = newItem;
                }
                else
                {
                    Tail = newItem;
                }
            }
            else
            {
                var newItem = new Item(newRange, null, null);
                Head = newItem;
                Tail = newItem;
            }
        }

        public SortedRangesList()
        {
        }

        public void AddLast(Range range)
        {
            Item newItem;
            if (Head == null)
            {
                newItem = new Item(range);
                Head = newItem;
                Tail = newItem;
                return;
            }

            if (Tail.Range.UpperBound == range.LowerBound)
            {
                Tail.Range = new Range(Tail.Range.LowerBound, range.UpperBound);
                return;
            }

            newItem = new Item(range, Tail, null);
            Tail.Next = newItem;
            Tail = newItem;
        }

        public void Add(Range range)
        {
            Item newItem;
            if (Head == null)
            {
                newItem = new Item(range);
                Head = newItem;
                Tail = newItem;
                return;
            }

            var currentItem = Head;
            while (currentItem != null && currentItem.Range.LowerBound < range.LowerBound)
            {
                currentItem = currentItem.Next;
            }

            if (currentItem == null)
            {
                currentItem = Tail;
            }

            var nextItem = currentItem.Next;
            bool glued = false;

            if (currentItem.Range.UpperBound == range.LowerBound)
            {
                currentItem.Range = new Range(currentItem.Range.LowerBound, range.UpperBound);
                glued = true;

                if (nextItem != null)
                {
                    if (currentItem.Range.UpperBound == nextItem.Range.LowerBound)
                    {
                        currentItem.Range = new Range(currentItem.Range.LowerBound, nextItem.Range.UpperBound);
                        var newNextItem = nextItem.Next;
                        currentItem.Next = newNextItem;

                        if (newNextItem == null)
                        {
                            Tail = currentItem;
                        }
                        else
                        {
                            newNextItem.Prev = currentItem;
                        }
                    }
                }
            }

            if (nextItem != null)
            {
                if (range.UpperBound == nextItem.Range.LowerBound)
                {
                    nextItem.Range = new Range(range.LowerBound, nextItem.Range.UpperBound);
                    glued = true;
                }
            }

            if (!glued)
            {
                newItem = new Item(range, currentItem, nextItem);
                currentItem.Next = newItem;
                if (nextItem == null)
                {
                    Tail = newItem;
                }
                else
                {
                    nextItem.Prev = newItem;
                }
            }

            currentItem = nextItem;
            var offset = range.Length;

            while (currentItem != null)
            {
                currentItem.Offset(offset);
                currentItem = currentItem.Next;
            }
        }

        public void CutOff(int value)
        {
            var currentItem = Head;

            while (currentItem != null)
            {
                if (currentItem.Range.UpperBound > value)
                {
                    currentItem.Range = new Range(currentItem.Range.LowerBound, value);

                    if (currentItem.Range.Length == 0)
                    {
                        if (currentItem.Prev != null)
                        {
                            currentItem.Prev.Next = null;
                            Tail = currentItem.Prev;
                        }
                        else
                        {
                            Tail = null;
                            Head = null;
                        }
                    }
                    else
                    {
                        currentItem.Next = null;
                        Tail = currentItem;
                    }
                }
                currentItem = currentItem.Next;
            }
        }

        internal void RemoveItem(Item item)
        {
            var prevItem = item.Prev;
            var nextItem = item.Next;
            if (prevItem != null)
            {
                prevItem.Next = nextItem;
            }
            else
            {
                Head = nextItem;
            }
            if (nextItem != null)
            {
                nextItem.Prev = prevItem;
            }
            else
            {
                Tail = prevItem;
            }
        }

        public void Remove(Range range)
        {
            if (range.Length == 0) return;

            Item currentItem = Head;
            int offset = -range.Length;
            while (currentItem != null)
            {
                if (currentItem.Range.Overlaps(range))
                {
                    var split = currentItem.Range.TrySubstract(range, out var modifiedRange, out var newRange);
                    currentItem.Range = modifiedRange;

                    if (split)
                    {
                        AddAfter(currentItem, newRange);
                    }

                    if (currentItem.Range.Length == 0)
                    {
                        RemoveItem(currentItem);
                    }

                    if (split)
                    {
                        currentItem = currentItem.Next;
                        currentItem.Offset(offset);
                        currentItem = currentItem.Next;
                    }
                    else
                    {
                        currentItem = currentItem.Next;
                    }
                }
                else
                {
                    if (currentItem.Range.LowerBound > range.UpperBound)
                    {
                        currentItem.Offset(offset);
                    }
                    currentItem = currentItem.Next;
                }
            }
        }

        internal void SafeInsert(Range newRange)
        {
            if (newRange.Length == 0) return;

            var currentItem = Head;
            int offset = newRange.Length;

            //Search for the first range after insert position
            while (currentItem != null && currentItem.Range.LowerBound >= newRange.LowerBound)
            {
                currentItem = currentItem.Next;
            }

            if (currentItem == null)
            {
                AddLast(newRange);
                return;
            }

            if (currentItem.Range.Touches(newRange))
            {
                currentItem.Range = newRange.ConvexHull(currentItem.Range);
                currentItem = currentItem.Next;
            }
            else
            {
                if (currentItem.Next != null && currentItem.Next.Range.Touches(newRange.LowerBound))
                {
                    currentItem.Next.Range = newRange.ConvexHull(currentItem.Next.Range);
                    currentItem = currentItem.Next.Next;
                }
                else
                {
                    AddAfter(currentItem, newRange);
                }
            }

            //Offset the rest of the ranges
            while (currentItem != null)
            {
                currentItem.Offset(offset);
                currentItem = currentItem.Next;
            }
        }

        internal void UncheckedInsert(Range newRange)
        {
            if (newRange.Length == 0) return;

            var currentItem = Head;
            int offset = newRange.Length;

            while (currentItem != null)
            {
                if (currentItem.Range.Contains(newRange.LowerBound))
                {
                    AddAfter(currentItem, new Range(newRange.UpperBound, currentItem.Range.UpperBound));
                    currentItem.Range = new Range(currentItem.Range.LowerBound, newRange.LowerBound);
                }
                else
                {
                    if (currentItem.Range.LowerBound > newRange.LowerBound)
                    {
                        currentItem.Offset(offset);
                    }
                }
                currentItem = currentItem.Next;
            }
        }

        internal void Clear()
        {
            Head = null;
            Tail = null;
        }

        public IEnumerator<Range> GetEnumerator()
        {
            var currentItem = Head;

            while (currentItem != null)
            {
                yield return currentItem.Range;
                currentItem = currentItem.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public List<Range> ToList()
        {
            return new List<Range>(this);
        }

        internal void Replace(string currentString, string oldValue, string newValue, int startIndex, int count, bool unChecked = false)
        {
            new Replacer(currentString, this, unChecked).Replace(oldValue, newValue, startIndex, count);
        }
    }
}
