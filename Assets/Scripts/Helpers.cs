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
    }
}