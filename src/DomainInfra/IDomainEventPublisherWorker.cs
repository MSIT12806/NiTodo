namespace DomainInfra
{
    /*
     * 說是用來延遲事件發布的，但是看不太懂
     */
    public interface IDomainEventPublisherWorker
    {
        /// <summary>
        /// 啟動事件發布邏輯
        /// </summary>
        void Start();

        /// <summary>
        /// 停止事件發布邏輯
        /// </summary>
        void Stop();
    }

}
