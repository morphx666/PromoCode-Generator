#pragma warning disable CS8618
using System.Diagnostics;
using System.Text;

public static class PromoCodeGenerator {
    private delegate double RandomFunction();
    private static RandomFunction rnd;
    private const string valid = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 'I', 'O', 0 and 1 are excluded to avoid confusion

    public static void Main(string[] args) {
        string mode = args.Length > 0 ? args[0] : "";

        switch(mode) {
            case "g":
                if(args.Length < 3) {
                    ShowUsage("Invalid argumen count");
                    break;
                }

                int idx = 1;
                uint seed = (uint)(Random.Shared.Next() * 0xDEADBEEF);
                int length;
                ulong count;
                ulong skip = 0;
                string format = "";
                int[]? tabs = null;
                if(args[idx].StartsWith('+')) seed = uint.Parse(args[idx++]);
                if(args[idx].StartsWith('.')) skip = ulong.Parse(args[idx++].TrimStart('.'));

                length = int.Parse(args[idx + 0]);
                count = ulong.Parse(args[idx + 1]);
                if(args.Length == idx + 3) {
                    format = args[idx + 2];
                    if(format.Split('-').Select(int.Parse).Sum() != length) {
                        ShowUsage($"Invalid format: '{format}'");
                        break;
                    } else {
                        tabs = format.Split('-').Select(int.Parse).ToArray();
                    }
                }

                double max = Math.Pow(valid.Length, length - 1);
                if(max < count || count + skip > max) {
                    ShowUsage($"Unable to generate {count} unique codes of length {length} {(skip > 0 ? $"skipping {skip}" : "")}\nThe maximum number of codes is {max - skip}");
                    break;
                }

                rnd = Xoshiro128ss(0x9E3779B9, 0x243F6A88, 0xB7E15162, seed);
                //rnd = Sfc32(0x9E3779B9, 0x243F6A88, 0xB7E15162, seed);
                rnd(); // Discard first random number

                Stopwatch sw = Stopwatch.StartNew();
                for(ulong i = 0; i < skip; i++) GenerateRandomString(length);
                for(ulong i = 0; i < count; i++) {
                    Console.WriteLine(GenerateRandomString(length, tabs));
                }
                Console.WriteLine($"\r\nElapsed: {sw.ElapsedMilliseconds:N2}ms");
                break;
            case "v":
                if(args.Length < 2) {
                    ShowUsage("Invalid argumen count");
                    break;
                }

                string code = args[1];
                Console.WriteLine($"The code is{(IsValid(code) ? "" : " not")} valid");
                break;
            default:
                ShowUsage("Missing mode argument: [g]enerate, [v]alidate");
                break;
        }

        Console.WriteLine();
    }

    static string GenerateRandomString(int length, int[]? tabs = null) {
        StringBuilder code = new();
        int acc = length;
        int p = 0;
        for(int i = 0; i < length - 1; i++) {
            int k = (int)((Tausworthe(p * i) + rnd() * valid.Length) % valid.Length);
            char c = valid[k];
            code.Append(c);
            p = c;
            acc += Luhn(p, i);
        }
        string result = code.Append(valid[acc % valid.Length]).ToString();

        if(tabs != null) {
            int c = 0;
            StringBuilder r = new();
            for(int i = 0; i < tabs.Length; i++) {
                r.Append(result.AsSpan(c, tabs[i])).Append('-');
                c += tabs[i];
            }
            result = r.ToString().TrimEnd('-');
        }

        return result;
    }

    static bool IsValid(string code) {
        code = code.Replace("-", "");
        int length = code.Length;
        int acc = length;
        for(int i = 0; i < length; i++) {
            if(!valid.Contains(code[i])) return false;
            if(i < length - 1) acc += Luhn(code[i], i);
        }
        return code[length - 1] == valid[acc % valid.Length];
    }

    static int Tausworthe(int seed) { // Pseudo-random number generator
        seed ^= seed >> 13;
        seed ^= seed << 18;
        return seed & 0x7FFFFFFF;
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

    static void ShowUsage(string extra = "") {
        Console.WriteLine("PromoCode Generator Usage:");
        Console.WriteLine("  Generate: pcg g [+seed] [.skip] length count [format]");
        Console.WriteLine("  Validate: pcg v code");
        if(extra != "") Console.WriteLine($"\r\n{extra}");
    }
}