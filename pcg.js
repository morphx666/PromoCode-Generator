const valid = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 'I', 'O', 0 and 1 are excluded to avoid confusion
const args = process.argv.slice(2);
const mode = args.length > 0 ? args[0] : "";
let rnd;
switch(mode) {
    case "g":
        let idx = 1;
        let seed = Math.random() * 0xDEADBEEF;
        let length;
        let count;
        if(args.length == 4) {
            seed = parseInt(args[1]);
            idx = 2;
        } else if(args.length != 3) {
            showUsage();
            break;
        }
        length = parseInt(args[idx + 0]);
        count = parseInt(args[idx + 1]);
        rnd = xoshiro128ss(0x9E3779B9, 0x243F6A88, 0xB7E15162, seed);
        rnd(); // Discard first random number

        for(let i = 0; i < count; i++) {
            console.log(generateRandomString(length));
        }
        break;
    case "v":
        const code = args[1];
        console.log(isValid(code));
        break;
    default:
        showUsage();
        break;
}

function generateRandomString(length) {
    let result = "";
    let acc = length;
    let p = 0;
    for (let i = 0; i < length - 1; i++) {
        const c = valid[tausworthe(p * i + rnd() * valid.length) % valid.length];
        result += c;
        p = c.charCodeAt(0);
        acc += luhn(p, i);
    }
    return result + valid[acc % valid.length];
}

function isValid(code) {
    let length = code.length;
    let acc = length;
    for(let i = 0; i < length; i++) {
        if(valid.indexOf(code[i]) == -1) return false;
        if(i < length - 1) acc += luhn(code[i].charCodeAt(0), i);
    }
    return code[length - 1] == valid[acc % valid.length];
}

function luhn(n, i) { // Luhn algorithm
    if(i % 2 == 0) n *= 2;
    if(n >= valid.length - 1) {
        const ip = Math.floor(n / 10.0);
        const fp = n - ip * 10;
        n = ip + fp;
    }
    return n;
}

function tausworthe(seed) { // Pseudo-random number generator
    seed ^= seed >> 13;
    seed ^= seed << 18;
    return seed & 0x7fffffff;
}

function xoshiro128ss(a, b, c, d) {
    return function() {
        var t = b << 9, r = a * 5; r = (r << 7 | r >>> 25) * 9;
        c ^= a; d ^= b;
        b ^= c; a ^= d; c ^= t;
        d = d << 11 | d >>> 21;
        return (r >>> 0) / 4294967296;
    }
}

function sfc32(a, b, c, d) {
    return function() {
      a >>>= 0; b >>>= 0; c >>>= 0; d >>>= 0; 
      var t = (a + b) | 0;
      a = b ^ b >>> 9;
      b = c + (c << 3) | 0;
      c = (c << 21 | c >>> 11);
      d = d + 1 | 0;
      t = t + d | 0;
      c = c + t | 0;
      return (t >>> 0) / 4294967296;
    }
}

function showUsage() {
    console.log("PromoCode Generator Usage:");
    console.log("  Generate: node pcg.js g [seed] length count");
    console.log("  Validate: node pcg.js v code");
}