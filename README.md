# 💳 Estado de Cuenta - Tarjeta de Crédito

**Autor:** Rafael Orlando Guardado Díaz  
**Repositorio:** [https://github.com/Rafa07C/Estado_Cuenta_Tarjeta_Credito](https://github.com/Rafa07C/Estado_Cuenta_Tarjeta_Credito)

> Prueba Técnica — Desarrollador de Servicios Web

---

## 📋 Descripción

Aplicación web para visualizar y gestionar el estado de cuenta de una tarjeta de crédito. Permite consultar el saldo actual, registrar compras y pagos, calcular intereses y cuota mínima, y exportar el estado de cuenta en PDF y Excel.

---

## 🏗️ Arquitectura de la Solución

La solución está compuesta por **3 proyectos**:

```
CreditCardStatement/
├── CreditCardStatement.Core      # Entidades, DTOs, Interfaces, Validadores
├── CreditCardStatement.Api       # REST API (ASP.NET Web API)
└── CreditCardStatement.Mvc       # Frontend (ASP.NET MVC + Razor)
```

### Patrones y Prácticas Implementadas

| Patrón / Tecnología | Descripción |
|---------------------|-------------|
| **CQRS** | Separación de Commands (escritura) y Queries (lectura) con MediatR |
| **UnitOfWork** | Gestión centralizada de repositorios y conexión a base de datos |
| **Repository Pattern** | Abstracción del acceso a datos con Dapper |
| **AutoMapper** | Mapeo automático entre DTOs y ViewModels |
| **FluentValidation** | Validación de datos de entrada en la API |
| **GlobalException** | Middleware para manejo centralizado de errores |
| **Healthcheck** | Endpoint `/health` para monitoreo del estado de la API |
| **Swagger** | Documentación interactiva de la API en `/swagger` |
| **SOLID** | Principios aplicados en toda la arquitectura |

---

## 🛠️ Tecnologías Utilizadas

- **.NET 6** — Framework principal
- **ASP.NET Web API** — REST API
- **ASP.NET MVC + Razor** — Frontend
- **SQL Server** — Base de datos
- **Dapper** — ORM ligero para Stored Procedures
- **MediatR 11** — Implementación de CQRS
- **AutoMapper 12** — Mapeo de objetos
- **FluentValidation 11** — Validaciones
- **DinkToPdf** — Exportación a PDF
- **ClosedXML** — Exportación a Excel
- **Bootstrap 5 + jQuery** — UI/UX
- **Swagger / Swashbuckle** — Documentación de API

---

## 🗄️ Base de Datos

### Tablas

| Tabla | Descripción |
|-------|-------------|
| `CardHolders` | Titulares de tarjetas |
| `CreditCards` | Tarjetas de crédito |
| `Transactions` | Compras y pagos |
| `StatementConfig` | Configuración de tasas de interés |

### Stored Procedures

| Procedimiento | Descripción |
|--------------|-------------|
| `sp_GetStatement` | Obtiene el estado de cuenta completo con cálculos |
| `sp_GetMonthTransactions` | Obtiene las transacciones de un mes específico |
| `sp_AddPurchase` | Registra una nueva compra |
| `sp_AddPayment` | Registra un nuevo pago |

### Datos de Prueba

- **Titular:** Iván Patiño
- **Tarjeta:** **** **** **** 1234
- **Límite de Crédito:** $10,000.00
- **Tasa de Interés:** 25%
- **Porcentaje Saldo Mínimo:** 5%

---

## ⚙️ Configuración y Ejecución

### Pre-requisitos

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [SQL Server](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) (local o Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o VS Code

### Paso 1 — Clonar el repositorio

```bash
git clone https://github.com/Rafa07C/Estado_Cuenta_Tarjeta_Credito.git
cd Estado_Cuenta_Tarjeta_Credito
```

### Paso 2 — Configurar la base de datos

1. Abre **SQL Server Management Studio (SSMS)**
2. Ejecuta el script ubicado en:
   ```
   db/database.sql
   ```
   Este script crea la base de datos, tablas, stored procedures e inserta los datos de prueba.

### Paso 3 — Configurar la cadena de conexión

En `src/CreditCardStatement.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CreditCardStatementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Si tu instancia de SQL Server tiene un nombre diferente (ej. `SQLEXPRESS`), cambia `Server=.` por `Server=.\SQLEXPRESS`.

### Paso 4 — Ejecutar la solución

#### Opción A — Visual Studio
1. Abre `src/CreditCardStatement.sln`
2. Clic derecho en la solución → **Establecer proyectos de inicio**
3. Selecciona **Varios proyectos de inicio**
4. Marca `CreditCardStatement.Api` y `CreditCardStatement.Mvc` como **Iniciar**
5. Presiona `F5`

#### Opción B — Terminal

```bash
# Terminal 1 - API
cd src/CreditCardStatement.Api
dotnet run

# Terminal 2 - MVC
cd src/CreditCardStatement.Mvc
dotnet run
```

### URLs de acceso

| Aplicación | URL |
|-----------|-----|
| **MVC (Frontend)** | https://localhost:7063 |
| **API** | https://localhost:7213 |
| **Swagger** | https://localhost:7213/swagger |
| **Health Check** | https://localhost:7213/health |

---

## 🔌 Endpoints de la API

### Statement

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/Statement/{cardId}/{year}/{month}` | Obtiene el estado de cuenta |

**Ejemplo:**
```
GET https://localhost:7213/api/Statement/1/2026/3
```

**Respuesta:**
```json
{
  "cardHolderName": "Iván Patiño",
  "cardId": 1,
  "cardNumberLast4": "1234",
  "creditLimit": 10000.00,
  "currentBalance": 250.00,
  "availableBalance": 9750.00,
  "purchasesThisMonth": 250.00,
  "purchasesPreviousMonth": 0.00,
  "interestRate": 0.25,
  "minimumPaymentRate": 0.05,
  "interestBonificable": 62.50,
  "minimumPayment": 12.50,
  "totalToPay": 250.00,
  "totalToPayWithInterest": 312.50
}
```

---

### Transactions

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/Transactions/{cardId}/{year}/{month}` | Lista transacciones del mes |
| `POST` | `/api/transactions/purchase` | Registra una compra |
| `POST` | `/api/transactions/payment` | Registra un pago |

**POST Compra — Body:**
```json
{
  "cardId": 1,
  "txDate": "2026-03-04",
  "description": "Supermercado La Colonia",
  "amount": 250.00
}
```

**POST Pago — Body:**
```json
{
  "cardId": 1,
  "txDate": "2026-03-04",
  "amount": 500.00
}
```

---

## 🧪 Probar con Postman

1. Importa el archivo `postman/CreditCardStatement.postman_collection.json` en Postman
2. Selecciona el environment **CreditCardStatement Local**
3. Asegúrate de que ambos proyectos estén corriendo
4. Ejecuta los requests en orden

> **Nota:** Si aparece el error "Unable to verify certificate", desactiva la verificación SSL en Postman: **Settings → SSL certificate verification → OFF**

---

## 📱 Pantallas del Frontend

| Pantalla | URL | Descripción |
|----------|-----|-------------|
| Estado de Cuenta | `/` | Vista principal con resumen financiero y exportación PDF |
| Registrar Compra | `/Purchases` | Formulario para agregar compras |
| Registrar Pago | `/Payments` | Formulario para registrar pagos |
| Historial | `/Transactions` | Lista de transacciones con exportación Excel |

---

## 📐 Fórmulas de Cálculo

| Concepto | Fórmula |
|----------|---------|
| Interés Bonificable | `Saldo Total × Tasa de Interés` |
| Cuota Mínima | `Saldo Total × Porcentaje Mínimo` |
| Monto Total a Pagar | `Saldo Total` |
| Contado con Intereses | `Saldo Total + Interés Bonificable` |

**Ejemplo con Saldo = $114.47, Interés = 25%, Mínimo = 5%:**
- Interés Bonificable = $114.47 × 25% = **$28.61**
- Cuota Mínima = $114.47 × 5% = **$5.72**
- Total a Pagar = **$114.47**
- Contado con Intereses = $114.47 + $28.61 = **$143.08**

---

## 📁 Estructura del Repositorio

```
Estado_Cuenta_Tarjeta_Credito/
├── src/
│   ├── CreditCardStatement.sln
│   ├── CreditCardStatement.Core/
│   │   ├── DTOs/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   └── Validators/
│   ├── CreditCardStatement.Api/
│   │   ├── Controllers/
│   │   ├── CQRS/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── Infrastructure/
│   │   │   ├── Repositories/
│   │   │   └── UnitOfWork.cs
│   │   └── Middleware/
│   └── CreditCardStatement.Mvc/
│       ├── Controllers/
│       ├── Services/
│       ├── ViewModels/
│       └── Views/
├── db/
│   └── database.sql
└── postman/
    └── CreditCardStatement.postman_collection.json
```

---

*Desarrollado por Rafael Orlando Guardado Díaz — Prueba Técnica Banco Atlántida 2026*
