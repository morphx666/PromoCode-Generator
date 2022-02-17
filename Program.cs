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
                if(args.Length == 4) {
                    seed = uint.Parse(args[1]);
                    idx = 2;
                } else if(args.Length != 3) {
                    ShowUsage();
                    break;
                }
                length = int.Parse(args[idx + 0]);
                count = int.Parse(args[idx + 1]);
                rnd = Xoshiro128ss(0x9E3779B9, 0x243F6A88, 0xB7E15162, seed);
                rnd(); // Discard first random number

                for(int i = 0; i < count; i++) {
                    Console.WriteLine(GenerateRandomString(length));
                }
                break;
            case "v":
                string code = args[1];
                Console.WriteLine(IsValid(code));
                break;
            default:
                ShowUsage();
                break;
        }
    }

    static string GenerateRandomString(int length) {
        string result = "";
        int acc = length;
        int p = 0;
        for(int i = 0; i < length - 1; i++) {
            char c = valid[Tausworthe(p * i + (int)(rnd() * valid.Length)) % valid.Length];
            result += c;
            acc += c;
            p = c;
        }
        return result + valid[acc % valid.Length];
    }

    static int Tausworthe(int seed) { // Pseudo-random number generator
        seed ^= seed >> 13;
        seed ^= seed << 18;
        return seed & 0x7fffffff;
    }

    static bool IsValid(string code) {
        int length = code.Length;
        int acc = length;
        for(int i = 0; i < length; i++) {
            if(valid.IndexOf(code[i]) == -1) return false;
            if(i < length - 1) acc += code[i];
        }
        return code[length - 1] == valid[acc % valid.Length];
    }

    static RandomFunction Xoshiro128ss(uint a, uint b, uint c, uint d) {
        return delegate () {
            uint t = b << 9;
            uint r = a * 5;
            r = (r << 7 | r >> 25) * 9;
            c ^= a; d ^= b;
            b ^= c; a ^= d; c ^= t;
            d = d << 11 | d >> 21;
            return (r >> 0) / 4294967296.0;
        };
    }

    static RandomFunction Sfc32(uint a, uint b, uint c, uint d) {
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
            return (t >> 0) / 4294967296.0;
        };
    }

    static void ShowUsage() {
        Console.WriteLine("PromoCode Generator Usage:");
        Console.WriteLine("  Generate: pcg g [seed] length count");
        Console.WriteLine("  Validate: pcg v code");
    }
}