namespace TrackTrackServer.AdditionalModels
{
    public class StringAndValue
    {
        public string String {  get; set; }
        public int Value { get; set; }
        public StringAndValue(string s, int v)
        {
            String = s;
            Value = v;
        }
    }
}
