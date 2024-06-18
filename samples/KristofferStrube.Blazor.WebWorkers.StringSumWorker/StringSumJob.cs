namespace KristofferStrube.Blazor.WebWorkers.StringSumWorker;

public class StringSumJob : JSONJob<string, int>
{
    public override int Work(string input)
    {
        int result = 0;
        for (int i = 0; i < input.Length; i++)
        {
            result += input[i];
        }
        return result;
    }
}
