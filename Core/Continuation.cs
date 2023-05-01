namespace Skyward.Skygrate.Core
{
    public static class Resolve
    {
        public static Continuation<T> With<T>(T value) => new Continuation<T>(null, null, true, value, new List<Option<T>>());

        public static async Task<Continuation<T>> After<T, K>(Continuation<K> value, Func<K, Task<Continuation<T>>> resume)
        {
            var cursor = value;
            if (!cursor.Resolved)
            {
                return new Continuation<T>(
                    value.Name, 
                    value.Description, 
                    false, 
                    default(T), 
                    value.Options.Select(o => 
                        new Option<T>(o.Name, o.Description, async () => await Resolve.After(await o.Resolver(), resume))
                    ).ToList());
            }
            return await resume(cursor.Value!);
        }
    }

    public record Continuation<T>(string? Name, string? Description, bool Resolved, T Value, List<Option<T>> Options);

    public record Option<T>(string Name, string Description, Func<Task<Continuation<T>>> Resolver);
}
