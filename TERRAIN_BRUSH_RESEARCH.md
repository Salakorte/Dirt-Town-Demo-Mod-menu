# Pinceles de terreno — investigación de la build 26690600

Fecha: 18-11-2026. La investigación inicial fue estática; las secciones posteriores describen la implementación. El usuario confirmó contorno y rendimiento normales en 0.6.2. La deformación física y la cámara de 0.6.3 siguen pendientes de prueba manual.

## Base encontrada en el juego

El ejecutable contiene un editor con 38 herramientas. La tabla de nombres de herramientas en RVA14D14A0, utilizada por425B30 en425CA8–425CB6, incluye raise, lift, lower, paint, flatten y smooth. No son nombres inventados a partir de Cities: Skylines: existen en esta build, aunque su existencia no confirma que estén accesibles ni adaptadas a una partida activa.

La tabla de operaciones RVA1647C60 tiene registros de24bytes: identificador, puntero al nombre y banderas. Operaciones relevantes identificadas:

| ID | Operación | Uso candidato |
|---|---|---|
| 1 | raise | Elevar con pincel; parámetros y tratamiento de volumen pendientes |
| 2 | lift | Elevación alternativa; hay que distinguirla de raise |
| 3 | lower | Rebajar terreno; gestión del material retirado pendiente |
| 4 | paint | Pintar material; no asumir que añade volumen |
| 7 | flatten | Nivelar; referencia de altura pendiente de adaptar |
| 8 | smooth | Suavizar; mantener borde y filtrar pendientes |
| 83 | pile | Añadir montón; ya usado en la versión anterior del menú |
| 97 / 98 | undo / redo | Historial existente, alcance y compatibilidad con simulación pendientes |

El panel del editor425B30 ofrece ajustes de herramienta y pincel. Sus textos describen radio, caída hacia el borde, altura objetivo y suavizado. Las cadenas cercanas a14D0390 describen una plataforma a la altura del punto pulsado;14D09B8 describe suavizar conservando el contorno del área. Los textos14C7170 y14CB7A0 describen caída radial y clonación desde otra zona.

Hay además un sistema diferente de puntos de control del campo del terreno. Función349EA0 nivela puntos seleccionados a su altura media y34A380 los suaviza. Trabajan con estructuras de autoría del escenario y selección del editor, no con un World* intercambiable. No se deben llamar pasando el mundo de la partida por semejanza de nombres.

## Ruta propuesta

Primera prueba: individual, sobre copia de una partida. Trazar el constructor de evento388940 y el aplicadorA02A80 para las operaciones1,3,7,8, igual que se hizo con pile83. Comprobar qué campos lee cada variante: el descriptor de montón no debe reutilizarse sin verificar la disposición de cada operación.

Para el primer pincel bastaría un círculo de radio0,5–10m, intensidad limitada y caída suave, con modos elevar, bajar, nivelar y suavizar. La altura de nivelado se tomaría de un punto del mundo; la de añadir material usaría el selector del menú. La posición debe salir de un raycast real desde la cámara, con previsualización del área antes de aplicar. Esa conversión pantalla/mundo todavía no se ha identificado.

La aplicación continua debe separar inicio, muestras de trazo y fin, limitar frecuencia y agrupar historial para deshacer una pasada completa. Esc cancela. No se aplicaría al escribir en chat, con el juego desenfocado o al interactuar con el propio panel. No se simulará un pincel repitiendo montones mientras se mantiene un clic.

## Validaciones necesarias

- Cambios visibles y colisiones del suelo coinciden, sin recargar el mapa.
- Suavizado/nivelado conservan borde y respetan intensidad y altura elegidas.
- Material y volumen se corresponden con lo solicitado; distinguir pintar de depositar.
- Reacción de agua, árboles, rocas, vehículos y capas subterráneas.
- Deshacer/rehacer y persistencia tras guardar/cargar, con copia de seguridad.
- Coste por trazo y ausencia de reconstrucciones simultáneas del terreno.

El editor existente puede modificar la receta y reconstruir el escenario, mientras que la partida mantiene simulación y registros propios. Hay que verificar cómo enlazan ambos antes de exponer botones de esculpido en vivo.

En cooperativo el protocolo actual solo cubre creación de vehículos. Un futuro pincel requeriría acordar autoridad, operación, material, muestras del trazo, identificador único y confirmaciones, o demostrar que los records nativos ya lo transportan. No se ha hecho esa prueba. Hasta entonces, no se ofrece esculpido cooperativo.

## Evidencia y límites

Fuentes: ejecutable instalado y sus instrucciones seleccionadas; nombres, identificadores y puntos de integración documentados arriba. No se distribuye una descompilación ni archivos del juego. La nueva0.5.0 únicamente señala en su panel que los pinceles están en investigación; no presenta estas herramientas como funciones terminadas.

## Implementación0.6.0 — pincel en el cursor

Constructor nativo de evento1317F20 genera descriptor completo0x7A8, destructor1318900. Tipo+28; nombreGNUstring+8; material+30; centroX/Z+50/+54; radio+58; dureza+5C (confirmada en4216A0); fuerza/profundidad+60; altura objetivo+64 (panel420470). La capa0 en+5A0 corresponde over everything, aceptada por9D12B0 para las4operaciones. Se utiliza el constructor completo, no el descriptor de pile copiado con otro nombre.

El camino A02A80 valida y registra la operación; para estas herramientas llama9FD020, que espera reconstrucciones y actualiza suelo/registros nativos. La aceptación se comprueba porque aumenta el vector de ediciones, sin tratarlo como prueba de colisiones final.

Conversión pantalla/mundo identificada:27D960 bool(World*,Camera*,float mouseX,float mouseY,float width,float height,DVec3*hit,Vec3*normalOpcional). La llamada549A42 pasa normalnull. Raycast contra mundo y fallback por altura del terreno con bisección. Getter altura93CB90 retornafloat. Proyección inversa a pantalla27D770 bool(Camera*,DVec3*,float width,float height,ImVec2*out).

Cámara obtenida del argumento5 del HUD57AAE0. Puente MASM captura World/copiaCamera0x30 preservandoGPR/XMM y todos los argumentos de pila y salta al trampolín. Prologo19bytes no relativos verificado, distinto del hookImGuiEndFrame16. La copia tiene caducidad1500ms y debe corresponder al mismoWorld.

Panel cuatro modos, radio0.5–10m,fuerza0.01–1,dureza0–1,altura capturada/manual; previsualización48segmentos mediante proyección nativa y AddLine. Modo clic y continuo máximo4Hz, sin pintar durante reconstrucción. Esc/F7/cierre cancelan; no pinta sobre panel, desenfocado o introduciendo texto. Captura de ratón/teclado mientras está activo. Cooperativo rechazado.

No deshacer expuesto: agrupar/deshacer un trazo completo sigue pendiente. El historial nativo permanece, pero no se manipula a ciegas. Build0.6.0 y pruebas de cargador/perfil/protocolo pasan; la prueba real de los pinceles requiere actualizar DLL con el juego cerrado y verificación manual.

## Corrección0.6.1 — cámara y entrada

Prueba REAL de lectura en0.6.0: camera=false,hit=false en individual. El enganche del editor57AAE0 no proporcionaba cámara durante esta partida. Corregido al HUD de etiquetas de juegoC723E0, cámara argumento3(R8), que se usa en la proyecciónC72737. El World se toma del punto de tick previamente validado. Nueva firma19bytes verificada. Diagnóstico cmd11 añade contador de capturas y estado de entrada.

La0.6.0 solo forzaba io.WantCaptureMouse/Keyboard durante el pincel y NewFrame podía recomputarlos. La0.6.1 solicita captura mientras F7 esté visible y también en el próximo frame con SetNextFrameWantCaptureMouse114D610 y Keyboard114D5F0. Se han comprobado las escrituras a context2D04/2D08 y su consumo en el actualizador de entrada1146160. No se modifican eventos de ImGui ni se bloquea su propio ratón.

Compilación/pruebas PASS. Instalada0.6.1 con juego cerrado, respaldo de DLL anterior. Pendiente lectura real camera/hit y prueba manual de que el juego respete la captura y de los cuatro pinceles. No dar el resultado físico por verificado solo por compilar.

## 0.6.2 — error de ABI de la previsualización

Usuario activó0.6.1: pantalla verde y ralentización severa, antes de confirmar aplicación. Diagnóstico estático encontró un error propio: RVA11A9630 es ImDrawList::AddCircle, no AddLine. Lee radio en XMM2, segmentos y grosor en pila; se la llamaba como AddLine con segundo punto en R8, por tanto recibía parámetros incompatibles. Corregido a11A8510, que lee ambos puntos desde RDX/R8 y grosor en pila. Verificados los bytes del punto de entrada en BuildProfile.

La previsualización tiene un máximo fijo48segmentos. Se recorta en CPU al viewport y se rechazanNaN/infinito; pruebas nativas verifican líneas interiores, exteriores, cruces horizontales/verticales y coordenadas extremas. Las consultas del terreno/raycast se cachean50ms(20Hz) y se renuevan inmediatamente en un clic/aplicación para evitar usar un punto viejo. El dibujado de líneas permanece por frame. No modifica el algoritmo de deformación del terreno ni convierte el éxito de las pruebas técnicas en verificación física ingame.

Juego cerrado por usuario, sin entradas automatizadas. Registro muestra salida normal código0; no prueba de edición de pincel confirmada. Pendiente comprobar la nueva previsualización visualmente y luego un solo clic de terreno.

## 0.6.3 — cámara libre para el pincel

La ruta de juego 264540 mantiene la pose en World+19EC0 y el estado de cámara en World+19A40. En 265492 se obtiene el puntero a ese estado; en 26905E–269088 se pasa como cuarto argumento a 24C010, junto con World y la pose como tercer argumento. En 24CA63 se lee su primer entero y la traza nativa lo traduce como chase=0, cab=1 y free=2. La rama free de 24C0EE no ejecuta el seguimiento de máquina. Esto distingue el modo de cámara del selector de objeto World+EAA4, que no se cambia.

Al armar el pincel se guarda modo y pose (48 bytes), se activa free y se permite navegar con WASD, Q/E, Mayús y arrastre derecho. La base direccional está verificada con 27D770 y sinf/cosf: avance=(sin(yaw)cos(pitch),sin(pitch),-cos(yaw)cos(pitch)); ángulos en radianes. El movimiento limita dt a 50 ms, normaliza diagonales y valida límites/finitos. El ratón y teclado siguen capturados para impedir acciones del juego; solo la cámara recibe los controles del mod. No pinta durante el giro derecho. Esc/F7/cierre, pérdida de foco y cancelación restauran modo y pose en el mismo mundo; si deja de llegar el tick, se aplaza la restauración hasta que vuelve a ser válido. Nunca se restaura una escena anterior en otro World.

Las pruebas nativas cubren ejes, diagonales, dt, rechazo de NaN, límites de posición y pitch. No sustituyen la prueba manual de entrada, navegación, restauración y pincel dentro de la partida. No se ha automatizado el manejo del juego.
