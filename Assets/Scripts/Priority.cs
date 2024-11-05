using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<KeyValuePair<T, int>> elements = new List<KeyValuePair<T, int>>();

    // Number of items in the priority queue
    public int Count => elements.Count;

    // Enqueue an element with a priority
    public void Enqueue(T item, int priority)
    {
        elements.Add(new KeyValuePair<T, int>(item, priority));
    }

    // Dequeue the element with the lowest priority
    public T Dequeue()
    {
        int bestIndex = 0;

        // Find the element with the lowest priority
        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].Value < elements[bestIndex].Value)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Key;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    // Check if the queue contains a specific item
    public bool Contains(T item)
    {
        foreach (var element in elements)
        {
            if (EqualityComparer<T>.Default.Equals(element.Key, item))
            {
                return true;
            }
        }
        return false;
    }
}
