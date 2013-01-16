namespace PushSharp.Apple
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PendingItemsQueue : Queue<SentNotification>
    {
        private readonly object lockObject = new object();

        public bool ItemsPending
        {
            get
            {
                return this.Any();
            }
        }

        public void QueueItem(SentNotification sentNotification)
        {
            lock (lockObject)
            {
                this.Enqueue(sentNotification);
            }
        }

        public IEnumerable<SentNotification> ExtractItemsSentBefore(DateTime dateTime)
        {
            lock (lockObject)
            {
                var succesfulItems = new List<SentNotification>();
                while (this.Count > 0 && this.Peek().SentAt < dateTime)
                {
                    succesfulItems.Add(this.Dequeue());
                }

                return succesfulItems;
            }
        }

        public AbortedQueueContents FailureAt(int identifier)
        {
            lock (lockObject)
            {
                var result = new AbortedQueueContents(this.DeQueueItemsUpTo(identifier), this.Dequeue(), this.ToArray());
                this.Clear();
                return result;
            }
        }

        private IEnumerable<SentNotification> DeQueueItemsUpTo(int identifier)
        {
            var succesfulItems = new List<SentNotification>();
            while (this.Count > 0)
            {
                var item = this.Peek();
                if (item.Identifier == identifier)
                {
                    break;
                }

                succesfulItems.Add(this.Dequeue());
            }

            return succesfulItems;
        }
    }
}