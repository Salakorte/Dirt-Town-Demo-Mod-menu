"""Generate our forwarding stubs from the Windows API export names (no DLL copied)."""
import pathlib, struct, os
root=pathlib.Path(__file__).resolve().parents[1]
data=(pathlib.Path(os.environ['SystemRoot'])/'System32'/'winmm.dll').read_bytes()
nt=struct.unpack_from('<I',data,60)[0];opt=nt+24
n=struct.unpack_from('<H',data,nt+6)[0];size=struct.unpack_from('<H',data,nt+20)[0]
sections=[struct.unpack_from('<IIII',data,opt+size+i*40+8) for i in range(n)]
def offset(rva):
    for vs,va,rs,raw in sections:
        if va<=rva<va+rs:return raw+rva-va
    raise ValueError(hex(rva))
rva=struct.unpack_from('<I',data,opt+112)[0];e=offset(rva)
base,functions,names,af,an,ao=struct.unpack_from('<IIIIII',data,e+16)
exports={}
for i in range(names):
    p=offset(struct.unpack_from('<I',data,offset(an)+i*4)[0]);name=data[p:data.index(b'\0',p)].decode('ascii')
    index=struct.unpack_from('<H',data,offset(ao)+i*2)[0];exports[index]=name
rows=[]
for index in range(functions):
    if struct.unpack_from('<I',data,offset(af)+index*4)[0]:rows.append((base+index,exports.get(index)))
header=['// Generated forwarding metadata, not Windows implementation code.',f'constexpr size_t WINMM_COUNT={len(rows)};', 'struct WinmmExport {const char* name;unsigned ordinal;};','static const WinmmExport winmmExports[]={']
definition=['LIBRARY winmm','EXPORTS']
asm=['option casemap:none','EXTERN winmmResolve:PROC','EXTERN winmmTargets:QWORD','.code']
for i,(ordinal,name) in enumerate(rows):
    header.append('{'+('"'+name+'"' if name else 'nullptr')+f',{ordinal}'+'},')
    definition.append(f'{name or "Ordinal"+str(ordinal)}=winmmStub{i} @{ordinal}'+(' NONAME' if name is None else ''))
    asm += [f'PUBLIC winmmStub{i}',f'winmmStub{i} PROC',f'mov rax,QWORD PTR [winmmTargets+{i*8}]','test rax,rax',f'jnz winmmReady{i}',f'mov r11d,{i}','jmp winmmLazy',f'winmmReady{i}:','jmp rax',f'winmmStub{i} ENDP']
header.append('};')
definition.append('DirtTownMenuVerifyBuild')
definition.append('DirtTownMenuPreviewSelfTest')
definition.append('DirtTownMenuCameraSelfTest')
asm += ['winmmLazy PROC','sub rsp,88h','mov [rsp+20h],rcx','mov [rsp+28h],rdx','mov [rsp+30h],r8','mov [rsp+38h],r9','movdqu [rsp+40h],xmm0','movdqu [rsp+50h],xmm1','movdqu [rsp+60h],xmm2','movdqu [rsp+70h],xmm3','mov ecx,r11d','call winmmResolve','mov rcx,[rsp+20h]','mov rdx,[rsp+28h]','mov r8,[rsp+30h]','mov r9,[rsp+38h]','movdqu xmm0,[rsp+40h]','movdqu xmm1,[rsp+50h]','movdqu xmm2,[rsp+60h]','movdqu xmm3,[rsp+70h]','add rsp,88h','jmp rax','winmmLazy ENDP','END']
for name,lines in [('winmm_exports.h',header),('winmm_proxy.def',definition),('winmm_proxy.asm',asm)]:
    (root/'native'/name).write_text('\n'.join(lines)+'\n')
print(f'Generated {len(rows)} Windows multimedia forwarding stubs.')
