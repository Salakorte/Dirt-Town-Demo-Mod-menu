"""Extract the one lobby adapter address from the installed mod's COFF symbols."""
import pathlib,struct,sys
path=pathlib.Path(r'C:\Program Files (x86)\Steam\steamapps\common\Dirt Town Demo\version.dll')
d=path.read_bytes();nt=struct.unpack_from('<I',d,60)[0]
p,n=struct.unpack_from('<II',d,nt+12);sz=struct.unpack_from('<H',d,nt+20)[0];st=p+n*18;i=0
while i<n:
    at=p+i*18;name=d[at:at+8]
    if name[:4]==b'\0'*4:
        start=st+struct.unpack_from('<I',name,4)[0];name=d[start:d.index(b'\0',start)]
    else:name=name.split(b'\0')[0]
    if name==b'_ZN4coop12_GLOBAL__N_1L10chat_lobbyE':
        value,section=struct.unpack_from('<Ih',d,at+8)
        rva=struct.unpack_from('<I',d,nt+24+sz+(section-1)*40+12)[0]+value
        timestamp=struct.unpack_from('<I',d,nt+8)[0];image=struct.unpack_from('<I',d,nt+24+56)[0]
        result=f'#pragma once\nconstexpr unsigned COOP_LOBBY_RVA=0x{rva:X};\nconstexpr unsigned COOP_TIMESTAMP=0x{timestamp:X};\nconstexpr unsigned COOP_IMAGE_SIZE=0x{image:X};\n'
        pathlib.Path('native/coop_layout.h').write_text(result)
        print(result);break
    i+=1+d[at+17]
else:raise SystemExit('Lobby symbol not found; no guessed address emitted.')
