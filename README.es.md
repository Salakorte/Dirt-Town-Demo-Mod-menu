# Dirt Town Demo — Mod Menu 0.6.3 experimental

Menú integrado que se carga al arrancar el juego. Windows x64, Dirt Town Demo build26690600. **En individual no necesitas abrir ningún programa externo.** El coordinador externo es opcional y solo se utiliza para sincronizar vehículos en cooperativo.

## Instalar el menú integrado

1. Cierra Dirt Town y los menús anteriores.
2. Extrae DirtTownMenu-0.6.3-integrado.zip.
3. Copia únicamente **winmm.dll** junto a **dirttown.exe**, en la carpeta del juego. En Steam: Administrar → Explorar archivos locales. Si ya tienes otra winmm.dll de otro mod, conserva ese archivo y revisa la compatibilidad antes de sustituirlo.
4. Conserva **version.dll** del cooperativo. Este menú no la reemplaza ni la incluye.
5. Abre Dirt Town normalmente desde Steam. El menú se carga automáticamente. **F7 lo abre y cierra dentro del juego**.

No necesitas MelonLoader ni otro cargador: la DLL propia reenvía las funciones de WinMM a la biblioteca original de Windows y carga la interfaz. No modifica dirttown.exe. El archivo Abrir-ModMenu.cmd es solo un acceso para abrir el juego desde Steam.

## Materiales y controles

Carga una partida y reanúdala. Selecciona material y volquete desde el panel F7. Por defecto se utiliza grava y el volquete hauler que conduces.

- **F8:** llenar el volquete seleccionado hasta su capacidad física.
- **Asignar atajo de llenado:** pulsa una letra A–Z o F1–F12. F7 está reservada al menú; Esc cancela. Elige una tecla que no esté asignada a otra función del juego.
- **Añadir material:** usa el volumen elegido, sin superar la capacidad del volquete.

Con F7 abierto, el menú solicita captura de ratón y teclado tanto para el fotograma actual como para el siguiente. El atajo solo funciona con el juego enfocado, el panel cerrado y sin captura de texto/teclado por ImGui. Mantenerlo pulsado no repite la acción. Puede usarse un volquete explícito o el que conduces. La tecla se guarda en `%LOCALAPPDATA%/DirtTownMenu/DirtTownMenu.ini`.

Otras opciones integradas: crear12clases de vehículos junto a tu posición, crear montones ajustando radio/altura y retirar el bloqueo activo de excavadora. No todos los modelos están probados; los controles completos de excavadora siguen sin confirmarse. La colocación separa de otros vehículos, pero no comprueba árboles ni relieve.

## Cooperativo: coordinador opcional

Probado junto a [Dirt Town Demo Co-op de VornixMods en Nexus Mods](https://www.nexusmods.com/dirttown/mods/1). Este proyecto no incluye, modifica ni sustituye su DLL. No es un proyecto oficial del juego ni del autor del cooperativo.

Para sincronizar vehículos adicionales:

1. Instala la misma winmm.dll del menú0.6.3 en ambos equipos.
2. Extrae el paquete DirtTownMenu-0.6.3-coop.zip en una carpeta aparte en cada ordenador. Incluye el runtime.NET: no necesitas instalarlo por separado.
3. Entrad en la misma partida cooperativa y abrid **Abrir-Coordinador-Coop.cmd** en ambos equipos. El coordinador se conecta al menú ya cargado; no es necesario para individual.
4. La sala se detecta y la sincronización se activa automáticamente. El estado aparece dentro del menú y en la ventana del coordinador.
5. Solo el anfitrión pulsa Crear vehículo. El invitado recibe la creación automáticamente; no debe pulsar Crear también. Esperad la confirmación de ambos equipos.

Se comprueba la misma lista inicial, se prepara la creación, se crea una instancia por equipo y se verifican índice/cantidad. El movimiento y las transferencias de conductor siguen dependiendo de la red nativa. Al salir de cooperativo o cambiar de sala se desconecta/reconecta automáticamente. Un fallo parcial detiene nuevas creaciones y no se borra por seguir consultando la misma sala.

Si una sesión ya tiene vehículos creados solo en un equipo, reiniciadla antes de probar. Si falla una creación después de iniciarse, no la repitáis: reiniciad la sesión. No hay recuperación automática al incorporarse tarde.

El protocolo solo sincroniza **creación de vehículos**. Carga de material, montones y desbloqueo de excavadora todavía no se han verificado entre equipos.

## Pinceles de terreno

El panel TERRENO está en el menú F7. Desplázate hacia abajo si no aparece a primera vista. La versión 0.6.3 añade cámara libre automática al activar el pincel.

1. Entra en una partida **individual** y abre F7. Escoge Subir/añadir material, Bajar, Nivelar o Suavizar.
2. Ajusta radio, fuerza por aplicación y dureza del borde. Empieza con radio1–2m y poca fuerza.
3. Pulsa Activar pincel: entra en cámara libre desde la posición actual. WASD desplaza la cámara, Q/E baja/sube y Mayús acelera. Mantén el botón derecho y arrastra para girar. Suéltalo para apuntar al terreno con el círculo del pincel.
4. Haz clic sobre el terreno. En modo un clic aplica una vez; en modo mantener clic se aplica como máximo cuatro veces por segundo, esperando si el juego está reconstruyendo bloques.
5. Esc desactiva el pincel y restaura la posición y el modo de cámara anteriores. Cerrar F7 o el panel, o cambiar a otra ventana, también sale del pincel. No se cambia la máquina seleccionada.

Subir utiliza el material seleccionado en el menú. Bajar rebaja el terreno. Nivelar usa una altura objetivo: puedes escribirla con el deslizador y pulsar Usar altura indicada, o pulsar Tomar altura del terreno y elegirla con el siguiente clic. Si no eliges ninguna, el primer clic de nivelado toma la altura de ese punto. Suavizar emplea la operación nativa del juego.

No se aplica sobre el propio panel, mientras se escribe texto, con el juego desenfocado ni en cooperativo. El círculo se proyecta sobre el terreno usando la cámara y consultas nativas; los cambios pasan por el sistema de ediciones y actualización de bloques del juego. No hay botón de deshacer en esta primera versión.

**Estado:** el usuario confirmó contorno y rendimiento normales en 0.6.2. La cámara libre de 0.6.3 y el resultado físico de los cuatro modos requieren prueba manual. No se afirma todavía una prueba completa de colisiones, agua ni persistencia. [TERRAIN_BRUSH_RESEARCH.md](TERRAIN_BRUSH_RESEARCH.md) conserva los detalles y las comprobaciones pendientes.

## Pruebas y límites

Compilan la DLL de carga automática y el coordinador autónomo x64. Pasan:

- Reenvío real de timeGetTime/timeBeginPeriod/timeEndPeriod a Windows, en un proceso propio.
- Verificación SHA256 nativa que acepta la build instalada y rechaza otro ejecutable.
- Protocolo simulado de dos receptores, sin duplicados y con cancelación ante flotas distintas.
- Detección automática, cambio de sala, salida a individual y conservación del bloqueo tras fallo parcial.
- Validación del punto de dibujo de ImGui, disposición IO y pruebas de memoria RX en proceso propio.

**Prueba manual confirmada por el usuario:** el menú se carga al abrir el juego, aparece con F7 y puede utilizarse. El llenado mediante atajo y la conducción sincronizada entre dos PCs siguen pendientes de confirmar. No se ha automatizado el manejo del juego. La0.4.0 anterior sí leyó una sala real con dos miembros, pero eso no verifica las novedades de0.6.3.

## Copias y desinstalación

Antes de la primera acción que modifica la partida en cada ejecución, la DLL copia partidas y configuración a `%LOCALAPPDATA%/DirtTownMenu/backups/`. Si no puede completar la copia, rechaza esa acción. Los vehículos/materiales pueden quedar guardados por el juego. No se restauran ni eliminan partidas automáticamente.

Para desinstalar, cierra el juego y retira la **winmm.dll de este menú** de su carpeta. Conserva version.dll si quieres seguir usando el cooperativo. Cerrar el coordinador no descarga la DLL del proceso: hace falta cerrar el juego.

## Compilar desde el código

Requiere SDK.NET10 y Visual Studio2022Community con C++x64 y MASM. Ajusta native/build.cmd si Visual Studio está en otra ubicación. En PowerShell desde la raíz:

```powershell
.\Build.ps1 -SelfContained
.\Package.ps1
```

Los paquetes quedan en artifacts/. El cargador está en artifacts/DirtTownMenu-0.6.3/winmm.dll. No recompiles una DLL cargada. -SkipNative solo recompila C# si el código nativo no cambia. tools/generate_winmm_proxy.py regenera las declaraciones de reenvío desde las exportaciones de Windows, sin copiar su implementación.

Pruebas: DirtTownMenu.exe --loader-self-test, --coop-self-test y --self-test. Al ser WinExe, espera al proceso y redirige su salida. COOP_COMPATIBILITY.md documenta el protocolo. El menú utiliza ImGui1.92.9-docking ya enlazado en el juego; las cabeceras oficiales y su licenciaMIT están en native/vendor/imgui.

Créditos: cooperativo de VornixMods; Dear ImGui de Omar Cornut y colaboradores; .NET/Windows Forms de Microsoft; Capstone para análisis local. Desarrollado con asistencia de Codex/OpenAI. Referencias: [Steam Matchmaking](https://partner.steamgames.com/doc/api/ISteamMatchmaking) y [Dear ImGui](https://github.com/ocornut/imgui/tree/v1.92.9-docking).
