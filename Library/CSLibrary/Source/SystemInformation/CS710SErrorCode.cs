using System.Collections.Generic;

namespace CSLibrary
{
    internal class ErrorCode
    {
        public int Code { get; set; }
        public string Description { get; set; }

        public ErrorCode(int code, string description)
        {
            Code = code;
            Description = description;
        }
    }

    public static class CS710SErrorCodes
    {
        private static List<ErrorCode> ErrorCodes;

        static CS710SErrorCodes()
        {
            ErrorCodes = new List<ErrorCode>
            {
                new ErrorCode(0x0001, "Tag cache table buffer is overflowed."),
                new ErrorCode(0x0002, "Wrong register address"),
                new ErrorCode(0x0003, "Register length too large"),
                new ErrorCode(0x0004, "E710 not powered up"),
                new ErrorCode(0x0005, "Invalid parameter"),
                new ErrorCode(0x0006, "Event fifo full"),
                new ErrorCode(0x0007, "TX not ramped up"),
                new ErrorCode(0x0008, "Register read only"),
                new ErrorCode(0x0009, "Failed to halt"),
                new ErrorCode(0x000A, "PLL not locked"),
                new ErrorCode(0x000B, "Power control target failed"),
                new ErrorCode(0x000C, "Radio power not enabled"),
                new ErrorCode(0x000D, "E710 command error"),
                new ErrorCode(0x000E, "E710 Op timeout"),
                new ErrorCode(0x000F, "E710 Aggregate error (e.g. battery low, antenna disconnected, metal reflection)"),
                new ErrorCode(0x0010, "E710 hardware link error"),
                new ErrorCode(0x0011, "E710 event fail to send error"),
                new ErrorCode(0x0012, "E710 antenna error (e.g. antenna disconnected, metal reflection)"),
                new ErrorCode(0x0FFF, "Other error")
            };
        }

        public static string GetErrorDescription(int errorCode)
        {
            var error = ErrorCodes.Find(e => e.Code == errorCode);
            return error?.Description ?? "unknow error";
        }
    }
}
