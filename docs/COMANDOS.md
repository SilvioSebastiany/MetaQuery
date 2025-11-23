# ⚡ Comandos Úteis - SqlKata

## 🚀 Executar Exemplos

```bash
# Navegar até a pasta
cd c:\SilvioArquivos\dev\DaniloAparecido\querybuilder\Exemplos

# Executar
dotnet run

# Compilar sem executar
dotnet build

# Limpar build
dotnet clean
```

---

## 🔧 Build do Projeto

```bash
# Build de tudo
cd c:\SilvioArquivos\dev\DaniloAparecido\querybuilder
dotnet build

# Build apenas QueryBuilder
cd QueryBuilder
dotnet build

# Build apenas Exemplos
cd Exemplos
dotnet build

# Build em Release
dotnet build -c Release
```

---

## 🧪 Executar Testes

```bash
# Todos os testes
cd MetaQuery.Tests
dotnet test

# Testes com detalhes
dotnet test -v detailed

# Testes de uma classe específica
dotnet test --filter "FullyQualifiedName~SelectTests"

# Testes por nome
dotnet test --filter "Name~BasicSelect"
```

---

## 📦 Gerenciar Pacotes

```bash
# Restaurar dependências
dotnet restore

# Adicionar SqlKata ao seu projeto
dotnet add package SqlKata
dotnet add package SqlKata.Execution

# Atualizar pacotes
dotnet list package --outdated
dotnet add package SqlKata --version <nova-versao>

# Remover pacote
dotnet remove package SqlKata
```

---

## 🗂️ Explorar Solution

```bash
# Ver projetos na solution
dotnet sln list

# Adicionar projeto à solution
dotnet sln add <caminho-do-projeto.csproj>

# Remover projeto da solution
dotnet sln remove <caminho-do-projeto.csproj>
```

---

## 📝 Visualizar Arquivos

### Windows PowerShell
```powershell
# Ver conteúdo de arquivo
Get-Content GUIA_APRENDIZADO.md

# Buscar texto em arquivo
Select-String -Path "*.md" -Pattern "Query"

# Listar arquivos
Get-ChildItem -Recurse -Filter "*.md"

# Abrir no editor padrão
notepad GUIA_APRENDIZADO.md

# Abrir no VS Code
code GUIA_APRENDIZADO.md
```

### Linux/Mac
```bash
# Ver conteúdo de arquivo
cat GUIA_APRENDIZADO.md

# Ver com paginação
less GUIA_APRENDIZADO.md

# Buscar texto
grep -r "Query" *.md

# Listar arquivos
find . -name "*.md"

# Abrir no VS Code
code GUIA_APRENDIZADO.md
```

---

## 🔍 Depuração

### Visual Studio Code
```bash
# Abrir pasta no VS Code
code .

# Abrir arquivo específico
code Exemplos/01_BasicoSelect.cs

# Debug
# Pressione F5 após configurar launch.json
```

### Breakpoints no Código
```csharp
// Adicione isto onde quiser parar:
System.Diagnostics.Debugger.Break();
```

---

## 📊 Análise de Código

```bash
# Formato do código
dotnet format

# Análise estática
dotnet build /p:TreatWarningsAsErrors=true

# Ver warnings
dotnet build /p:WarningLevel=4
```

---

## 🗄️ Git (Controle de Versão)

```bash
# Ver status
git status

# Ver histórico
git log --oneline

# Ver diferenças
git diff

# Ver arquivos modificados
git diff --name-only

# Adicionar arquivos
git add .

# Commit
git commit -m "Mensagem"

# Push
git push origin main

# Ver branches
git branch -a
```

---

## 🏃 Atalhos VS Code

| Atalho | Ação |
|--------|------|
| `Ctrl+P` | Abrir arquivo rápido |
| `Ctrl+Shift+P` | Command Palette |
| `Ctrl+F` | Buscar no arquivo |
| `Ctrl+Shift+F` | Buscar em todos arquivos |
| `F5` | Iniciar debug |
| `F9` | Toggle breakpoint |
| `Ctrl+K Ctrl+C` | Comentar |
| `Ctrl+K Ctrl+U` | Descomentar |
| `Ctrl+Space` | IntelliSense |
| `Ctrl+.` | Quick Fix |

---

## 📚 Comandos de Documentação

```bash
# Abrir documentação
start https://sqlkata.com/docs  # Windows
open https://sqlkata.com/docs   # Mac
xdg-open https://sqlkata.com/docs  # Linux

# Gerar documentação do código
dotnet tool install -g docfx
docfx init
docfx build
```

---

## 🧹 Limpeza

```bash
# Limpar builds
dotnet clean

# Remover bin e obj recursivamente (PowerShell)
Get-ChildItem -Include bin,obj -Recurse | Remove-Item -Recurse -Force

# Remover bin e obj recursivamente (Linux/Mac)
find . -name "bin" -o -name "obj" | xargs rm -rf

# Limpar cache NuGet
dotnet nuget locals all --clear
```

---

## 📦 Publicação

```bash
# Publicar para Windows
dotnet publish -c Release -r win-x64 --self-contained

# Publicar para Linux
dotnet publish -c Release -r linux-x64 --self-contained

# Publicar para Mac
dotnet publish -c Release -r osx-x64 --self-contained

# Single file
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

---

## 🔧 Configuração de Ambiente

```bash
# Ver versão do .NET
dotnet --version

# Ver SDKs instalados
dotnet --list-sdks

# Ver runtimes instalados
dotnet --list-runtimes

# Ver informações completas
dotnet --info
```

---

## 📝 Criação de Novos Projetos

```bash
# Console app
dotnet new console -n MeuProjeto

# Class library
dotnet new classlib -n MinhaLib

# xUnit test project
dotnet new xunit -n MeusTestes

# Ver templates disponíveis
dotnet new --list
```

---

## 🔗 Criar Referências entre Projetos

```bash
# Adicionar referência
cd MeuProjeto
dotnet add reference ../QueryBuilder/MetaQuery.csproj

# Remover referência
dotnet remove reference ../QueryBuilder/MetaQuery.csproj

# Listar referências
dotnet list reference
```

---

## 🎯 Exemplos Específicos

### Executar exemplo específico (modificado)
```csharp
// Modifique o Program.cs para executar direto:
static void Main(string[] args)
{
    BasicoSelect.Executar();  // Executa apenas o exemplo 1
}
```

### Compilar e executar em um comando
```bash
dotnet build && dotnet run
```

### Assistir mudanças e recompilar
```bash
dotnet watch run
```

---

## 📊 Performance

```bash
# Benchmark
dotnet add package BenchmarkDotNet
# Criar classe de benchmark

# Profile de memória
dotnet-trace collect --process-id <PID>

# Análise de performance
dotnet-counters monitor --process-id <PID>
```

---

## 🐛 Troubleshooting

```bash
# Erro de build - limpar tudo
dotnet clean
rm -rf bin obj
dotnet restore
dotnet build

# Erro de referência - verificar
dotnet list reference
dotnet restore

# Erro de package - limpar cache
dotnet nuget locals all --clear
dotnet restore
```

---

## 💡 Dicas Úteis

### Ver SQL gerado sem executar
```csharp
var query = new Query("users").Where("id", 1);
var compiler = new SqlServerCompiler();
var result = compiler.Compile(query);
Console.WriteLine(result.Sql);
Console.WriteLine(string.Join(", ", result.Bindings));
```

### Debug de Clauses
```csharp
var query = new Query("users").Where("id", 1);
foreach (var clause in query.Clauses)
{
    Console.WriteLine($"{clause.Component}: {clause.GetType().Name}");
}
```

### Benchmark manual
```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
// Seu código aqui
sw.Stop();
Console.WriteLine($"Tempo: {sw.ElapsedMilliseconds}ms");
```

---

## 🔍 Busca no Código

### Buscar string no código fonte
```bash
# PowerShell
Select-String -Path "QueryBuilder\*.cs" -Pattern "WhereRaw" -Recurse

# Linux/Mac
grep -r "WhereRaw" QueryBuilder/*.cs
```

### Buscar definição de método
```bash
# VS Code: F12 (Go to Definition)
# ou Ctrl+Click no método
```

---

## 📚 Comandos Markdown

### Visualizar Markdown no VS Code
- Pressione `Ctrl+Shift+V` para preview
- Pressione `Ctrl+K V` para preview lado a lado

### Gerar HTML do Markdown
```bash
# Instalar markdown-it
npm install -g markdown-it

# Converter
markdown-it GUIA_APRENDIZADO.md > guia.html
```

---

**Salve este arquivo para consulta rápida! 🚀**
