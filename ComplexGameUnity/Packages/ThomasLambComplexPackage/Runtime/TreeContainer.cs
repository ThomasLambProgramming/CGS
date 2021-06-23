using System;

/*
* File: TreeContainer.cs
*
* Author: Thomas Lamb (s200498@students.aie.edu.au)
* Date Created: 10th May 2021
* Date Last Modified: 12th June 2021
*
* This is a binary tree / heap container 
* Just used to have a more efficient way to sort 
* data to always have the lowest value at the front for pathfinding
 * 
*/
//this is a tree like container for pathfinding, its purpose is to have the nodes sorted by cost
//rather than the original version of pathfinding where it cycled through the openset 
//most of this is copied from a tutorial as there wasnt much i could change without breaking the implementation
//it was hand copied not ctrl c and i added comments to show that i do know what its doing
public class Heap<T> where T : IHeapItem<T>
{
    public T[] items;
    //keeps current "used" amount in the array
    int currentCount;
    public Heap(int a_maxSize)
    {
        items = new T[a_maxSize];
    }
    public void Add(T item)
    {
        //makes the index = current count (count = last index + 1 so no need to do checks)
        item.ItemIndex = currentCount;
        //as we tell the heap the max size out of range checks arent needed as it means the size was incorrectly made
        items[currentCount] = item;
        //sort the item to correct position
        SortUp(item);
        currentCount++;
    }
    public bool Contains(T a_item)
    {
        //as the items are forced to have the index in them then its index should equal itself (if not its not there)
        return Equals(items[a_item.ItemIndex], a_item);
    }
    public int Count
    {
        get { return currentCount; }
    }
    //this is for the pathfinding when a node is checked with a different parent the fcost might change and it needs
    //to be updated
    public void UpdateItem(T a_item)
    {
        SortUp(a_item);
    }
    public T RemoveFirst()
    {
        T rootItem = items[0];
        currentCount--;
        //make the last item the root item
        items[0] = items[currentCount];
        items[0].ItemIndex = 0;
        //sort the root down to fill in the removal of the first
        SortDown(items[0]);
        return rootItem;
    }
    public void SortDown(T a_item)
    {
        while (true)
        {
            //as this is a sort of binary tree the nodes are always twos (i dont know if there are edge cases it jsut seems to work)

            int childLeftIndex = a_item.ItemIndex * 2 + 1;
            int childRightIndex = a_item.ItemIndex * 2 + 2;
            int swapIndex;

            //if the index is in range
            if (childLeftIndex < currentCount)
            {
                swapIndex = childLeftIndex;

                //if the right index is also in range of the array
                if (childRightIndex < currentCount)
                {
                    //if the left index has lower priority then right then we set right
                    if (items[childLeftIndex].CompareTo(items[childRightIndex]) < 0)
                    {
                        swapIndex = childRightIndex;
                    }
                }
                //if our item has a lower prio then the child then we swap
                if (a_item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(a_item, items[swapIndex]);
                }
                else
                    return;
            }
            //as we always add and remove to the left if the index is out of range on the left then
            //we know there is no children of this node and can exit out as it is already sorted down
            else
                return;
        }
    }
    public void SortUp(T a_item)
    {
        //same as the sortdown the structure of this tree makes it so parent = (n-1) /2 to get index
        int parentIndex = (a_item.ItemIndex - 1) / 2;
        while (true)
        {
            T parent = items[parentIndex];
            //if the item has a higher priority then swap else its in the right position
            if (a_item.CompareTo(parent) > 0)
                Swap(a_item, parent);
            else
                break;

            parentIndex = (a_item.ItemIndex - 1) / 2;
        }
    }
    public void Swap(T item1, T item2)
    {
        items[item1.ItemIndex] = item2;
        items[item2.ItemIndex] = item1;

        //make a small buffer so it doesnt overwrite
        int item1Index = item1.ItemIndex;
        item1.ItemIndex = item2.ItemIndex;
        item2.ItemIndex = item1Index;
    }
}
public interface IHeapItem<T> : IComparable<T>
{
    int ItemIndex { get; set; }
}