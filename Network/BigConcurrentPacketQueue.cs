using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Network
{
    public class BigConcurrentPacketQueue
    {
        const int capacity = ushort.MaxValue - 1;//8191
        private byte[] queue = new byte[capacity + 1];
        private int enqueuePointer = 0, dequeuePointer = 0, enqueuedData = 0,
                    currentPacketSize = -1, tqPadding;
        private object syncRoot;

        public BigConcurrentPacketQueue(int padding = 8)
        {
            tqPadding = padding;
            syncRoot = new object();
        }

        public int CurrentLength { get { reviewPacketSize(); return currentPacketSize; } }

        public void Enqueue(byte[] buffer, int length)
        {
            lock (syncRoot)
            {
                for (int i = 0; i < length; i++, enqueuePointer++)
                    queue[enqueuePointer] = buffer[i];
                enqueuedData += length;
            }
        }

        private void reviewPacketSize()
        {
            lock (syncRoot)
            {
                if (enqueuedData < 2)
                    currentPacketSize = -1;
                else
                    currentPacketSize = ((queue[dequeuePointer] | (queue[(dequeuePointer + 1)] << 8)) + tqPadding);

            }
        }

        public bool CanDequeue()
        {
            reviewPacketSize();
            if (currentPacketSize == -1) return false;
            return (enqueuedData >= currentPacketSize);
        }

        public byte[] Dequeue()
        {
            lock (syncRoot)
            {
                if (currentPacketSize == -1 || enqueuedData < currentPacketSize)
                    throw new OperationCanceledException("Before calling Dequeue(), always call CanDequeue()!");
                byte[] array = new byte[currentPacketSize];
                for (int i = 0; i < currentPacketSize; i++, dequeuePointer++)
                    array[i] = queue[dequeuePointer];
                enqueuedData -= currentPacketSize;
                reviewPacketSize();
                return array;
            }
        }

    }
}
