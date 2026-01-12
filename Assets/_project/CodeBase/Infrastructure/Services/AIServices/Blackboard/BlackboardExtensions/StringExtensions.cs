namespace CodeBase.Infrastructure.Services.AIServices.Blackboard.BlackboardExtensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Computes the FNV-1a hash for he input string
        /// The FNV-1a hash is a non-cryptographic hash function know for itsspeeed and good distribution properties.
        /// Useful for creating Dictionary keys instead of using strings
        /// </summary>
        /// <param _name="str">The inpus string to hash</param>
        /// <returns>An integer representing the FNV-1a hash of the input string</returns>
        public static int ComputeFNV1aHash(this string str)
        {
            uint hash = 2166136261;
            foreach (char c in str)
            {
                hash = (hash ^ c) * 16777619;
            }
            return unchecked((int)hash);
        }
    }
}
