namespace Zazz.Core
{
    public sealed class Keys
    {
        //Base64: BKZDhXECuJ4=

        /// <summary>
        /// This key should be used to sign passwords in DB, using SHA1 (HMACSHA1)
        /// </summary>
        public static byte[] PasswordHashSecret = { 4, 166, 67, 133, 113, 2, 184, 158 }; 
    }
}