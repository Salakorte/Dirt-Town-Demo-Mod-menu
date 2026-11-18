# Cooperativo: creación coordinada experimental, 0.4.0

## Alcance actual

El usuario autoriza menú en ambos equipos, sustituyendo la restricción anterior de solo anfitrión. También autoriza modificar version.dll si fuera necesario. Esta implementación la conserva y añade el receptor a nuestro menú.

DLL facilitada: 977396 bytes, SHA256 5019912BF18A971AB9E9461793359FF80953E056CFA30B44B928E9ED7F1506AE. Hash en disco comprobado sin cambios. Sin código fuente disponible.

Juego: build26690600, SHA256 D8FDDD701778B9C4E19FFEEDC68243D49BAC983C87E6900CC6D14F6997C0E898.

## Evidencia del diagnóstico

- A77150 crea una máquina y devuelve un índice local. La0.3.1 no comunicaba su construcción al invitado.
- Receptor state, función5D3660, decodifica tipo5 con1FF050. En5D4AFB–5D4B10 descarta índices >= número de máquinas E8C0/E8C8. Una posición no construye el vehículo ausente.
- Tabla RVA1498560 contiene27 tipos de mensaje. No identificado spawn dinámico nativo; no se han descartado todos los subtipos.
- Receptor owners5D67C2 exige longitud igual al número de máquinas en5D6806–5D6825. State compara dueño recibido con vector EAF0 en5D4B79–5D4B84. Flotas distintas pueden impedir movimiento y cambios de control.
- A77150 llama A3F120(false): conserva propietarios previos y amplía con cero. No hay evidencia de reset total. El puente conserva explícitamente esas entradas y asigna solo el nuevo al jugador nativo del anfitrión transmitido.

Esto respalda igualar índices, cuerpos y propietarios. No prueba que todos los bloqueos del cooperativo estén resueltos.

## Adaptador

El script tools/coop_layout.py obtiene del COFF el símbolo _ZN4coop12_GLOBAL__N_1L10chat_lobbyE: RVA56940. Se comprueban timestamp6AF9E7C5 y SizeOfImage8D000 antes de leer. Es validación del adaptador concreto, no un bloqueo general del menú por presencia de version.dll.

Steam API ya cargada: Lobby Data dtmenu_spawn_v1 por propietario y Member Data dtmenu_ready_v1 por miembro. Según la [documentación oficial](https://partner.steamgames.com/doc/api/ISteamMatchmaking), solo el propietario escribe datos de sala, cada miembro escribe los suyos y los cambios se agrupan con demora. Transporte de órdenes ocasionales, no movimiento frecuente ni chat.

Puente8, bloque compartido8304 bytes; cmd6 lee sala/flota/jugador nativo y cmd7 publica orden o confirmación. Se procesa en el hilo principal y la simulación debe avanzar.

## Protocolo

1. Ambos anuncian versión1, huella del orden de nombres de máquinas y cantidad.
2. El anfitrión valida receptores con la misma flota y publica prepare con token, modelo, posición y dueño nativo inicial.
3. El invitado valida y confirma prepared sin crear.
4. El anfitrión crea exactamente una máquina en el índice esperado y publica commit con huella/cantidad posteriores.
5. El invitado exige una preparación idéntica, marca el token antes de crear, crea una vez y valida índice/huella. Confirma applied.
6. El anfitrión espera a todos antes de anunciar éxito. Consulta750ms, timeout40s.

Cambio de sala, dueño o miembros cancela. Fallo antes de mutar publica abort; después detiene creaciones sin rollback físico ni reenvío incierto. Metadata antigua al iniciar receptor no se reproduce. Sin catch-up para incorporación tardía: ambos deben empezar con la misma flota y activar antes de crear. Solo anfitrión inicia. La red nativa conserva movimiento y transferencias de control. Otros comandos quedan fuera del protocolo.

## Verificación

- Release autónoma x64 y puente8 compilados.
- Ocho casos simulados pasan: dos creaciones con índices/orden/dueño host coincidentes y sin replay; receptor ausente; flota distinta; metadata antigua; timeout; ACK preparado incorrecto; índice incorrecto tras commit y sin repetición; salida de miembro durante preparación.
- Self-test de ajustes, build, memoria RX en proceso propio y restauración correcto.
- Lectura REAL cmd6: adaptador disponible, red activa, anfitrión jugador0,21 máquinas,2 miembros. No se publicaron órdenes ni crearon objetos durante esta comprobación.
- La primera carga superó10s; terminó después y leer el puente cargado funcionó. Se amplía la espera inicial a30s.

Pendiente real: publicación/recepción Steam con ambos PCs y prueba de bulldozer visible/móvil, entrar/salir/cambiar máquina en ambas direcciones. No confirmado manejo físico ni resolución final del bloqueo de controles.

## 0.5.0 — interfaz integrada y activación automática

La interfaz de acciones se dibuja con el ImGui del juego. El coordinador externo sigue ejecutando CoopSync y consulta el buzón de creaciones del panel. Cmd8 lee el evento,9 confirma su resultado y10 comunica estado/heartbeat. Las acciones locales tienen un bloque Shared temporal separado: no pisan peticiones IPC del coordinador.

AutoCoopSession detecta la sala periódicamente. Activa el receptor al entrar, conserva la misma instancia mientras la sala/autoridad no cambian, la detiene al salir y la reconstruye cuando cambia la sala. Una sesión con fallo parcial se conserva detenida: refrescar no elimina la protección contra duplicados. Invitado no autoriza creaciones. No se añade un protocolo de movimiento propio.

Pruebas simuladas de detección, cambio de sala, salida a individual y conservación de fallo pasan. La0.5.0 no ha sido probada aún en la sala real ni en el juego: la lectura real descrita arriba corresponde a0.4.0. El arranque de la sincronización ahora es automático, sin botón de activación.

Aclaración final0.5.0: carga automática por winmm.dll propia, importada por el juego. No hace falta programa externo para individual. El coordinador únicamente abre el mapping del módulo ya cargado; no inyecta DLLs ni arranca el menú. Mantenerlo abierto en ambos equipos para sincronización cooperativa. Los nuevos respaldos nativos se crean bajo LOCALAPPDATA/DirtTownMenu/backups antes de la primera mutación.
