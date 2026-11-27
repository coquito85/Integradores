using ReferenciaStandard.Entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;




/// <summary>
/// Grava {"raiz":[ item, item, ... ]} em streaming, sem manter tudo na memoria.
/// Em modo arquivo pode dividir em varios arquivos (maxPerFile); em modo virtual acumula em memoria.
/// </summary>
public sealed class JsonSink : IDisposable
{
    private static readonly JsonWriterOptions WriterOptions = new JsonWriterOptions()
    {
        Indented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping   // acentos legiveis, sem \u00E7
    };

    private readonly string _rootName;
    private readonly bool _toFiles;
    private readonly string _folder;
    private readonly string _baseName;
    private readonly int _maxPerFile;

    private Stream _stream;
    private Utf8JsonWriter _writer;
    private string _currentPath;
    private long _inFile;
    private int _fileIndex;
    private readonly List<GeneratedFile> _files = new List<GeneratedFile>();
    private MemoryStream _memory;

    public long TotalRows { get; private set; }
    public IReadOnlyList<GeneratedFile> Files => _files;

    /// <summary>Conteudo gerado (somente modo virtual, apos Complete).</summary>
    public string VirtualJson { get; private set; }

    public JsonSink(string rootName, bool toFiles, string folder, string baseName, int maxPerFile)
    {
        _rootName = rootName;
        _toFiles = toFiles;
        _folder = folder ?? "";
        _baseName = baseName;
        _maxPerFile = Math.Max(0, maxPerFile);
        if (toFiles) Directory.CreateDirectory(_folder);
    }

    public void Write(JsonNode item)
    {
        if (_writer == null || (_maxPerFile > 0 && _inFile >= _maxPerFile))
            Rotate();
        item.WriteTo(_writer);
        _inFile++;
        TotalRows++;
    }

    private void Rotate()
    {
        CloseCurrent();
        _fileIndex++;

        if (_toFiles)
        {
            _currentPath = UniquePath();
            _stream = new FileStream(_currentPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 64 * 1024);
        }
        else
        {
            _memory = new MemoryStream();
            _stream = _memory;
        }

        _writer = new Utf8JsonWriter(_stream, WriterOptions);
        _writer.WriteStartObject();
        _writer.WritePropertyName(_rootName);
        _writer.WriteStartArray();
        _inFile = 0;
    }

    private string UniquePath()
    {
        var name = _maxPerFile > 0 || _fileIndex > 1 ? $"{_baseName}_{_fileIndex:000}" : _baseName;
        var path = Path.Combine(_folder, name + ".json");
        var n = 2;
        while (File.Exists(path))
            path = Path.Combine(_folder, $"{name}_{n++}.json");
        return path;
    }

    private void CloseCurrent()
    {
        if (_writer == null) return;
        _writer.WriteEndArray();
        _writer.WriteEndObject();
        _writer.Flush();
        _writer.Dispose();
        _writer = null;

        if (_toFiles && _currentPath != null)
        {
            _stream.Dispose();
            _files.Add(new GeneratedFile(_currentPath, _inFile, new FileInfo(_currentPath).Length));
        }
        else if (_memory != null)
        {
            VirtualJson = Encoding.UTF8.GetString(_memory.ToArray());
        }
        _stream = null;
    }

    /// <summary>Fecha o que estiver aberto. Mesmo com zero linhas gera um documento valido (array vazio).</summary>
    public void Complete()
    {
        if (_writer == null && TotalRows == 0) Rotate();
        CloseCurrent();
    }

    /// <summary>Cancelamento/erro: fecha e apaga os arquivos parciais.</summary>
    public void Abort()
    {
        try { _writer?.Dispose(); } catch { }
        try { _stream?.Dispose(); } catch { }
        _writer = null;
        _stream = null;
        if (!_toFiles) return;
        foreach (var f in _files) TryDelete(f.Path);
        if (_currentPath != null) TryDelete(_currentPath);
        _files.Clear();
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    public void Dispose()
    {
        try { _writer?.Dispose(); } catch { }
        try { _stream?.Dispose(); } catch { }
    }
}