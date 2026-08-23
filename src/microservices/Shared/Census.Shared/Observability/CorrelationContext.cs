using System.Diagnostics;

namespace Census.Shared.Observability
{
    public static class CorrelationContext
    {
        private static readonly AsyncLocal<string?> CurrentCorrelationId = new();

        public static string? CorrelationId
        {
            get => CurrentCorrelationId.Value ?? Activity.Current?.TraceId.ToString();
            set => CurrentCorrelationId.Value = value;
        }

        public static string EnsureCorrelationId()
        {
            if (string.IsNullOrEmpty(CorrelationId))
            {
                CorrelationId = Guid.NewGuid().ToString("N");
            }

            return CorrelationId!;
        }
    }
}
