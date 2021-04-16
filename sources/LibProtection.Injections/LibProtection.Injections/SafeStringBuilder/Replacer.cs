using System;
using System.Collections.Generic;

namespace LibProtection.Injections
{
    internal partial class SortedRangesList : IEnumerable<Range>
    {
        internal class Replacer
        {
            private readonly SortedRangesList list;
            private int index;
            private int count;
            private int offsetStep;
            private int offsetValue;
            private readonly string currentString;
            private string oldValue;
            private string newValue;
            private Range rangeToBeReplaced;
            private Range newRange;
            private Item currentItem;
            private Item firstItem;

            public Replacer(string currentString, SortedRangesList list)
            {
                this.currentString = currentString;
                this.list = list;
            }

            public Replacer(string currentString)
            {
                this.currentString = currentString;
            }

            public void Replace(string oldValue, string newValue, int startIndex, int count)
            {
                this.oldValue = oldValue;
                this.newValue = newValue;
                index = startIndex;
                this.count = count;
                offsetStep = newValue.Length - oldValue.Length;
                offsetValue = 0;
                StartReplacement();
            }

            bool TryFindNextRange()
            {
                var oldIndex = index;
                index = currentString.IndexOf(oldValue, startIndex: index, count: count);

                if (index == -1)
                {
                    rangeToBeReplaced = new Range();
                    newRange = new Range();
                    return false;
                }

                rangeToBeReplaced = new Range(index, index + oldValue.Length);
                newRange = new Range(index, index + newValue.Length);
                index += oldValue.Length;
                count -= index - oldIndex;
                return true;
            }

            void PerformReplacment()
            {
                //At this point firstItem.Range contains convex hull of all ranges touching rangeToBeReplaced
                //So all we need to do is to calculate new bounds for it.

                var range = firstItem.Range;
                var offset = newRange.Length - rangeToBeReplaced.Length;
                //LowerBound is simple
                var newLowerBounnd = Math.Min(range.LowerBound, rangeToBeReplaced.LowerBound);
                int newUpperBound;
                if (range.UpperBound < rangeToBeReplaced.UpperBound)
                {
                    if (offset > 0)
                    {
                        newUpperBound = rangeToBeReplaced.UpperBound + offset;
                    }
                    else
                    {
                        newUpperBound = range.UpperBound + offset;
                    }
                }
                else
                {
                    newUpperBound = range.UpperBound + offset;
                }

                firstItem.Range = new Range(newLowerBounnd, newUpperBound);
                //Offset based on the previously replaced ranges
                firstItem.Offset(offsetValue);

                //If range collapsed after replacement, remove it from the list
                if (firstItem.Range.Length == 0)
                {
                    list.RemoveItem(firstItem);
                }
                offsetValue += offsetStep;
            }

            bool ShouldSwitchState()
            {
                return currentItem.Range.Touches(rangeToBeReplaced);
            }

            void AddSkippedRange()
            {
                list.AddBefore(currentItem, newRange.Offset(offsetValue));
            }

            private void StartReplacement()
            {
                currentItem = list.Head;
                firstItem = null;

                bool betweenRR = true;
                bool nextRRExists;

                nextRRExists = TryFindNextRange();

                while (currentItem != null)
                {
                    if (nextRRExists) //There is another range to be replaced
                    {
                        if (betweenRR) //We haven't reached it yet
                        {
                            if (ShouldSwitchState())//We did reach it
                            {
                                firstItem = currentItem;
                                betweenRR = false;
                            }
                            else //We still haven't reached it
                            {
                                //if we stepped over rangeToBeReplaced completly
                                if (currentItem.Range.LowerBound >= rangeToBeReplaced.UpperBound)
                                {
                                    AddSkippedRange();
                                    offsetValue += offsetStep;
                                    nextRRExists = TryFindNextRange();
                                }
                                currentItem.Offset(offsetValue);
                            }
                            currentItem = currentItem.Next;
                        }
                        else //We previously found a range that touches rangeToBeReplaced
                        {
                            if (ShouldSwitchState()) //still touching
                            {
                                firstItem.Range = currentItem.Range.ConvexHull(firstItem.Range);
                                firstItem.Next = currentItem.Next;
                                if (currentItem.Next != null)
                                {
                                    currentItem.Next.Prev = firstItem;
                                }
                                else
                                {
                                    list.Tail = firstItem;
                                }
                                currentItem = currentItem.Next;
                            }
                            else //current range no longer touches rangeToBeReplaced
                            {
                                betweenRR = true;

                                PerformReplacment();

                                //Check if we replaced the space between two ranges
                                if (firstItem.Prev != null && firstItem.Range.Touches(firstItem.Prev.Range))
                                {
                                    var prevItem = firstItem.Prev;
                                    prevItem.Range = prevItem.Range.ConvexHull(firstItem.Range);
                                    prevItem.Next = firstItem.Next;
                                    if (prevItem.Next != null)
                                    {
                                        prevItem.Next.Prev = prevItem;
                                    }
                                    else
                                    {
                                        list.Tail = prevItem;
                                    }
                                }

                                nextRRExists = TryFindNextRange();
                            }
                        }
                    }
                    else //After processing all rangesToReplace, we only need to offset the of the ranges
                    {
                        currentItem.Offset(offsetValue);
                        currentItem = currentItem.Next;
                    }
                }

                if (!betweenRR && firstItem != null)
                {
                    PerformReplacment();
                    nextRRExists = TryFindNextRange();
                }

                while (nextRRExists && newRange.Length != 0)
                {
                    list.AddLast(newRange.Offset(offsetValue));
                    offsetValue += offsetStep;
                    nextRRExists = TryFindNextRange();
                }

            }
        }
    }
}
