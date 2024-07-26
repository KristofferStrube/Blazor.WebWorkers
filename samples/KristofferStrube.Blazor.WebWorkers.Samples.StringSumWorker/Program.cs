namespace KristofferStrube.Blazor.WebWorkers.Samples.StringSumWorker
{

    /// <summary>
    /// 
    /// </summary>
    public class Program
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static async Task Main(string[] args)
        {


            if (!OperatingSystem.IsBrowser())
                throw new PlatformNotSupportedException("Can only be run in the browser!");

            await new StringSumJob().StartAsync();

        }

    }

}