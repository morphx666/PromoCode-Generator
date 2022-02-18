#pragma warning disable CS8618
public static class PromoCodeGenerator {
    private delegate double RandomFunction();
    private static RandomFunction rnd;
    private const string valid = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 'I', 'O', 0 and 1 are excluded to avoid confusion

    public static void Main(string[] args) {
        string mode = args.Length > 0 ? args[0] : "";

        switch(mode) {
            case "g":
                int idx = 1;
                uint seed = (uint)(Random.Shared.Next() * 0xDEADBEEF);
                int length;
                int count;
                int skip = 0;
                string format = "";
                if(args[idx].StartsWith('+')) {
                    seed = uint.Parse(args[1]);
                    idx++;
                }
                if(args[idx].StartsWith('.')) {
                    skip = int.Parse(args[idx].TrimStart('.'));
                    idx++;
                }
                length = int.Parse(args[idx + 0]);
                count = int.Parse(args[idx + 1]);
                if(args.Length == idx + 3) {
                    format = args[idx + 2];
                    if(format.Split('-').Select(int.Parse).Sum() != length) {
                        ShowUsage();
                        Console.WriteLine($"\r\nInvalid Format: '{format}'");
                        break;
                    }
                }
                
                rnd = Xoshiro128ss(0x9E3779B9, 0x243F6A88, 0xB7E15162, seed);
                rnd(); // Discard first random number

                for(int i = 0; i < skip; i++) GenerateRandomString(length);
                for(int i = 0; i < count; i++) {
                    Console.WriteLine(GenerateRandomString(length, format));
                }
                break;
            case "v":
                string code = args[1];
                Console.WriteLine($"Is Valid: {IsValid(code)}");
                break;
            default:
                ShowUsage();
                break;
        }

        Console.WriteLine();
    }

    static string GenerateRandomString(int length, string format = "") {
        string result = "";
        int acc = length;
        int p = 0;
        for(int i = 0; i < length - 1; i++) {
            int k = (int)((Tausworthe(p * i) + rnd() * valid.Length) % valid.Length);
            char c = valid[k];
            result += c;
            p = c;
            acc += Luhn(p, i);
        }
        result += valid[acc % valid.Length];

        if(format != "") {
            int c = 0;
            int[] tabs = format.Split('-').Select(int.Parse).ToArray();
            string r = "";
            for(int i = 0; i < tabs.Length; i++) {
                r += result.Substring(c, tabs[i]) + "-";
                c += tabs[i];
            }
            result = r.TrimEnd('-');
        }
        return result;
    }

    static int Tausworthe(int seed) { // Pseudo-random number generator
        seed ^= seed >> 13;
        seed ^= seed << 18;
        return seed & 0x7FFFFFFF;
    }

    static bool IsValid(string code) {
        code = code.Replace("-", "");
        int length = code.Length;
        int acc = length;
        for(int i = 0; i < length; i++) {
            if(valid.IndexOf(code[i]) == -1) return false;
            if(i < length - 1) acc += Luhn(code[i], i);
        }
        return code[length - 1] == valid[acc % valid.Length];
    }

    static int Luhn(int n, int i) { // Luhn algorithm
        if(i % 2 == 0) n *= 2;
        if(n >= valid.Length - 1) {
            int ip = (int)Math.Floor(n / 10.0);
            int fp = n - ip * 10;
            n = ip + fp;
        }
        return n;
    }

    static RandomFunction Xoshiro128ss(uint a, uint b, uint c, uint d) { // xoshiro128**
        return delegate () {
            uint t = b << 9;
            uint r = a * 5;
            r = (r << 7 | r >> 25) * 9;
            c ^= a; d ^= b;
            b ^= c; a ^= d; c ^= t;
            d = d << 11 | d >> 21;
            return (r >> 0) / (double)0x100000000;
        };
    }

    static RandomFunction Sfc32(uint a, uint b, uint c, uint d) { // Simple Fast Counter
        return delegate () {
            a >>= 0; b >>= 0;
            c >>= 0; d >>= 0;
            uint t = (a + b) | 0;
            a = b ^ b >> 9;
            b = c + (c << 3) | 0;
            c = (c << 21 | c >> 11);
            d = d + 1 | 0;
            t = t + d | 0;
            c = c + t | 0;
            return (t >> 0) / (double)0x100000000;
        };
    }

    static void ShowUsage() {
        Console.WriteLine("PromoCode Generator Usage:");
        Console.WriteLine("  Generate: pcg g [+seed] [.skip] length count [format]");
        Console.WriteLine("  Validate: pcg v code");
    }
}