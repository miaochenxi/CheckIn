using System.Collections.Generic;

namespace CheckInWpf
{
    class FaceResult
    {
        public List<Results> Results { get; set; }
    }
    public class Results
    {
        public List<Candidates> Candidates { get; set; }
    }
    public class Candidates
    {
        public string PersonId { get; set; }
        public float Score { get; set; }
    }
}
