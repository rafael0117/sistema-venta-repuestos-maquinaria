# Manual completo: Dockerización y despliegue en Azure

Este manual está basado en el análisis de tu rama actual (`work`) y en la estructura real de tu solución `SistemaRepuestosMaquinas.sln`.

## 1) Diagnóstico actual de la rama

### 1.1 Arquitectura identificada

Tu solución tiene una arquitectura por capas:

- `SistemaRepuestosMaquinas.API`: API ASP.NET Core (.NET 8) con JWT.
- `SistemaRepuestosMaquinas.Web`: frontend ASP.NET Core MVC/Razor.
- `SistemaRepuestosMaquinas.Business`, `Data`, `Entity`, `Common`: capas de negocio y acceso a datos.

### 1.2 Hallazgos importantes para dockerizar

1. **Framework objetivo**: `net8.0` (ideal para imágenes oficiales de Microsoft).
2. **Base de datos**: SQL Server por `UseSqlServer` en la API.
3. **Inicialización DB**: `DatabaseInitializer.InitializeAsync` ejecuta migraciones/EnsureCreated y seed de roles/admin al iniciar API.
4. **Comunicación Web -> API**: Web usa `Api:BaseUrl` (se puede sobreescribir por variable de entorno `Api__BaseUrl`).
5. **Secretos en appsettings**: hay llaves JWT, credenciales SMTP y tokens de pago en `appsettings.json`; en Azure deben salir del repositorio y manejarse como variables seguras.

## 2) Archivos Docker agregados

Se agregaron estos archivos en la raíz:

- `Dockerfile.api`
- `Dockerfile.web`
- `docker-compose.yml`
- `.dockerignore`

Objetivo:

- Correr localmente todo con `docker compose`.
- Dejar base lista para publicar en Azure Container Apps o Azure Web App for Containers.

## 3) Requisitos previos

Instala/local verifica:

- Docker Desktop 4.x+
- Docker Compose v2+
- Azure CLI (`az`)
- (Opcional) `jq`

Login Azure:

```bash
az login
az account show
```

## 4) Prueba local con Docker Compose

## 4.1 Levantar servicios

Desde la raíz del repo:

```bash
docker compose up --build -d
```

Servicios y puertos:

- SQL Server: `localhost:1433`
- API: `http://localhost:8081`
- Web: `http://localhost:8082`

## 4.2 Verificar estado

```bash
docker compose ps
docker compose logs -f api
docker compose logs -f web
```

Checklist mínima:

- API responde (ejemplo): `http://localhost:8081/swagger` (si habilitas entorno dev) o endpoints públicos.
- Web responde: `http://localhost:8082`.
- Seed admin ejecutado por `DatabaseInitializer`.

## 4.3 Apagar entorno

```bash
docker compose down
# Si quieres borrar datos también:
docker compose down -v
```

## 5) Endurecimiento antes de producción

Antes de Azure, aplica esto sí o sí:

1. **JWT secret fuerte** (mínimo 32 caracteres de alta entropía).
2. **No subir secretos al repositorio**: tokens de MercadoPago, SMTP, etc.
3. **Usar Key Vault** o variables secretas en el servicio Azure.
4. **CORS/HTTPS**: validar políticas y redirecciones según topología final.
5. **Health checks** HTTP para API/Web.
6. **Logs estructurados** y diagnóstico centralizado (Application Insights/Log Analytics).

## 6) Estrategia recomendada en Azure

Para tu arquitectura, te recomiendo esta opción por simplicidad operativa:

- **Azure Container Apps**:
  - `srm-api` (contenedor API)
  - `srm-web` (contenedor Web)
- **Azure SQL Database** (PaaS)
- **Azure Container Registry (ACR)** para imágenes
- **(Opcional) Key Vault** para secretos

> Alternativa válida: Azure App Service for Containers (dos Web Apps separadas).

## 7) Despliegue en Azure paso a paso (Container Apps)

> Ajusta nombres para evitar colisiones globales.

### 7.1 Variables base

```bash
RG="rg-srm-prod"
LOC="eastus"
ACR="acrsrmprod001"
ENV="cae-srm-prod"
API_APP="srm-api"
WEB_APP="srm-web"
SQL_SERVER="sql-srm-prod-001"
SQL_DB="RSISTEMADB"
SQL_ADMIN="sqladmin"
SQL_PASS='CambiaEsto!Pass12345'
IMAGE_API="$ACR.azurecr.io/srm-api:1.0.0"
IMAGE_WEB="$ACR.azurecr.io/srm-web:1.0.0"
JWT_SECRET='CAMBIA_AQUI_POR_SECRETO_32+_MUY_FUERTE'
```

### 7.2 Crear recursos base

```bash
az group create -n $RG -l $LOC
az acr create -g $RG -n $ACR --sku Basic
az acr login -n $ACR
az extension add --name containerapp --upgrade
```

### 7.3 Build y push de imágenes

```bash
docker build -f Dockerfile.api -t $IMAGE_API .
docker build -f Dockerfile.web -t $IMAGE_WEB .
docker push $IMAGE_API
docker push $IMAGE_WEB
```

### 7.4 Crear Azure SQL

```bash
az sql server create -g $RG -l $LOC -n $SQL_SERVER -u $SQL_ADMIN -p "$SQL_PASS"
az sql db create -g $RG -s $SQL_SERVER -n $SQL_DB --service-objective S0
az sql server firewall-rule create -g $RG -s $SQL_SERVER -n AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

Cadena de conexión objetivo (API):

```text
Server=tcp:<tu_sql_server>.database.windows.net,1433;Initial Catalog=RSISTEMADB;Persist Security Info=False;User ID=<user>;Password=<pass>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### 7.5 Crear entorno Container Apps

```bash
az containerapp env create -g $RG -n $ENV -l $LOC
```

### 7.6 Crear API Container App

```bash
az containerapp create \
  -g $RG -n $API_APP --environment $ENV \
  --image $IMAGE_API \
  --target-port 8080 --ingress external \
  --registry-server "$ACR.azurecr.io" \
  --cpu 0.5 --memory 1.0Gi \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    Jwt__Issuer=SistemaRepuestosMaquinas.API \
    Jwt__Audience=SistemaRepuestosMaquinas.Web \
    Jwt__SecretKey="$JWT_SECRET" \
    Jwt__ExpirationMinutes=120 \
    ConnectionStrings__DefaultConnection="Server=tcp:$SQL_SERVER.database.windows.net,1433;Initial Catalog=$SQL_DB;Persist Security Info=False;User ID=$SQL_ADMIN;Password=$SQL_PASS;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
    MercadoPago__PublicKey="<secret>" \
    MercadoPago__AccessToken="<secret>" \
    PasswordRecovery__SmtpHost="smtp.gmail.com" \
    PasswordRecovery__SmtpPort=587 \
    PasswordRecovery__SmtpUser="<secret>" \
    PasswordRecovery__SmtpPassword="<secret>" \
    PasswordRecovery__FromEmail="<secret>" \
    PasswordRecovery__FrontendResetUrl="https://<url-web>/Cuenta/RestablecerContrasena" \
    PasswordRecovery__UseSsl=true
```

Obtén URL pública de API:

```bash
API_FQDN=$(az containerapp show -g $RG -n $API_APP --query properties.configuration.ingress.fqdn -o tsv)
echo "https://$API_FQDN"
```

### 7.7 Crear Web Container App

```bash
WEB_API_BASE="https://$API_FQDN/"

az containerapp create \
  -g $RG -n $WEB_APP --environment $ENV \
  --image $IMAGE_WEB \
  --target-port 8080 --ingress external \
  --registry-server "$ACR.azurecr.io" \
  --cpu 0.5 --memory 1.0Gi \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    Api__BaseUrl="$WEB_API_BASE" \
    MercadoPago__PublicKey="<secret>"
```

Obtén URL pública Web:

```bash
WEB_FQDN=$(az containerapp show -g $RG -n $WEB_APP --query properties.configuration.ingress.fqdn -o tsv)
echo "https://$WEB_FQDN"
```

## 8) Post-despliegue (validación obligatoria)

1. Navegar a `https://<web_fqdn>`.
2. Registrar/login de usuario cliente.
3. Flujo catálogo → carrito → pedido.
4. Login admin (`admin@repuestos.com`) y validar módulo administrativo.
5. Confirmar correos/recuperación de contraseña.
6. Validar pagos sandbox/prod según llaves.
7. Revisar logs:

```bash
az containerapp logs show -g $RG -n $API_APP --follow
az containerapp logs show -g $RG -n $WEB_APP --follow
```

## 9) CI/CD recomendado (resumen)

En GitHub Actions/Azure DevOps:

1. Trigger en `main`.
2. Build test (`dotnet restore/build/test`).
3. Build imágenes Docker API/Web.
4. Push a ACR con tag (`sha`, `semver`).
5. `az containerapp update` para cada app.
6. Smoke tests.

## 10) Riesgos detectados y mitigación

1. **Credenciales expuestas en `appsettings`**
   - Mitigar: rotar secretos + mover a variables/Key Vault.
2. **Acoplamiento Web/API por URL fija local**
   - Mitigar: usar `Api__BaseUrl` por entorno.
3. **Inicialización DB en arranque**
   - Mitigar: controlar estrategia de migraciones en producción y permisos mínimos.
4. **Dependencia SMTP Gmail**
   - Mitigar: usar servicio transaccional (SendGrid, ACS Email, etc.) o cuenta dedicada.

## 11) Runbook de operación rápida

### Reinicio

```bash
az containerapp revision restart -g $RG -n $API_APP --revision "$(az containerapp revision list -g $RG -n $API_APP --query "[0].name" -o tsv)"
az containerapp revision restart -g $RG -n $WEB_APP --revision "$(az containerapp revision list -g $RG -n $WEB_APP --query "[0].name" -o tsv)"
```

### Escalado manual básico

```bash
az containerapp update -g $RG -n $API_APP --min-replicas 1 --max-replicas 3
az containerapp update -g $RG -n $WEB_APP --min-replicas 1 --max-replicas 3
```

### Rollback

Container Apps mantiene revisiones; puedes redirigir tráfico a una revisión anterior desde portal o CLI.

## 12) Checklist final para go-live

- [ ] Secretos rotados y fuera de Git.
- [ ] TLS + dominio personalizado configurado.
- [ ] Backups de Azure SQL habilitados.
- [ ] Alertas y monitoreo configurados.
- [ ] Pruebas E2E aprobadas.
- [ ] Política de costos y autoscaling revisada.

---

Si quieres, el siguiente paso puede ser crear el pipeline CI/CD exacto (GitHub Actions) para que el despliegue a Azure quede 100% automatizado con tags y rollback controlado.
