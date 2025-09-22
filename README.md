# ContextWeaver
## La Herramienta CLI  para el Context Engineering y Análisis Arquitectónico

`ContextWeaver` es una potente herramienta de línea de comandos (.NET Global Tool) diseñada para ingenieros de software, arquitectos y desarrolladores. Transforma cualquier codebase en un **documento Markdown único, coherente y enriquecido**, optimizado para el análisis por parte de Large Language Models (LLMs) y la colaboración en equipos.

### ¿Por qué ContextWeaver?

En la era del desarrollo asistido por IA, la calidad del contexto de entrada lo es todo. `ContextWeaver` aborda este desafío al:
1. **Consolidar el Código**: Combina múltiples archivos de un repositorio en un único documento Markdown, haciendo que el consumo por parte de LLMs sea más eficiente y completo.
2. **Optimizar el Contexto**: Filtra directorios y archivos irrelevantes (ej. `node_modules`, `bin`, `obj`), enfocándose en el código fuente y los artefactos clave.
3. **Proporcionar un Mapa de Código Inteligente**:
   - **Árbol de Directorios Navegable**: Una representación visual de la estructura del proyecto con enlaces directos a cada archivo dentro del documento.
   - **"Repo Map" por Archivo**: Extrae las firmas públicas (API) y las dependencias (`using`/`import`) de cada archivo de código, ofreciendo un resumen de alto nivel.
   - **Métricas Clave**: Incluye el conteo de Líneas de Código (LOC) y Complejidad Ciclomática a nivel de archivo.
4. **Identificar "Hotspots"**: Destaca automáticamente los 5 archivos con mayor tamaño (LOC) y mayor acoplamiento (número de imports), permitiendo enfocar la atención en áreas críticas.
5. **Análisis de Inestabilidad Arquitectónica (Métrica de Robert C. Martin)**: Calcula la métrica de Inestabilidad (I = Ce / (Ca + Ce)) a nivel de módulos (carpetas/proyectos) para ayudar a entender la dirección y la salud de las dependencias arquitectónicas. Identifica módulos estables (núcleo) e inestables (implementaciones).
6. **Configuración Flexible y por Proyecto**: La herramienta busca un archivo `.contextweaver.json` en el directorio analizado para usar configuraciones específicas del proyecto. Si no lo encuentra, utiliza la configuración global por defecto, permitiendo una gran adaptabilidad.

### Casos de Uso:
- **Context Engineering para IA**: Genera un contexto rico y estructurado para tareas complejas como refactorización estratégica, análisis de seguridad preliminar o generación de documentación.
- **Onboarding Acelerado**: Facilita a los nuevos miembros del equipo la comprensión rápida de un codebase complejo.
- **Revisiones de Código y Arquitectura**: Proporciona una visión macro del proyecto para revisiones más informadas y basadas en datos.
- **Transferencia de Conocimiento**: Crea artefactos permanentes del estado de un proyecto en un punto específico en el tiempo.

### Instalación y Uso:
`ContextWeaver` se distribuye como una .NET Global Tool a través de NuGet.

```
Bash

# Instalación
dotnet tool install --global ContextWeaver
```

#### Uso Básico (Recomendado)

La forma más sencilla de usar la herramienta es navegar hasta el directorio raíz de tu proyecto y ejecutar el comando. `ContextWeaver` analizará el directorio actual.

```bash
# 1. Navega a tu proyecto
cd C:\ruta\a\tu\repositorio

# 2. Ejecuta el comando (generará un analysis_report.md)
contextweaver
```

#### Uso Explícito

También puedes especificar todas las opciones manualmente desde cualquier ubicación.

```bash
contextweaver --directorio "C:\ruta\a\tu\repositorio" --output "reporte_personalizado.md" --format "markdown"
```

#### Configuración por Proyecto (Opcional)

Para anular la configuración global, crea un archivo `.contextweaver.json` en la raíz del proyecto que deseas analizar.

**Ejemplo de `.contextweaver.json`:**

```json
{
  "AnalysisSettings": {
    "IncludedExtensions": [
      ".cs",
      ".csproj",
      ".yml"
    ],
    "ExcludePatterns": [
      "bin",
      "obj",
      "node_modules",
      "docs"
    ]
  }
}
```
