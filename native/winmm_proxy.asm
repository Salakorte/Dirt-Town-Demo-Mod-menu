option casemap:none
EXTERN winmmResolve:PROC
EXTERN winmmTargets:QWORD
.code
PUBLIC winmmStub0
winmmStub0 PROC
mov rax,QWORD PTR [winmmTargets+0]
test rax,rax
jnz winmmReady0
mov r11d,0
jmp winmmLazy
winmmReady0:
jmp rax
winmmStub0 ENDP
PUBLIC winmmStub1
winmmStub1 PROC
mov rax,QWORD PTR [winmmTargets+8]
test rax,rax
jnz winmmReady1
mov r11d,1
jmp winmmLazy
winmmReady1:
jmp rax
winmmStub1 ENDP
PUBLIC winmmStub2
winmmStub2 PROC
mov rax,QWORD PTR [winmmTargets+16]
test rax,rax
jnz winmmReady2
mov r11d,2
jmp winmmLazy
winmmReady2:
jmp rax
winmmStub2 ENDP
PUBLIC winmmStub3
winmmStub3 PROC
mov rax,QWORD PTR [winmmTargets+24]
test rax,rax
jnz winmmReady3
mov r11d,3
jmp winmmLazy
winmmReady3:
jmp rax
winmmStub3 ENDP
PUBLIC winmmStub4
winmmStub4 PROC
mov rax,QWORD PTR [winmmTargets+32]
test rax,rax
jnz winmmReady4
mov r11d,4
jmp winmmLazy
winmmReady4:
jmp rax
winmmStub4 ENDP
PUBLIC winmmStub5
winmmStub5 PROC
mov rax,QWORD PTR [winmmTargets+40]
test rax,rax
jnz winmmReady5
mov r11d,5
jmp winmmLazy
winmmReady5:
jmp rax
winmmStub5 ENDP
PUBLIC winmmStub6
winmmStub6 PROC
mov rax,QWORD PTR [winmmTargets+48]
test rax,rax
jnz winmmReady6
mov r11d,6
jmp winmmLazy
winmmReady6:
jmp rax
winmmStub6 ENDP
PUBLIC winmmStub7
winmmStub7 PROC
mov rax,QWORD PTR [winmmTargets+56]
test rax,rax
jnz winmmReady7
mov r11d,7
jmp winmmLazy
winmmReady7:
jmp rax
winmmStub7 ENDP
PUBLIC winmmStub8
winmmStub8 PROC
mov rax,QWORD PTR [winmmTargets+64]
test rax,rax
jnz winmmReady8
mov r11d,8
jmp winmmLazy
winmmReady8:
jmp rax
winmmStub8 ENDP
PUBLIC winmmStub9
winmmStub9 PROC
mov rax,QWORD PTR [winmmTargets+72]
test rax,rax
jnz winmmReady9
mov r11d,9
jmp winmmLazy
winmmReady9:
jmp rax
winmmStub9 ENDP
PUBLIC winmmStub10
winmmStub10 PROC
mov rax,QWORD PTR [winmmTargets+80]
test rax,rax
jnz winmmReady10
mov r11d,10
jmp winmmLazy
winmmReady10:
jmp rax
winmmStub10 ENDP
PUBLIC winmmStub11
winmmStub11 PROC
mov rax,QWORD PTR [winmmTargets+88]
test rax,rax
jnz winmmReady11
mov r11d,11
jmp winmmLazy
winmmReady11:
jmp rax
winmmStub11 ENDP
PUBLIC winmmStub12
winmmStub12 PROC
mov rax,QWORD PTR [winmmTargets+96]
test rax,rax
jnz winmmReady12
mov r11d,12
jmp winmmLazy
winmmReady12:
jmp rax
winmmStub12 ENDP
PUBLIC winmmStub13
winmmStub13 PROC
mov rax,QWORD PTR [winmmTargets+104]
test rax,rax
jnz winmmReady13
mov r11d,13
jmp winmmLazy
winmmReady13:
jmp rax
winmmStub13 ENDP
PUBLIC winmmStub14
winmmStub14 PROC
mov rax,QWORD PTR [winmmTargets+112]
test rax,rax
jnz winmmReady14
mov r11d,14
jmp winmmLazy
winmmReady14:
jmp rax
winmmStub14 ENDP
PUBLIC winmmStub15
winmmStub15 PROC
mov rax,QWORD PTR [winmmTargets+120]
test rax,rax
jnz winmmReady15
mov r11d,15
jmp winmmLazy
winmmReady15:
jmp rax
winmmStub15 ENDP
PUBLIC winmmStub16
winmmStub16 PROC
mov rax,QWORD PTR [winmmTargets+128]
test rax,rax
jnz winmmReady16
mov r11d,16
jmp winmmLazy
winmmReady16:
jmp rax
winmmStub16 ENDP
PUBLIC winmmStub17
winmmStub17 PROC
mov rax,QWORD PTR [winmmTargets+136]
test rax,rax
jnz winmmReady17
mov r11d,17
jmp winmmLazy
winmmReady17:
jmp rax
winmmStub17 ENDP
PUBLIC winmmStub18
winmmStub18 PROC
mov rax,QWORD PTR [winmmTargets+144]
test rax,rax
jnz winmmReady18
mov r11d,18
jmp winmmLazy
winmmReady18:
jmp rax
winmmStub18 ENDP
PUBLIC winmmStub19
winmmStub19 PROC
mov rax,QWORD PTR [winmmTargets+152]
test rax,rax
jnz winmmReady19
mov r11d,19
jmp winmmLazy
winmmReady19:
jmp rax
winmmStub19 ENDP
PUBLIC winmmStub20
winmmStub20 PROC
mov rax,QWORD PTR [winmmTargets+160]
test rax,rax
jnz winmmReady20
mov r11d,20
jmp winmmLazy
winmmReady20:
jmp rax
winmmStub20 ENDP
PUBLIC winmmStub21
winmmStub21 PROC
mov rax,QWORD PTR [winmmTargets+168]
test rax,rax
jnz winmmReady21
mov r11d,21
jmp winmmLazy
winmmReady21:
jmp rax
winmmStub21 ENDP
PUBLIC winmmStub22
winmmStub22 PROC
mov rax,QWORD PTR [winmmTargets+176]
test rax,rax
jnz winmmReady22
mov r11d,22
jmp winmmLazy
winmmReady22:
jmp rax
winmmStub22 ENDP
PUBLIC winmmStub23
winmmStub23 PROC
mov rax,QWORD PTR [winmmTargets+184]
test rax,rax
jnz winmmReady23
mov r11d,23
jmp winmmLazy
winmmReady23:
jmp rax
winmmStub23 ENDP
PUBLIC winmmStub24
winmmStub24 PROC
mov rax,QWORD PTR [winmmTargets+192]
test rax,rax
jnz winmmReady24
mov r11d,24
jmp winmmLazy
winmmReady24:
jmp rax
winmmStub24 ENDP
PUBLIC winmmStub25
winmmStub25 PROC
mov rax,QWORD PTR [winmmTargets+200]
test rax,rax
jnz winmmReady25
mov r11d,25
jmp winmmLazy
winmmReady25:
jmp rax
winmmStub25 ENDP
PUBLIC winmmStub26
winmmStub26 PROC
mov rax,QWORD PTR [winmmTargets+208]
test rax,rax
jnz winmmReady26
mov r11d,26
jmp winmmLazy
winmmReady26:
jmp rax
winmmStub26 ENDP
PUBLIC winmmStub27
winmmStub27 PROC
mov rax,QWORD PTR [winmmTargets+216]
test rax,rax
jnz winmmReady27
mov r11d,27
jmp winmmLazy
winmmReady27:
jmp rax
winmmStub27 ENDP
PUBLIC winmmStub28
winmmStub28 PROC
mov rax,QWORD PTR [winmmTargets+224]
test rax,rax
jnz winmmReady28
mov r11d,28
jmp winmmLazy
winmmReady28:
jmp rax
winmmStub28 ENDP
PUBLIC winmmStub29
winmmStub29 PROC
mov rax,QWORD PTR [winmmTargets+232]
test rax,rax
jnz winmmReady29
mov r11d,29
jmp winmmLazy
winmmReady29:
jmp rax
winmmStub29 ENDP
PUBLIC winmmStub30
winmmStub30 PROC
mov rax,QWORD PTR [winmmTargets+240]
test rax,rax
jnz winmmReady30
mov r11d,30
jmp winmmLazy
winmmReady30:
jmp rax
winmmStub30 ENDP
PUBLIC winmmStub31
winmmStub31 PROC
mov rax,QWORD PTR [winmmTargets+248]
test rax,rax
jnz winmmReady31
mov r11d,31
jmp winmmLazy
winmmReady31:
jmp rax
winmmStub31 ENDP
PUBLIC winmmStub32
winmmStub32 PROC
mov rax,QWORD PTR [winmmTargets+256]
test rax,rax
jnz winmmReady32
mov r11d,32
jmp winmmLazy
winmmReady32:
jmp rax
winmmStub32 ENDP
PUBLIC winmmStub33
winmmStub33 PROC
mov rax,QWORD PTR [winmmTargets+264]
test rax,rax
jnz winmmReady33
mov r11d,33
jmp winmmLazy
winmmReady33:
jmp rax
winmmStub33 ENDP
PUBLIC winmmStub34
winmmStub34 PROC
mov rax,QWORD PTR [winmmTargets+272]
test rax,rax
jnz winmmReady34
mov r11d,34
jmp winmmLazy
winmmReady34:
jmp rax
winmmStub34 ENDP
PUBLIC winmmStub35
winmmStub35 PROC
mov rax,QWORD PTR [winmmTargets+280]
test rax,rax
jnz winmmReady35
mov r11d,35
jmp winmmLazy
winmmReady35:
jmp rax
winmmStub35 ENDP
PUBLIC winmmStub36
winmmStub36 PROC
mov rax,QWORD PTR [winmmTargets+288]
test rax,rax
jnz winmmReady36
mov r11d,36
jmp winmmLazy
winmmReady36:
jmp rax
winmmStub36 ENDP
PUBLIC winmmStub37
winmmStub37 PROC
mov rax,QWORD PTR [winmmTargets+296]
test rax,rax
jnz winmmReady37
mov r11d,37
jmp winmmLazy
winmmReady37:
jmp rax
winmmStub37 ENDP
PUBLIC winmmStub38
winmmStub38 PROC
mov rax,QWORD PTR [winmmTargets+304]
test rax,rax
jnz winmmReady38
mov r11d,38
jmp winmmLazy
winmmReady38:
jmp rax
winmmStub38 ENDP
PUBLIC winmmStub39
winmmStub39 PROC
mov rax,QWORD PTR [winmmTargets+312]
test rax,rax
jnz winmmReady39
mov r11d,39
jmp winmmLazy
winmmReady39:
jmp rax
winmmStub39 ENDP
PUBLIC winmmStub40
winmmStub40 PROC
mov rax,QWORD PTR [winmmTargets+320]
test rax,rax
jnz winmmReady40
mov r11d,40
jmp winmmLazy
winmmReady40:
jmp rax
winmmStub40 ENDP
PUBLIC winmmStub41
winmmStub41 PROC
mov rax,QWORD PTR [winmmTargets+328]
test rax,rax
jnz winmmReady41
mov r11d,41
jmp winmmLazy
winmmReady41:
jmp rax
winmmStub41 ENDP
PUBLIC winmmStub42
winmmStub42 PROC
mov rax,QWORD PTR [winmmTargets+336]
test rax,rax
jnz winmmReady42
mov r11d,42
jmp winmmLazy
winmmReady42:
jmp rax
winmmStub42 ENDP
PUBLIC winmmStub43
winmmStub43 PROC
mov rax,QWORD PTR [winmmTargets+344]
test rax,rax
jnz winmmReady43
mov r11d,43
jmp winmmLazy
winmmReady43:
jmp rax
winmmStub43 ENDP
PUBLIC winmmStub44
winmmStub44 PROC
mov rax,QWORD PTR [winmmTargets+352]
test rax,rax
jnz winmmReady44
mov r11d,44
jmp winmmLazy
winmmReady44:
jmp rax
winmmStub44 ENDP
PUBLIC winmmStub45
winmmStub45 PROC
mov rax,QWORD PTR [winmmTargets+360]
test rax,rax
jnz winmmReady45
mov r11d,45
jmp winmmLazy
winmmReady45:
jmp rax
winmmStub45 ENDP
PUBLIC winmmStub46
winmmStub46 PROC
mov rax,QWORD PTR [winmmTargets+368]
test rax,rax
jnz winmmReady46
mov r11d,46
jmp winmmLazy
winmmReady46:
jmp rax
winmmStub46 ENDP
PUBLIC winmmStub47
winmmStub47 PROC
mov rax,QWORD PTR [winmmTargets+376]
test rax,rax
jnz winmmReady47
mov r11d,47
jmp winmmLazy
winmmReady47:
jmp rax
winmmStub47 ENDP
PUBLIC winmmStub48
winmmStub48 PROC
mov rax,QWORD PTR [winmmTargets+384]
test rax,rax
jnz winmmReady48
mov r11d,48
jmp winmmLazy
winmmReady48:
jmp rax
winmmStub48 ENDP
PUBLIC winmmStub49
winmmStub49 PROC
mov rax,QWORD PTR [winmmTargets+392]
test rax,rax
jnz winmmReady49
mov r11d,49
jmp winmmLazy
winmmReady49:
jmp rax
winmmStub49 ENDP
PUBLIC winmmStub50
winmmStub50 PROC
mov rax,QWORD PTR [winmmTargets+400]
test rax,rax
jnz winmmReady50
mov r11d,50
jmp winmmLazy
winmmReady50:
jmp rax
winmmStub50 ENDP
PUBLIC winmmStub51
winmmStub51 PROC
mov rax,QWORD PTR [winmmTargets+408]
test rax,rax
jnz winmmReady51
mov r11d,51
jmp winmmLazy
winmmReady51:
jmp rax
winmmStub51 ENDP
PUBLIC winmmStub52
winmmStub52 PROC
mov rax,QWORD PTR [winmmTargets+416]
test rax,rax
jnz winmmReady52
mov r11d,52
jmp winmmLazy
winmmReady52:
jmp rax
winmmStub52 ENDP
PUBLIC winmmStub53
winmmStub53 PROC
mov rax,QWORD PTR [winmmTargets+424]
test rax,rax
jnz winmmReady53
mov r11d,53
jmp winmmLazy
winmmReady53:
jmp rax
winmmStub53 ENDP
PUBLIC winmmStub54
winmmStub54 PROC
mov rax,QWORD PTR [winmmTargets+432]
test rax,rax
jnz winmmReady54
mov r11d,54
jmp winmmLazy
winmmReady54:
jmp rax
winmmStub54 ENDP
PUBLIC winmmStub55
winmmStub55 PROC
mov rax,QWORD PTR [winmmTargets+440]
test rax,rax
jnz winmmReady55
mov r11d,55
jmp winmmLazy
winmmReady55:
jmp rax
winmmStub55 ENDP
PUBLIC winmmStub56
winmmStub56 PROC
mov rax,QWORD PTR [winmmTargets+448]
test rax,rax
jnz winmmReady56
mov r11d,56
jmp winmmLazy
winmmReady56:
jmp rax
winmmStub56 ENDP
PUBLIC winmmStub57
winmmStub57 PROC
mov rax,QWORD PTR [winmmTargets+456]
test rax,rax
jnz winmmReady57
mov r11d,57
jmp winmmLazy
winmmReady57:
jmp rax
winmmStub57 ENDP
PUBLIC winmmStub58
winmmStub58 PROC
mov rax,QWORD PTR [winmmTargets+464]
test rax,rax
jnz winmmReady58
mov r11d,58
jmp winmmLazy
winmmReady58:
jmp rax
winmmStub58 ENDP
PUBLIC winmmStub59
winmmStub59 PROC
mov rax,QWORD PTR [winmmTargets+472]
test rax,rax
jnz winmmReady59
mov r11d,59
jmp winmmLazy
winmmReady59:
jmp rax
winmmStub59 ENDP
PUBLIC winmmStub60
winmmStub60 PROC
mov rax,QWORD PTR [winmmTargets+480]
test rax,rax
jnz winmmReady60
mov r11d,60
jmp winmmLazy
winmmReady60:
jmp rax
winmmStub60 ENDP
PUBLIC winmmStub61
winmmStub61 PROC
mov rax,QWORD PTR [winmmTargets+488]
test rax,rax
jnz winmmReady61
mov r11d,61
jmp winmmLazy
winmmReady61:
jmp rax
winmmStub61 ENDP
PUBLIC winmmStub62
winmmStub62 PROC
mov rax,QWORD PTR [winmmTargets+496]
test rax,rax
jnz winmmReady62
mov r11d,62
jmp winmmLazy
winmmReady62:
jmp rax
winmmStub62 ENDP
PUBLIC winmmStub63
winmmStub63 PROC
mov rax,QWORD PTR [winmmTargets+504]
test rax,rax
jnz winmmReady63
mov r11d,63
jmp winmmLazy
winmmReady63:
jmp rax
winmmStub63 ENDP
PUBLIC winmmStub64
winmmStub64 PROC
mov rax,QWORD PTR [winmmTargets+512]
test rax,rax
jnz winmmReady64
mov r11d,64
jmp winmmLazy
winmmReady64:
jmp rax
winmmStub64 ENDP
PUBLIC winmmStub65
winmmStub65 PROC
mov rax,QWORD PTR [winmmTargets+520]
test rax,rax
jnz winmmReady65
mov r11d,65
jmp winmmLazy
winmmReady65:
jmp rax
winmmStub65 ENDP
PUBLIC winmmStub66
winmmStub66 PROC
mov rax,QWORD PTR [winmmTargets+528]
test rax,rax
jnz winmmReady66
mov r11d,66
jmp winmmLazy
winmmReady66:
jmp rax
winmmStub66 ENDP
PUBLIC winmmStub67
winmmStub67 PROC
mov rax,QWORD PTR [winmmTargets+536]
test rax,rax
jnz winmmReady67
mov r11d,67
jmp winmmLazy
winmmReady67:
jmp rax
winmmStub67 ENDP
PUBLIC winmmStub68
winmmStub68 PROC
mov rax,QWORD PTR [winmmTargets+544]
test rax,rax
jnz winmmReady68
mov r11d,68
jmp winmmLazy
winmmReady68:
jmp rax
winmmStub68 ENDP
PUBLIC winmmStub69
winmmStub69 PROC
mov rax,QWORD PTR [winmmTargets+552]
test rax,rax
jnz winmmReady69
mov r11d,69
jmp winmmLazy
winmmReady69:
jmp rax
winmmStub69 ENDP
PUBLIC winmmStub70
winmmStub70 PROC
mov rax,QWORD PTR [winmmTargets+560]
test rax,rax
jnz winmmReady70
mov r11d,70
jmp winmmLazy
winmmReady70:
jmp rax
winmmStub70 ENDP
PUBLIC winmmStub71
winmmStub71 PROC
mov rax,QWORD PTR [winmmTargets+568]
test rax,rax
jnz winmmReady71
mov r11d,71
jmp winmmLazy
winmmReady71:
jmp rax
winmmStub71 ENDP
PUBLIC winmmStub72
winmmStub72 PROC
mov rax,QWORD PTR [winmmTargets+576]
test rax,rax
jnz winmmReady72
mov r11d,72
jmp winmmLazy
winmmReady72:
jmp rax
winmmStub72 ENDP
PUBLIC winmmStub73
winmmStub73 PROC
mov rax,QWORD PTR [winmmTargets+584]
test rax,rax
jnz winmmReady73
mov r11d,73
jmp winmmLazy
winmmReady73:
jmp rax
winmmStub73 ENDP
PUBLIC winmmStub74
winmmStub74 PROC
mov rax,QWORD PTR [winmmTargets+592]
test rax,rax
jnz winmmReady74
mov r11d,74
jmp winmmLazy
winmmReady74:
jmp rax
winmmStub74 ENDP
PUBLIC winmmStub75
winmmStub75 PROC
mov rax,QWORD PTR [winmmTargets+600]
test rax,rax
jnz winmmReady75
mov r11d,75
jmp winmmLazy
winmmReady75:
jmp rax
winmmStub75 ENDP
PUBLIC winmmStub76
winmmStub76 PROC
mov rax,QWORD PTR [winmmTargets+608]
test rax,rax
jnz winmmReady76
mov r11d,76
jmp winmmLazy
winmmReady76:
jmp rax
winmmStub76 ENDP
PUBLIC winmmStub77
winmmStub77 PROC
mov rax,QWORD PTR [winmmTargets+616]
test rax,rax
jnz winmmReady77
mov r11d,77
jmp winmmLazy
winmmReady77:
jmp rax
winmmStub77 ENDP
PUBLIC winmmStub78
winmmStub78 PROC
mov rax,QWORD PTR [winmmTargets+624]
test rax,rax
jnz winmmReady78
mov r11d,78
jmp winmmLazy
winmmReady78:
jmp rax
winmmStub78 ENDP
PUBLIC winmmStub79
winmmStub79 PROC
mov rax,QWORD PTR [winmmTargets+632]
test rax,rax
jnz winmmReady79
mov r11d,79
jmp winmmLazy
winmmReady79:
jmp rax
winmmStub79 ENDP
PUBLIC winmmStub80
winmmStub80 PROC
mov rax,QWORD PTR [winmmTargets+640]
test rax,rax
jnz winmmReady80
mov r11d,80
jmp winmmLazy
winmmReady80:
jmp rax
winmmStub80 ENDP
PUBLIC winmmStub81
winmmStub81 PROC
mov rax,QWORD PTR [winmmTargets+648]
test rax,rax
jnz winmmReady81
mov r11d,81
jmp winmmLazy
winmmReady81:
jmp rax
winmmStub81 ENDP
PUBLIC winmmStub82
winmmStub82 PROC
mov rax,QWORD PTR [winmmTargets+656]
test rax,rax
jnz winmmReady82
mov r11d,82
jmp winmmLazy
winmmReady82:
jmp rax
winmmStub82 ENDP
PUBLIC winmmStub83
winmmStub83 PROC
mov rax,QWORD PTR [winmmTargets+664]
test rax,rax
jnz winmmReady83
mov r11d,83
jmp winmmLazy
winmmReady83:
jmp rax
winmmStub83 ENDP
PUBLIC winmmStub84
winmmStub84 PROC
mov rax,QWORD PTR [winmmTargets+672]
test rax,rax
jnz winmmReady84
mov r11d,84
jmp winmmLazy
winmmReady84:
jmp rax
winmmStub84 ENDP
PUBLIC winmmStub85
winmmStub85 PROC
mov rax,QWORD PTR [winmmTargets+680]
test rax,rax
jnz winmmReady85
mov r11d,85
jmp winmmLazy
winmmReady85:
jmp rax
winmmStub85 ENDP
PUBLIC winmmStub86
winmmStub86 PROC
mov rax,QWORD PTR [winmmTargets+688]
test rax,rax
jnz winmmReady86
mov r11d,86
jmp winmmLazy
winmmReady86:
jmp rax
winmmStub86 ENDP
PUBLIC winmmStub87
winmmStub87 PROC
mov rax,QWORD PTR [winmmTargets+696]
test rax,rax
jnz winmmReady87
mov r11d,87
jmp winmmLazy
winmmReady87:
jmp rax
winmmStub87 ENDP
PUBLIC winmmStub88
winmmStub88 PROC
mov rax,QWORD PTR [winmmTargets+704]
test rax,rax
jnz winmmReady88
mov r11d,88
jmp winmmLazy
winmmReady88:
jmp rax
winmmStub88 ENDP
PUBLIC winmmStub89
winmmStub89 PROC
mov rax,QWORD PTR [winmmTargets+712]
test rax,rax
jnz winmmReady89
mov r11d,89
jmp winmmLazy
winmmReady89:
jmp rax
winmmStub89 ENDP
PUBLIC winmmStub90
winmmStub90 PROC
mov rax,QWORD PTR [winmmTargets+720]
test rax,rax
jnz winmmReady90
mov r11d,90
jmp winmmLazy
winmmReady90:
jmp rax
winmmStub90 ENDP
PUBLIC winmmStub91
winmmStub91 PROC
mov rax,QWORD PTR [winmmTargets+728]
test rax,rax
jnz winmmReady91
mov r11d,91
jmp winmmLazy
winmmReady91:
jmp rax
winmmStub91 ENDP
PUBLIC winmmStub92
winmmStub92 PROC
mov rax,QWORD PTR [winmmTargets+736]
test rax,rax
jnz winmmReady92
mov r11d,92
jmp winmmLazy
winmmReady92:
jmp rax
winmmStub92 ENDP
PUBLIC winmmStub93
winmmStub93 PROC
mov rax,QWORD PTR [winmmTargets+744]
test rax,rax
jnz winmmReady93
mov r11d,93
jmp winmmLazy
winmmReady93:
jmp rax
winmmStub93 ENDP
PUBLIC winmmStub94
winmmStub94 PROC
mov rax,QWORD PTR [winmmTargets+752]
test rax,rax
jnz winmmReady94
mov r11d,94
jmp winmmLazy
winmmReady94:
jmp rax
winmmStub94 ENDP
PUBLIC winmmStub95
winmmStub95 PROC
mov rax,QWORD PTR [winmmTargets+760]
test rax,rax
jnz winmmReady95
mov r11d,95
jmp winmmLazy
winmmReady95:
jmp rax
winmmStub95 ENDP
PUBLIC winmmStub96
winmmStub96 PROC
mov rax,QWORD PTR [winmmTargets+768]
test rax,rax
jnz winmmReady96
mov r11d,96
jmp winmmLazy
winmmReady96:
jmp rax
winmmStub96 ENDP
PUBLIC winmmStub97
winmmStub97 PROC
mov rax,QWORD PTR [winmmTargets+776]
test rax,rax
jnz winmmReady97
mov r11d,97
jmp winmmLazy
winmmReady97:
jmp rax
winmmStub97 ENDP
PUBLIC winmmStub98
winmmStub98 PROC
mov rax,QWORD PTR [winmmTargets+784]
test rax,rax
jnz winmmReady98
mov r11d,98
jmp winmmLazy
winmmReady98:
jmp rax
winmmStub98 ENDP
PUBLIC winmmStub99
winmmStub99 PROC
mov rax,QWORD PTR [winmmTargets+792]
test rax,rax
jnz winmmReady99
mov r11d,99
jmp winmmLazy
winmmReady99:
jmp rax
winmmStub99 ENDP
PUBLIC winmmStub100
winmmStub100 PROC
mov rax,QWORD PTR [winmmTargets+800]
test rax,rax
jnz winmmReady100
mov r11d,100
jmp winmmLazy
winmmReady100:
jmp rax
winmmStub100 ENDP
PUBLIC winmmStub101
winmmStub101 PROC
mov rax,QWORD PTR [winmmTargets+808]
test rax,rax
jnz winmmReady101
mov r11d,101
jmp winmmLazy
winmmReady101:
jmp rax
winmmStub101 ENDP
PUBLIC winmmStub102
winmmStub102 PROC
mov rax,QWORD PTR [winmmTargets+816]
test rax,rax
jnz winmmReady102
mov r11d,102
jmp winmmLazy
winmmReady102:
jmp rax
winmmStub102 ENDP
PUBLIC winmmStub103
winmmStub103 PROC
mov rax,QWORD PTR [winmmTargets+824]
test rax,rax
jnz winmmReady103
mov r11d,103
jmp winmmLazy
winmmReady103:
jmp rax
winmmStub103 ENDP
PUBLIC winmmStub104
winmmStub104 PROC
mov rax,QWORD PTR [winmmTargets+832]
test rax,rax
jnz winmmReady104
mov r11d,104
jmp winmmLazy
winmmReady104:
jmp rax
winmmStub104 ENDP
PUBLIC winmmStub105
winmmStub105 PROC
mov rax,QWORD PTR [winmmTargets+840]
test rax,rax
jnz winmmReady105
mov r11d,105
jmp winmmLazy
winmmReady105:
jmp rax
winmmStub105 ENDP
PUBLIC winmmStub106
winmmStub106 PROC
mov rax,QWORD PTR [winmmTargets+848]
test rax,rax
jnz winmmReady106
mov r11d,106
jmp winmmLazy
winmmReady106:
jmp rax
winmmStub106 ENDP
PUBLIC winmmStub107
winmmStub107 PROC
mov rax,QWORD PTR [winmmTargets+856]
test rax,rax
jnz winmmReady107
mov r11d,107
jmp winmmLazy
winmmReady107:
jmp rax
winmmStub107 ENDP
PUBLIC winmmStub108
winmmStub108 PROC
mov rax,QWORD PTR [winmmTargets+864]
test rax,rax
jnz winmmReady108
mov r11d,108
jmp winmmLazy
winmmReady108:
jmp rax
winmmStub108 ENDP
PUBLIC winmmStub109
winmmStub109 PROC
mov rax,QWORD PTR [winmmTargets+872]
test rax,rax
jnz winmmReady109
mov r11d,109
jmp winmmLazy
winmmReady109:
jmp rax
winmmStub109 ENDP
PUBLIC winmmStub110
winmmStub110 PROC
mov rax,QWORD PTR [winmmTargets+880]
test rax,rax
jnz winmmReady110
mov r11d,110
jmp winmmLazy
winmmReady110:
jmp rax
winmmStub110 ENDP
PUBLIC winmmStub111
winmmStub111 PROC
mov rax,QWORD PTR [winmmTargets+888]
test rax,rax
jnz winmmReady111
mov r11d,111
jmp winmmLazy
winmmReady111:
jmp rax
winmmStub111 ENDP
PUBLIC winmmStub112
winmmStub112 PROC
mov rax,QWORD PTR [winmmTargets+896]
test rax,rax
jnz winmmReady112
mov r11d,112
jmp winmmLazy
winmmReady112:
jmp rax
winmmStub112 ENDP
PUBLIC winmmStub113
winmmStub113 PROC
mov rax,QWORD PTR [winmmTargets+904]
test rax,rax
jnz winmmReady113
mov r11d,113
jmp winmmLazy
winmmReady113:
jmp rax
winmmStub113 ENDP
PUBLIC winmmStub114
winmmStub114 PROC
mov rax,QWORD PTR [winmmTargets+912]
test rax,rax
jnz winmmReady114
mov r11d,114
jmp winmmLazy
winmmReady114:
jmp rax
winmmStub114 ENDP
PUBLIC winmmStub115
winmmStub115 PROC
mov rax,QWORD PTR [winmmTargets+920]
test rax,rax
jnz winmmReady115
mov r11d,115
jmp winmmLazy
winmmReady115:
jmp rax
winmmStub115 ENDP
PUBLIC winmmStub116
winmmStub116 PROC
mov rax,QWORD PTR [winmmTargets+928]
test rax,rax
jnz winmmReady116
mov r11d,116
jmp winmmLazy
winmmReady116:
jmp rax
winmmStub116 ENDP
PUBLIC winmmStub117
winmmStub117 PROC
mov rax,QWORD PTR [winmmTargets+936]
test rax,rax
jnz winmmReady117
mov r11d,117
jmp winmmLazy
winmmReady117:
jmp rax
winmmStub117 ENDP
PUBLIC winmmStub118
winmmStub118 PROC
mov rax,QWORD PTR [winmmTargets+944]
test rax,rax
jnz winmmReady118
mov r11d,118
jmp winmmLazy
winmmReady118:
jmp rax
winmmStub118 ENDP
PUBLIC winmmStub119
winmmStub119 PROC
mov rax,QWORD PTR [winmmTargets+952]
test rax,rax
jnz winmmReady119
mov r11d,119
jmp winmmLazy
winmmReady119:
jmp rax
winmmStub119 ENDP
PUBLIC winmmStub120
winmmStub120 PROC
mov rax,QWORD PTR [winmmTargets+960]
test rax,rax
jnz winmmReady120
mov r11d,120
jmp winmmLazy
winmmReady120:
jmp rax
winmmStub120 ENDP
PUBLIC winmmStub121
winmmStub121 PROC
mov rax,QWORD PTR [winmmTargets+968]
test rax,rax
jnz winmmReady121
mov r11d,121
jmp winmmLazy
winmmReady121:
jmp rax
winmmStub121 ENDP
PUBLIC winmmStub122
winmmStub122 PROC
mov rax,QWORD PTR [winmmTargets+976]
test rax,rax
jnz winmmReady122
mov r11d,122
jmp winmmLazy
winmmReady122:
jmp rax
winmmStub122 ENDP
PUBLIC winmmStub123
winmmStub123 PROC
mov rax,QWORD PTR [winmmTargets+984]
test rax,rax
jnz winmmReady123
mov r11d,123
jmp winmmLazy
winmmReady123:
jmp rax
winmmStub123 ENDP
PUBLIC winmmStub124
winmmStub124 PROC
mov rax,QWORD PTR [winmmTargets+992]
test rax,rax
jnz winmmReady124
mov r11d,124
jmp winmmLazy
winmmReady124:
jmp rax
winmmStub124 ENDP
PUBLIC winmmStub125
winmmStub125 PROC
mov rax,QWORD PTR [winmmTargets+1000]
test rax,rax
jnz winmmReady125
mov r11d,125
jmp winmmLazy
winmmReady125:
jmp rax
winmmStub125 ENDP
PUBLIC winmmStub126
winmmStub126 PROC
mov rax,QWORD PTR [winmmTargets+1008]
test rax,rax
jnz winmmReady126
mov r11d,126
jmp winmmLazy
winmmReady126:
jmp rax
winmmStub126 ENDP
PUBLIC winmmStub127
winmmStub127 PROC
mov rax,QWORD PTR [winmmTargets+1016]
test rax,rax
jnz winmmReady127
mov r11d,127
jmp winmmLazy
winmmReady127:
jmp rax
winmmStub127 ENDP
PUBLIC winmmStub128
winmmStub128 PROC
mov rax,QWORD PTR [winmmTargets+1024]
test rax,rax
jnz winmmReady128
mov r11d,128
jmp winmmLazy
winmmReady128:
jmp rax
winmmStub128 ENDP
PUBLIC winmmStub129
winmmStub129 PROC
mov rax,QWORD PTR [winmmTargets+1032]
test rax,rax
jnz winmmReady129
mov r11d,129
jmp winmmLazy
winmmReady129:
jmp rax
winmmStub129 ENDP
PUBLIC winmmStub130
winmmStub130 PROC
mov rax,QWORD PTR [winmmTargets+1040]
test rax,rax
jnz winmmReady130
mov r11d,130
jmp winmmLazy
winmmReady130:
jmp rax
winmmStub130 ENDP
PUBLIC winmmStub131
winmmStub131 PROC
mov rax,QWORD PTR [winmmTargets+1048]
test rax,rax
jnz winmmReady131
mov r11d,131
jmp winmmLazy
winmmReady131:
jmp rax
winmmStub131 ENDP
PUBLIC winmmStub132
winmmStub132 PROC
mov rax,QWORD PTR [winmmTargets+1056]
test rax,rax
jnz winmmReady132
mov r11d,132
jmp winmmLazy
winmmReady132:
jmp rax
winmmStub132 ENDP
PUBLIC winmmStub133
winmmStub133 PROC
mov rax,QWORD PTR [winmmTargets+1064]
test rax,rax
jnz winmmReady133
mov r11d,133
jmp winmmLazy
winmmReady133:
jmp rax
winmmStub133 ENDP
PUBLIC winmmStub134
winmmStub134 PROC
mov rax,QWORD PTR [winmmTargets+1072]
test rax,rax
jnz winmmReady134
mov r11d,134
jmp winmmLazy
winmmReady134:
jmp rax
winmmStub134 ENDP
PUBLIC winmmStub135
winmmStub135 PROC
mov rax,QWORD PTR [winmmTargets+1080]
test rax,rax
jnz winmmReady135
mov r11d,135
jmp winmmLazy
winmmReady135:
jmp rax
winmmStub135 ENDP
PUBLIC winmmStub136
winmmStub136 PROC
mov rax,QWORD PTR [winmmTargets+1088]
test rax,rax
jnz winmmReady136
mov r11d,136
jmp winmmLazy
winmmReady136:
jmp rax
winmmStub136 ENDP
PUBLIC winmmStub137
winmmStub137 PROC
mov rax,QWORD PTR [winmmTargets+1096]
test rax,rax
jnz winmmReady137
mov r11d,137
jmp winmmLazy
winmmReady137:
jmp rax
winmmStub137 ENDP
PUBLIC winmmStub138
winmmStub138 PROC
mov rax,QWORD PTR [winmmTargets+1104]
test rax,rax
jnz winmmReady138
mov r11d,138
jmp winmmLazy
winmmReady138:
jmp rax
winmmStub138 ENDP
PUBLIC winmmStub139
winmmStub139 PROC
mov rax,QWORD PTR [winmmTargets+1112]
test rax,rax
jnz winmmReady139
mov r11d,139
jmp winmmLazy
winmmReady139:
jmp rax
winmmStub139 ENDP
PUBLIC winmmStub140
winmmStub140 PROC
mov rax,QWORD PTR [winmmTargets+1120]
test rax,rax
jnz winmmReady140
mov r11d,140
jmp winmmLazy
winmmReady140:
jmp rax
winmmStub140 ENDP
PUBLIC winmmStub141
winmmStub141 PROC
mov rax,QWORD PTR [winmmTargets+1128]
test rax,rax
jnz winmmReady141
mov r11d,141
jmp winmmLazy
winmmReady141:
jmp rax
winmmStub141 ENDP
PUBLIC winmmStub142
winmmStub142 PROC
mov rax,QWORD PTR [winmmTargets+1136]
test rax,rax
jnz winmmReady142
mov r11d,142
jmp winmmLazy
winmmReady142:
jmp rax
winmmStub142 ENDP
PUBLIC winmmStub143
winmmStub143 PROC
mov rax,QWORD PTR [winmmTargets+1144]
test rax,rax
jnz winmmReady143
mov r11d,143
jmp winmmLazy
winmmReady143:
jmp rax
winmmStub143 ENDP
PUBLIC winmmStub144
winmmStub144 PROC
mov rax,QWORD PTR [winmmTargets+1152]
test rax,rax
jnz winmmReady144
mov r11d,144
jmp winmmLazy
winmmReady144:
jmp rax
winmmStub144 ENDP
PUBLIC winmmStub145
winmmStub145 PROC
mov rax,QWORD PTR [winmmTargets+1160]
test rax,rax
jnz winmmReady145
mov r11d,145
jmp winmmLazy
winmmReady145:
jmp rax
winmmStub145 ENDP
PUBLIC winmmStub146
winmmStub146 PROC
mov rax,QWORD PTR [winmmTargets+1168]
test rax,rax
jnz winmmReady146
mov r11d,146
jmp winmmLazy
winmmReady146:
jmp rax
winmmStub146 ENDP
PUBLIC winmmStub147
winmmStub147 PROC
mov rax,QWORD PTR [winmmTargets+1176]
test rax,rax
jnz winmmReady147
mov r11d,147
jmp winmmLazy
winmmReady147:
jmp rax
winmmStub147 ENDP
PUBLIC winmmStub148
winmmStub148 PROC
mov rax,QWORD PTR [winmmTargets+1184]
test rax,rax
jnz winmmReady148
mov r11d,148
jmp winmmLazy
winmmReady148:
jmp rax
winmmStub148 ENDP
PUBLIC winmmStub149
winmmStub149 PROC
mov rax,QWORD PTR [winmmTargets+1192]
test rax,rax
jnz winmmReady149
mov r11d,149
jmp winmmLazy
winmmReady149:
jmp rax
winmmStub149 ENDP
PUBLIC winmmStub150
winmmStub150 PROC
mov rax,QWORD PTR [winmmTargets+1200]
test rax,rax
jnz winmmReady150
mov r11d,150
jmp winmmLazy
winmmReady150:
jmp rax
winmmStub150 ENDP
PUBLIC winmmStub151
winmmStub151 PROC
mov rax,QWORD PTR [winmmTargets+1208]
test rax,rax
jnz winmmReady151
mov r11d,151
jmp winmmLazy
winmmReady151:
jmp rax
winmmStub151 ENDP
PUBLIC winmmStub152
winmmStub152 PROC
mov rax,QWORD PTR [winmmTargets+1216]
test rax,rax
jnz winmmReady152
mov r11d,152
jmp winmmLazy
winmmReady152:
jmp rax
winmmStub152 ENDP
PUBLIC winmmStub153
winmmStub153 PROC
mov rax,QWORD PTR [winmmTargets+1224]
test rax,rax
jnz winmmReady153
mov r11d,153
jmp winmmLazy
winmmReady153:
jmp rax
winmmStub153 ENDP
PUBLIC winmmStub154
winmmStub154 PROC
mov rax,QWORD PTR [winmmTargets+1232]
test rax,rax
jnz winmmReady154
mov r11d,154
jmp winmmLazy
winmmReady154:
jmp rax
winmmStub154 ENDP
PUBLIC winmmStub155
winmmStub155 PROC
mov rax,QWORD PTR [winmmTargets+1240]
test rax,rax
jnz winmmReady155
mov r11d,155
jmp winmmLazy
winmmReady155:
jmp rax
winmmStub155 ENDP
PUBLIC winmmStub156
winmmStub156 PROC
mov rax,QWORD PTR [winmmTargets+1248]
test rax,rax
jnz winmmReady156
mov r11d,156
jmp winmmLazy
winmmReady156:
jmp rax
winmmStub156 ENDP
PUBLIC winmmStub157
winmmStub157 PROC
mov rax,QWORD PTR [winmmTargets+1256]
test rax,rax
jnz winmmReady157
mov r11d,157
jmp winmmLazy
winmmReady157:
jmp rax
winmmStub157 ENDP
PUBLIC winmmStub158
winmmStub158 PROC
mov rax,QWORD PTR [winmmTargets+1264]
test rax,rax
jnz winmmReady158
mov r11d,158
jmp winmmLazy
winmmReady158:
jmp rax
winmmStub158 ENDP
PUBLIC winmmStub159
winmmStub159 PROC
mov rax,QWORD PTR [winmmTargets+1272]
test rax,rax
jnz winmmReady159
mov r11d,159
jmp winmmLazy
winmmReady159:
jmp rax
winmmStub159 ENDP
PUBLIC winmmStub160
winmmStub160 PROC
mov rax,QWORD PTR [winmmTargets+1280]
test rax,rax
jnz winmmReady160
mov r11d,160
jmp winmmLazy
winmmReady160:
jmp rax
winmmStub160 ENDP
PUBLIC winmmStub161
winmmStub161 PROC
mov rax,QWORD PTR [winmmTargets+1288]
test rax,rax
jnz winmmReady161
mov r11d,161
jmp winmmLazy
winmmReady161:
jmp rax
winmmStub161 ENDP
PUBLIC winmmStub162
winmmStub162 PROC
mov rax,QWORD PTR [winmmTargets+1296]
test rax,rax
jnz winmmReady162
mov r11d,162
jmp winmmLazy
winmmReady162:
jmp rax
winmmStub162 ENDP
PUBLIC winmmStub163
winmmStub163 PROC
mov rax,QWORD PTR [winmmTargets+1304]
test rax,rax
jnz winmmReady163
mov r11d,163
jmp winmmLazy
winmmReady163:
jmp rax
winmmStub163 ENDP
PUBLIC winmmStub164
winmmStub164 PROC
mov rax,QWORD PTR [winmmTargets+1312]
test rax,rax
jnz winmmReady164
mov r11d,164
jmp winmmLazy
winmmReady164:
jmp rax
winmmStub164 ENDP
PUBLIC winmmStub165
winmmStub165 PROC
mov rax,QWORD PTR [winmmTargets+1320]
test rax,rax
jnz winmmReady165
mov r11d,165
jmp winmmLazy
winmmReady165:
jmp rax
winmmStub165 ENDP
PUBLIC winmmStub166
winmmStub166 PROC
mov rax,QWORD PTR [winmmTargets+1328]
test rax,rax
jnz winmmReady166
mov r11d,166
jmp winmmLazy
winmmReady166:
jmp rax
winmmStub166 ENDP
PUBLIC winmmStub167
winmmStub167 PROC
mov rax,QWORD PTR [winmmTargets+1336]
test rax,rax
jnz winmmReady167
mov r11d,167
jmp winmmLazy
winmmReady167:
jmp rax
winmmStub167 ENDP
PUBLIC winmmStub168
winmmStub168 PROC
mov rax,QWORD PTR [winmmTargets+1344]
test rax,rax
jnz winmmReady168
mov r11d,168
jmp winmmLazy
winmmReady168:
jmp rax
winmmStub168 ENDP
PUBLIC winmmStub169
winmmStub169 PROC
mov rax,QWORD PTR [winmmTargets+1352]
test rax,rax
jnz winmmReady169
mov r11d,169
jmp winmmLazy
winmmReady169:
jmp rax
winmmStub169 ENDP
PUBLIC winmmStub170
winmmStub170 PROC
mov rax,QWORD PTR [winmmTargets+1360]
test rax,rax
jnz winmmReady170
mov r11d,170
jmp winmmLazy
winmmReady170:
jmp rax
winmmStub170 ENDP
PUBLIC winmmStub171
winmmStub171 PROC
mov rax,QWORD PTR [winmmTargets+1368]
test rax,rax
jnz winmmReady171
mov r11d,171
jmp winmmLazy
winmmReady171:
jmp rax
winmmStub171 ENDP
PUBLIC winmmStub172
winmmStub172 PROC
mov rax,QWORD PTR [winmmTargets+1376]
test rax,rax
jnz winmmReady172
mov r11d,172
jmp winmmLazy
winmmReady172:
jmp rax
winmmStub172 ENDP
PUBLIC winmmStub173
winmmStub173 PROC
mov rax,QWORD PTR [winmmTargets+1384]
test rax,rax
jnz winmmReady173
mov r11d,173
jmp winmmLazy
winmmReady173:
jmp rax
winmmStub173 ENDP
PUBLIC winmmStub174
winmmStub174 PROC
mov rax,QWORD PTR [winmmTargets+1392]
test rax,rax
jnz winmmReady174
mov r11d,174
jmp winmmLazy
winmmReady174:
jmp rax
winmmStub174 ENDP
PUBLIC winmmStub175
winmmStub175 PROC
mov rax,QWORD PTR [winmmTargets+1400]
test rax,rax
jnz winmmReady175
mov r11d,175
jmp winmmLazy
winmmReady175:
jmp rax
winmmStub175 ENDP
PUBLIC winmmStub176
winmmStub176 PROC
mov rax,QWORD PTR [winmmTargets+1408]
test rax,rax
jnz winmmReady176
mov r11d,176
jmp winmmLazy
winmmReady176:
jmp rax
winmmStub176 ENDP
PUBLIC winmmStub177
winmmStub177 PROC
mov rax,QWORD PTR [winmmTargets+1416]
test rax,rax
jnz winmmReady177
mov r11d,177
jmp winmmLazy
winmmReady177:
jmp rax
winmmStub177 ENDP
PUBLIC winmmStub178
winmmStub178 PROC
mov rax,QWORD PTR [winmmTargets+1424]
test rax,rax
jnz winmmReady178
mov r11d,178
jmp winmmLazy
winmmReady178:
jmp rax
winmmStub178 ENDP
PUBLIC winmmStub179
winmmStub179 PROC
mov rax,QWORD PTR [winmmTargets+1432]
test rax,rax
jnz winmmReady179
mov r11d,179
jmp winmmLazy
winmmReady179:
jmp rax
winmmStub179 ENDP
PUBLIC winmmStub180
winmmStub180 PROC
mov rax,QWORD PTR [winmmTargets+1440]
test rax,rax
jnz winmmReady180
mov r11d,180
jmp winmmLazy
winmmReady180:
jmp rax
winmmStub180 ENDP
winmmLazy PROC
sub rsp,88h
mov [rsp+20h],rcx
mov [rsp+28h],rdx
mov [rsp+30h],r8
mov [rsp+38h],r9
movdqu [rsp+40h],xmm0
movdqu [rsp+50h],xmm1
movdqu [rsp+60h],xmm2
movdqu [rsp+70h],xmm3
mov ecx,r11d
call winmmResolve
mov rcx,[rsp+20h]
mov rdx,[rsp+28h]
mov r8,[rsp+30h]
mov r9,[rsp+38h]
movdqu xmm0,[rsp+40h]
movdqu xmm1,[rsp+50h]
movdqu xmm2,[rsp+60h]
movdqu xmm3,[rsp+70h]
add rsp,88h
jmp rax
winmmLazy ENDP
END
