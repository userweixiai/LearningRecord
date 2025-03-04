namespace LearningRecordWebApi
{
    public class ResponseContent
    {
        /// <summary>
        /// 状态
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 保存响应数据
        /// </summary>
        public object Data { get; set; }
    }
}
