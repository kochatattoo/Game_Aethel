import { readdirSync, mkdirSync, writeFileSync, readFileSync, existsSync } from 'node:fs';
import { resolve, join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';

// Offline fallback for the engine-independent assembly. This is not Unity Test Runner.
const root = resolve(fileURLToPath(new URL('..', import.meta.url)));
const output = join(root, 'Library', 'TacticsCodeChecks');
const cache = join(root, 'Library', 'PackageCache');
const nunitPackage = readdirSync(cache).find(name => name.startsWith('com.unity.ext.nunit@'));
if (!nunitPackage) throw new Error('Open the project in Unity once to restore the existing NUnit package.');
const escapeXml = value => value.replaceAll('&', '&amp;').replaceAll('"', '&quot;').replaceAll('<', '&lt;');
let source = join(root, 'Assets', '_project', 'CodeBase', 'Modules', 'Tactics');
const nunit = join(cache, nunitPackage, 'net40', 'unity-custom', 'nunit.framework.dll');
mkdirSync(output, { recursive: true });
if (process.argv.includes('--head')) {
  const trackedRoot = 'Assets/_project/CodeBase/Modules/Tactics/Runtime';
  const git = args => {
    const result = spawnSync('git', args, { cwd: root, encoding: 'utf8' });
    if (result.status !== 0) throw new Error(result.stderr);
    return result.stdout;
  };
  source = join(output, 'head-source-' + git(['rev-parse', '--short', 'HEAD']).trim());
  for (const file of git(['ls-tree', '-r', '--name-only', 'HEAD', '--', trackedRoot]).trim().split('\n')) {
    if (!file.endsWith('.cs')) continue;
    const destination = join(source, 'Runtime', file.slice(trackedRoot.length + 1));
    mkdirSync(dirname(destination), { recursive: true });
    writeFileSync(destination, git(['show', `HEAD:${file}`]));
  }
}
writeFileSync(join(output, 'NuGet.Config'), '<configuration><packageSources><clear /></packageSources></configuration>');
writeFileSync(join(output, 'Checks.csproj'), `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings><Nullable>disable</Nullable><LangVersion>9</LangVersion>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="${escapeXml(join(source, 'Runtime', '**', '*.cs'))}" />
    <Compile Include="${escapeXml(join(root, 'Assets', '_project', 'CodeBase', 'Modules', 'Tactics', 'Tests', 'Editor', '*.cs'))}" />
    <Reference Include="nunit.framework"><HintPath>${escapeXml(nunit)}</HintPath></Reference>
  </ItemGroup>
</Project>`);
writeFileSync(join(output, 'Program.cs'), `using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using NUnit.Framework;

internal static class Program
{
    private static int Main(string[] args)
    {
        var results = new List<object>();
        var failed = 0;
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.Namespace == "PetProject.Tactics.Tests").OrderBy(t => t.FullName))
        foreach (var method in type.GetMethods().Where(m => m.IsDefined(typeof(TestAttribute)))
            .OrderBy(m => m.Name))
        {
            // Deliberately support only the synchronous fixture shape used by this assembly.
            // Fail loudly if a test later requires TestCase, lifecycle hooks or UnityTest.
            string error = null;
            try
            {
                if (type.GetMethods().Any(m => m.IsDefined(typeof(SetUpAttribute)) || m.IsDefined(typeof(TearDownAttribute))))
                    throw new InvalidOperationException("Use Unity Test Runner for fixtures with lifecycle hooks");
                if (method.GetParameters().Length != 0 || method.ReturnType != typeof(void))
                    throw new InvalidOperationException("Use Unity Test Runner for parameterized/async tests");
                method.Invoke(Activator.CreateInstance(type), null);
            }
            catch (Exception ex) { error = (ex.InnerException ?? ex).ToString(); ++failed; }
            results.Add(new { name = type.Name + "." + method.Name, passed = error == null, error });
        }
        var unsupported = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "PetProject.Tactics.Tests")
            .SelectMany(t => t.GetMethods()).Where(m => m.GetCustomAttributes(false).Any(a =>
                a.GetType().Name == "TestCaseAttribute" || a.GetType().Name == "TestCaseSourceAttribute" ||
                a.GetType().Name == "UnityTestAttribute")).Select(m => m.Name).ToArray();
        if (unsupported.Length != 0) throw new InvalidOperationException("Unsupported tests: " + string.Join(",", unsupported));
        var publicApi = Assembly.GetExecutingAssembly().GetExportedTypes()
            .Where(t => t.Namespace == "PetProject.Tactics")
            .SelectMany(t => new[] { "TYPE " + t.FullName }.Concat(t.GetMembers(BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(m => t.FullName + " :: " + m.ToString())))
            .OrderBy(s => s, StringComparer.Ordinal).ToArray();
        var report = new { utc = DateTime.UtcNow.ToString("O"), runner = ".NET 9 offline reflection runner (not Unity)",
            passed = results.Count - failed, failed, total = results.Count, publicApi, results };
        File.WriteAllText(args[0], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Tactics source checks: {results.Count - failed}/{results.Count}; report: {args[0]}");
        return failed == 0 && results.Count > 0 ? 0 : 1;
    }
}
`);
const label = process.argv[2] ?? 'current';
if (!/^[a-z0-9-]+$/i.test(label)) throw new Error('Report label must use letters, digits and hyphens.');
// Compile production sources against the actual Editor's .NET Standard contract when installed.
// This runs Roslyn directly, not the Editor import pipeline or a player build.
const version = /m_EditorVersion:\s*(\S+)/.exec(readFileSync(join(root, 'ProjectSettings', 'ProjectVersion.txt'), 'utf8'))[1];
const unityData = process.env.UNITY_EDITOR_DATA ?? join(process.env.ProgramFiles ?? 'C:/Program Files', 'Unity', version, 'Editor', 'Data');
const compiler = join(unityData, 'DotNetSdkRoslyn', 'csc.dll');
const standard = join(unityData, 'NetStandard', 'ref', '2.1.0', 'netstandard.dll');
if (existsSync(compiler) && existsSync(standard)) {
  const sources = directory => readdirSync(directory, { withFileTypes: true }).flatMap(item => {
    const path = join(directory, item.name);
    return item.isDirectory() ? sources(path) : item.isFile() && item.name.endsWith('.cs') ? [path] : [];
  });
  const destination = join(output, 'unity-compiler-' + label);
  mkdirSync(destination, { recursive: true });
  const args = [compiler, '-nologo', '-noconfig', '-nostdlib+', '-langversion:9', '-target:library',
    '-reference:' + standard, '-out:' + join(destination, 'PetProject.Tactics.dll'), ...sources(join(source, 'Runtime'))];
  const compiled = spawnSync(join(unityData, 'NetCoreRuntime', 'dotnet.exe'), args, { cwd: root, encoding: 'utf8', windowsHide: true });
  const evidence = { utc: new Date().toISOString(), compiler, reference: standard, sourceCount: args.length - 8,
    exitCode: compiled.status, success: compiled.status === 0, stdout: compiled.stdout, stderr: compiled.stderr };
  writeFileSync(join(output, `unity-compiler-${label}.json`), JSON.stringify(evidence, null, 2));
  if (compiled.error) throw compiled.error;
  if (!evidence.success) throw new Error('Unity Roslyn compilation failed: ' + compiled.stdout + compiled.stderr);
  console.log('Unity Roslyn / .NET Standard 2.1: compiled (Editor import not exercised).');
} else console.log('Unity Roslyn check unavailable; set UNITY_EDITOR_DATA to the Editor Data directory.');
const report = join(output, `${label}.json`);
const result = spawnSync('dotnet', ['run', '--project', join(output, 'Checks.csproj'),
  `-p:RestoreConfigFile=${join(output, 'NuGet.Config')}`, '--', report],
  { cwd: root, stdio: 'inherit', env: { ...process.env, DOTNET_CLI_HOME: output,
    DOTNET_CLI_TELEMETRY_OPTOUT: '1', DOTNET_NOLOGO: '1' } });
if (result.error) throw result.error;
process.exitCode = result.status ?? 1;
