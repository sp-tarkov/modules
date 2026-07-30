using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace SPT.PrePatch;

/// <summary>
///     Uses Windows WinHTTP because Mono's TLS provider is not initialized when prepatchers run.
/// </summary>
internal static class WinHttpClient
{
    private const uint AccessTypeNoProxy = 1;
    private const uint FlagSecure = 0x00800000;
    private const uint QueryStatusCode = 19;
    private const uint QueryFlagNumber = 0x20000000;
    private const uint OptionSecurityFlags = 31;
    private const uint SecurityFlagIgnoreUnknownCa = 0x00000100;
    private const uint SecurityFlagIgnoreCertWrongUsage = 0x00000200;
    private const uint SecurityFlagIgnoreCertCnInvalid = 0x00001000;
    private const uint SecurityFlagIgnoreCertDateInvalid = 0x00002000;

    public static string GetString(Uri uri)
    {
        var session = IntPtr.Zero;
        var connection = IntPtr.Zero;
        var request = IntPtr.Zero;

        try
        {
            session = WinHttpOpen("spt-prepatch/1.0", AccessTypeNoProxy, null, null, 0);
            EnsureHandle(session, "open a WinHTTP session");

            if (!WinHttpSetTimeouts(session, 10_000, 10_000, 10_000, 10_000))
            {
                ThrowLastError("configure WinHTTP timeouts");
            }

            connection = WinHttpConnect(session, uri.Host, checked((ushort)uri.Port), 0);
            EnsureHandle(connection, "connect to the SPT server");

            request = WinHttpOpenRequest(
                connection,
                "GET",
                uri.PathAndQuery,
                null,
                null,
                IntPtr.Zero,
                uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? FlagSecure : 0
            );
            EnsureHandle(request, "create the SPT server request");

            if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                var securityFlags =
                    SecurityFlagIgnoreUnknownCa
                    | SecurityFlagIgnoreCertWrongUsage
                    | SecurityFlagIgnoreCertCnInvalid
                    | SecurityFlagIgnoreCertDateInvalid;

                if (!WinHttpSetOption(request, OptionSecurityFlags, ref securityFlags, sizeof(uint)))
                {
                    ThrowLastError("configure the SPT HTTPS certificate policy");
                }
            }

            if (!WinHttpSendRequest(request, null, 0, IntPtr.Zero, 0, 0, IntPtr.Zero))
            {
                ThrowLastError("send the SPT server request");
            }

            if (!WinHttpReceiveResponse(request, IntPtr.Zero))
            {
                ThrowLastError("receive the SPT server response");
            }

            var statusCode = GetStatusCode(request);
            var responseBytes = ReadResponse(request);
            var response = Encoding.UTF8.GetString(DecompressZlib(responseBytes));
            if (statusCode < 200 || statusCode >= 300)
            {
                throw new InvalidOperationException($"The SPT server returned HTTP {statusCode}: {response}");
            }

            return response;
        }
        finally
        {
            CloseHandle(request);
            CloseHandle(connection);
            CloseHandle(session);
        }
    }

    private static uint GetStatusCode(IntPtr request)
    {
        var statusCode = 0u;
        var statusCodeSize = (uint)sizeof(uint);
        if (!WinHttpQueryHeaders(request, QueryStatusCode | QueryFlagNumber, null, ref statusCode, ref statusCodeSize, IntPtr.Zero))
        {
            ThrowLastError("read the SPT server status code");
        }

        return statusCode;
    }

    private static byte[] ReadResponse(IntPtr request)
    {
        using var stream = new MemoryStream();

        while (true)
        {
            if (!WinHttpQueryDataAvailable(request, out var available))
            {
                ThrowLastError("query the SPT response size");
            }

            if (available == 0)
            {
                break;
            }

            var buffer = new byte[available];
            if (!WinHttpReadData(request, buffer, available, out var bytesRead))
            {
                ThrowLastError("read the SPT server response");
            }

            stream.Write(buffer, 0, checked((int)bytesRead));
        }

        return stream.ToArray();
    }

    private static byte[] DecompressZlib(byte[] response)
    {
        if (!HasZlibHeader(response))
        {
            return response;
        }

        // A zlib stream wraps raw DEFLATE data in a two-byte header and
        // a four-byte Adler-32 checksum. DeflateStream expects the raw payload.
        var hasPresetDictionary = (response[1] & 0x20) != 0;
        if (hasPresetDictionary)
        {
            throw new InvalidDataException("The SPT server response uses an unsupported zlib preset dictionary.");
        }

        using var compressed = new MemoryStream(response, 2, response.Length - 6, false);
        using var deflate = new DeflateStream(compressed, CompressionMode.Decompress);
        using var decompressed = new MemoryStream();
        deflate.CopyTo(decompressed);
        return decompressed.ToArray();
    }

    private static bool HasZlibHeader(byte[] response)
    {
        if (response.Length < 6)
        {
            return false;
        }

        var compressionMethod = response[0] & 0x0F;
        var compressionInfo = response[0] >> 4;
        var headerChecksum = (response[0] << 8) + response[1];

        return compressionMethod == 8 && compressionInfo <= 7 && headerChecksum % 31 == 0;
    }

    private static void EnsureHandle(IntPtr handle, string operation)
    {
        if (handle == IntPtr.Zero)
        {
            ThrowLastError(operation);
        }
    }

    private static void CloseHandle(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            WinHttpCloseHandle(handle);
        }
    }

    private static void ThrowLastError(string operation)
    {
        throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to {operation}");
    }

    [DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr WinHttpOpen(string userAgent, uint accessType, string? proxyName, string? proxyBypass, uint flags);

    [DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr WinHttpConnect(IntPtr session, string serverName, ushort serverPort, uint reserved);

    [DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr WinHttpOpenRequest(
        IntPtr connection,
        string verb,
        string objectName,
        string? version,
        string? referrer,
        IntPtr acceptTypes,
        uint flags
    );

    [DllImport("winhttp.dll", SetLastError = true)]
    private static extern bool WinHttpSetTimeouts(
        IntPtr handle,
        int resolveTimeout,
        int connectTimeout,
        int sendTimeout,
        int receiveTimeout
    );

    [DllImport("winhttp.dll", SetLastError = true)]
    private static extern bool WinHttpSetOption(IntPtr handle, uint option, ref uint buffer, uint bufferLength);

    [DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool WinHttpSendRequest(
        IntPtr request,
        string? headers,
        uint headersLength,
        IntPtr optional,
        uint optionalLength,
        uint totalLength,
        IntPtr context
    );

    [DllImport("winhttp.dll", SetLastError = true)]
    private static extern bool WinHttpReceiveResponse(IntPtr request, IntPtr reserved);

    [DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool WinHttpQueryHeaders(
        IntPtr request,
        uint infoLevel,
        string? name,
        ref uint buffer,
        ref uint bufferLength,
        IntPtr index
    );

    [DllImport("winhttp.dll", SetLastError = true)]
    private static extern bool WinHttpQueryDataAvailable(IntPtr request, out uint available);

    [DllImport("winhttp.dll", SetLastError = true)]
    private static extern bool WinHttpReadData(IntPtr request, [Out] byte[] buffer, uint bytesToRead, out uint bytesRead);

    [DllImport("winhttp.dll", SetLastError = true)]
    private static extern bool WinHttpCloseHandle(IntPtr handle);
}
