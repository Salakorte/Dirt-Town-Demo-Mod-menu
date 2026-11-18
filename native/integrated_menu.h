#pragma once
#include "vendor/imgui/imgui.h"
// Calls the exact ImGui build already linked into the game; no second renderer/context.
namespace ingame {
static_assert(sizeof(ImGuiIO)==0xC28,"ImGui IO layout differs from this game build");
struct Outbox {unsigned event=0,ack=0;int command=0;float x=0,z=0;char model[64]{};} static outbox;
static void* world=nullptr;
static DWORD worldSeen=0,agentSeen=0;
static bool visible=false,binding=false,keys[256]{};
static bool captureApplied=false,previousCursor=false,previousNav=false;
static int fillKey=VK_F8,material=-1,target=-1,model=0;
static float amount=5,radius=2,height=1;
static std::string message="F7 abre el menu. F8 llena el volquete que conduces.",sync="Esperando al coordinador";
static wchar_t configPath[MAX_PATH]{};
static const char* models[]={"dozer","hauler","bandvagn","truck","excavator","large_loader","medium_loader","tractor","dirtbike","snowmobile","hooklift","feeder"};
template<class F>F api(uintptr_t rva){return reinterpret_cast<F>(base+rva);}
using EndFrame=void(__fastcall*)();static EndFrame originalEnd;
static ImGuiIO& io(){return *api<ImGuiIO*(__fastcall*)()>(0x1145620)();}
static void label(const std::string& s){api<void(__fastcall*)(const char*,const char*)>(0x11D8530)(s.c_str(),nullptr);}
static bool button(const std::string& s){ImVec2 size(0,0);return api<bool(__fastcall*)(const char*,const ImVec2&)>(0x11DADB0)(s.c_str(),size);}
static void slider(const char* name,float& value,float lo,float hi){api<bool(__fastcall*)(const char*,float*,float,float,const char*,int)>(0x1205F00)(name,&value,lo,hi,"%.1f",0);}
static bool focus(){DWORD pid=0;GetWindowThreadProcessId(GetForegroundWindow(),&pid);return pid==GetCurrentProcessId();}
static bool bindable(int key){return (key>='A'&&key<='Z')||(key>=VK_F1&&key<=VK_F12&&key!=VK_F7);}
static std::string keyName(int key){char b[64]{};GetKeyNameTextA(LONG(MapVirtualKeyW(key,MAPVK_VK_TO_VSC)<<16),b,64);return b[0]?b:"Sin tecla";}
static void* flyingWorld=nullptr;
static int savedCameraMode=0;
static freecam::Pose savedCamera{};
static DWORD flyAt=0;
static bool looking=false;
static ImVec2 lookMouse{};
static void restoreCamera(){
    // A paused scene may stop the world hook. Defer restoration until it is fresh again.
    if(flyingWorld&&flyingWorld==world&&GetTickCount()-worldSeen>=1500){looking=false;flyAt=0;return;}
    // Never write an old scene after the active world changes or becomes stale.
    if(flyingWorld&&flyingWorld==world&&GetTickCount()-worldSeen<1500&&worldOK(world)&&readable((char*)world+0x19A40,0x4B0)){
        at<int>(world,0x19A40)=savedCameraMode;at<freecam::Pose>(world,0x19EC0)=savedCamera;
    }
    flyingWorld=nullptr;looking=false;flyAt=0;
}
static void cameraFrame(bool ready,ImVec2 panelPos,ImVec2 panelSize){
    if(!ready||!visible||!terrain::armed||!focus()||at<unsigned char>(world,0xEE18)){
        restoreCamera();return;
    }
    if(flyingWorld&&flyingWorld!=world)restoreCamera();
    // Native gameplay camera, traced from World through the 24C010 update call.
    if(!readable((char*)world+0x19A40,0x4B0)||!freecam::valid(at<freecam::Pose>(world,0x19EC0))||at<int>(world,0x19A40)<0||at<int>(world,0x19A40)>2){
        terrain::armed=false;restoreCamera();terrain::status="No se puede activar la camara libre en esta vista.";return;
    }
    if(!flyingWorld){
        if(terrain::cameraWorld!=world||GetTickCount()-terrain::cameraSeen>1500)return;
        savedCameraMode=at<int>(world,0x19A40);savedCamera=at<freecam::Pose>(world,0x19EC0);
        flyingWorld=world;flyAt=GetTickCount();
    }
    at<int>(world,0x19A40)=2; // native enum: chase=0, cab=1, free=2; selection EAA0/EAA4 untouched
    DWORD now=GetTickCount();float dt=float(now-flyAt)*.001f;flyAt=now;
    auto& input=io();auto mouse=input.MousePos;
    bool over=mouse.x>=panelPos.x&&mouse.y>=panelPos.y&&mouse.x<=panelPos.x+panelSize.x&&mouse.y<=panelPos.y+panelSize.y;
    bool mouseValid=std::isfinite(mouse.x)&&std::isfinite(mouse.y)&&mouse.x>=0&&mouse.y>=0&&mouse.x<input.DisplaySize.x&&mouse.y<input.DisplaySize.y;
    bool turn=mouseValid&&input.MouseDown[1]&&!input.WantTextInput&&(!over||looking);
    float dx=turn&&looking?mouse.x-lookMouse.x:0,dy=turn&&looking?mouse.y-lookMouse.y:0;
    looking=turn;lookMouse=mouse;
    if(input.WantTextInput||binding)return;
    auto key=[](int k){return (GetAsyncKeyState(k)&0x8000)?1.f:0.f;};
    if(!freecam::move(at<freecam::Pose>(world,0x19EC0),key('W')-key('S'),key('D')-key('A'),key('E')-key('Q'),dx,dy,dt,key(VK_SHIFT)>0)){
        terrain::armed=false;restoreCamera();terrain::status="Camara detenida: limite de posicion o valor no valido.";
    }
}
static void terrainFrame(bool ready,ImVec2 panelPos,ImVec2 panelSize){
    struct Segment{ImVec2 a,b;};static Segment segments[48];static int segmentCount=0;
    static DWORD previewAt=0;static alignas(32) double point[4]{};static bool found=false;
    if(!ready||!focus()){terrain::armed=false;terrain::pickLevel=false;terrain::wasDown=false;return;}
    if(!terrain::armed||!visible||!ready||!focus()){terrain::wasDown=false;return;}
    if(at<unsigned char>(world,0xEE18)){terrain::armed=false;terrain::status="Pinceles disponibles solo en individual.";return;}
    auto& input=io();bool down=input.MouseDown[0],edge=down&&!terrain::wasDown;terrain::wasDown=down;
    // Navigation and sculpting are mutually exclusive, including a held continuous stroke.
    if(looking||input.MouseDown[1])return;
    float mx=input.MousePos.x,my=input.MousePos.y;
    bool overPanel=mx>=panelPos.x&&my>=panelPos.y&&mx<=panelPos.x+panelSize.x&&my<=panelPos.y+panelSize.y;
    // Block game input on the next event pump, while retaining ImGui input delivery.
    input.WantCaptureMouse=true;input.WantCaptureKeyboard=true;
    if(overPanel||input.WantTextInput||mx<0||my<0||mx>=input.DisplaySize.x||my>=input.DisplaySize.y)return;
    bool stroke=edge||(down&&terrain::continuous&&GetTickCount()-terrain::lastApply>=250);
    if(stroke||GetTickCount()-previewAt>=50){
        previewAt=GetTickCount();segmentCount=0;
        found=terrain::hit(world,mx,my,input.DisplaySize.x,input.DisplaySize.y,point);
        if(found){
            ImVec2 previous{};bool lastGood=false;
            for(int n=0;n<=48;n++){
                float a=float(n)*6.2831853f/48;alignas(32) double p[4]={point[0]+cosf(a)*terrain::radius,0,point[2]+sinf(a)*terrain::radius,0};
                p[1]=terrain::ground(world,float(p[0]),float(p[2]))+0.12;
                ImVec2 screen{};bool good=terrain::project(p,input.DisplaySize.x,input.DisplaySize.y,screen);
                if(good&&lastGood&&segmentCount<48){ImVec2 from=previous,to=screen;if(preview::clip(from,to,input.DisplaySize.x,input.DisplaySize.y))segments[segmentCount++]={from,to};}
                lastGood=good;previous=screen;
            }
        }
    }
    if(!found){
        terrain::status=terrain::cameraWorld!=world||GetTickCount()-terrain::cameraSeen>1500?"Esperando camara de la partida...":"No hay terreno bajo el cursor. Apunta al suelo.";return;
    }
    auto list=api<void*(__fastcall*)(void*)>(0x11456B0)(nullptr);
    // 11A8510 is AddLine. 11A9630 is AddCircle and has a different ABI.
    for(int n=0;n<segmentCount;n++)api<void(__fastcall*)(void*,const ImVec2&,const ImVec2&,unsigned,float)>(0x11A8510)(list,segments[n].a,segments[n].b,0xFF69E3AD,2.f);
    if(terrain::pickLevel){if(edge){terrain::level=float(point[1]);terrain::levelPicked=true;terrain::pickLevel=false;terrain::status="Altura objetivo capturada.";}return;}
    if(!edge&&!(down&&terrain::continuous&&GetTickCount()-terrain::lastApply>=250))return;
    if(at<uintptr_t>(world,0x17648))return;
    if(terrain::mode==2&&!terrain::levelPicked){terrain::level=float(point[1]);terrain::levelPicked=true;}
    terrain::lastApply=GetTickCount();
    try{terrain::apply(world,material,point);}catch(const char* error){terrain::status=error;terrain::armed=false;}catch(...){terrain::status="Error del pincel; no repetir hasta revisar.";terrain::armed=false;}
}
static void loadConfig(HINSTANCE){
    try{auto root=standalone::localRoot();std::filesystem::create_directories(root);wcscpy_s(configPath,(root/L"DirtTownMenu.ini").c_str());}catch(...){configPath[0]=0;}
    int key=GetPrivateProfileIntW(L"Keys",L"FillTruck",VK_F8,configPath);if(bindable(key))fillKey=key;
}
static void saveKey(){wchar_t value[16];swprintf_s(value,L"%d",fillKey);if(!configPath[0]||!WritePrivateProfileStringW(L"Keys",L"FillTruck",value,configPath))message="La tecla funciona en esta sesion pero no se ha podido guardar.";}
static void action(unsigned command,int vehicle=0,int mat=0,float volume=1,float x=0,float z=0,const char* code="",float h=1){
    // UI actions have their own request storage; the external coordinator's IPC is never overwritten.
    Shared local{};local.request=1;local.command=command;local.vehicle=vehicle;local.material=mat;local.amount=volume;local.x=x;local.z=z;local.yaw=h;
    strncpy_s(local.model,code,_TRUNCATE);auto saved=shared;shared=&local;execute(world);shared=saved;
    if(strstr(local.reply,"\"ok\":false")){
        auto error=strstr(local.reply,"\"error\":\"");message=error?error+9:"El juego ha rechazado la accion.";
        auto end=message.find('"');if(end!=std::string::npos)message.resize(end);
    }else if(command==2){
        auto added=strstr(local.reply,"\"added\":");char b[160];snprintf(b,sizeof(b),"Anadidos %.2f m3 al volquete.",added?strtod(added+8,nullptr):0);message=b;
    }else if(command==3)message="Vehiculo creado en la partida individual.";
    else if(command==4)message="Operacion de desbloqueo aplicada. Vuelve a entrar para probar los controles.";
    else if(command==5)message="Monton aceptado por el juego.";
}
static int selectedTruck(){
    int id=target<0?at<int>(world,0xEAA0):target;auto m=machine(world,id);
    return m&&text(at<char*>(m,8)).find("hauler")!=std::string::npos?id:-1;
}
static void ensureMaterial(){
    auto begin=at<uintptr_t>(world,0x15B0),end=at<uintptr_t>(world,0x15B8);
    if(end<begin||(end-begin)%0x128)return;size_t count=(end-begin)/0x128;
    if(count<1||count>128||!readable((void*)begin,count*0x128))return;
    if(material<0||size_t(material)>=count){material=0;for(size_t i=0;i<count;i++)if(text(at<char*>((void*)(begin+i*0x128),0))=="gravel")material=int(i);}
}
static void fill(bool full){
    int id=selectedTruck();if(id<0){message="Selecciona/conduce un volquete hauler para llenarlo.";return;}
    if(!materialOK(world,material)){message="Selecciona primero un material.";return;}
    action(2,id,material,full?0:amount);
}
static bool position(float distance,float& x,float& z){
    auto m=machine(world,at<int>(world,0xEAA0));if(!m){message="No se encuentra tu posicion.";return false;}
    alignas(32) double p[4]{};auto table=at<void**>(m,0);api<void*(__fastcall*)(void*,void*)>(uintptr_t(table[0x88/8])-base)(p,m);
    float best=-1;size_t count=(at<uintptr_t>(world,0xE8C8)-at<uintptr_t>(world,0xE8C0))/8;
    for(int n=0;n<24;n++){
        float a=float(n)*6.2831853f/24,cx=float(p[0])+cosf(a)*distance,cz=float(p[2])+sinf(a)*distance,clear=1e9f;
        for(size_t i=0;i<count;i++){auto other=machine(world,int(i));if(!other||text(at<char*>(other,8)).find("walker")==0)continue;
            alignas(32) double q[4]{};auto v=at<void**>(other,0);reinterpret_cast<void*(__fastcall*)(void*,void*)>(v[0x88/8])(q,other);
            float dx=cx-float(q[0]),dz=cz-float(q[2]);clear=fminf(clear,dx*dx+dz*dz);
        }if(clear>best){best=clear;x=cx;z=cz;}
    }if(best<16){message="Busca una zona mas despejada.";return false;}return true;
}
static void create(){
    float x=0,z=0;if(!position(18,x,z))return;
    if(at<unsigned char>(world,0xEE18)){
        if(outbox.event!=outbox.ack){message="Hay una creacion pendiente. No repitas la accion.";return;}
        if(GetTickCount()-agentSeen>5000){message="Abre el coordinador para sincronizar la creacion cooperativa.";return;}
        outbox.command=3;outbox.x=x;outbox.z=z;strncpy_s(outbox.model,models[model],_TRUNCATE);outbox.event++;
        message="Creacion enviada al coordinador. Espera la confirmacion.";
    }else action(3,0,0,1,x,z,models[model]);
}
static void pollKeys(bool ready){
    if(!focus()){for(auto& key:keys)key=false;return;}
    for(int k=1;k<256;k++){
        bool down=(GetAsyncKeyState(k)&0x8000)!=0,edge=down&&!keys[k];keys[k]=down;if(!edge)continue;
        if(binding){if(k==VK_ESCAPE){binding=false;message="Asignacion cancelada.";}else if(bindable(k)){fillKey=k;binding=false;saveKey();message="Atajo guardado: "+keyName(k);}continue;}
        if(k==VK_F7){visible=!visible;if(!visible)terrain::armed=false;continue;}
        if(k==VK_ESCAPE&&terrain::armed){terrain::armed=false;terrain::pickLevel=false;continue;}
        if(k==fillKey&&ready&&!io().WantCaptureKeyboard&&!io().WantTextInput&&!visible)fill(true);
    }
}
static void draw(){
    bool ready=world&&GetTickCount()-worldSeen<1500&&worldOK(world);
    if(ready)ensureMaterial();pollKeys(ready);
    if(!visible){restoreCamera();if(captureApplied){io().MouseDrawCursor=previousCursor;if(!previousNav)io().ConfigFlags&=~ImGuiConfigFlags_NavEnableKeyboard;captureApplied=false;}return;}
    if(!captureApplied){previousCursor=io().MouseDrawCursor;previousNav=(io().ConfigFlags&ImGuiConfigFlags_NavEnableKeyboard)!=0;captureApplied=true;}
    io().ConfigFlags|=ImGuiConfigFlags_NavEnableKeyboard;
    io().MouseDrawCursor=true;
    ImVec2 size(530,650);api<void(__fastcall*)(const ImVec2&,int)>(0x11490D0)(size,ImGuiCond_FirstUseEver);
    bool contents=api<bool(__fastcall*)(const char*,bool*,int)>(0x1170050)("Dirt Town Mod Menu 0.6.3 [F7]",&visible,ImGuiWindowFlags_NoSavedSettings);
    ImVec2 panelPos{},panelSize{};
    auto context=*reinterpret_cast<unsigned char**>(base+0x1E084E8);auto window=at<void*>(context,0x1620);
    if(readable(window,0x70)){panelPos=at<ImVec2>(window,0x60);panelSize=at<ImVec2>(window,0x68);}
    if(contents){
        label("F7: cerrar/abrir | Menu integrado en el juego");
        if(ready&&at<unsigned char>(world,0xEE18))label(GetTickCount()-agentSeen<5000?sync:"Cooperativo: abre el coordinador en ambos equipos para crear vehiculos.");
        else label("Individual: el menu funciona sin programas externos.");
        if(!ready)label("Carga una partida y reanudala.");
        else{
            auto begin=at<uintptr_t>(world,0x15B0),end=at<uintptr_t>(world,0x15B8);int n=int((end-begin)/0x128);
            if(n>0&&n<128){
                ensureMaterial();
                auto name=text(at<char*>((void*)(begin+material*0x128),0));if(button("Material: "+name+" [siguiente]"))material=(material+1)%n;
            }
            std::string truck=target<0?"el que conduces":std::to_string(target);
            if(button("Volquete: "+truck+" [siguiente]")){
                size_t count=(at<uintptr_t>(world,0xE8C8)-at<uintptr_t>(world,0xE8C0))/8;int next=-1;
                for(int i=target+1;i<int(count);i++){auto m=machine(world,i);if(m&&text(at<char*>(m,8)).find("hauler")!=std::string::npos){next=i;break;}}target=next;
            }
            slider("Volumen m3",amount,0.1f,20);
            if(button("Anadir material al volquete"))fill(false);
            if(button("Llenar volquete hasta capacidad"))fill(true);
            if(button(binding?"Pulsa A-Z o F1-F12 (Esc cancela)":"Asignar atajo de llenado: "+keyName(fillKey)))binding=true;
            label("El atajo usa este material y volquete. Una pulsacion = una accion.");
            if(button(std::string("Vehiculo: ")+models[model]+" [siguiente]"))model=(model+1)%12;
            if(button("Crear vehiculo junto a mi"))create();
            slider("Radio del monton",radius,0.25f,5);slider("Altura del monton",height,0.1f,3);
            if(button("Crear monton del material seleccionado")){float x=0,z=0;if(position(radius+5,x,z))action(5,0,material,radius,x,z,"",height);}
            if(button("Excavadora: retirar bloqueo activo"))action(4);
            label("TERRENO · Experimental · Individual");
            if(button(std::string("Pincel: ")+terrain::names[terrain::mode]+" [siguiente]"))terrain::mode=(terrain::mode+1)%4;
            slider("Radio del pincel (m)",terrain::radius,0.5f,10);
            slider("Fuerza por aplicacion",terrain::strength,0.01f,1);
            slider("Dureza del borde",terrain::hardness,0,1);
            if(button(terrain::continuous?"Trazo: mantener clic [cambiar]":"Trazo: un clic [cambiar]"))terrain::continuous=!terrain::continuous;
            if(terrain::mode==2){
                slider("Altura objetivo (m)",terrain::level,-20,100);
                if(button("Tomar altura del terreno (siguiente clic)")){terrain::armed=true;terrain::pickLevel=true;}
                if(button("Usar altura indicada"))terrain::levelPicked=true;
            }
            if(button(terrain::armed?"Desactivar pincel [Esc]":"Activar pincel")){terrain::armed=!terrain::armed;terrain::wasDown=true;terrain::status=terrain::armed?"Activado: apunta al terreno fuera del panel.":"Pincel desactivado";}
            label("Camara libre automatica: WASD | Q/E: bajar/subir");
            label("Boton derecho: girar | Mayus: rapido | Clic izquierdo: pintar");
            label("Esc o F7: salir y restaurar la camara anterior.");label(terrain::status);
            label("Carga/montones/excavadora: sincronizacion cooperativa sin verificar.");
        }
        label(message);
    }
    api<void(__fastcall*)()>(0x117C970)();
    if(!visible){terrain::armed=false;terrain::pickLevel=false;}
    cameraFrame(ready,panelPos,panelSize);
    terrainFrame(ready,panelPos,panelSize);
    if(!terrain::armed)restoreCamera();
}
static void __fastcall frame(){
    auto context=*reinterpret_cast<unsigned char**>(base+0x1E084E8);
    if(context&&GetCurrentThreadId()==mainThread&&context[1]&&at<int>(context,4)!=at<int>(context,8)){
        try{
            draw();
            if(visible){
                // NewFrame recomputes capture every frame. Queue the override as well as
                // setting current IO so neither phase of the game's input loop sees a gap.
                io().WantCaptureMouse=true;io().WantCaptureKeyboard=true;
                api<void(__fastcall*)(bool)>(0x114D610)(true);
                api<void(__fastcall*)(bool)>(0x114D5F0)(true);
            }
        }catch(...){terrain::armed=false;restoreCamera();visible=false;message="Error de interfaz; reinicia el juego.";}
    }
    originalEnd();
}
static void readOutbox(){reply("{\"ok\":true,\"event\":"+std::to_string(outbox.event)+",\"pending\":"+(outbox.event!=outbox.ack?"true":"false")+",\"command\":"+std::to_string(outbox.command)+",\"x\":"+std::to_string(outbox.x)+",\"z\":"+std::to_string(outbox.z)+",\"model\":\""+outbox.model+"\",\"visible\":"+(visible?"true":"false")+"}");}
static void acknowledge(){if(unsigned(shared->vehicle)==outbox.event){outbox.ack=outbox.event;message=std::string(shared->reply,strnlen_s(shared->reply,511));}reply("{\"ok\":true}");}
static void heartbeat(){agentSeen=GetTickCount();sync=std::string(shared->reply,strnlen_s(shared->reply,511));reply("{\"ok\":true}");}
static void terrainProbe(void* w){
    auto& input=io();alignas(32) double p[4]{};bool found=terrain::hit(w,input.MousePos.x,input.MousePos.y,input.DisplaySize.x,input.DisplaySize.y,p);
    reply("{\"ok\":true,\"camera\":"+std::string(terrain::cameraWorld==w&&GetTickCount()-terrain::cameraSeen<1500?"true":"false")+",\"captureCalls\":"+std::to_string(terrain::captureCalls)+",\"hit\":"+(found?"true":"false")+",\"armed\":"+(terrain::armed?"true":"false")+",\"captureMouse\":"+(input.WantCaptureMouse?"true":"false")+",\"captureKeyboard\":"+(input.WantCaptureKeyboard?"true":"false")+",\"x\":"+std::to_string(p[0])+",\"y\":"+std::to_string(p[1])+",\"z\":"+std::to_string(p[2])+",\"network\":"+(at<unsigned char>(w,0xEE18)?"true":"false")+"}");
}
}
