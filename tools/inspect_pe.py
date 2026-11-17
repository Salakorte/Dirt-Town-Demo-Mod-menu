"""Read-only PE64 inspection for the user's installed build; no game data emitted."""
import argparse
import pathlib
import re
import struct


class PE:
    def __init__(self, path):
        self.data = pathlib.Path(path).read_bytes()
        self.nt = struct.unpack_from('<I', self.data, 0x3c)[0]
        opt = self.nt + 24
        self.base = struct.unpack_from('<Q', self.data, opt + 24)[0]
        self.sections = []
        n = struct.unpack_from('<H', self.data, self.nt + 6)[0]
        size = struct.unpack_from('<H', self.data, self.nt + 20)[0]
        for i in range(n):
            off = opt + size + i * 40
            name = self.data[off:off+8].split(b'\0')[0].decode()
            vs, va, rs, raw = struct.unpack_from('<IIII', self.data, off+8)
            self.sections.append((name, va, vs, raw, rs))
        prva, psize = struct.unpack_from('<II', self.data, opt + 112 + 3*8)
        poff = self.offset(prva)
        self.functions = [struct.unpack_from('<III', self.data, i) for i in range(poff, poff+psize, 12)] if prva else []

    def offset(self, rva):
        for _, va, _, raw, rs in self.sections:
            if va <= rva < va+rs:
                return raw+rva-va
        raise ValueError(hex(rva))

    def rva(self, offset):
        for _, va, _, raw, rs in self.sections:
            if raw <= offset < raw+rs:
                return va+offset-raw
        raise ValueError(hex(offset))

    def function(self, rva):
        return next(((s,e) for s,e,_ in self.functions if s<=rva<e), None)

    def xrefs(self, target):
        # Candidate RIP-relative LEA/MOV references; verify with a disassembler.
        for name, va, _, raw, rs in self.sections:
            if name != '.text':
                continue
            code = self.data[raw:raw+rs]
            for m in re.finditer(rb'[\x48-\x4f][\x8d\x8b][\x05\x0d\x15\x1d\x25\x2d\x35\x3d]', code):
                p = m.start()
                if p+7 <= len(code) and va+p+7+struct.unpack_from('<i',code,p+3)[0] == target:
                    yield va+p


if __name__ == '__main__':
    ap=argparse.ArgumentParser()
    ap.add_argument('exe')
    ap.add_argument('query')
    args=ap.parse_args()
    pe=PE(args.exe)
    for m in re.finditer(rb'[\x20-\x7e]{3,}',pe.data):
        s=m.group().decode()
        if re.search(args.query,s,re.I):
            rva=pe.rva(m.start())
            print(hex(rva),repr(s[:300]))
            for x in pe.xrefs(rva):
                f=pe.function(x)
                print(' xref',hex(pe.base+x),'function',tuple(hex(pe.base+t) for t in f) if f else None)
