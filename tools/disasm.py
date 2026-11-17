"""Print selected instructions with read-only string annotations; local analysis only."""
import sys,pathlib,struct,re
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]/'.tools'))
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
from capstone.x86_const import X86_OP_MEM,X86_REG_RIP
from inspect_pe import PE
p=PE(r'C:\Program Files (x86)\Steam\steamapps\common\Dirt Town Demo\dirttown.exe')
d=Cs(CS_ARCH_X86,CS_MODE_64);d.detail=True
for arg in sys.argv[1:]:
    parts=arg.split(':');start=int(parts[0],16);end=int(parts[1],16) if len(parts)>1 else p.function(start)[1]
    print('BLOCK',hex(start),hex(end))
    for i in d.disasm(p.data[p.offset(start):p.offset(end-1)+1],p.base+start):
        notes=[]
        for op in i.operands:
            if op.type==X86_OP_MEM and op.mem.base==X86_REG_RIP:
                target=i.address+i.size+op.mem.disp-p.base
                try:
                    data=p.data[p.offset(target):p.offset(target)+160]
                    string=data.split(b'\0')[0]
                    if len(string)>2 and all(32<=b<127 for b in string):notes.append(repr(string.decode()))
                except ValueError: pass
        print(f'{i.address-p.base:08x} {i.mnemonic:10} {i.op_str:48}',*notes)
