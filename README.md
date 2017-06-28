# nimp
Naive implementation of a MIPS processor

## How complete is this implementation?
Not very. It's difficult to piece together information. Right now, most instructions are implemented in one way or another, but I'm still working on correctness.

## How fast is this implementation?
~40 MIPS on an i5-2540m
~90 MIPS on a Ryzen 1700

## How do I load programs?
Right now, the supported file format looks something like this:

    [00400000] 8fa40000 ; 1 word
	
OR

    [10000000] 6c6c6548  6f57206f  0a646c72  00203a00 ; 4 words

I'm sure you can figure it out.

## Anything else?
0x7fffffff-0x7fff0000 is designated as stack space and has its own jumbo page.
$sp is initialized to 0x7fffffff
$gp is initialized to 0x10008000
You can find various documentation in [docs/](https://github.com/hexafluoride/nimp/tree/master/docs).