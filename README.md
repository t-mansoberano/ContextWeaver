# ContextWeaver
## La Herramienta CLI Esencial para Generar Contexto de Código Optimizado para LLMs y Análisis Arquitectónico

`ContextWeaver` es una potente herramienta de línea de comandos (CLI) diseñada para ingenieros de software, arquitectos y desarrolladores que trabajan con Large Language Models (LLMs) o necesitan una visión profunda y estructurada de sus repositorios de código. Transforma tu codebase en un documento Markdown cohesivo y navegable, eliminando el ruido y resaltando la información más relevante.

### ¿Por qué ContextWeaver?

En el mundo de la ingeniería de prompts y el análisis de código asistido por IA, la calidad del contexto de entrada es paramount. ContextWeaver aborda este desafío al:
1. **Consolidar el Código**: Combina múltiples archivos de un repositorio en un único documento Markdown, haciendo que el consumo por parte de LLMs sea más eficiente y completo.
2. **Optimizar para LLMs**: Elimina archivos binarios y directorios irrelevantes (ej. node_modules, bin, obj), enfocándose solo en el código fuente y la estructura clave.
3. **Proporcionar un Mapa de Código Inteligente**:
   - **Árbol de Directorios Navegable**: Una representación visual de la estructura del proyecto con enlaces directos a cada archivo en el documento.
   - **"Repo Map" por Archivo**: Extrae y lista las firmas públicas (clases, métodos, propiedades) y las declaraciones using/import para cada archivo de código (actualmente C#), ofreciendo un resumen de su interfaz pública y sus dependencias.
   - **Métricas Clave**: Incluye el conteo de Líneas de Código (LOC) y Complejidad Ciclomática para C# a nivel de archivo.
4. **Identificar "Hotspots"**: Destaca automáticamente los 5 archivos con más Líneas de Código (LOC) y los 5 con mayor número de imports/usings, permitiendo identificar rápidamente áreas de alta complejidad o acoplamiento.
5. **Análisis de Inestabilidad Arquitectónica (Métrica de Robert C. Martin)**: Calcula la métrica de Inestabilidad (I = Ce / (Ca + Ce)) a nivel de módulos (carpetas/proyectos) para ayudar a entender la dirección y la salud de las dependencias arquitectónicas. Identifica módulos estables (núcleo) e inestables (implementaciones).
6. **Configuración Flexible**: Permite definir patrones de exclusión de directorios y extensiones de archivos incluidas a través de appsettings.json, adaptándose a las necesidades de cualquier proyecto.

### Casos de Uso:
- **Ingeniería de Prompts**: Genera un contexto de código rico y estructurado para modelos de lenguaje, mejorando la calidad de las respuestas en tareas como refactorización, generación de nuevas funcionalidades o análisis de vulnerabilidades.
- **Revisión de Código Asistida por IA**: Proporciona un resumen ejecutivo y enlaces directos a las partes más relevantes del código para acelerar las revisiones.
- **Análisis Arquitectónico Rápido**: Obtén una visión general de la estructura de dependencias y los "hotspots" del proyecto sin necesidad de herramientas complejas.
- **Onboarding**: Facilita la comprensión rápida de un nuevo codebase por parte de nuevos miembros del equipo.

### Instalación y Uso:
`ContextWeaver` se distribuye como una .NET Global Tool a través de NuGet.

```
Bash

# Instalación
dotnet tool install --global ContextWeaver

# Uso
contextweaver --directorio "C:\ruta\a\tu\repositorio" --output "reporte_contexto.md" --format "markdown"
contextweaver --output "reporte_contexto.md" --format "markdown"
contextweaver
```
