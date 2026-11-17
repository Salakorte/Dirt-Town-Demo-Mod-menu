using System.Diagnostics;
using System.Text.Json;

namespace DirtTownMenu;

internal sealed class LiveForm:Form
{
    record Item(int Id,string Name){public override string ToString()=>Name;}
    record Model(string Code,string Name){public override string ToString()=>Name;}
    LiveBridge? bridge;
    readonly ComboBox materials=new(){DropDownStyle=ComboBoxStyle.DropDownList},vehicles=new(){DropDownStyle=ComboBoxStyle.DropDownList},models=new(){DropDownStyle=ComboBoxStyle.DropDownList};
    readonly Label status=new(){AutoSize=true,MaximumSize=new Size(490,0)};
    readonly List<Button> buttons=[];
    bool busy;
    public LiveForm(string backupRoot)
    {
        Text="Materiales y vehículos · Tiempo real · Experimental";ClientSize=new Size(550,760);Font=new Font("Segoe UI",10);StartPosition=FormStartPosition.CenterParent;
        var panel=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(20),AutoScroll=true};Controls.Add(panel);
        void Label(string s)=>panel.Controls.Add(new Label {Text=s,AutoSize=true,MaximumSize=new Size(490,0),Margin=new Padding(0,8,0,5)});
        Button Button(string title,Func<Task> action)
        {
            var b=new Button {Text=title,Width=490,Height=35};buttons.Add(b);panel.Controls.Add(b);
            b.Click+=async(_,_)=>{if(busy)return;busy=true;buttons.ForEach(x=>x.Enabled=false);try{await action();}catch(Exception ex){status.Text=ex.Message;}finally{busy=false;buttons.ForEach(x=>x.Enabled=true);}};return b;
        }
        Label("Carga una partida y reanúdala. Cooperativo experimental: ambos necesitan el menú 0.4.0 y activar la sincronización antes de crear desde el anfitrión. Se hará una copia al conectar. F6 muestra u oculta el panel.");
        Button("Conectar y leer materiales / vehículos",async()=>
        {
            if(bridge is null)
            {
                var games=Process.GetProcessesByName("dirttown");if(games.Length!=1)throw new IOException("Abre una sola instancia del juego.");
                string backup=await Task.Run(()=>ModSession.CreateBackup(backupRoot));
                bridge=await Task.Run(()=>new LiveBridge(games[0]));
            }
            using var result=await bridge.Request(1);LoadItems(result.RootElement);status.Text="Lista leída de la partida actual.";
        });
        Button("Activar sincronización cooperativa (en ambos equipos)",async()=>
        {
            if(bridge is null)throw new IOException("Conecta primero a la partida.");
            await bridge.EnableCoop();status.Text=bridge.CoopStatus;
        });
        var syncStatus=new Label {AutoSize=true,MaximumSize=new Size(490,0)};panel.Controls.Add(syncStatus);
        var syncTimer=new System.Windows.Forms.Timer{Interval=1000};
        syncTimer.Tick+=(_,_)=>syncStatus.Text=bridge?.CoopStatus??"";syncTimer.Start();
        Label("La sincronización nueva cubre vehículos. Montones, carga y desbloqueo de excavadora aún no se han verificado entre equipos.");
        Label("Material y volquete de destino");materials.Width=vehicles.Width=490;panel.Controls.Add(materials);panel.Controls.Add(vehicles);
        var amount=new NumericUpDown {Minimum=0.1m,Maximum=20,Value=5,DecimalPlaces=1,Increment=1,Width=490};Label("Añadir volumen al volquete (m³; hasta su capacidad física)");panel.Controls.Add(amount);
        Button("Añadir material al volquete seleccionado",async()=>
        {
            if(bridge is null||materials.SelectedItem is not Item mat||vehicles.SelectedItem is not Item vehicle)throw new IOException("Conecta y selecciona un material y un volquete.");
            using var result=await bridge.Request(2,vehicle.Id,mat.Id,(float)amount.Value);
            status.Text=$"El juego añadió {result.RootElement.GetProperty("added").GetDouble():0.00} m³. Comprueba la caja del volquete.";
        });
        Button("Llenar el volquete hasta su capacidad",async()=>
        {
            if(bridge is null||materials.SelectedItem is not Item mat||vehicles.SelectedItem is not Item vehicle)throw new IOException("Conecta y selecciona material y volquete.");
            using var result=await bridge.Request(2,vehicle.Id,mat.Id,0);status.Text=$"Añadidos {result.RootElement.GetProperty("added").GetDouble():0.00} m³, con el límite de capacidad del juego.";
        });
        Label("Crear vehículo adicional · Algunos modelos aún no están probados");models.Width=490;
        models.Items.AddRange(new Model[]{new("dozer","Bulldozer"),new("hauler","Volquete"),new("bandvagn","Transporte de orugas"),new("truck","Camión"),new("excavator","Excavadora"),new("large_loader","Pala cargadora grande"),new("medium_loader","Pala cargadora mediana"),new("tractor","Tractor"),new("dirtbike","Moto de campo"),new("snowmobile","Moto de nieve"),new("hooklift","Portacontenedores"),new("feeder","Alimentador")});models.SelectedIndex=0;panel.Controls.Add(models);
        var near=new CheckBox {Text="Crear junto a mí (posición automática)",Checked=true,AutoSize=true};panel.Controls.Add(near);
        var coordinates=new FlowLayoutPanel {Width=490,Height=35,Enabled=false};var x=new NumericUpDown {Minimum=-10000,Maximum=10000,DecimalPlaces=1,Width=180};var z=new NumericUpDown {Minimum=-10000,Maximum=10000,DecimalPlaces=1,Width=180};coordinates.Controls.Add(new Label {Text="X",AutoSize=true});coordinates.Controls.Add(x);coordinates.Controls.Add(new Label {Text="Z",AutoSize=true});coordinates.Controls.Add(z);panel.Controls.Add(coordinates);
        near.CheckedChanged+=(_,_)=>coordinates.Enabled=!near.Checked;
        async Task Position(float distance)
        {
            if(!near.Checked)return;
            if(bridge is null)throw new IOException("Conecta primero.");
            using var snapshot=await bridge.Request(1);
            var root=snapshot.RootElement;int id=root.GetProperty("selected").GetInt32();
            var player=root.GetProperty("vehicles").EnumerateArray().FirstOrDefault(v=>v.GetProperty("id").GetInt32()==id);
            if(player.ValueKind==JsonValueKind.Undefined)throw new IOException("No se ha podido localizar al jugador.");
            float px=player.GetProperty("x").GetSingle(),pz=player.GetProperty("z").GetSingle();
            float bestX=px+distance,bestZ=pz,bestClearance=-1;
            for(int i=0;i<24;i++)
            {
                float angle=i*MathF.Tau/24,cx=px+distance*MathF.Cos(angle),cz=pz+distance*MathF.Sin(angle),clearance=float.MaxValue;
                foreach(var m in root.GetProperty("vehicles").EnumerateArray())
                {
                    if(m.GetProperty("name").GetString()!.StartsWith("walker",StringComparison.OrdinalIgnoreCase))continue;
                    float dx=cx-m.GetProperty("x").GetSingle(),dz=cz-m.GetProperty("z").GetSingle();clearance=Math.Min(clearance,dx*dx+dz*dz);
                }
                if(clearance>bestClearance){bestX=cx;bestZ=cz;bestClearance=clearance;}
            }
            if(bestClearance<16)throw new IOException("Hay demasiados vehículos cerca. Muévete a una zona despejada.");
            x.Value=Math.Clamp((decimal)bestX,x.Minimum,x.Maximum);z.Value=Math.Clamp((decimal)bestZ,z.Minimum,z.Maximum);
        }
        var radius=new NumericUpDown {Minimum=0.25m,Maximum=5,DecimalPlaces=2,Value=2,Width=225};var height=new NumericUpDown {Minimum=0.1m,Maximum=3,DecimalPlaces=1,Value=1,Width=225};
        Label("Montón: radio / altura en metros");panel.Controls.Add(radius);panel.Controls.Add(height);
        Button("Crear montón junto a mí",async()=>
        {
            if(bridge is null||materials.SelectedItem is not Item mat)throw new IOException("Conecta y elige un material.");
            await Position((float)radius.Value+5);
            using var result=await bridge.Request(5,material:mat.Id,amount:(float)radius.Value,x:(float)x.Value,z:(float)z.Value,height:(float)height.Value);status.Text="El juego ha aceptado el montón. Comprueba su aparición en el terreno.";
        });
        Button("Crear vehículo junto a mí",async()=>
        {
            if(bridge is null)throw new IOException("Conecta primero.");
            await Position(18);
            using var result=await bridge.Request(3,x:(float)x.Value,z:(float)z.Value,model:((Model)models.SelectedItem!).Code);
            status.Text="Vehículo creado con índice "+result.RootElement.GetProperty("vehicle").GetInt32()+". Actualiza la lista si vas a llenarlo.";
        });
        Button("Excavadora: retirar el bloqueo de los datos activos",async()=>
        {
            if(bridge is null)throw new IOException("Conecta primero.");using var result=await bridge.Request(4);
            status.Text="Bloqueos retirados: "+result.RootElement.GetProperty("unlocked").GetInt32()+". Sal y vuelve a entrar para probar los controles.";
        });
        status.AutoSize=false;status.Dock=DockStyle.Bottom;status.Height=82;status.Padding=new Padding(15,8,15,8);status.BackColor=Color.LightGoldenrodYellow;Controls.Add(status);
        status.Text="La colocación automática está activada. Ponte en una zona despejada antes de crear objetos.";
        FormClosing+=(_,e)=>{if(busy)e.Cancel=true;};FormClosed+=(_,_)=>{syncTimer.Dispose();bridge?.Dispose();};
    }
    void LoadItems(JsonElement root)
    {
        materials.Items.Clear();vehicles.Items.Clear();
        var names=new Dictionary<string,string>{{"sand","Arena"},{"gravel","Grava"},{"clay","Arcilla"},{"asphalt","Asfalto"},{"ice","Hielo"},{"snow","Nieve"},{"rock","Roca"},{"roadmix","Mezcla para caminos"},{"concrete","Hormigón"},{"duff","Materia orgánica"},{"topsoil","Tierra vegetal"},{"moraine","Morrena"},{"blue_ore","Mineral azul"},{"paint_rock","Roca pigmentada"},{"blackjack","Blackjack"},{"dirt","Tierra"}};
        foreach(var m in root.GetProperty("materials").EnumerateArray()){string name=m.GetProperty("name").GetString()!;materials.Items.Add(new Item(m.GetProperty("id").GetInt32(),names.GetValueOrDefault(name,name)));}
        foreach(var m in root.GetProperty("vehicles").EnumerateArray())if(m.GetProperty("name").GetString()!.Contains("hauler",StringComparison.OrdinalIgnoreCase))vehicles.Items.Add(new Item(m.GetProperty("id").GetInt32(),m.GetProperty("name").GetString()!));
        if(materials.Items.Count>0)materials.SelectedIndex=0;if(vehicles.Items.Count>0)vehicles.SelectedIndex=0;
    }
}
