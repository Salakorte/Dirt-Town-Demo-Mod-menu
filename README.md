# Dirt Town Demo — Mod Menu 0.4.0 experimental

Windows x64 · Dirt Town Demo build 26690600. El ZIP autónomo incluye .NET. Conserva el mod cooperativo version.dll que ya utilizáis.

## Proyecto y requisitos

Menú externo para materiales, vehículos y ajustes de Dirt Town Demo. Este repositorio contiene el código fuente; no incluye el juego, partidas ni la DLL del cooperativo.

**Probado junto a [Dirt Town Demo Co-op de VornixMods en Nexus Mods](https://www.nexusmods.com/dirttown/mods/1).** Se ha comprobado la convivencia y la detección de una sala real con dos jugadores. La creación sincronizada 0.4.0 sigue en pruebas: el manejo del vehículo adicional y los cambios de máquina entre ambos equipos aún están pendientes de confirmar.

Para jugar en cooperativo, instala ese mod siguiendo sus instrucciones en ambos equipos. Este menú no lo incluye ni sustituye su `version.dll`. No es un proyecto oficial del juego ni del autor del cooperativo.

## Obtener el menú desde el código

Necesitas Windows x64, el SDK de .NET 10 y Visual Studio 2022 Community con las herramientas de C++ x64. La ruta de Visual Studio utilizada está en `native/build.cmd`; ajústala si usas otra edición o ubicación.

Descarga o clona este repositorio. En PowerShell, dentro de su carpeta:

```powershell
.\Build.ps1 -SelfContained
.\Package.ps1
```

La salida ejecutable está en `artifacts/DirtTownMenu-0.4.0/`. Abre `Abrir-ModMenu.cmd` de la raíz. El ZIP para compartir entre equipos se genera en `artifacts/DirtTownMenu-0.4.0-coop.zip`; extrae su contenido completo antes de abrirlo. La compilación autónoma descarga componentes oficiales de .NET y el destinatario no necesita instalar .NET.

**No hay que reemplazar ningún archivo en la carpeta del juego para instalar este menú.**

## Prueba cooperativa

1. Cerrad el menú anterior y reiniciad los dos juegos. La sesión con vehículos creados solo en un equipo puede tener listas diferentes.
2. Extraed TODO el ZIP en una carpeta propia en cada ordenador, por ejemplo en Documentos. No lo copiéis a la carpeta del juego. Ejecutad Abrir-ModMenu.cmd en ambos.
3. Entrad en la misma partida cooperativa. En ambos menús abrid **Materiales y vehículos · Tiempo real**, pulsad **Conectar y leer materiales / vehículos** y después **Activar sincronización cooperativa (en ambos equipos)**.
4. Mantened ambos menús abiertos y las partidas sin pausa. El estado debe indicar anfitrión / invitado con receptor activo.
5. **Desde el anfitrión**, elegid Bulldozer y pulsad **Crear vehículo junto a mí** una sola vez, en una zona despejada. Esperad **Creación confirmada en ambos equipos**. Steam puede tardar varios segundos.
6. Comprobad que ambos veis el vehículo. Que el invitado lo conduzca, salga y entre en otra máquina mientras el anfitrión comprueba el movimiento. Después intercambiad los papeles. Esta es la comprobación pendiente del problema de controles.

El invitado recibe la creación automáticamente: **no debe pulsar Crear también**. En esta versión solo el anfitrión inicia una creación cooperativa. No utilicéis 0.3.1 junto con 0.4.0.

Si falta un receptor, difieren las listas o cambia la sala, se rechaza la operación. Si el anfitrión ya ha creado y la confirmación falla, el menú detiene nuevas creaciones: no repitáis el botón; reiniciad la sesión antes de volver a probar. No hay recuperación automática de creación parcial ni reproducción de vehículos antiguos al conectar tarde. Ambos receptores deben estar activos antes de crear.

## Qué cambia

El anfitrión prepara una orden, ambos validan el mismo orden de vehículos, cada equipo crea una sola instancia y el invitado confirma. Se transmite el identificador nativo del anfitrión para asignar inicialmente el vehículo nuevo, conservando las asignaciones existentes de conductores. Los cambios posteriores de conductor y los estados de movimiento siguen usando la red nativa del juego y del cooperativo.

El transporte usa metadatos de la sala Steam ya abierta, sin IP, puertos ni mensajes de chat. El adaptador se ha preparado para la DLL cooperativa facilitada; otra versión puede necesitar adaptación.

**Verificación:** compilación y ocho escenarios simulados del protocolo correctos. El puente nuevo ha leído la sala real del anfitrión con dos miembros, su flota y jugador nativo. **Esto no confirma todavía conducción ni replicación entre dos ordenadores.** Sigue siendo experimental.

## Otras opciones

- Añadir material a un volquete hauler o llenarlo hasta su capacidad física.
- Crear un montón del material elegido, ajustando radio y altura.
- Crear bulldozer, volquete, transporte de orugas, camión, excavadora, palas grande y mediana, tractor, moto, moto de nieve, portacontenedores y alimentador. No todos están probados ni se garantiza que todos tengan datos completos en la demo.
- Retirar el bloqueo de los datos activos de la excavadora. Sus controles completos siguen sin confirmarse.
- Ajustes de cámara e hidráulica y distancia de interacción.

El protocolo nuevo sincroniza **creación de vehículos**. Montones, carga de material y desbloqueo de excavadora no se han verificado entre equipos. No se deben duplicar estas acciones en ambos equipos por suposición.

Los materiales se leen de la partida. **Crear junto a mí** obtiene la posición y elige un punto separado de otras máquinas; no comprueba árboles ni relieve. X/Z manuales son opcionales. F6 muestra u oculta el panel. Una acción pendiente puede ejecutarse al reanudar; no la envíes otra vez.

En individual se mantiene la creación local. Pruebas anteriores confirmaron en el registro 1 m³ de grava en un volquete, un montón de 0,425 m³ y creación de dozer_2, large_loader_2 y excavator_2. Esto no verifica todos los modelos ni sus controles.

## Copias y restauración

Al conectar se copian partidas y configuración a backups/ junto al ejecutable del menú. Las copias antiguas siguen en backups/ del proyecto. El juego puede guardar materiales y vehículos creados.

No se reemplaza dirttown.exe ni version.dll. El menú carga DirtTownBridge8.dll desde su propia carpeta; permanece en el proceso hasta cerrar el juego. **Restaurar y desconectar** solo restaura los parches de interacción; no elimina vehículos, montones ni retira el puente.

Para dejar de usarlo, cierra el juego y ábrelo sin conectar el menú. Para recuperar una partida, con el juego cerrado conserva los archivos actuales y restaura una copia de saves/ a %APPDATA%/Dirt Town Demo/<cuenta>/. No se han restaurado ni eliminado partidas automáticamente.

## Desarrollo

Build.ps1 -SelfContained compila el menú autónomo y el puente con Visual Studio C++ x64. Cierra el juego antes de recompilar una DLL cargada. -SkipNative actualiza solo C# reutilizando un puente compilado del mismo código. Package.ps1 empaqueta la salida, sin archivos del juego ni DLL cooperativa.

Pruebas: DirtTownMenu.exe --coop-self-test y --self-test. Al ser WinExe, redirige la salida y espera al proceso para leer resultados. COOP_COMPATIBILITY.md describe el protocolo.

Referencias: [Steam Matchmaking](https://partner.steamgames.com/doc/api/ISteamMatchmaking), [Capstone](https://www.capstone-engine.org/lang_python.html). Capstone se usa solo en análisis local, no en el menú distribuido.

Créditos: mod cooperativo de VornixMods, .NET/Windows Forms de Microsoft y Capstone para el análisis. Proyecto desarrollado con asistencia de Codex (OpenAI). Las licencias de los componentes .NET se incluyen al empaquetar la versión autónoma. Los archivos del juego y del cooperativo pertenecen a sus respectivos autores.
