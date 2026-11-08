# Contexto de Proyecto: Bloxstrap RPC Fix (Dimension Teleport Bug)

## 1. Descripcion del Problema

En Windows, al jugar a Roblox utilizando Bloxstrap con Discord Rich Presence (RPC) activado, cuando el jugador se teletransporta entre dimensiones, servidores reservados o sub-lugares dentro de una misma experiencia (por ejemplo: el Segundo o Tercer Mar en *Blox Fruits*, mazmorras en *Deepwoken*, raids en *Type Soul*), la presencia en Discord se desconecta, se restablece a vacia y Discord vuelve a mostrar la etiqueta generica "Roblox" o ningun estado por el resto de la sesion.

### Diferencia de Comportamiento: Linux (Sober) vs Windows (Bloxstrap)

* **Linux (Sober):** Funciona de forma ininterrumpida porque Sober es el propio runtime del juego ejecutandose nativamente. Tiene acceso directo al motor del juego, conoce en memoria el `UniverseId` raiz y el ciclo de vida de los cambios de escena.
* **Windows (Bloxstrap):** Bloxstrap es un proceso externo que lee archivos de texto en disco (`%localappdata%\Roblox\logs`). No tiene acceso directo a la memoria del juego y depende de expresiones regulares e integraciones de red.

---

## 2. Analisis Tecnico de la Falla en el Codigo Fuente

La investigacion del codigo fuente oficial de Bloxstrap revelo tres causas encadenadas:

### Causa A: Falso cierre de sesion en cada teletransporte (`ActivityWatcher.cs`)

En las lineas 276-287 de `Bloxstrap/Integrations/ActivityWatcher.cs`:

```csharp
if (logMessage.StartsWith(GameDisconnectedEntry))
{
    App.Logger.WriteLine(LOG_IDENT, $"Disconnected from Game ({Data})");

    Data.TimeLeft = DateTime.Now;
    History.Insert(0, Data);

    InGame = false;
    Data = new();

    OnGameLeave?.Invoke(this, EventArgs.Empty);
}
```

* `GameDisconnectedEntry` corresponde a `[FLog::Network] Time to disconnect replication data:`.
* Esta linea se imprime cada vez que el juego cambia de servidor o dimension.
* Bloxstrap borra todos los datos de la partida anterior (`Data = new()`) y dispara el evento `OnGameLeave`, asumiendo que el usuario cerro el juego, incluso cuando previamente se habia detectado el inicio de un teletransporte (`GameTeleportingEntry` -> `_teleportMarker = true`).

### Causa B: Borrado prematuro y bloqueo por Rate Limit en Discord (`DiscordRichPresence.cs`)

En las lineas 328-337 de `Bloxstrap/Integrations/DiscordRichPresence.cs`:

```csharp
if (!_activityWatcher.InGame)
{
    App.Logger.WriteLine(LOG_IDENT, "Not in game, clearing presence");

    _currentPresence = _originalPresence = null;
    _messageQueue.Clear();

    UpdatePresence(); // Ejecuta _rpcClient.ClearPresence()
    return true;
}
```

1. Al recibir `OnGameLeave`, `SetCurrentGame()` ejecuta de inmediato `_rpcClient.ClearPresence()`.
2. Al borrarse la presencia en Discord, el detector nativo de procesos de Discord ve `RobloxPlayerBeta.exe` y sobrescribe el perfil con la etiqueta generica "Roblox".
3. Discord impone un **rate-limit estricto de 1 actualizacion cada 15 segundos**. Como Bloxstrap envio `ClearPresence()` en el segundo 0, cuando el nuevo sub-lugar termina de cargar 3 o 4 segundos despues y se dispara `OnGameJoin`, Discord **descarta silenciosamente el nuevo paquete** por exceso de frecuencia.

### Causa C: Excepcion no controlada en sub-lugares privados (`UniverseDetails.cs`)

* En sub-lugares privados o dimensiones temporales, la API publica de Roblox (`https://games.roblox.com/v1/games?universeIds=...`) no siempre devuelve metadatos o miniaturas.
* En `Bloxstrap/Models/Entities/UniverseDetails.cs`, si la respuesta de miniaturas esta vacia o no coincide con el ID, la llamada a `.First()` lanza una excepcion `InvalidOperationException`.
* Esto aborta `SetCurrentGame()` en `Bloxstrap/Integrations/DiscordRichPresence.cs`, dejando la presencia nula de forma permanente.

---

## 3. Plan de Modificacion e Implementacion

### Modificacion 1: Persistencia de Sesion durante Teletransportes en `ActivityWatcher.cs`

1. Incorporar la propiedad publica `InTeleport` que se activa al detectar `GameTeleportingEntry`.
2. Al encontrar `GameDisconnectedEntry`:
   * Si `InTeleport` o `_teleportMarker` es verdadero, **no invocar `OnGameLeave`**.
   * Preservar el `UniverseId`, las miniaturas cargadas y la referencia al `RootActivity` del juego principal para que la nueva dimension los herede inmediatamente.
3. Solo invocar `OnGameLeave` cuando se reciba `GameLeavingEntry` (`leaveUGCGameInternal`, retorno a la app principal) o cuando el proceso `RobloxPlayerBeta.exe` haya terminado.

### Modificacion 2: Retencion de Estado y Tolerancia a Fallos en `DiscordRichPresence.cs`

1. En `SetCurrentGame()`, si `InGame` es falso pero `InTeleport` es verdadero, **no enviar `ClearPresence()`**; mantener el Rich Presence visible en Discord mientras carga la nueva dimension.
2. Si la consulta a la API de Roblox falla para un sub-lugar, aplicar un fallback que reutilice el nombre y la miniatura del `UniverseDetails` del juego raiz en vez de vaciar el estado.

### Modificacion 3: Pipeline de Compilacion Automatica en `.github/workflows/ci-release.yml`

1. Reemplazar la seccion dependiente del servicio de firmas privado (`SignPath`) por una accion estandar de GitHub Actions (`softprops/action-gh-release@v2`).
2. Configurar el workflow para compilar el binario unico para Windows x64:
   ```bash
   dotnet publish -p:PublishSingleFile=true -r win-x64 -c Release --self-contained false .\Bloxstrap\Bloxstrap.csproj
   ```
3. Publicar automaticamente `Bloxstrap.exe` en los Releases del repositorio propio al hacer push o crear un tag.
