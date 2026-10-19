using System;
using System.Collections.Generic;
using System.Text;

namespace Templating;

internal sealed class LimitedTextWriter(int maxCharacters) : TextWriter
{
    private readonly StringBuilder _buffer = new();
    public override Encoding Encoding => Encoding.UTF8;
    public override void Write(char value)
    {
        EnsureCapacity(1);
        _buffer.Append(value);
    }

    public override void Write(string? value)
    {
        if(value is null)
        {
            return;
        }
        EnsureCapacity(value.Length);
        _buffer.Append(value);
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        EnsureCapacity(buffer.Length);
        _buffer.Append(buffer);
    }

    public override Task WriteAsync(char value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(string? value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Write(buffer.Span);
        return Task.CompletedTask;
    }

    public override string ToString() => _buffer.ToString();

    private void EnsureCapacity(int additionalCharacters)
    {
        if (additionalCharacters > maxCharacters - _buffer.Length)
        {
            throw new InvalidOperationException(
                $"Rendered output exceeds the limit of {maxCharacters} characters.");
        }
    }
}
