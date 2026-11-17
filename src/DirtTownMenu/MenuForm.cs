using System.Diagnostics;

namespace DirtTownMenu;

internal sealed class MenuForm : Form
{
    readonly Button connect=new(), restore=new(), launch=new(), backups=new();
    readonly CheckBox unlock=new(), top=new();
    readonly ComboBox range=new();
    readonly Label status=new(), backupStatus=new();
    readonly System.Windows.Forms.Timer timer=new() {Interval=1000};
    ModSession? session;
    bool changing, busy, hotkey;
    string? lastBackup;
    string BackupRoot => Path.Combine(AppContext.BaseDirectory,"backups");
    static readonly Color Paper=Color.FromArgb(235,235,225), Muted=Color.FromArgb(165,173,168), Amber=Color.FromArgb(241,182,66);

    public MenuForm()
    {
        Text="Dirt Town · Mod Menu 0.4.0";
        ClientSize=new Size(452,610);
        MinimumSize=new Size(468,649);
        FormBorderStyle=FormBorderStyle.FixedSingle;
        MaximizeBox=false;
        StartPosition=FormStartPosition.CenterScreen;
        BackColor=Color.FromArgb(26,32,29);
        ForeColor=Paper;
        Font=new Font("Segoe UI",10);
        TopMost=true;
        Label LabelAt(string text,int x,int y,int w,int h,Color? color=null,float size=10)
        {
            var control=new Label {Text=text,Location=new Point(x,y),Size=new Size(w,h),ForeColor=color??Paper,Font=new Font("Segoe UI",size),UseMnemonic=false};
            Controls.Add(control); return control;
        }
        void ButtonAt(Button button,string text,int x,int y,int w)
        {
            button.Text=text; button.SetBounds(x,y,w,37);
            button.FlatStyle=FlatStyle.Flat;button.FlatAppearance.BorderColor=Color.FromArgb(76,91,80);
            button.BackColor=Color.FromArgb(42,54,46);button.ForeColor=Paper;
            Controls.Add(button);
        }
        LabelAt("DIRT TOWN",24,22,390,34,Amber,22);
        LabelAt("MOD MENU  /  DEMO · BUILD 26690600",26,63,400,22,Muted,9);
        LabelAt("Vehículos cooperativos · Versión experimental",26,91,400,24,Muted,10);
        ButtonAt(launch,"Abrir juego en Steam",26,129,192);
        ButtonAt(connect,"Conectar al juego",230,129,196);
        status.SetBounds(26,177,400,53);status.Text="Abre el juego y quédate en el menú principal. Después pulsa Conectar para copiar tus partidas.";
        status.ForeColor=Muted;Controls.Add(status);
        LabelAt("MAQUINARIA",26,244,400,23,Amber,10);
        var settings=new Button();
        ButtonAt(settings,"Materiales y vehículos · Tiempo real",26,276,400);
        settings.Click+=(_,_)=>Try(()=>{using var editor=new LiveForm(BackupRoot);editor.ShowDialog(this);});
        var configuration=new Button();ButtonAt(configuration,"Ajustes de cámara e hidráulica…",26,322,400);
        configuration.Click+=(_,_)=>Try(()=>{using var editor=new SettingsForm(BackupRoot);editor.ShowDialog(this);});
        LabelAt("Distancia para entrar en vehículos",26,368,400,24);
        range.SetBounds(26,399,400,31);range.DropDownStyle=ComboBoxStyle.DropDownList;
        range.Items.AddRange(["Original del juego","10 metros","25 metros","50 metros"]);range.SelectedIndex=0;range.Enabled=false;Controls.Add(range);
        ButtonAt(restore,"Restaurar y desconectar",26,448,400);restore.Enabled=false;
        top.Text="Panel siempre visible";top.SetBounds(26,498,245,27);top.Checked=true;Controls.Add(top);
        LabelAt("F6 · Mostrar / ocultar",281,503,155,23,Muted,9);
        ButtonAt(backups,"Ver copias de seguridad",26,538,400);
        backupStatus.SetBounds(26,581,400,22);backupStatus.Font=new Font("Segoe UI",8);backupStatus.ForeColor=Muted;backupStatus.Text="No modifica el ejecutable ni instala DLL.";Controls.Add(backupStatus);

        connect.Click+=async (_,_)=>await Attach();
        launch.Click+=(_,_)=>Try(()=>Process.Start(new ProcessStartInfo("steam://rungameid/5471710"){UseShellExecute=true}));
        unlock.CheckedChanged+=(_,_)=>
        {
            if(changing||session is null)return;
            bool requested=unlock.Checked;
            if(!Try(()=>session.SetUnlock(requested))) {changing=true;unlock.Checked=!requested;changing=false;}
            else SetStatus(requested?"Desbloqueo activo. Acércate a la excavadora y usa el control habitual para entrar.":"Bloqueo de uso restaurado.");
        };
        range.SelectedIndexChanged+=(_,_)=>
        {
            if(changing||session is null)return;
            int previous=Convert.ToInt32(range.Tag??0);
            if(Try(()=>session.SetRange(new[]{0,10,25,50}[range.SelectedIndex])))
            {range.Tag=range.SelectedIndex;SetStatus("Distancia de interacción aplicada.");}
            else {changing=true;range.SelectedIndex=previous;changing=false;}
        };
        restore.Click+=(_,_)=>Disconnect();
        top.CheckedChanged+=(_,_)=>TopMost=top.Checked;
        backups.Click+=(_,_)=>Try(()=>
        {
            string folder=lastBackup??BackupRoot;
            Directory.CreateDirectory(folder);
            Process.Start(new ProcessStartInfo(folder){UseShellExecute=true});
        });
        timer.Tick+=(_,_)=>
        {
            if(session is not null && session.HasExited)
            {session.Dispose();session=null;ResetControls();SetStatus("El juego se ha cerrado. Los cambios de memoria han desaparecido.");}
        };
        timer.Start();
        FormClosing+=(_,e)=>{if(busy){e.Cancel=true;return;} if(!Disconnect())e.Cancel=true;};
        FormClosed+=(_,_)=>{timer.Dispose();if(hotkey)Native.UnregisterHotKey(Handle,1);};
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        hotkey=Native.RegisterHotKey(Handle,1,0x4000,0x75);
        if(!hotkey)SetStatus("F6 está ocupado por otra aplicación. Puedes usar el panel desde la barra de tareas.");
    }
    protected override void WndProc(ref Message m)
    {
        if(m.Msg==0x0312 && m.WParam==(nint)1)
        {
            var popup=OwnedForms.LastOrDefault();
            if(popup is not null)
            {
                bool visible=Native.IsWindowVisible(popup.Handle);
                Native.ShowWindow(popup.Handle,visible?0:5);Native.ShowWindow(Handle,visible?0:5);
                if(!visible)popup.Activate();
            }
            else if(Visible)Hide();else{Show();Activate();}
            return;
        }
        base.WndProc(ref m);
    }
    async Task Attach()
    {
        if(busy||session is not null)return;
        busy=true;connect.Enabled=false;launch.Enabled=false;
        SetStatus("Comprobando versión y copiando partidas…");
        try
        {
            var targets=Process.GetProcessesByName("dirttown");
            if(targets.Length!=1)throw new InvalidOperationException(targets.Length==0?"Primero abre Dirt Town Demo y espera al menú principal.":"Hay varias instancias del juego. Deja solo una abierta.");
            session=await Task.Run(()=>new ModSession(targets[0],BackupRoot));
            lastBackup=session.BackupPath;
            unlock.Enabled=range.Enabled=restore.Enabled=true;
            backupStatus.Text="Copia guardada: "+Path.GetFileName(lastBackup);
            SetStatus($"Conectado · PID {session.ProcessId}. Activa las opciones que quieras. Usa F6 para mostrar u ocultar este panel.");
        }
        catch(Exception ex){SetStatus(ex.Message,true);MessageBox.Show(this,ex.Message,"No se ha conectado",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        finally{busy=false;connect.Enabled=session is null;launch.Enabled=true;}
    }
    bool Disconnect()
    {
        if(session is null)return true;
        if(!Try(()=>session.Restore()))return false;
        session.Dispose();session=null;ResetControls();SetStatus("Cambios originales restaurados. Menú desconectado.");return true;
    }
    void ResetControls()
    {
        changing=true;unlock.Checked=false;range.SelectedIndex=0;range.Tag=0;changing=false;
        unlock.Enabled=range.Enabled=restore.Enabled=false;connect.Enabled=true;
    }
    void SetStatus(string text,bool error=false){status.Text=text;status.ForeColor=error?Color.Salmon:Muted;}
    bool Try(Action action)
    {
        try{action();return true;}
        catch(Exception ex){SetStatus(ex.Message,true);MessageBox.Show(this,ex.Message,"Dirt Town Menu",MessageBoxButtons.OK,MessageBoxIcon.Warning);return false;}
    }
}
