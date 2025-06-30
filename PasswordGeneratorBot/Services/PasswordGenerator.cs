using System.Security.Cryptography;
using System.Text;

namespace PasswordGeneratorBot.Services
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private readonly RandomNumberGenerator _rng;
        private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string SpecialChars = "!$#@%&?";

        public PasswordGenerator()
        {
            _rng = RandomNumberGenerator.Create();
        }

        public async Task<string> GeneratePasswordAsync()
        {
            return await Task.Run(() =>
            {
                var password = new StringBuilder();


                password.Append(GetRandomChar(UpperCase));
                password.Append(GetRandomChar(Digits));
                password.Append(GetRandomChar(SpecialChars));


                var allChars = LowerCase + UpperCase + Digits + SpecialChars;
                for (int i = 0; i < 12; i++) 
                {
                    password.Append(GetRandomChar(allChars));
                }


                return ShuffleString(password.ToString());
            });
        }

        private char GetRandomChar(string chars)
        {
            byte[] randomNumber = new byte[1];
            _rng.GetBytes(randomNumber);
            return chars[randomNumber[0] % chars.Length];
        }

        private string ShuffleString(string input)
        {
            var arr = input.ToCharArray();
            byte[] randomNumbers = new byte[arr.Length];
            _rng.GetBytes(randomNumbers);

            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = randomNumbers[i] % (i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }

            return new string(arr);
        }
    }
}
