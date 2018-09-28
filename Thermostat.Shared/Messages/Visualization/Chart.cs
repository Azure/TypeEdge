namespace ThermostatApplication.Messages.Visualization
{
    public class Chart
    {
        public string Name { get; set; }
        public string X_Label { get; set; }
        public string Y_Label { get; set; }
        public string[] Headers { get; set; }
        public bool Append { get; set; }
    }
}