namespace McTools.Xrm.Connection
{
    public class Solution
    {
        public SolutionDetail SolutionDetail { get; set; }

        public override string ToString()
        {
            return SolutionDetail.FriendlyName;
        }
    }
}
