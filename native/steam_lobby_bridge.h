#pragma once
#include "coop_layout.h"
namespace lobbybridge {
constexpr const char* operationKey="dtmenu_spawn_v1";
constexpr const char* memberKey="dtmenu_ready_v1";
template<class T>T bind(const char* name){auto dll=GetModuleHandleW(L"steam_api64.dll");return reinterpret_cast<T>(dll?GetProcAddress(dll,name):nullptr);}
static std::string quoted(const char* s){
    std::string out="\"";if(s)for(size_t i=0;i<3500&&readable(s+i,1)&&s[i];i++){
        unsigned char c=s[i];if(c=='"'||c=='\\')out+='\\';if(c>=32&&c<127)out+=char(c);
    }return out+'"';
}
static uint64_t roster(void* w){
    uint64_t hash=14695981039346656037ull;
    size_t count=(at<uintptr_t>(w,0xE8C8)-at<uintptr_t>(w,0xE8C0))/8;
    for(size_t i=0;i<count;i++){
        void* m=machine(w,int(i));if(!m)throw "Vehiculo no legible";
        auto name=text(at<char*>(m,8));for(unsigned char c:name){hash^=c;hash*=1099511628211ull;}
        hash^=255;hash*=1099511628211ull;
    }return hash;
}
struct Api {
    void* mm=nullptr;uint64_t room=0,self=0,owner=0;
    using GetData=const char*(__cdecl*)(void*,uint64_t,const char*);
    using GetMember=const char*(__cdecl*)(void*,uint64_t,uint64_t,const char*);
    using MemberCount=int(__cdecl*)(void*,uint64_t);
    using MemberAt=uint64_t(__cdecl*)(void*,uint64_t,int);
    GetData get=nullptr;GetMember member=nullptr;MemberCount count=nullptr;MemberAt atIndex=nullptr;
    bool load(){
        auto mod=GetModuleHandleW(L"version.dll");if(!mod)return false;
        auto dos=reinterpret_cast<IMAGE_DOS_HEADER*>(mod);auto nt=reinterpret_cast<IMAGE_NT_HEADERS64*>((char*)mod+dos->e_lfanew);
        if(nt->FileHeader.TimeDateStamp!=COOP_TIMESTAMP||nt->OptionalHeader.SizeOfImage!=COOP_IMAGE_SIZE)return false;
        room=*reinterpret_cast<uint64_t*>((char*)mod+COOP_LOBBY_RVA);if(!room)return false;
        auto matchmaking=bind<void*(__cdecl*)()>("SteamAPI_SteamMatchmaking_v009");
        auto user=bind<void*(__cdecl*)()>("SteamAPI_SteamUser_v023");
        auto id=bind<uint64_t(__cdecl*)(void*)>("SteamAPI_ISteamUser_GetSteamID");
        auto ownerId=bind<uint64_t(__cdecl*)(void*,uint64_t)>("SteamAPI_ISteamMatchmaking_GetLobbyOwner");
        get=bind<GetData>("SteamAPI_ISteamMatchmaking_GetLobbyData");member=bind<GetMember>("SteamAPI_ISteamMatchmaking_GetLobbyMemberData");
        count=bind<MemberCount>("SteamAPI_ISteamMatchmaking_GetNumLobbyMembers");atIndex=bind<MemberAt>("SteamAPI_ISteamMatchmaking_GetLobbyMemberByIndex");
        if(!matchmaking||!user||!id||!ownerId||!get||!member||!count||!atIndex)return false;
        mm=matchmaking();auto u=user();if(!mm||!u)return false;self=id(u);owner=ownerId(mm,room);return self&&owner;
    }
};
static void snapshot(void* w){
    Api api;bool available=api.load();
    std::string result="{\"ok\":true,\"available\":"+std::string(available?"true":"false")+",\"network\":"+(at<unsigned char>(w,0xEE18)?"true":"false")+
        ",\"nativePlayer\":"+std::to_string(at<int>(w,0xEAEC))+",\"roster\":\""+std::to_string(roster(w))+"\",\"count\":"+std::to_string((at<uintptr_t>(w,0xE8C8)-at<uintptr_t>(w,0xE8C0))/8);
    if(available){
        int n=api.count(api.mm,api.room);if(n<1||n>8)throw "Sala no compatible (1 a 8 jugadores)";
        result+=",\"lobby\":"+std::to_string(api.room)+",\"self\":"+std::to_string(api.self)+",\"owner\":"+std::to_string(api.owner)+",\"payload\":"+quoted(api.get(api.mm,api.room,operationKey))+",\"members\":[";
        for(int i=0;i<n;i++){auto id=api.atIndex(api.mm,api.room,i);if(i)result+=',';result+="{\"id\":"+std::to_string(id)+",\"data\":"+quoted(api.member(api.mm,api.room,id,memberKey))+"}";}
        result+=']';
    }result+='}';reply(result);
}
static void publish(){
    Api api;if(!api.load())throw "No se encuentra una sala del cooperativo compatible";
    std::string value(shared->reply,strnlen_s(shared->reply,3072));if(value.size()>=3072)throw "Mensaje de sincronizacion demasiado largo";
    if(shared->vehicle==0){
        if(api.self!=api.owner)throw "Solo el anfitrion puede publicar la creacion";
        auto set=bind<bool(__cdecl*)(void*,uint64_t,const char*,const char*)>("SteamAPI_ISteamMatchmaking_SetLobbyData");
        if(!set||!set(api.mm,api.room,operationKey,value.c_str()))throw "Steam no ha aceptado la orden";
    }else if(shared->vehicle==1){
        auto set=bind<void(__cdecl*)(void*,uint64_t,const char*,const char*)>("SteamAPI_ISteamMatchmaking_SetLobbyMemberData");
        if(!set)throw "Falta la API de confirmacion";set(api.mm,api.room,memberKey,value.c_str());
    }else throw "Destino de sincronizacion no valido";
    reply("{\"ok\":true}");
}
}
