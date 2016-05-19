namespace Payroll.Infrastructure
{
    public interface ILogger
    {
        void Trace(string logger, string message);
        void Debug(string logger, string message);
        void Info(string logger, string message);
        void Warn(string logger, string message);
        void Error(string logger, string message);
        void Fatal(string logger, string message);
    }
}
