// PriorityQueue.cs
using System;
using System.Collections.Generic;

namespace INStructed.Services
{
    /// <summary>
    /// Реализация приоритетной очереди на основе бинарной кучи.
    /// </summary>
    /// <typeparam name="TElement">Тип элементов в очереди.</typeparam>
    /// <typeparam name="TPriority">Тип приоритета элементов.</typeparam>
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        /// <summary>
        /// Список для хранения элементов кучи.
        /// </summary>
        private List<(TElement Element, TPriority Priority)> heap;

        /// <summary>
        /// Конструктор приоритетной очереди.
        /// </summary>
        public PriorityQueue()
        {
            heap = new List<(TElement, TPriority)>();
        }

        /// <summary>
        /// Добавляет элемент с указанным приоритетом в очередь.
        /// </summary>
        /// <param name="element">Элемент для добавления.</param>
        /// <param name="priority">Приоритет элемента.</param>
        public void Enqueue(TElement element, TPriority priority)
        {
            heap.Add((element, priority));
            HeapifyUp(heap.Count - 1);
        }

        /// <summary>
        /// Удаляет и возвращает элемент с наивысшим приоритетом (наименьшим значением).
        /// </summary>
        /// <returns>Элемент с наивысшим приоритетом.</returns>
        public TElement Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Очередь пуста.");

            TElement result = heap[0].Element;
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);
            HeapifyDown(0);
            return result;
        }

        /// <summary>
        /// Проверяет, пуста ли очередь.
        /// </summary>
        public bool IsEmpty => heap.Count == 0;

        /// <summary>
        /// Возвращает количество элементов в очереди.
        /// </summary>
        public int Count => heap.Count;

        /// <summary>
        /// Восстанавливает свойства кучи после добавления элемента.
        /// </summary>
        /// <param name="index">Индекс недавно добавленного элемента.</param>
        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (heap[index].Priority.CompareTo(heap[parent].Priority) >= 0)
                    break;

                Swap(index, parent);
                index = parent;
            }
        }

        /// <summary>
        /// Восстанавливает свойства кучи после удаления элемента.
        /// </summary>
        /// <param name="index">Индекс элемента для восстановления.</param>
        private void HeapifyDown(int index)
        {
            int lastIndex = heap.Count - 1;
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;

                if (left <= lastIndex && heap[left].Priority.CompareTo(heap[smallest].Priority) < 0)
                    smallest = left;

                if (right <= lastIndex && heap[right].Priority.CompareTo(heap[smallest].Priority) < 0)
                    smallest = right;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        /// <summary>
        /// Обменивает два элемента в куче.
        /// </summary>
        /// <param name="i">Индекс первого элемента.</param>
        /// <param name="j">Индекс второго элемента.</param>
        private void Swap(int i, int j)
        {
            var temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }
    }
}