namespace CRMS_API.Domain.DTOs
{
    public class DashboardChartDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> DataPoints { get; set; } = new();
    }
}