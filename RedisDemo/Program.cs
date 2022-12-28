using FreeRedis;
using Newtonsoft.Json;

class Program
{
    const int REDIS_DEF_EXP = 60;
    const string REDIS_KEY_ALBUMS = "albums";
    const string MODE_XML = "xml";

    public static async Task Main(string[] args)
    {
        var redisKey = args.ElementAtOrDefault(0) ?? REDIS_KEY_ALBUMS;
        var mode = args.ElementAtOrDefault(1);
        
        var albums = await GetAndOrSetFromRedis(redisKey, async () => 
        {
            HttpClient client = new();
            var res = await client.GetStringAsync("https://jsonplaceholder.typicode.com/posts/1");


            if (string.Equals(mode, MODE_XML))
            {
                res = JsonConvert.DeserializeXNode(res, "Root")?.ToString();
            }

            return res!;
        });

        System.Console.WriteLine(albums);


    }

    public static async Task<string> GetAndOrSetFromRedis(string key, Func<Task<string>> cb)
    {
        var redisClient = new RedisClient("localhost:6379");
        string val = redisClient.Get(key);
        if (val == null)
        {
            val = await cb();
            redisClient.SetEx(key, REDIS_DEF_EXP, val);
            System.Console.WriteLine("Saved to reddis");
        }
        redisClient.Dispose();

        return val;
    }
}

