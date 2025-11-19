using System;

namespace Ddon.Pipeline.Exceptions
{
    public class PipelineException : Exception
    {
        public int Index { get; set; }

        public PipelineDirection Direction { get; set; }

        public PipelineException(PipelineDirection direction, int index, Exception innerException) : base(string.Empty, innerException)
        {
            Index = index;
            Direction = direction;
        }

        public PipelineException(PipelineDirection direction, int index, string message, Exception innerException) : base(message, innerException)
        {
            Index = index;
            Direction = direction;
        }
    }

    public enum PipelineDirection
    {
        /// <summary>
        /// 前进
        /// </summary>
        Forward,
        /// <summary>
        /// 后退
        /// </summary>
        Backward
    }
}
