namespace PushSharp.Apple
{
    using System.Collections.Generic;

    public class AbortedQueueContents
    {
        public AbortedQueueContents(IEnumerable<SentNotification> completedItems, SentNotification failedNotification, IEnumerable<SentNotification> itemsIgnoredByApple)
        {
            this.ItemsIgnoredByApple = itemsIgnoredByApple;
            this.FailedNotification = failedNotification;
            this.CompletedItems = completedItems;
        }

        public IEnumerable<SentNotification> CompletedItems { get; private set; }

        public SentNotification FailedNotification { get; private set; }

        public IEnumerable<SentNotification> ItemsIgnoredByApple { get; private set; }
    }
}