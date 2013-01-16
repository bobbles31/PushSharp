namespace PushSharp.Apple.UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PendingItemsQueueTests
    {
        [TestMethod]
        public void QueuedItemShouldAppearInTheQueue()
        {
            var queue = new PendingItemsQueue();
            var sentNotification = new SentNotification(new AppleNotification());
            queue.QueueItem(sentNotification);
            Assert.IsTrue(queue.Contains(sentNotification));
        }
    }
}
