## Linux platform files for ip22
https://github.com/torvalds/linux/tree/master/arch/mips/sgi-ip22
https://github.com/torvalds/linux/tree/master/arch/mips/include/asm/sgi
https://github.com/torvalds/linux/blob/master/drivers/scsi/sgiwd93.c
https://github.com/torvalds/linux/blob/master/sound/mips/hal2.c
https://github.com/torvalds/linux/blob/master/drivers/net/ethernet/seeq/sgiseeq.c
https://github.com/torvalds/linux/blob/master/drivers/tty/serial/ip22zilog.c

### Platform init
* [ip22-setup.c](https://github.com/torvalds/linux/blob/master/arch/mips/sgi-ip22/ip22-setup.c) - platform init
* [ip22-hpc.c](https://github.com/torvalds/linux/blob/master/arch/mips/sgi-ip22/ip22-hpc.c) - gets called shortly after to find out board parameters(distinguishes between indy and indigo2, differing in interrupts)
* [ip22-mc.c](https://github.com/torvalds/linux/blob/master/arch/mips/sgi-ip22/ip22-mc.c) - now that we know about the board, time to initialize the memory controller
* [env.c](https://github.com/torvalds/linux/blob/master/arch/mips/fw/arc/env.c) and consequently [sgiarcs.h](https://github.com/torvalds/linux/blob/master/arch/mips/include/asm/sgiarcs.h) - ARC API, used to figure out serial output parameters right now

## ARC documentation
https://www.linux-mips.org/wiki/ARC
https://github.com/torvalds/linux/tree/master/arch/mips/fw/arc
https://github.com/torvalds/linux/blob/master/arch/mips/include/asm/sgialib.h