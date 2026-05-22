using System;

namespace Ddon.VitrinPLC.Models
{
    // ─────────────────────────────────────────────
    // 写入结果（设计原则4：写入仅发送，不保证生效）
    // ─────────────────────────────────────────────
    public sealed class WriteResult
    {
        public bool Success { get; init; }
        public string TagName { get; init; }
        public object Value { get; init; }
        public DateTime SentTime { get; init; }
        public bool NeedConfirmByScan { get; init; } = true;
        public string ErrorMessage { get; init; }
        public Exception Exception { get; init; }

        public static WriteResult Ok(string tagName, object value) => new()
        {
            Success = true,
            TagName = tagName,
            Value = value,
            SentTime = DateTime.UtcNow,
            NeedConfirmByScan = true
        };

        public static WriteResult Fail(string tagName, string message, Exception ex = null) => new()
        {
            Success = false,
            TagName = tagName,
            SentTime = DateTime.UtcNow,
            ErrorMessage = message,
            Exception = ex
        };

        public override string ToString() =>
            Success
                ? $"OK  | {TagName} = {Value} @ {SentTime:HH:mm:ss.fff} (待扫描确认)"
                : $"ERR | {TagName} : {ErrorMessage}";
    }
}
