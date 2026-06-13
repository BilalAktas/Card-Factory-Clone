using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public static class Helpers
    {
        public static async Task WaitForSecondsUnity(float seconds)
        {
            var endTime = Time.time + seconds;

            while (Time.time < endTime)
                await Task.Yield();
        }
        
        private static string[] _suffix = { "", "K", "M", "B", "T", "Q", "QU", "S" };

        public static string ConvertToKBM(float value)
        {
            if (value < 1000)
                return value.ToString();

            var count = 0;
            while (value >= 1000f)
            {
                count++;
                value /= 1000f;
            }

            return value < .01f && value != 0 ? $"{value:0.000}{_suffix[count]}" : $"{value:F}{_suffix[count]}";
        }
    }
}