using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DirtTownMenu;

internal static class SettingsEditor
{
    public static string[] Profiles() => Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Dirt Town Demo"),"settings.machine.toml",SearchOption.AllDirectories);
    public static string SetRoot(string text,string key,string value)
    {
        int section=Regex.Match(text,@"(?m)^\s*\[").Success?Regex.Match(text,@"(?m)^\s*\[").Index:text.Length;
        string root=text[..section],tail=text[section..];
        var pattern=new Regex(@"(?m)^(\s*"+Regex.Escape(key)+@"\s*=\s*)[^\r\n#]*(.*)$");
        root=pattern.IsMatch(root)?pattern.Replace(root,m=>m.Groups[1].Value+value+m.Groups[2].Value):root.TrimEnd()+"\r\n"+key+" = "+value+"\r\n\r\n";
        return root+tail;
    }
    public static string Read(string text,string key,string fallback)
    {
        var m=Regex.Match(text,@"(?m)^\s*"+Regex.Escape(key)+@"\s*=\s*([^#\r\n]+)");
        return m.Success?m.Groups[1].Value.Trim():fallback;
    }
    public static string Save(string path,Dictionary<string,string> changes,string backupRoot)
    {
        if(Process.GetProcessesByName("dirttown").Length>0)throw new InvalidOperationException("Cierra el juego antes de guardar estos ajustes. Se aplican al próximo arranque.");
        string original=File.ReadAllText(path),updated=original;
        foreach(var pair in changes)updated=SetRoot(updated,pair.Key,pair.Value);
        string backup=ModSession.CreateBackup(backupRoot);
        string temporary=path+".modmenu.tmp";
        try {File.WriteAllText(temporary,updated);File.Move(temporary,path,true);}
        finally {if(File.Exists(temporary))File.Delete(temporary);}
        return backup;
    }
}

internal sealed class SettingsForm:Form
{
    public SettingsForm(string backupRoot)
    {
        Text="Maquinaria y cámara · Ajustes para el próximo arranque";
        ClientSize=new Size(540,535);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;
        StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",10);
        var panel=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=2,RowCount=10,AutoScroll=true};
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,65));panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35));Controls.Add(panel);
        var note=new Label {Text="Guarda con el juego cerrado. Se conserva una copia de partidas y configuración antes de escribir. Estos ajustes se aplican al arrancar el juego.",AutoSize=true,MaximumSize=new Size(490,0),Margin=new Padding(0,0,0,14)};
        panel.Controls.Add(note);panel.SetColumnSpan(note,2);
        var profile=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Top};
        foreach(string p in SettingsEditor.Profiles())profile.Items.Add(p);
        panel.Controls.Add(new Label {Text="Perfil",AutoSize=true});panel.Controls.Add(profile);
        NumericUpDown Number(string title,decimal min,decimal max,int decimals,decimal increment)
        {
            panel.Controls.Add(new Label {Text=title,AutoSize=true,Margin=new Padding(0,12,0,0)});
            var n=new NumericUpDown {Minimum=min,Maximum=max,DecimalPlaces=decimals,Increment=increment,Dock=DockStyle.Top,Margin=new Padding(0,8,0,0)};panel.Controls.Add(n);return n;
        }
        var hydraulics=Number("Velocidad hidráulica (0 = automática)",0,3,2,0.5m);
        var mouse=Number("Sensibilidad de cámara",0.05m,10,2,0.1m);
        CheckBox Check(string title)
        {
            var c=new CheckBox {Text=title,AutoSize=true,Margin=new Padding(0,9,0,0)};panel.Controls.Add(c);panel.SetColumnSpan(c,2);return c;
        }
        var bucket=Check("La cámara sigue el cucharón");
        var chase=Check("Cámara exterior: seguir el cucharón");
        var mirrors=Check("Retrovisores activados");
        var invert=Check("Invertir cámara vertical");
        var window=Check("Modo ventana (facilita usar el panel)");
        var save=new Button {Text="Guardar con copia de seguridad",AutoSize=true,Dock=DockStyle.Top,Margin=new Padding(0,18,0,0)};
        panel.Controls.Add(save);panel.SetColumnSpan(save,2);
        var result=new Label {AutoSize=true,MaximumSize=new Size(490,0)};panel.Controls.Add(result);panel.SetColumnSpan(result,2);
        void LoadProfile()
        {
            if(profile.SelectedItem is not string path)return;
            string t=File.ReadAllText(path);
            decimal Value(string k,decimal fallback)=>decimal.TryParse(SettingsEditor.Read(t,k,fallback.ToString(CultureInfo.InvariantCulture)),CultureInfo.InvariantCulture,out var v)?v:fallback;
            hydraulics.Value=Math.Clamp(Value("hydraulic_speed",0),hydraulics.Minimum,hydraulics.Maximum);
            mouse.Value=Math.Clamp(Value("mouse",1),mouse.Minimum,mouse.Maximum);
            bucket.Checked=SettingsEditor.Read(t,"follow_bucket","true")=="true";
            chase.Checked=SettingsEditor.Read(t,"chase_follow_bucket","true")=="true";
            mirrors.Checked=SettingsEditor.Read(t,"mirrors","true")=="true";
            invert.Checked=SettingsEditor.Read(t,"invert_look","false")=="true";
            window.Checked=SettingsEditor.Read(t,"fullscreen","true")=="false";
        }
        profile.SelectedIndexChanged+=(_,_)=>LoadProfile();
        if(profile.Items.Count>0)profile.SelectedIndex=0;else save.Enabled=false;
        save.Click+=(_,_)=>
        {
            try
            {
                if(profile.SelectedItem is not string path)return;
                if(hydraulics.Value>0 && hydraulics.Value<0.5m)throw new InvalidOperationException("La velocidad hidráulica admite 0 (automática) o valores entre 0,5 y 3.");
                string b=SettingsEditor.Save(path,new() {
                    ["hydraulic_speed"]=hydraulics.Value.ToString(CultureInfo.InvariantCulture),
                    ["mouse"]=mouse.Value.ToString(CultureInfo.InvariantCulture),
                    ["follow_bucket"]=bucket.Checked?"true":"false",["chase_follow_bucket"]=chase.Checked?"true":"false",
                    ["mirrors"]=mirrors.Checked?"true":"false",["invert_look"]=invert.Checked?"true":"false",["fullscreen"]=window.Checked?"false":"true"
                },backupRoot);
                result.Text="Guardado. Abre el juego para aplicar. Copia: "+Path.GetFileName(b);
            }
            catch(Exception ex){MessageBox.Show(this,ex.Message,"No se han guardado los ajustes",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
        };
    }
}
