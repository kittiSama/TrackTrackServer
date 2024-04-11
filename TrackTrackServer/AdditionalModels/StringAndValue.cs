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
    public class StringAndValueComparer : IComparer<StringAndValue>
    {
        int IComparer<StringAndValue>.Compare(StringAndValue a, StringAndValue b)
        {
            if (a.Value > b.Value)
            {
                return 1;
            }
            if (a.Value < b.Value)
            {
                return -1;
            }
            if (a.Value == b.Value)
            {
                return 0;
            }
            throw (new ArgumentException("missing value"));

        }
    }
}
