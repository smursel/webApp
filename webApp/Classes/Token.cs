namespace webApp.Classes
{
    public class VerificationToken
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }

    public static class TokenStore
    {


        public static List<VerificationToken> Tokens = new List<VerificationToken>();

        public static string GenerateToken(string email)
        {
            string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            DateTime expiration = DateTime.UtcNow.AddHours(48);

            Tokens.Add(new VerificationToken
            {
                Email = email,
                Token = token,
                Expiration = expiration
            });

            return token;
        }

        public static async Task RemoveExpiredTokensLoop()
        {
            for (; ; )
            {
                Tokens.RemoveAll(t => t.Expiration < DateTime.UtcNow);

                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        public static VerificationToken? GetToken(string token)
        {
            Tokens.RemoveAll(t => t.Expiration < DateTime.UtcNow);

            return Tokens.Find(t => t.Token == token);
        }
    }


}