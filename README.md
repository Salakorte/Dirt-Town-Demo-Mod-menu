# Dirt Town Demo Mod Menu

**Version 0.6.3 · Experimental · Windows x64 · Game build 26690600**

An in-game mod menu for choosing materials, filling dump trucks, spawning vehicles and material piles, and experimenting with terrain brushes. It loads automatically with the game and opens with **F7**. **Single-player needs no external application, MelonLoader or other mod loader.**

[Español](README.es.md) · [Co-op technical notes](COOP_COMPATIBILITY.md) · [Terrain implementation notes](TERRAIN_BRUSH_RESEARCH.md)

## Installation

1. **Close Dirt Town completely.** A running game keeps its previous DLL loaded.
2. Download the compiled [winmm.dll](https://github.com/Salakorte/Dirt-Town-Demo-Mod-menu/raw/refs/heads/main/dist/winmm.dll), or open [dist/winmm.dll](dist/winmm.dll) and click **Download raw file**. Alternatively, download the repository ZIP and use the file inside `dist`.
3. In Steam, right-click **Dirt Town Demo → Manage → Browse local files**.
4. Copy **winmm.dll** into the same folder as **dirttown.exe**. If another mod already supplies a file with that name, back it up and check compatibility before replacing it.
5. If you use the co-op mod, **keep its version.dll**. This project does not replace or distribute it.
6. Launch the game normally through Steam, load a save and press **F7**.

```text
Dirt Town Demo/
├── dirttown.exe          # Existing game executable
├── winmm.dll             # This mod, from dist/
└── version.dll           # Optional, separately installed co-op mod
```

Only `winmm.dll` is needed for single-player. Do not copy the source folders into the game directory. `Abrir-ModMenu.cmd` is an optional shortcut that starts the game through Steam. The mod does not modify the game executable.

Supported game executable SHA-256:

```text
D8FDDD701778B9C4E19FFEEDC68243D49BAC983C87E6900CC6D14F6997C0E898
```

The loader checks this exact build before installing hooks. Compatibility with other builds is not established. The compiled mod checksum is in [dist/SHA256SUMS.txt](dist/SHA256SUMS.txt).

## Controls and features

The current in-game interface is in Spanish; these instructions are in English.

| Control | Action |
| --- | --- |
| **F7** | Open or close the menu |
| **F8**, by default | Fill the selected hauler to its physical capacity |
| **Assign fill shortcut** | Bind A–Z or F1–F12; F7 is reserved and Esc cancels |

Select a material and hauler in the panel. The defaults are gravel and the hauler you are driving. Add a chosen volume or fill the container completely. The shortcut works with the game focused, the panel closed and no keyboard/text capture; holding the key does not repeat the action. The binding is saved in `%LOCALAPPDATA%\DirtTownMenu\DirtTownMenu.ini`.

Other options:

- Spawn 12 nearby vehicle classes: dozer, hauler, bandvagn, truck, excavator, large loader, medium loader, tractor, dirt bike, snowmobile, hooklift and feeder.
- Spawn piles with the chosen material, radius and height.
- Attempt to remove the excavator's active usage lock. **Full excavator controls are not confirmed working.**
- Use experimental terrain brushes with an automatic free camera.

Not every vehicle model has been tested. Placement separates vehicles but does not fully check trees or terrain. The menu requests mouse and keyboard capture while open so the panel can handle clicks.

## Terrain brushes and free camera

Brushes currently require **single-player**. Open F7 and scroll down to **TERRENO** if necessary.

1. Choose **Raise/add material**, **Lower**, **Flatten** or **Smooth**. Adjust radius, strength and edge hardness; start with a 1–2 m radius and low strength.
2. Click **Activar pincel** to enable the brush and enter free camera from the current position.
3. Use **WASD** to move, **Q/E** to descend/ascend and **Shift** to move faster. Hold and drag the **right mouse button** to look around; release it to aim the brush.
4. **Left-click** visible ground outside the panel. Single-click mode applies once; continuous mode applies at most four times per second while held and waits for terrain updates. Sculpting is suspended while turning with the right mouse button.
5. **Esc**, closing F7 or closing the panel disables the brush and restores the previous camera mode and position. Losing game focus also cancels it. The selected vehicle is preserved.

Raise uses the selected material. Flatten uses a manually chosen target height or samples the ground with **Tomar altura del terreno**. Without a target, the first flatten click samples that point. There is **no exposed undo button**.

The brush contour and normal performance were manually confirmed in 0.6.2. **The 0.6.3 camera navigation/restoration and physical results of all four brush modes still require in-game confirmation.** Collision, water interaction and persistence have not been fully validated.

## Optional co-op synchronization

**Tested alongside [Dirt Town Demo Co-op by VornixMods on Nexus Mods](https://www.nexusmods.com/dirttown/mods/1).** Download that mod separately. Coexistence and room detection were tested; reliable driving synchronization for every spawned vehicle is not established.

Coordinated spawning requires the menu **and an external coordinator on both PCs**:

1. Install the co-op mod separately and keep its `version.dll`.
2. Install the same 0.6.3 `winmm.dll` on both PCs.
3. Build the coordinator using the instructions below. Keep the published files together in a separate folder on each PC. A self-contained build includes .NET; **a prebuilt coordinator package is not included in `dist/`**.
4. Join the same co-op session. Run **Abrir-Coordinador-Coop.cmd** from this project, or run the published **DirtTownMenu.exe**, on both PCs.
5. The coordinator detects the room and enables synchronization automatically. Check its status and the in-game panel. **Only the host clicks Create vehicle.** The guest receives the creation automatically. Wait for confirmation before creating another vehicle.

The coordinator checks matching starting fleets, coordinates one creation per client and verifies vehicle indices/counts. Movement and driver ownership still depend on the game's native networking. Room changes and returns to single-player update the coordinator connection automatically.

If vehicles exist on only one PC, restart the session first. A partial spawn failure blocks further creations: restart instead of repeating it. Late-join recovery is not implemented. This protocol covers **vehicle creation**; truck materials, piles and excavator unlocking are not confirmed synchronized. Terrain brushes are disabled in co-op.

## Backups, troubleshooting and uninstalling

Before the first world-changing action in each game process, the mod backs up saves and configuration to `%LOCALAPPDATA%\DirtTownMenu\backups\`. If the backup fails, the action is rejected. The game may save your changes; backups are not restored automatically.

- **F7 does nothing:** check the exact filename `winmm.dll`, its location beside `dirttown.exe`, the supported game build and that the game was restarted after installation.
- **An old menu appears:** exit the game completely before replacing the DLL. Opening another external menu does not reload it.
- **Fill does nothing:** select/drive a hauler, choose a material, close the panel and press the bound key. Other vehicle types do not use this fill action.
- **No brush target:** aim at visible ground outside the panel in an unpaused single-player game.
- **Co-op vehicles are missing or frozen:** check matching menu/coordinator versions and starting fleets on both PCs. Follow the session restart guidance above; driving remains experimental.
- **Logs:** check `%APPDATA%\Dirt Town Demo\dirttown.log`. Avoid sharing saves or personal information in reports.

To uninstall, close the game and remove **this mod's winmm.dll**. Keep `version.dll` if you still use co-op. Closing the coordinator does not unload the in-game DLL. Uninstalling does not undo saved world changes.

## Build from source

Requirements: **Windows x64, .NET 10 SDK, Visual Studio 2022 Community with C++ x64 tools, Windows SDK and MASM**. Adjust the Visual Studio path in `native/build.cmd` if needed. In PowerShell from the repository root:

```powershell
.\Build.ps1 -SelfContained
.\Package.ps1
```

| Output | Purpose |
| --- | --- |
| `artifacts/DirtTownMenu-0.6.3/winmm.dll` | Integrated menu; install beside the game executable |
| `artifacts/DirtTownMenu-0.6.3/DirtTownMenu.exe` | Optional coordinator; keep its companion files together |
| `artifacts/DirtTownMenu-0.6.3-integrado.zip` | Small integrated-menu package |
| `artifacts/DirtTownMenu-0.6.3-coop.zip` | Coordinator package with .NET runtime and documentation |

`-SkipNative` only rebuilds C# and needs an existing native build in the output folder. `tools/generate_winmm_proxy.py` regenerates forwarding declarations from Windows export names/ordinals without copying the system library implementation.

Available checks: `--loader-self-test`, `--coop-self-test`, `--self-test` and `--verify`. This is a GUI executable, so wait for the process and redirect output when testing from PowerShell:

```powershell
$test = Start-Process .\artifacts\DirtTownMenu-0.6.3\DirtTownMenu.exe `
  -ArgumentList '--loader-self-test' -Wait -PassThru `
  -RedirectStandardOutput .\loader-test.txt -RedirectStandardError .\loader-error.txt
Get-Content .\loader-test.txt, .\loader-error.txt
$test.ExitCode
```

Checks cover system WinMM forwarding, exact-build validation, preview clipping, camera movement math and simulated co-op coordination. They do not replace two-PC driving or terrain gameplay tests. The supplied DLL passed the loader checks. Automatic loading, F7 interaction and the 0.6.2 brush preview were manually confirmed.

## Recent changes

- **0.6.3:** automatic brush free camera, navigation controls and previous-view restoration.
- **0.6.2:** corrected preview drawing, viewport clipping and cached queries; normal contour and performance confirmed.
- **0.6.1:** gameplay camera capture and menu input capture fixes.
- **0.6.0:** experimental terrain brushes.
- **0.5.x:** integrated F7 menu, automatic loading through the mod's WinMM proxy and optional external co-op coordinator.

## Credits

Co-op mod: **VornixMods**. Dear ImGui: **Omar Cornut and contributors**. .NET/Windows Forms: **Microsoft**. Capstone was used for local analysis. Developed with assistance from **Codex / OpenAI**.

The menu uses the game's existing Dear ImGui 1.92.9-docking context. Official headers and their MIT license are in [native/vendor/imgui](native/vendor/imgui). Game files, saves, the co-op DLL and decompiled game source are not included. This is not an official game or co-op-mod release.

References: [Dear ImGui](https://github.com/ocornut/imgui/tree/v1.92.9-docking) and [Steam Matchmaking](https://partner.steamgames.com/doc/api/ISteamMatchmaking).
