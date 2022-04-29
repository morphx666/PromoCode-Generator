# PromoCode-Generator

This is a tool to generate, pseudo-random, auto-verifiable and promotional codes. The codes could also be used as serial numbers, registration keys, etc...

The codes are generated from several pseudo-random generators and are then _signed_ with a checksum, so they can be validated.<br>
Each code can contain letters (except for 'I' and 'O') and numbers (except for '0' and '1'). This is to avoid confusions.

## Usage

The tools has to main operational modes:

- Generate: `pcg g [+seed] [.skip] length count [format]`
- Validate: `pcg v code`

## Generating Codes

For example, to generate 5, 10 characters long codes:<br>
`pcg g 10 5`

This will produce an output similar to this one:
```
U4HQFZMVDJ
M9V24GVGNH
G5ZKPJTGBL
ZRZR795D7C
MCPP2SDDFG
```
Note that the list of codes will vary as they will depend on a random seed that changes every time the program is run.

A seed can also be specified in order to always generate the same set of randoms codes:<br>
`pcg g +1971 10 5`

And this is the output, which will be the same for everybody:
```
Y33KZY8XHV
W996WBUER2
84GTGZQYVK
FFJTQ5JXNY
BCJR68G3FH
```

You can also skip any number of codes. The above example shows the first 10 codes for the given parameters, to get the next 10 we use:<br>
`pcg g +1971 .10 10 5`

And the result will be:
```
TZPS6XGKUX
EK35HT6GML
LF2Q85D27P
YLMRH7N6KY
SZ8Y64BTJT
```

Codes can also be formatted by specifying a string representing the digits groups:<br>
`pcg g +1971 .10 10 5 4-4-2`

This is how the formatted values will look like:
```
TZPS-6XGK-UX
EK35-HT6G-ML
LF2Q-85D2-7P
YLMR-H7N6-KY
SZ8Y-64BT-JT
```

## Validating Codes

Codes can be easily validated by using the `v` operation mode and passing the code as the second parameter:<br>
`pcg v TZPS-6XGK-UX`

This will return that the code is valid, which means, that the code satisfies its checksum.
