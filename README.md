# Sistema de gestión de turnos para peluquería

Aplicación para administrar clientes, profesionales, servicios y turnos de una peluquería. Está desarrollada con **C# y .NET 8** y se organiza en una **API REST ASP.NET Core** y una **interfaz web ASP.NET Core Razor Pages**. Los datos se guardan en **Microsoft SQL Server LocalDB** mediante **Entity Framework Core**.

## Características

- Inicio y cierre de sesión con perfiles de Administrador y Recepcionista.
- Panel inicial con información de la agenda, turnos del día e indicadores.
- Alta, consulta, modificación, desactivación y reactivación de clientes, profesionales y servicios.
- Gestión de turnos: creación, consulta, reprogramación, cambio de estado, desactivación y reactivación.
- Estados de turno: Pendiente, Confirmado, Finalizado y Cancelado.
- Filtros por fechas, personas, servicios, profesional y estado, según el módulo.
- Validaciones de horario de atención, intervalos, duración y superposición de reservas.
- Borrado lógico: los registros desactivados permanecen almacenados en la base de datos.
- Documentación interactiva de la API con Swagger/OpenAPI y solicitudes de prueba `.http`.

## Tecnologías y herramientas

| Componente | Tecnología |
| --- | --- |
| Lenguaje | C# |
| Plataforma | .NET 8 |
| Backend | ASP.NET Core Web API |
| Frontend | ASP.NET Core Razor Pages, HTML, CSS y JavaScript |
| Acceso a datos | Entity Framework Core y consultas LINQ |
| Base de datos | SQL Server LocalDB |
| Seguridad | ASP.NET Core Authentication/Authorization, cookies web y tokens Bearer opacos para la API |
| Documentación y pruebas | Swagger/OpenAPI y archivos HTTP |
| Entorno de desarrollo | Visual Studio 2022 (compatible con .NET 8) o CLI de .NET |

## Estructura de la solución

```text
PeluqueriaTurnos/
├── PeluqueriaTurnos.sln
├── Peluqueria.Api/
│   ├── Controllers/       # Endpoints REST y validaciones de operaciones
│   ├── DTOs/              # Contratos de entrada y salida HTTP
│   ├── Models/            # Entidades y enumeraciones
│   ├── Repositories/      # Interfaces y acceso a datos
│   ├── Data/              # DbContext, inicialización y migraciones
│   ├── Security/          # Tokens, autenticación y roles
│   ├── Configuracion/     # Opciones de la agenda
│   ├── Middleware/        # Manejo global de excepciones
│   ├── Services/          # Notificaciones
│   ├── HTTP/              # Solicitudes de prueba
│   └── Program.cs         # Registro de servicios y arranque de la API
└── Peluqueria.Web/
    ├── Pages/             # Pantallas Razor y sus PageModels
    ├── Models/            # Modelos y DTO consumidos por la web
    ├── Services/          # Cliente HTTP de la API
    ├── wwwroot/           # Recursos estáticos
    └── Program.cs         # Configuración y arranque de la web
```

## Requisitos previos

1. Windows y **.NET SDK 8** instalado.
2. **SQL Server Express LocalDB**, con instancia `(localdb)\MSSQLLocalDB` disponible.
3. Visual Studio 2022 con soporte de desarrollo ASP.NET Core, o una terminal con `dotnet`.
4. Certificado HTTPS de desarrollo configurado y confiable para usar las direcciones `https://localhost`.

**SQL Server Management Studio** es opcional: permite inspeccionar las tablas y los registros.

## Configuración

La API utiliza la cadena de conexión definida en `Peluqueria.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PeluqueriaTurnosDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

El horario de atención se configura en `Agenda` dentro de los archivos `appsettings.json` de la API y de la web:

```json
"Agenda": {
  "HoraApertura": 9,
  "HoraCierre": 20,
  "IntervaloMinutos": 30
}
```

La interfaz web utiliza esta dirección para comunicarse con la API, indicada en `Peluqueria.Web/appsettings.json`:

```json
"ApiBaseUrl": "https://localhost:7184/"
```

Si se cambian los puertos o las opciones de agenda, adecuar la configuración de ambos proyectos.

## Cómo ejecutar el sistema

### Desde Visual Studio

1. Abrir `PeluqueriaTurnos.sln`.
2. Restaurar los paquetes NuGet, si se solicita.
3. Confirmar que SQL Server LocalDB esté disponible.
4. Iniciar `Peluqueria.Api` y luego `Peluqueria.Web`. Se pueden configurar ambos como proyectos de inicio de la solución.
5. Abrir la interfaz web en `https://localhost:7287`.
6. Para consultar o probar la API, acceder a `https://localhost:7184/swagger`.

### Desde una terminal

En la carpeta donde está `PeluqueriaTurnos.sln`:

```bash
dotnet restore PeluqueriaTurnos.sln
dotnet build PeluqueriaTurnos.sln
```

En una terminal, iniciar la API:

```bash
dotnet run --project Peluqueria.Api --launch-profile Peluqueria.Api
```

En otra terminal, iniciar la web:

```bash
dotnet run --project Peluqueria.Web --launch-profile Peluqueria.Web
```

Si se requiere confiar en el certificado HTTPS de desarrollo:

```bash
dotnet dev-certs https --trust
```

Al arrancar la API, `DbInitializer` ejecuta `Database.MigrateAsync()` para aplicar las migraciones pendientes y preparar la estructura de la base de datos. Cuando aún no hay usuarios, genera las cuentas iniciales de demostración.

## Usuarios de demostración

| Usuario | Contraseña | Perfil |
| --- | --- | --- |
| `admin` | `Admin123!` | Administrador |
| `recepcion` | `Recep123!` | Recepcionista |

Estas credenciales corresponden al entorno de práctica. No se deben utilizar para publicar un entorno real de producción.

### Permisos

- **Administrador:** gestiona clientes, profesionales, servicios y turnos.
- **Recepcionista:** consulta profesionales y servicios; puede gestionar clientes y turnos.

Las operaciones protegidas de la API requieren autenticación mediante token Bearer. En la web, el ingreso se mantiene mediante cookies de autenticación y sesión.

## Endpoints REST

La API agrupa sus rutas en cinco controladores:

| Área | Rutas principales |
| --- | --- |
| Autenticación | `POST /api/auth/login`, `GET /api/auth/me`, `POST /api/auth/logout` |
| Clientes | `GET /api/clientes`, `GET /api/clientes/{id}`, `POST /api/clientes`, `PUT /api/clientes/{id}`, `DELETE /api/clientes/{id}`, `POST /api/clientes/{id}/activar` |
| Profesionales | `GET /api/profesionales`, `GET /api/profesionales/{id}`, `POST /api/profesionales`, `PUT /api/profesionales/{id}`, `DELETE /api/profesionales/{id}`, `POST /api/profesionales/{id}/activar` |
| Servicios | `GET /api/servicios`, `GET /api/servicios/{id}`, `POST /api/servicios`, `PUT /api/servicios/{id}`, `DELETE /api/servicios/{id}`, `POST /api/servicios/{id}/activar` |
| Turnos | `GET /api/turnos`, `GET /api/turnos/{id}`, `POST /api/turnos`, `PUT /api/turnos/{id}`, `DELETE /api/turnos/{id}`, `POST /api/turnos/{id}/activar` |

El listado de turnos acepta filtros opcionales como `fecha`, `fechaDesde`, `fechaHasta`, `profesionalId`, `estado`, `cliente`, `profesional`, `servicio` e `incluirInactivos`.

Ejemplo:

```http
GET /api/turnos?estado=Pendiente&profesionalId=2
```

## Reglas principales de los turnos

- Las nuevas reservas deben tener fecha y hora futuras.
- Su inicio debe respetar los intervalos configurados (30 minutos por defecto).
- Deben comenzar dentro del horario de atención y finalizar antes del cierre.
- El cliente, el profesional y el servicio elegidos deben estar activos.
- No se permiten reservas cuyos horarios se superpongan para el mismo profesional.
- Al reprogramar o reactivar un turno se comprueba nuevamente la disponibilidad.
- Una reserva Cancelada libera el horario; una reserva desactivada conserva su registro mediante borrado lógico.

## Base de datos y persistencia

El modelo de datos se administra con Entity Framework Core. Las entidades principales son **Cliente**, **Profesional**, **Servicio**, **Turno**, **Usuario** y **Sesión**. La persistencia utiliza SQL Server LocalDB y migraciones para configurar tablas, relaciones e índices.

Los métodos `DELETE` de clientes, profesionales, servicios y turnos aplican **borrado lógico** (`Activo = false`). Los listados permiten incluir registros inactivos cuando corresponde; se dispone de endpoints para reactivarlos.

## Pruebas de la API

### Swagger

1. Iniciar la API y abrir `https://localhost:7184/swagger`.
2. Ejecutar `POST /api/auth/login` con un usuario de demostración.
3. Copiar el token devuelto y presionar **Authorize**.
4. Probar las rutas habilitadas para el perfil autenticado.

Ejemplo de inicio de sesión:

```json
{
  "usuario": "admin",
  "password": "Admin123!"
}
```

### Archivos HTTP

La carpeta `Peluqueria.Api/HTTP` contiene archivos de solicitudes para autenticación y pruebas de clientes, profesionales, servicios y turnos. Deben adaptarse las variables de token y los identificadores a la información de la base utilizada.

## Notificaciones

Al crear o modificar turnos, la implementación activa de `INotificador` registra mensajes en la consola. El servicio está organizado mediante interfaz para facilitar su sustitución por una integración de mensajería o correo.

## Documentación adicional

Para explicaciones de arquitectura, clases, métodos, flujo de operaciones, instalación, validaciones y casos de prueba, consultar el **Manual técnico y funcional del sistema de gestión de turnos para peluquería**.
