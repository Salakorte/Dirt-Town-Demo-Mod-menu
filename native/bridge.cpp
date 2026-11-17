#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <tlhelp32.h>
#include <cstdint>
#include <cstddef>
#include <cstdio>
#include <cstring>
#include <cmath>
#include <string>
#include <vector>

// Exact-build bridge. The external coordinator replicates creation through Steam lobby metadata.
struct Shared {
    uint32_t magic,request,completed,command;
    int32_t vehicle,material;
    float amount,x,z,yaw;
    uint32_t heartbeat,enabled;
    char model[64];
    char reply[8192];
};
static_assert(offsetof(Shared,reply)==112);
static Shared* shared;
static uintptr_t base;
static DWORD mainThread;
using Tick=bool(__fastcall*)(void*);
static Tick original;
static thread_local bool inside=false;
template<class T> T& at(void* p,size_t n){return *reinterpret_cast<T*>(reinterpret_cast<uintptr_t>(p)+n);}
static bool readable(const void* ptr,size_t size){
    MEMORY_BASIC_INFORMATION m{};
    if(!ptr || !VirtualQuery(ptr,&m,sizeof(m)) || m.State!=MEM_COMMIT || (m.Protect&(PAGE_GUARD|PAGE_NOACCESS)))return false;
    return uintptr_t(ptr)+size<=uintptr_t(m.BaseAddress)+m.RegionSize;
}
static std::string text(const char* s){
    if(!readable(s,128))return "?";
    size_t n=strnlen_s(s,127);std::string out;
    for(size_t i=0;i<n;i++){char c=s[i];if(c=='"'||c=='\\')out+='\\';if(c>=32&&c<127)out+=c;}
    return out;
}
static void reply(const std::string& value){strncpy_s(shared->reply,value.c_str(),_TRUNCATE);}
static bool worldOK(void* w){
    if(!readable(w,0x13200))return false;
    auto begin=at<uintptr_t>(w,0xE8C0),end=at<uintptr_t>(w,0xE8C8);
    return end>=begin && (end-begin)%8==0 && (end-begin)/8<256 && readable((void*)begin,size_t(end-begin));
}
static void* machine(void* w,int id){
    auto begin=at<uintptr_t>(w,0xE8C0),end=at<uintptr_t>(w,0xE8C8);
    if(id<0 || uint64_t(id)>=(end-begin)/8)return nullptr;
    void* m=reinterpret_cast<void**>(begin)[id];return readable(m,0xC00)?m:nullptr;
}
static bool materialOK(void* w,int id){
    auto begin=at<uintptr_t>(w,0x15B0),end=at<uintptr_t>(w,0x15B8);
    return end>=begin && (end-begin)%0x128==0 && id>=0 && size_t(id)<(end-begin)/0x128 && (end-begin)/0x128<128;
}
static void snapshot(void* w){
    std::string out="{\"ok\":true,\"materials\":[";
    auto begin=at<uintptr_t>(w,0x15B0),end=at<uintptr_t>(w,0x15B8);
    size_t n=(end>=begin)?(end-begin)/0x128:0;
    if(n>128)throw "Tabla de materiales incompatible";
    for(size_t i=0;i<n;i++){
        auto rec=(void*)(begin+i*0x128);if(!readable(rec,0x128))throw "Material no legible";
        if(i)out+=',';out+="{\"id\":"+std::to_string(i)+",\"name\":\""+text(at<char*>(rec,0))+"\"}";
    }
    out+="],\"vehicles\":[";
    size_t count=(at<uintptr_t>(w,0xE8C8)-at<uintptr_t>(w,0xE8C0))/8;bool first=true;
    for(size_t i=0;i<count;i++){
        void* m=machine(w,(int)i);if(!m)continue;
        std::string name=text(at<char*>(m,8));
        if(!first)out+=',';first=false;
        alignas(32) double position[4]{};
        auto vtable=at<void**>(m,0);
        using Position=void*(__fastcall*)(void*,void*);
        reinterpret_cast<Position>(vtable[0x88/8])(position,m);
        out+="{\"id\":"+std::to_string(i)+",\"name\":\""+name+"\",\"x\":"+std::to_string(position[0])+",\"z\":"+std::to_string(position[2])+"}";
    }
    out+="],\"selected\":"+std::to_string(at<int>(w,0xEAA0))+"}";reply(out);
}
struct GnuString{const char* data;uint64_t length;char storage[16];};
#include "steam_lobby_bridge.h"
static void execute(void* w){
    if(shared->request==shared->completed)return;
    uint32_t request=shared->request;
    try{
        if(!worldOK(w))throw "Carga una partida primero";
        if(shared->command==1)snapshot(w);
        else if(shared->command==2){
            void* m=machine(w,shared->vehicle);
            if(!m || text(at<char*>(m,8)).find("hauler")==std::string::npos)throw "Selecciona un volquete hauler";
            if(!materialOK(w,shared->material))throw "Material no valido";
            if(!std::isfinite(shared->amount)||shared->amount<0||shared->amount>20)throw "Cantidad entre 0.1 y 20 m3; 0 llena hasta capacidad";
            using Fill=double(__fastcall*)(void*,void*,int,double);
            double added=reinterpret_cast<Fill>(base+0xC365B0)(w,m,shared->material,shared->amount==0?1e9:shared->amount);
            reply("{\"ok\":true,\"added\":"+std::to_string(added)+"}");
        }
        else if(shared->command==3){
            // Constructors present in the native executable; this is currently a local operation.
            static const char* models[]={"dozer","hauler","bandvagn","truck","excavator","large_loader","medium_loader","tractor","dirtbike","snowmobile","hooklift","feeder"};
            bool valid=false;for(auto name:models)if(strcmp(shared->model,name)==0)valid=true;
            if(!valid)throw "Clase no admitida por la demo";
            if(!std::isfinite(shared->x)||!std::isfinite(shared->z)||fabs(shared->x)>10000||fabs(shared->z)>10000)throw "Posicion no valida";
            GnuString s{shared->model,strlen(shared->model),{}};
            size_t before=(at<uintptr_t>(w,0xE8C8)-at<uintptr_t>(w,0xE8C0))/8;
            bool network=at<unsigned char>(w,0xEE18)!=0;
            std::vector<int> owners;
            if(network){
                auto start=at<int*>(w,0xEAF0),end=at<int*>(w,0xEAF8);
                if(uintptr_t(end)<uintptr_t(start)||uintptr_t(end)-uintptr_t(start)!=before*4||!readable(start,before*4))throw "Registro de conductores incompatible";
                if(shared->vehicle<0||shared->vehicle>1024)throw "Propietario nativo no valido";
                owners.assign(start,end);
            }
            using Spawn=int(__fastcall*)(void*,GnuString*,float,float,float);
            // The front-end gate limits classes and refuses session spawns. Skipping it does NOT
            // replicate creation to peers. A host-authoritative creation protocol is still required.
            // Restore the original gate immediately after this main-thread call.
            auto gate=(unsigned char*)(base+0xA77180);
            unsigned char saved[7];memcpy(saved,gate,7);
            const unsigned char expectedGate[]={0x80,0xB9,0x30,0xB9,0,0,0};
            if(memcmp(saved,expectedGate,7))throw "El selector nativo ha cambiado";
            DWORD old;
            if(!VirtualProtect(gate,7,PAGE_EXECUTE_READWRITE,&old))throw "No se pudo preparar el selector";
            gate[0]=0xE9;int32_t delta=int32_t(0xA779C0-0xA77180-5);memcpy(gate+1,&delta,4);gate[5]=gate[6]=0x90;FlushInstructionCache(GetCurrentProcess(),gate,7);
            int id=reinterpret_cast<Spawn>(base+0xA77150)(w,&s,shared->x,shared->z,0.f);
            memcpy(gate,saved,7);DWORD ignored;VirtualProtect(gate,7,old,&ignored);FlushInstructionCache(GetCurrentProcess(),gate,7);
            if(id<0)throw "El juego ha rechazado crear el vehiculo (consulta dirttown.log)";
            size_t after=(at<uintptr_t>(w,0xE8C8)-at<uintptr_t>(w,0xE8C0))/8;
            if(size_t(id)!=before||after!=before+1)throw "Creacion inesperada; no repetir, reinicia la sesion";
            if(network){
                auto start=at<int*>(w,0xEAF0),end=at<int*>(w,0xEAF8);
                if(uintptr_t(end)<uintptr_t(start)||uintptr_t(end)-uintptr_t(start)!=after*4||!readable(start,after*4))throw "Registro de conductores incompleto; reinicia la sesion";
                // Keep all existing handovers; only the appended machine starts under the host.
                memcpy(start,owners.data(),owners.size()*sizeof(int));start[id]=shared->vehicle;
            }
            reply("{\"ok\":true,\"vehicle\":"+std::to_string(id)+"}");
        }
        else if(shared->command==4){
            // Edit the source field, so every gameplay check observes the same unlocked state.
            auto specs=at<void**>(w,0xE8D8);
            size_t count=(at<uintptr_t>(w,0xE8C8)-at<uintptr_t>(w,0xE8C0))/8;int unlocked=0;
            for(size_t i=0;i<count;i++){
                void* m=machine(w,(int)i);if(!m||text(at<char*>(m,8)).find("excavator")==std::string::npos)continue;
                if(!readable(specs+i,8)||!readable(specs[i],0x180))continue;
                auto spec=specs[i];auto len=at<uint64_t>(spec,0x170);auto str=at<char*>(spec,0x168);
                if(len>0&&len<512&&readable(str,size_t(len)+1)){str[0]=0;at<uint64_t>(spec,0x170)=0;unlocked++;}
            }
            reply("{\"ok\":true,\"unlocked\":"+std::to_string(unlocked)+"}");
        }
        else if(shared->command==5){
            if(!materialOK(w,shared->material))throw "Material no valido";
            if(shared->amount<0.25f||shared->amount>5||shared->yaw<0.1f||shared->yaw>3||!std::isfinite(shared->x)||!std::isfinite(shared->z))throw "Radio entre 0.25 y 5 m y altura entre 0.1 y 3 m";
            if(at<uintptr_t>(w,0x17648))throw "El juego esta reconstruyendo el terreno; espera";
            alignas(32) unsigned char spec[0x420]{};
            // The native converter reads even empty strings, including the one at +0x2D8.
            // Match its constructor's small-string layout instead of using null pointers.
            for(size_t offset:{size_t(0),size_t(0x20),size_t(0x90),size_t(0xB8),size_t(0x130),size_t(0x150),size_t(0x178),size_t(0x198),size_t(0x1C0),size_t(0x220),size_t(0x270),size_t(0x2D8),size_t(0x2F8),size_t(0x368)}){
                at<char*>(spec,offset)=reinterpret_cast<char*>(spec+offset+16);
            }
            const char* type="pile";
            auto material=(void*)(at<uintptr_t>(w,0x15B0)+size_t(shared->material)*0x128);
            const char* mat=at<char*>(material,0);
            at<GnuString>(spec,0)=GnuString{type,4,{}};
            at<GnuString>(spec,0x20)=GnuString{mat,strlen(mat),{}};
            at<float>(spec,0x40)=shared->x;at<float>(spec,0x44)=shared->z;
            at<float>(spec,0x48)=shared->amount;at<float>(spec,0x4c)=shared->yaw;
            alignas(32) unsigned char event[0x1000]{};
            using Make=void*(__fastcall*)(void*,void*,void*);
            reinterpret_cast<Make>(base+0x388940)(event,w,spec);
            if(event[0x28]!=0x53)throw "El juego no ha reconocido la operacion pile";
            uint64_t before=at<uintptr_t>(w,0x1100)-at<uintptr_t>(w,0x10f8);
            using Apply=void(__fastcall*)(void*,void*);
            reinterpret_cast<Apply>(base+0xA02A80)(w,event);
            using Destroy=void(__fastcall*)(void*);
            reinterpret_cast<Destroy>(base+0x1318900)(event);
            uint64_t after=at<uintptr_t>(w,0x1100)-at<uintptr_t>(w,0x10f8);
            if(after<=before)throw "El juego no ha aceptado el monton; consulta dirttown.log";
            reply("{\"ok\":true,\"queued\":true}");
        }
        else if(shared->command==6)lobbybridge::snapshot(w);
        else if(shared->command==7)lobbybridge::publish();
        else throw "Comando desconocido";
    }catch(const char* message){reply("{\"ok\":false,\"error\":\""+std::string(message)+"\"}");}
    catch(...){reply("{\"ok\":false,\"error\":\"Excepcion interna; no repetir la accion\"}");}
    MemoryBarrier();InterlockedExchange((LONG*)&shared->completed,request);
}
static bool __fastcall hook(void* w){
    bool result=original(w);
    if(shared&&!inside&&GetCurrentThreadId()==mainThread){inside=true;shared->heartbeat=GetTickCount();execute(w);inside=false;}
    return result;
}
static void jump(unsigned char* p,const void* target){p[0]=0xFF;p[1]=0x25;memset(p+2,0,4);memcpy(p+6,&target,8);}
static DWORD WINAPI initialize(void*){
    base=(uintptr_t)GetModuleHandleW(nullptr);
    wchar_t exe[MAX_PATH];GetModuleFileNameW(nullptr,exe,MAX_PATH);
    if(!wcsstr(exe,L"dirttown.exe"))return 6;
    const unsigned char expected[]={0x48,0x83,0xEC,0x38,0x48,0xBA,0,0,0,0,2,0,0,0};
    auto target=(unsigned char*)(base+0x939DB0);
    Tick previous=nullptr;
    if(memcmp(target,expected,sizeof(expected))){
        if(target[0]!=0xFF||target[1]!=0x25)return 1;
        memcpy(&previous,target+6,8);MEMORY_BASIC_INFORMATION m{};
        if(!VirtualQuery((void*)previous,&m,sizeof(m))||(m.AllocationBase!=GetModuleHandleW(L"DirtTownBridge4.dll")&&m.AllocationBase!=GetModuleHandleW(L"DirtTownBridge5.dll")&&m.AllocationBase!=GetModuleHandleW(L"DirtTownBridge6.dll")))return 1;
    }
    wchar_t name[96];swprintf_s(name,L"Local\\DirtTownMenuBridge8-%lu",GetCurrentProcessId());
    HANDLE mapping=CreateFileMappingW(INVALID_HANDLE_VALUE,nullptr,PAGE_READWRITE,0,sizeof(Shared),name);
    if(!mapping)return 2;
    shared=(Shared*)MapViewOfFile(mapping,FILE_MAP_ALL_ACCESS,0,0,sizeof(Shared));if(!shared)return 3;
    memset(shared,0,sizeof(Shared));shared->magic=0x44544D33;
    auto trampoline=(unsigned char*)VirtualAlloc(nullptr,64,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);
    if(!trampoline)return 4;
    memcpy(trampoline,expected,sizeof(expected));jump(trampoline+sizeof(expected),target+sizeof(expected));
    DWORD protect;VirtualProtect(trampoline,64,PAGE_EXECUTE_READ,&protect);FlushInstructionCache(GetCurrentProcess(),trampoline,64);
    original=previous?previous:(Tick)trampoline;
    std::vector<HANDLE> stopped;
    HANDLE snap=CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD,0);THREADENTRY32 entry{sizeof(entry)};bool failed=false;ULARGE_INTEGER oldest;oldest.QuadPart=~0ull;
    if(Thread32First(snap,&entry))do{if(entry.th32OwnerProcessID==GetCurrentProcessId()&&entry.th32ThreadID!=GetCurrentThreadId()){
        HANDLE h=OpenThread(THREAD_SUSPEND_RESUME|THREAD_GET_CONTEXT|THREAD_SET_CONTEXT|THREAD_QUERY_INFORMATION,FALSE,entry.th32ThreadID);
        if(!h||SuspendThread(h)==DWORD(-1)){if(h)CloseHandle(h);failed=true;break;}stopped.push_back(h);
        FILETIME created,exited,kernel,user;
        if(GetThreadTimes(h,&created,&exited,&kernel,&user)){
            ULARGE_INTEGER time;time.LowPart=created.dwLowDateTime;time.HighPart=created.dwHighDateTime;
            if(time.QuadPart<oldest.QuadPart){oldest=time;mainThread=entry.th32ThreadID;}
        }
        CONTEXT context{};context.ContextFlags=CONTEXT_CONTROL;
        if(!GetThreadContext(h,&context)){failed=true;break;}
        if(context.Rip>=uintptr_t(target)&&context.Rip<uintptr_t(target)+sizeof(expected)){
            context.Rip=uintptr_t(trampoline)+context.Rip-uintptr_t(target);
            if(!SetThreadContext(h,&context)){failed=true;break;}
        }
    }}while(Thread32Next(snap,&entry));else failed=true;CloseHandle(snap);
    if(!failed&&VirtualProtect(target,sizeof(expected),PAGE_EXECUTE_READWRITE,&protect)){
        jump(target,(void*)hook);memset(target+14,0x90,sizeof(expected)-14);
        DWORD ignored;VirtualProtect(target,sizeof(expected),protect,&ignored);FlushInstructionCache(GetCurrentProcess(),target,sizeof(expected));shared->enabled=1;
    }
    for(auto h:stopped){ResumeThread(h);CloseHandle(h);}return shared->enabled?0:5;
}
BOOL WINAPI DllMain(HINSTANCE instance,DWORD reason,LPVOID){
    if(reason==DLL_PROCESS_ATTACH){DisableThreadLibraryCalls(instance);HANDLE thread=CreateThread(nullptr,0,initialize,nullptr,0,nullptr);if(thread)CloseHandle(thread);}return TRUE;
}
